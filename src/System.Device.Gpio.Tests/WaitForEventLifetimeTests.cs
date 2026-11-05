// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System.Collections;
using System.Device.Gpio.Drivers;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Threading;
using System.Threading.Tasks;
using Moq;
using Moq.Protected;
using Xunit;

namespace System.Device.Gpio.Tests;

public class WaitForEventLifetimeTests
{
    private const BindingFlags NonPublicInstance = BindingFlags.Instance | BindingFlags.NonPublic;
    private static readonly TimeSpan s_testTimeout = TimeSpan.FromSeconds(10);

    [Theory]
    [InlineData(false)]
    [InlineData(true)]
    public async Task SingleEventObservationCleansUpBeforeReturning(bool cancel)
    {
        Type observerType = GetDriverType("Drivers.LibGpiodV2EventObserver");
        using var observer = (IDisposable)Activator.CreateInstance(observerType)!;
        object offset = Activator.CreateInstance(GetDriverType("Libgpiod.V2.Offset"))!;
        object edge = Enum.Parse(GetDriverType("Libgpiod.V2.GpiodLineEdge"), "Rising");
        object subscription = Activator.CreateInstance(observerType.GetNestedType("EventSubscription", BindingFlags.NonPublic)!, offset, edge)!;
        MethodInfo observe = observerType.GetMethod("ObserveSingleEvent", NonPublicInstance)!;
        MethodInfo hasSubscriptions = observerType.GetMethod("HasSubscriptionsFor")!;

        // An already-observed request isolates subscription cleanup from native GPIO polling.
        object request = RuntimeHelpers.GetUninitializedObject(GetDriverType("Libgpiod.V2.LineRequest"));
        object observedRequests = GetField<object>(observer, "_observedRequests");
        observedRequests.GetType().GetMethod("Add")!.Invoke(observedRequests, new[] { request });
        IDictionary handlers = GetField<IDictionary>(observer, "_handlersBySubscription");

        for (int i = 0; i < 5; i++)
        {
            using var cancellation = new CancellationTokenSource(s_testTimeout);
            Task<WaitForEventResult> wait = Task.Run(() =>
                (WaitForEventResult)observe.Invoke(observer, new[] { request, subscription, cancellation.Token })!);
            try
            {
                Assert.True(SpinWait.SpinUntil(() => (bool)hasSubscriptions.Invoke(observer, new[] { offset })!, s_testTimeout));
                Assert.False(wait.IsCompleted);

                if (cancel)
                {
                    cancellation.Cancel();
                }
                else
                {
                    lock (handlers)
                    {
                        var callbacks = (IList)handlers[subscription]!;
                        Assert.Single(callbacks);
                        ((PinChangeEventHandler)callbacks[0]!).Invoke(observer, new PinValueChangedEventArgs(PinEventTypes.Rising, 0));
                    }
                }

                WaitForEventResult result = await wait.WaitAsync(s_testTimeout);
                Assert.Equal(cancel, result.TimedOut);
                Assert.Equal(cancel ? PinEventTypes.None : PinEventTypes.Rising, result.EventTypes);
                Assert.False((bool)hasSubscriptions.Invoke(observer, new[] { offset })!);
            }
            finally
            {
                cancellation.Cancel();
                await wait.WaitAsync(s_testTimeout);
            }
        }
    }

    [Theory]
    [InlineData(false)]
    [InlineData(true)]
    public async Task CallbackDispatchCanJoinCancelledWaitAndFinishAfterCleanup(bool cancelInCallback)
    {
        Type observerType = GetDriverType("Drivers.LibGpiodV2EventObserver");
        using var observer = (IDisposable)Activator.CreateInstance(observerType)!;
        object offset = Activator.CreateInstance(GetDriverType("Libgpiod.V2.Offset"))!;
        object edge = Enum.Parse(GetDriverType("Libgpiod.V2.GpiodLineEdge"), "Rising");
        object eventType = Enum.Parse(GetDriverType("Libgpiod.V2.GpiodEdgeEventType"), "RisingEdge");
        object subscription = Activator.CreateInstance(observerType.GetNestedType("EventSubscription", BindingFlags.NonPublic)!, offset, edge)!;
        object request = RuntimeHelpers.GetUninitializedObject(GetDriverType("Libgpiod.V2.LineRequest"));
        object observedRequests = GetField<object>(observer, "_observedRequests");
        observedRequests.GetType().GetMethod("Add")!.Invoke(observedRequests, new[] { request });
        IDictionary handlers = GetField<IDictionary>(observer, "_handlersBySubscription");
        MethodInfo observe = observerType.GetMethod("ObserveSingleEvent", NonPublicInstance)!;
        MethodInfo register = observerType.GetMethod("Observe", NonPublicInstance)!;
        MethodInfo dispatch = observerType.GetMethod("HandleEdgeEvent", NonPublicInstance, null, new[] { offset.GetType(), eventType.GetType() }, null)!;
        MethodInfo remove = observerType.GetMethod("RemoveCallback", NonPublicInstance)!;

        for (int i = 0; i < 5; i++)
        {
            using var cancellation = new CancellationTokenSource(s_testTimeout);
            using var callbackStarted = new ManualResetEventSlim();
            using var releaseCallback = new ManualResetEventSlim();
            Task<WaitForEventResult>? wait = null;
            PinChangeEventHandler callback = (sender, args) =>
            {
                callbackStarted.Set();
                if (cancelInCallback)
                {
                    cancellation.Cancel();
#pragma warning disable xUnit1031 // Synchronously joining from the callback is the deadlock regression.
                    Assert.True(wait!.WaitAsync(s_testTimeout).GetAwaiter().GetResult().TimedOut);
#pragma warning restore xUnit1031
                }
                else
                {
                    Assert.True(releaseCallback.Wait(s_testTimeout));
                }
            };
            register.Invoke(observer, new[] { request, subscription, callback });
            wait = Task.Run(() => (WaitForEventResult)observe.Invoke(observer, new[] { request, subscription, cancellation.Token })!);
            Task? dispatchTask = null;

            try
            {
                Assert.True(SpinWait.SpinUntil(() =>
                {
                    lock (handlers)
                    {
                        return ((IList)handlers[subscription]!).Count == 2;
                    }
                }, s_testTimeout));

                dispatchTask = Task.Run(() => dispatch.Invoke(observer, new[] { offset, eventType }));
                Assert.True(callbackStarted.Wait(s_testTimeout));
                if (!cancelInCallback)
                {
                    cancellation.Cancel();
                }

                WaitForEventResult result = await wait.WaitAsync(s_testTimeout);
                Assert.True(result.TimedOut);
                Assert.Equal(PinEventTypes.None, result.EventTypes);
                lock (handlers)
                {
                    Assert.Single((IList)handlers[subscription]!);
                }

                // Resume a dispatch snapshot that still contains the removed, disposed wait callback.
                releaseCallback.Set();
                await dispatchTask.WaitAsync(s_testTimeout);
            }
            finally
            {
                cancellation.Cancel();
                releaseCallback.Set();
                if (dispatchTask != null)
                {
                    await dispatchTask.WaitAsync(s_testTimeout);
                }

                await wait.WaitAsync(s_testTimeout);
                remove.Invoke(observer, new[] { offset, callback });
            }
        }
    }

