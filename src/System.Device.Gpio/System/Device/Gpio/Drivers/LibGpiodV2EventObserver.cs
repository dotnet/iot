// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System.Collections.Generic;
using System.Device.Gpio.Libgpiod.V2;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace System.Device.Gpio.Drivers;

/// <summary>
/// Class that observes libgpiod line requests for events.
/// </summary>
[Experimental(DiagnosticIds.SDGPIO0001, UrlFormat = DiagnosticIds.UrlFormat)]
internal sealed class LibGpiodV2EventObserver : IDisposable
{
    private readonly Dictionary<EventSubscription, List<PinChangeEventHandler>> _handlersBySubscription = new();

    private readonly List<Thread> _requestObserverThreads = new();
    private readonly HashSet<LineRequest> _observedRequests = new();

    private bool _shouldExit;

    /// <summary>
    /// Timeout for waiting on edge events.
    /// </summary>
    public TimeSpan WaitEdgeEventsTimeout { get; set; } = TimeSpan.FromMilliseconds(100);

    /// <summary>
    /// Registers an event subscription and returns a handle that can be waited on for the event.
    /// When the event appears, the subscription will get removed again
    /// </summary>
    /// <param name="request">The line request to observe</param>
    /// <param name="eventSubscription">The subscription for events</param>
    /// <returns>A handle to wait on</returns>
    internal EventWaitHandle ObserveSingleEvent(LineRequest request, EventSubscription eventSubscription)
    {
        lock (_handlersBySubscription)
        {
            EventWaitHandle eventWaitHandle = new();

            Observe(request, eventSubscription, eventWaitHandle.SetEventResult);

            Task.Run(() =>
            {
                eventWaitHandle.WaitForEventResult(CancellationToken.None);
                RemoveCallback(eventSubscription.Offset, eventWaitHandle.SetEventResult);
            });

            return eventWaitHandle;
        }
    }

    /// <summary>
    /// Registers an event subscription that may call <paramref name="callback"/>
    /// </summary>
    /// <param name="request">The line request to observe</param>
    /// <param name="eventSubscription">The subscription for events</param>
    /// <param name="callback">Method to call when event arrives</param>
    internal void Observe(LineRequest request, EventSubscription eventSubscription, PinChangeEventHandler callback)
    {
        lock (_handlersBySubscription)
        {
            if (_handlersBySubscription.TryGetValue(eventSubscription, out var eventHandlers))
            {
                eventHandlers.Add(callback);
            }
            else
            {
                _handlersBySubscription[eventSubscription] = new List<PinChangeEventHandler> { callback };
            }

            // observe request in only 1 task
            if (_observedRequests.Add(request))
            {
                var thread = new Thread(() => HandleEdgeEventsOfRequestInLoop(request));
                thread.Start();
                _requestObserverThreads.Add(thread);
            }
        }
    }

    /// <summary>
    /// Returns whether there exists a subscription for <param name="offset"></param>
    /// </summary>
    public bool HasSubscriptionsFor(Offset offset)
    {
        lock (_handlersBySubscription)
        {
            return _handlersBySubscription.Keys.Any(subscription => subscription.Offset == offset);
        }
    }

    /// <summary>
    /// Removes a callback of an event subscription. If no callbacks are present anymore, the event subscription also gets removed.
    /// </summary>
    internal void RemoveCallback(Offset offset, PinChangeEventHandler callback)
    {
        lock (_handlersBySubscription)
        {
            var subscription = _handlersBySubscription.Keys.FirstOrDefault(eventSubscription => eventSubscription.Offset == offset);
            if (subscription == null)
            {
                return;
            }

            var callbacks = _handlersBySubscription[subscription];
            if (!callbacks.Remove(callback))
            {
                return;
            }

            if (callbacks.Count == 0)
            {
                _handlersBySubscription.Remove(subscription);
            }
        }
    }

    /// <summary>
    /// Removes all event subscriptions of all specified offsets
    /// </summary>
    internal void RemoveSubscriptions(IEnumerable<Offset> offsets)
    {
        foreach (var offset in offsets)
        {
            RemoveSubscriptions(offset);
        }
    }

    /// <summary>
    /// Removes all event subscriptions of the specified offset
    /// </summary>
    internal void RemoveSubscriptions(Offset offset)
    {
        lock (_handlersBySubscription)
        {
            var subscription = _handlersBySubscription.Keys.FirstOrDefault(eventSubscription => eventSubscription.Offset == offset);
            if (subscription != null)
            {
                _handlersBySubscription.Remove(subscription);
            }
        }
    }

