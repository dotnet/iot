// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Device.Gpio;
using System.Linq;
using System.Threading;

namespace Iot.Device.Arduino
{
    internal class ArduinoGpioControllerDriver : GpioDriver
    {
        private readonly ArduinoBoard _arduinoBoard;
        private readonly List<SupportedPinConfiguration> _supportedPinConfigurations;
        private readonly Dictionary<int, CallbackContainer> _callbackContainers;
        private readonly ConcurrentDictionary<int, PinMode> _pinModes;
        private readonly object _callbackContainersLock;
        private readonly AutoResetEvent _waitForEventResetEvent;

        internal ArduinoGpioControllerDriver(ArduinoBoard arduinoBoard, List<SupportedPinConfiguration> supportedPinConfigurations)
        {
            _arduinoBoard = arduinoBoard ?? throw new ArgumentNullException(nameof(arduinoBoard));
            _supportedPinConfigurations = supportedPinConfigurations ?? throw new ArgumentNullException(nameof(supportedPinConfigurations));
            _callbackContainers = new Dictionary<int, CallbackContainer>();
            _waitForEventResetEvent = new AutoResetEvent(false);
            _callbackContainersLock = new object();
            _pinModes = new ConcurrentDictionary<int, PinMode>();

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
                    firmataMode = SupportedMode.DIGITAL_OUTPUT;
                    break;
                case PinMode.InputPullUp:
                    firmataMode = SupportedMode.INPUT_PULLUP;
                    break;
                case PinMode.Input:
                    firmataMode = SupportedMode.DIGITAL_INPUT;
                    break;
                default:
                    throw new NotSupportedException($"Mode {mode} is not supported for this operation");
            }

            var pinConfig = _supportedPinConfigurations.FirstOrDefault(x => x.Pin == pinNumber);
            if (pinConfig == null || !pinConfig.PinModes.Contains(firmataMode))
            {
                throw new NotSupportedException($"Mode {mode} is not supported on Pin {pinNumber}.");
            }

            _arduinoBoard.Firmata.SetPinMode(pinNumber, firmataMode);

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

            SupportedMode mode = _arduinoBoard.Firmata.GetPinMode(pinNumber);

            PinMode ret;
            switch (mode)
            {
                case SupportedMode.DIGITAL_OUTPUT:
                    ret = PinMode.Output;
                    break;
                case SupportedMode.INPUT_PULLUP:
                    ret = PinMode.InputPullUp;
                    break;
                case SupportedMode.DIGITAL_INPUT:
                    ret = PinMode.Input;
                    break;
                default:
                    ret = PinMode.Input; // TODO: Return "Unknown"
                    break;
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
                    firmataMode = SupportedMode.DIGITAL_OUTPUT;
                    break;
                case PinMode.InputPullUp:
                    firmataMode = SupportedMode.INPUT_PULLUP;
                    break;
                case PinMode.Input:
                    firmataMode = SupportedMode.DIGITAL_INPUT;
                    break;
                default:
                    return false;
            }

            var pinConfig = _supportedPinConfigurations.FirstOrDefault(x => x.Pin == pinNumber);
            if (pinConfig == null || !pinConfig.PinModes.Contains(firmataMode))
            {
                return false;
            }

            return true;
        }

        protected override PinValue Read(int pinNumber)
        {
            return _arduinoBoard.Firmata.GetDigitalPinState(pinNumber);
        }

        protected override void Write(int pinNumber, PinValue value)
        {
            _arduinoBoard.Firmata.WriteDigitalPin(pinNumber, value);
        }

        protected override WaitForEventResult WaitForEvent(int pinNumber, PinEventTypes eventTypes, CancellationToken cancellationToken)
        {
            PinEventTypes eventSeen = PinEventTypes.None;

            void WaitForEventPortValueUpdated(int pin, PinValue newvalue)
            {
                if (pin == pinNumber)
                {
                    if ((eventTypes & PinEventTypes.Rising) == PinEventTypes.Rising && newvalue == PinValue.High)
                    {
                        eventSeen = PinEventTypes.Rising;
                        _waitForEventResetEvent.Set();
                    }
                    else if ((eventTypes & PinEventTypes.Falling) == PinEventTypes.Falling && newvalue == PinValue.Low)
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

        private void FirmataOnDigitalPortValueUpdated(int pin, PinValue newvalue)
        {
            CallbackContainer cb = null;
            PinEventTypes eventTypeToFire = PinEventTypes.None;
            lock (_callbackContainersLock)
            {
                if (_callbackContainers.TryGetValue(pin, out cb))
                {
                    if (newvalue == PinValue.High && cb.EventTypes.HasFlag(PinEventTypes.Rising))
                    {
                        eventTypeToFire = PinEventTypes.Rising;
                    }
                    else if (newvalue == PinValue.Low && cb.EventTypes.HasFlag(PinEventTypes.Falling))
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

            public event PinChangeEventHandler OnPinChanged;

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
