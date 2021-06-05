// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Device.Gpio;
using System.Linq;
using System.Threading;
using Iot.Device.Common;
using Microsoft.Extensions.Logging;

namespace Iot.Device.Arduino
{
    internal class ArduinoGpioControllerDriver : GpioDriver
    {
        private readonly ArduinoBoard _arduinoBoard;
        private readonly IReadOnlyCollection<SupportedPinConfiguration> _supportedPinConfigurations;
        private readonly Dictionary<int, CallbackContainer> _callbackContainers;
        private readonly ConcurrentDictionary<int, PinMode> _pinModes;
        private readonly object _callbackContainersLock;
        private readonly AutoResetEvent _waitForEventResetEvent;
        private readonly ILogger _logger;

        internal ArduinoGpioControllerDriver(ArduinoBoard arduinoBoard, IReadOnlyCollection<SupportedPinConfiguration> supportedPinConfigurations)
        {
            _arduinoBoard = arduinoBoard ?? throw new ArgumentNullException(nameof(arduinoBoard));
            _supportedPinConfigurations = supportedPinConfigurations ?? throw new ArgumentNullException(nameof(supportedPinConfigurations));
            _callbackContainers = new Dictionary<int, CallbackContainer>();
            _waitForEventResetEvent = new AutoResetEvent(false);
            _callbackContainersLock = new object();
            _pinModes = new ConcurrentDictionary<int, PinMode>();
            _logger = this.GetCurrentClassLogger();

            PinCount = _supportedPinConfigurations.Count;
            _arduinoBoard.Firmata.DigitalPortValueUpdated += FirmataOnDigitalPortValueUpdated;
        }

        protected override int PinCount { get; }

        /// <summary>
        /// Arduino does not distinguish between logical and physical numbers, so this always returns identity
        /// </summary>
        protected override int ConvertPinNumberToLogicalNumberingScheme(int pinNumber)
        {
            return pinNumber;
        }

        protected override void OpenPin(int pinNumber)
        {
        }

        protected override void ClosePin(int pinNumber)
        {
            _pinModes.TryRemove(pinNumber, out _);
        }

        protected override void SetPinMode(int pinNumber, PinMode mode)
        {
            SupportedMode firmataMode;
            switch (mode)
            {
                case PinMode.Output:
                    firmataMode = SupportedMode.DigitalOutput;
                    break;
                case PinMode.InputPullUp:
                    firmataMode = SupportedMode.InputPullup;
                    break;
                case PinMode.Input:
                    firmataMode = SupportedMode.DigitalInput;
                    break;
                default:
                    _logger.LogError($"Mode {mode} is not supported as argument to {nameof(SetPinMode)}");
                    throw new NotSupportedException($"Mode {mode} is not supported for this operation");
            }

            var pinConfig = _supportedPinConfigurations.FirstOrDefault(x => x.Pin == pinNumber);
            if (pinConfig == null || !pinConfig.PinModes.Contains(firmataMode))
            {
                _logger.LogError($"Mode {mode} is not supported on Pin {pinNumber}");
                throw new NotSupportedException($"Mode {mode} is not supported on Pin {pinNumber}.");
            }

            _arduinoBoard.Firmata.SetPinMode(pinNumber, firmataMode.Value);

            // Cache this value. Since the GpioController calls GetPinMode when trying to write a pin (to verify whether it is set to output),
            // that would be very expensive here. And setting output pins should be cheap.
            _pinModes[pinNumber] = mode;
        }

        protected override PinMode GetPinMode(int pinNumber)
        {
            if (_pinModes.TryGetValue(pinNumber, out var existingValue))
            {
                return existingValue;
            }

            var mode = _arduinoBoard.Firmata.GetPinMode(pinNumber);

            PinMode ret;
            if (mode == SupportedMode.DigitalOutput.Value)
            {
                ret = PinMode.Output;
            }
            else if (mode == SupportedMode.DigitalInput.Value)
            {
                ret = PinMode.Input;
            }
            else if (mode == SupportedMode.InputPullup.Value)
            {
                ret = PinMode.InputPullUp;
            }
            else
            {
                _logger.LogError($"Unexpected pin mode found: {mode}. Is the pin not set to GPIO?");
                ret = PinMode.Input;
            }

            _pinModes[pinNumber] = ret;
            return ret;
        }

        protected override bool IsPinModeSupported(int pinNumber, PinMode mode)
        {
            SupportedMode firmataMode;
            switch (mode)
            {
                case PinMode.Output:
                    firmataMode = SupportedMode.DigitalOutput;
                    break;
                case PinMode.InputPullUp:
                    firmataMode = SupportedMode.InputPullup;
                    break;
                case PinMode.Input:
                    firmataMode = SupportedMode.DigitalInput;
                    break;
                default:
                    return false;
            }

            var pinConfig = _supportedPinConfigurations.FirstOrDefault(x => x.Pin == pinNumber);
            return pinConfig != null && pinConfig.PinModes.Contains(firmataMode);
        }