    /// <summary>
    /// Since <see cref="LibGpiodV2Driver.AddCallbackForPinValueChangedEvent"/> does not create new requests (only adjusts present requests)
    /// it can happen that at a later point in time a new request is created for lines that have present event subscriptions in the observer.
    /// This method ensures that the line config sent in the (new) request is enriched with edge detection based on present event subscriptions.
    /// There is also the case where the pin mode is changed to output, which requires edge detection to be reset. When switching back to input,
    /// any present event subscriptions should be re-considered.
    /// </summary>
    internal void EnrichLineConfigWithPresentEventSubscriptions(LineConfig lineConfig)
    {
        lock (_handlersBySubscription)
        {
            IReadOnlyDictionary<Offset, LineSettings> lineSettingsByOffset = lineConfig.GetSettingsByLine();

            foreach (EventSubscription eventSubscription in _handlersBySubscription.Keys)
            {
                Offset offset = eventSubscription.Offset;

                if (!lineSettingsByOffset.TryGetValue(offset, out var lineSettings))
                {
                    continue;
                }

                var direction = lineSettings.GetDirection();
                if (direction == GpiodLineDirection.Output)
                {
                    return;
                }

                GpiodLineEdge configuredEdgeDetection = lineSettings.GetEdgeDetection();

                switch (configuredEdgeDetection)
                {
                    case GpiodLineEdge.Both:
                        continue;
                    case GpiodLineEdge.None:
                        lineSettings.SetEdgeDetection(eventSubscription.Edge);
                        lineConfig.AddLineSettings(offset, lineSettings);
                        break;
                    case GpiodLineEdge.Rising:
                    case GpiodLineEdge.Falling:
                    default:
                        // the configured edge detection of the new request takes precedence.
                        break;
                }
            }
        }
    }

    private void HandleEdgeEventsOfRequestInLoop(LineRequest request)
    {
        List<Offset> requestedOffsets = new();

        try
        {
            requestedOffsets.AddRange(request.GetRequestedOffsets());

            using EdgeEventBuffer edgeEventBuffer = LibGpiodProxyFactory.CreateEdgeEventBuffer();

            while (request.IsAlive && !_shouldExit)
            {
                int waitResult = request.WaitEdgeEventsRespectfully(WaitEdgeEventsTimeout);
                bool isGpiodTimeout = waitResult == 0;
                if (isGpiodTimeout)
                {
                    continue;
                }

                bool isInterrupted = waitResult == 2;
                if (isInterrupted)
                {
                    continue;
                }

                int numberOfReadEvents = request.ReadEdgeEvents(edgeEventBuffer);

                for (int i = 0; i < numberOfReadEvents; i++)
                {
                    EdgeEvent edgeEvent = edgeEventBuffer.GetEvent((ulong)i);
                    HandleEdgeEvent(edgeEvent);
                }
            }
        }
        catch (Exception e) when (e is not OperationCanceledException)
        {
            Console.WriteLine($"Unhandled exception while handling libgpiodv2 edge events: {e}");
        }
        finally
        {
            RemoveSubscriptions(requestedOffsets);
            _observedRequests.Remove(request);
        }
    }

    private void HandleEdgeEvent(EdgeEvent edgeEvent)
    {
        lock (_handlersBySubscription)
        {
            Offset offset = edgeEvent.GetLineOffset();
            GpiodEdgeEventType eventType = edgeEvent.GetEventType();

            var relevantSubscriptions = _handlersBySubscription.Keys.Where(eventSubscription => eventSubscription.IsFor(offset, eventType));

            // Should an event handler re enter this class and modify the collection, it will throw, so ToArray the collection first
            foreach (var eventSubscription in relevantSubscriptions.ToArray())
            {
                if (_handlersBySubscription.TryGetValue(eventSubscription, out var eventHandlers))
                {
                    // Should an event handler re enter this class and modify the collection, it will throw, so ToArray the collection first
                    CallEventHandlers(offset, eventType, eventHandlers.ToArray());
                }
            }
        }
    }

    private void CallEventHandlers(Offset offset, GpiodEdgeEventType edgeEventType, IEnumerable<PinChangeEventHandler> eventHandlers)
    {
        foreach (PinChangeEventHandler eventHandler in eventHandlers)
        {
            eventHandler.Invoke(this, new PinValueChangedEventArgs(Translator.Translate(edgeEventType), (int)offset));
        }
    }

    #region Dispose

    /// <inheritdoc/>
    public void Dispose()
    {
        _shouldExit = true;

        // wait for all observer tasks to complete
        // as long as they are not complete, they might still wait on edge events, and until that returns the request is still open,
        // which means the lines are still reserved
        // this should only return after all requests have been released
        foreach (var t in _requestObserverThreads)
        {
            t.Join();
        }
    }

    #endregion

    internal sealed class EventWaitHandle
    {
        private readonly ManualResetEventSlim _resultSet = new();
        private WaitForEventResult _result;

        public WaitForEventResult WaitForEventResult(CancellationToken cancellationToken)
        {
            try
            {
                _resultSet.Wait(cancellationToken);
            }
            catch (OperationCanceledException)
            {
                return new WaitForEventResult { EventTypes = PinEventTypes.None, TimedOut = true };
            }

            return _result;
        }

        public void SetEventResult(object sender, PinValueChangedEventArgs pinValueChangedEventArgs)
        {
            _result = new WaitForEventResult { EventTypes = pinValueChangedEventArgs.ChangeType, TimedOut = false };
            _resultSet.Set();
        }
    }

    internal sealed record EventSubscription(Offset Offset, GpiodLineEdge Edge)
    {
        public bool IsFor(Offset offset, GpiodEdgeEventType edgeEventType)
        {
            if (offset != Offset)
            {
                return false;
            }

            if (Edge == GpiodLineEdge.None)
            {
                return false;
            }

            if (Edge == GpiodLineEdge.Both)
            {
                return true;
            }

            if (Edge == GpiodLineEdge.Falling && edgeEventType == GpiodEdgeEventType.FallingEdge)
            {
                return true;
            }

            if (Edge == GpiodLineEdge.Rising && edgeEventType == GpiodEdgeEventType.RisingEdge)
            {
                return true;
            }

            return false;
        }
    }
}