    [Theory]
    [InlineData(false, false)]
    [InlineData(true, false)]
    [InlineData(false, true)]
    [InlineData(true, true)]
    [Trait("SkipOnTestRun", "Windows_NT")]
    public async Task RaspberryPiAsyncWaitRetainsResourcesUntilCompletion(bool cancel, bool alreadyInUse)
    {
        const int Pin = 1;
        using var edge = new ManualResetEventSlim();
        using var started = new ManualResetEventSlim();
        var interruptDriver = new Mock<UnixDriver>();
        int opens = 0;
        int closes = 0;
        CancellationToken waitToken = default;

        interruptDriver.Protected().Setup("OpenPin", Pin).Callback(() => opens++);
        interruptDriver.Protected().Setup("ClosePin", Pin).Callback(() => closes++);
        interruptDriver.Protected()
            .Setup<WaitForEventResult>("WaitForEvent", Pin, PinEventTypes.Rising, ItExpr.IsAny<CancellationToken>())
            .Returns((int pin, PinEventTypes events, CancellationToken token) =>
            {
                waitToken = token;
                started.Set();
                try
                {
                    edge.Wait(token);
                    return new WaitForEventResult { EventTypes = events, TimedOut = false };
                }
                catch (OperationCanceledException)
                {
                    return new WaitForEventResult { EventTypes = PinEventTypes.None, TimedOut = true };
                }
            });

        Type driverType = GetDriverType("Drivers.RaspberryPi3LinuxDriver");
        using var driver = (GpioDriver)Activator.CreateInstance(driverType)!;
        driverType.GetField("_interruptDriver", NonPublicInstance)!.SetValue(driver, interruptDriver.Object);
        MethodInfo waitAsync = driverType.GetMethod("WaitForEventAsync", NonPublicInstance)!;
        Array pinModes = GetField<Array>(driver, "_pinModes");
        Type pinStateType = pinModes.GetType().GetElementType()!;
        object pinState = Activator.CreateInstance(pinStateType, PinMode.Input)!;
        pinModes.SetValue(pinState, Pin);
        if (alreadyInUse)
        {
            pinStateType.GetMethod("AddInterruptDriverUse")!.Invoke(pinState, null);
        }

        for (int i = 0; i < 5; i++)
        {
            edge.Reset();
            started.Reset();
            using var cancellation = new CancellationTokenSource(s_testTimeout);
            Task<WaitForEventResult> wait = ((ValueTask<WaitForEventResult>)waitAsync.Invoke(driver, new object[] { Pin, PinEventTypes.Rising, cancellation.Token })!).AsTask();
            try
            {
                Assert.True(started.Wait(s_testTimeout));
                Assert.False(wait.IsCompleted);
                Assert.False(waitToken.IsCancellationRequested);
                Assert.Equal(alreadyInUse ? 0 : i + 1, opens);
                Assert.Equal(alreadyInUse ? 0 : i, closes);

                if (cancel)
                {
                    cancellation.Cancel();
                }
                else
                {
                    edge.Set();
                }

                WaitForEventResult result = await wait.WaitAsync(s_testTimeout);
                Assert.Equal(cancel, result.TimedOut);
                Assert.Equal(cancel ? PinEventTypes.None : PinEventTypes.Rising, result.EventTypes);
                Assert.Equal(alreadyInUse ? 0 : i + 1, closes);
                Assert.True(waitToken.IsCancellationRequested);
            }
            finally
            {
                cancellation.Cancel();
                edge.Set();
                await wait.WaitAsync(s_testTimeout);
            }
        }
    }

    private static T GetField<T>(object instance, string name)
        => (T)instance.GetType().GetField(name, NonPublicInstance)!.GetValue(instance)!;

    private static Type GetDriverType(string name)
        => typeof(GpioDriver).Assembly.GetType("System.Device.Gpio." + name, throwOnError: true)!;
}