        protected override PinValue Read(int pinNumber)
        {
            return _arduinoBoard.Firmata.ReadDigitalPin(pinNumber);
        }

        protected override void Write(int pinNumber, PinValue value)
        {
            _arduinoBoard.Firmata.WriteDigitalPin(pinNumber, value);
        }

        protected override WaitForEventResult WaitForEvent(int pinNumber, PinEventTypes eventTypes, CancellationToken cancellationToken)
        {
            PinEventTypes eventSeen = PinEventTypes.None;

            void WaitForEventPortValueUpdated(object sender, PinValueChangedEventArgs e)
            {
                if (e.PinNumber == pinNumber)
                {
                    if ((eventTypes & PinEventTypes.Rising) == PinEventTypes.Rising && e.ChangeType == PinEventTypes.Rising)
                    {
                        eventSeen = PinEventTypes.Rising;
                        _waitForEventResetEvent.Set();
                    }
                    else if ((eventTypes & PinEventTypes.Falling) == PinEventTypes.Falling && e.ChangeType == PinEventTypes.Falling)
                    {
                        eventSeen = PinEventTypes.Falling;
                        _waitForEventResetEvent.Set();
                    }
                }
            }

            _arduinoBoard.Firmata.DigitalPortValueUpdated += WaitForEventPortValueUpdated;
            try
            {
                WaitHandle.WaitAny(new[] { cancellationToken.WaitHandle, _waitForEventResetEvent });
                if (cancellationToken.IsCancellationRequested)
                {
                    return new WaitForEventResult()
                    {
                        EventTypes = PinEventTypes.None,
                        TimedOut = true
                    };
                }
            }
            finally
            {
                _arduinoBoard.Firmata.DigitalPortValueUpdated -= WaitForEventPortValueUpdated;
            }

            return new WaitForEventResult()
            {
                EventTypes = eventSeen,
                TimedOut = false
            };
        }

        protected override void AddCallbackForPinValueChangedEvent(int pinNumber, PinEventTypes eventTypes, PinChangeEventHandler callback)
        {
            lock (_callbackContainersLock)
            {
                if (_callbackContainers.TryGetValue(pinNumber, out var cb))
                {
                    cb.EventTypes = cb.EventTypes | eventTypes;
                    cb.OnPinChanged += callback;
                }
                else
                {
                    var cb2 = new CallbackContainer(pinNumber, eventTypes);
                    cb2.OnPinChanged += callback;
                    _callbackContainers.Add(pinNumber, cb2);
                }
            }
        }

        protected override void RemoveCallbackForPinValueChangedEvent(int pinNumber, PinChangeEventHandler callback)
        {
            lock (_callbackContainersLock)
            {
                if (_callbackContainers.TryGetValue(pinNumber, out var cb))
                {
                    cb.OnPinChanged -= callback;
                    if (cb.NoEventsConnected)
                    {
                        _callbackContainers.Remove(pinNumber);
                    }
                }
            }
        }

        private void FirmataOnDigitalPortValueUpdated(object sender, PinValueChangedEventArgs e)
        {
            CallbackContainer? cb = null;
            PinEventTypes eventTypeToFire = PinEventTypes.None;
            lock (_callbackContainersLock)
            {
                if (_callbackContainers.TryGetValue(e.PinNumber, out cb))
                {
                    if (e.ChangeType == PinEventTypes.Rising && cb.EventTypes.HasFlag(PinEventTypes.Rising))
                    {
                        eventTypeToFire = PinEventTypes.Rising;
                    }
                    else if (e.ChangeType == PinEventTypes.Falling && cb.EventTypes.HasFlag(PinEventTypes.Falling))
                    {
                        eventTypeToFire = PinEventTypes.Falling;
                    }
                }
            }

            if (eventTypeToFire != PinEventTypes.None && cb != null)
            {
                cb.FireOnPinChanged(eventTypeToFire);
            }
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                lock (_callbackContainersLock)
                {
                    _callbackContainers.Clear();
                }

                _arduinoBoard.Firmata.DigitalPortValueUpdated -= FirmataOnDigitalPortValueUpdated;
            }

            base.Dispose(disposing);
        }

        private class CallbackContainer
        {
            public CallbackContainer(int pinNumber, PinEventTypes eventTypes)
            {
                PinNumber = pinNumber;
                EventTypes = eventTypes;
            }

            public event PinChangeEventHandler? OnPinChanged;

            public int PinNumber { get; }

            public PinEventTypes EventTypes
            {
                get;
                set;
            }

            public bool NoEventsConnected
            {
                get
                {
                    return OnPinChanged == null;
                }
            }

            public void FireOnPinChanged(PinEventTypes eventType)
            {
                // Copy event instance, prevents problems when elements are added or removed at the same time
                var threadSafeCopy = OnPinChanged;
                threadSafeCopy?.Invoke(PinNumber, new PinValueChangedEventArgs(eventType, PinNumber));
            }
        }
    }
}
