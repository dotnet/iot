// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System;
using System.Collections.Generic;
using System.Device.Gpio;
using System.Text;
using System.Threading;
using Iot.Device.Arduino;

namespace ArduinoCsCompiler
{
    /// <summary>
    /// This represents the driver that runs physically on the Arduino when
    /// an instance of a GpioController is requested there.
    /// </summary>
    public class ArduinoNativeGpioDriver : GpioDriver
    {
        private readonly ArduinoHardwareLevelAccess _hardwareLevelAccess;
        private readonly Dictionary<int, CallbackContainer> _callbackContainers;
        private readonly object _callbackContainersLock;
        private Thread? _callbackThread;

        public ArduinoNativeGpioDriver()
        {
            _hardwareLevelAccess = new ArduinoHardwareLevelAccess();
            _callbackContainersLock = new object();
            _callbackContainers = new Dictionary<int, CallbackContainer>();
        }

        protected override int PinCount
        {
            get
            {
                return _hardwareLevelAccess.GetPinCount();
            }
        }

        protected override int ConvertPinNumberToLogicalNumberingScheme(int pinNumber)
        {
            return pinNumber;
        }

        protected override void OpenPin(int pinNumber)
        {
        }

        protected override void ClosePin(int pinNumber)
        {
            _hardwareLevelAccess?.SetPinMode(pinNumber, PinMode.Input);
        }

        protected override void SetPinMode(int pinNumber, PinMode mode)
        {
            _hardwareLevelAccess?.SetPinMode(pinNumber, mode);
        }

        protected override PinMode GetPinMode(int pinNumber)
        {
            return _hardwareLevelAccess.GetPinMode(pinNumber);
        }

        protected override bool IsPinModeSupported(int pinNumber, PinMode mode)
        {
            return _hardwareLevelAccess.IsPinModeSupported(pinNumber, mode);
        }

        protected override PinValue Read(int pinNumber)
        {
            // We should not be throwing here, as this results in unnecessary dependencies
            return _hardwareLevelAccess.ReadPin(pinNumber);
        }

        protected override void Write(int pinNumber, PinValue value)
        {
            _hardwareLevelAccess.WritePin(pinNumber, value == PinValue.High ? 1 : 0);
        }

        protected override WaitForEventResult WaitForEvent(int pinNumber, PinEventTypes eventTypes, CancellationToken cancellationToken)
        {
            PinValue currentState = Read(pinNumber);
            while (!cancellationToken.IsCancellationRequested)
            {
                PinValue newValue = Read(pinNumber);
                if (currentState == PinValue.Low && (eventTypes & PinEventTypes.Rising) != 0 && newValue == PinValue.High)
                {
                    return new WaitForEventResult()
                    {
                        EventTypes = PinEventTypes.Rising
                    };
                }

                if (currentState == PinValue.High && (eventTypes & PinEventTypes.Falling) != 0 && newValue == PinValue.Low)
                {
                    return new WaitForEventResult()
                    {
                        EventTypes = PinEventTypes.Falling
                    };
                }

                currentState = newValue;
            }

            return new WaitForEventResult()
            {
                EventTypes = PinEventTypes.None,
                TimedOut = true
            };
        }

        private void WaitForEvents()
        {
            while (true)
            {
                bool endLoop = false;
                lock (_callbackContainersLock)
                {
                    if (_callbackContainers.Count == 0)
                    {
                        endLoop = true;
                        return;
                    }

                    foreach (var container in _callbackContainers)
                    {
                        PinValue newValue = Read(container.Key);
                        if (newValue != container.Value.PreviousValue)
                        {
                            container.Value.FireOnPinChanged(newValue == PinValue.High ? PinEventTypes.Rising : PinEventTypes.Falling);
                            container.Value.PreviousValue = newValue;
                        }
                    }
                }

                if (endLoop)
                {
                    _callbackThread = null;
                    return;
                }

                Thread.Yield();
            }
        }

        protected override void Dispose(bool disposing)
        {
            lock (_callbackContainersLock)
            {
                _callbackContainers.Clear(); // So the thread ends
            }

            base.Dispose(disposing);
        }

        protected override void AddCallbackForPinValueChangedEvent(int pinNumber, PinEventTypes eventTypes, PinChangeEventHandler callback)
        {
            lock (_callbackContainersLock)
            {
                if (_callbackContainers.Count == 0)
                {
                    // Empty: Start the background thread
                    _callbackThread = new Thread(WaitForEvents);
                    _callbackThread.Start();
                }

                if (_callbackContainers.TryGetValue(pinNumber, out var cb))
                {
                    cb.EventTypes = cb.EventTypes | eventTypes;
                    cb.OnPinChanged += callback;
                }
                else
                {
                    var cb2 = new CallbackContainer(pinNumber, eventTypes);
                    cb2.OnPinChanged += callback;
                    cb2.PreviousValue = Read(pinNumber);
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

            public PinValue PreviousValue
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
