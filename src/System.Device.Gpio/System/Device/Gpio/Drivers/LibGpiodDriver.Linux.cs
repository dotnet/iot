// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information

using System.Collections.Generic;
using System.Threading;
using System.Runtime.InteropServices;
using System.Collections.Concurrent;

namespace System.Device.Gpio.Drivers
{
    public class LibGpiodDriver : UnixDriver
    {
        private SafeChipHandle _chip;

        private Dictionary<int, SafeLineHandle> _pinNumberToSafeLineHandle;

        private ConcurrentDictionary<int, LibGpiodDriverEventHandler> _pinNumberToEventHandler = new ConcurrentDictionary<int, LibGpiodDriverEventHandler>();

        protected internal override int PinCount => Interop.GetNumberOfLines(_chip);

        public LibGpiodDriver(int gpioChip = 0)
        {
            try
            {
                _chip = Interop.OpenChipByNumber(gpioChip);
                if (_chip == null)
                {
                    throw ExceptionHelper.GetIOException(ExceptionResource.NoChipFound, Marshal.GetLastWin32Error());
                }
                _pinNumberToSafeLineHandle = new Dictionary<int, SafeLineHandle>(PinCount);
            }
            catch (DllNotFoundException)
            {
                throw ExceptionHelper.GetPlatformNotSupportedException(ExceptionResource.LibGpiodNotInstalled);
            }
        }

        protected internal override void AddCallbackForPinValueChangedEvent(int pinNumber, PinEventTypes eventTypes, PinChangeEventHandler callback)
        {
            if (eventTypes.HasFlag(PinEventTypes.Rising) || eventTypes.HasFlag(PinEventTypes.Falling))
            {
                LibGpiodDriverEventHandler eventHandler = _pinNumberToEventHandler.GetOrAdd(pinNumber, PopulateEventHandler);

                if (eventTypes.HasFlag(PinEventTypes.Rising))
                {
                    eventHandler.ValueRising += callback;
                }
                else if (eventTypes.HasFlag(PinEventTypes.Falling))
                {
                    eventHandler.ValueFalling += callback;
                }
            }
            else
            {
                throw ExceptionHelper.GetArgumentException(ExceptionResource.InvalidEventType);
            }
        }

        private LibGpiodDriverEventHandler PopulateEventHandler(int pinNumber)
        {
            if (_pinNumberToSafeLineHandle.TryGetValue(pinNumber, out SafeLineHandle pinHandle))
            {
                if (!Interop.LineIsFree(pinHandle))
                {
                    pinHandle.Dispose();
                    pinHandle = Interop.GetChipLineByOffset(_chip, pinNumber);
                    _pinNumberToSafeLineHandle[pinNumber] = pinHandle;
                }
            }
            return new LibGpiodDriverEventHandler(pinNumber, pinHandle);
        }

        protected internal override void ClosePin(int pinNumber)
        {
            if (_pinNumberToSafeLineHandle.TryGetValue(pinNumber, out SafeLineHandle pinHandle) && !IsListeningEvent(pinNumber))
            {
                pinHandle?.Dispose();
                _pinNumberToSafeLineHandle.Remove(pinNumber);
            }
        }

        private bool IsListeningEvent(int pinNumber)
        {
            return _pinNumberToEventHandler.ContainsKey(pinNumber);
        }

        protected internal override int ConvertPinNumberToLogicalNumberingScheme(int pinNumber) => 
            throw ExceptionHelper.GetPlatformNotSupportedException(ExceptionResource.ConvertPinNumberingSchemaError);

        protected internal override PinMode GetPinMode(int pinNumber)
        {
            if (!_pinNumberToSafeLineHandle.TryGetValue(pinNumber, out SafeLineHandle pinHandle))
            {
                throw ExceptionHelper.GetInvalidOperationException(ExceptionResource.PinNotOpenedError, pin: pinNumber);
            }
            return pinHandle.PinMode;
        }

        protected internal override bool IsPinModeSupported(int pinNumber, PinMode mode)
        {
            // Libgpiod Api do not support pull up or pull down resistors for now.
            return mode != PinMode.InputPullDown && mode != PinMode.InputPullUp;
        }

        protected internal override void OpenPin(int pinNumber)
        {
            SafeLineHandle pinHandle = Interop.GetChipLineByOffset(_chip, pinNumber);
            if (pinHandle == null)
            {
                throw ExceptionHelper.GetIOException(ExceptionResource.OpenPinError, Marshal.GetLastWin32Error());
            }

            _pinNumberToSafeLineHandle.Add(pinNumber, pinHandle);
        }

        protected internal override PinValue Read(int pinNumber)
        {
            if (_pinNumberToSafeLineHandle.TryGetValue(pinNumber, out SafeLineHandle pinHandle))
            {
                int result = Interop.GetGpiodLineValue(pinHandle);
                if (result == -1)
                {
                    throw ExceptionHelper.GetIOException(ExceptionResource.ReadPinError, Marshal.GetLastWin32Error(), pinNumber);
                }

                return result;
            }
            throw ExceptionHelper.GetInvalidOperationException(ExceptionResource.PinNotOpenedError, pin: pinNumber);
        }

        protected internal override void RemoveCallbackForPinValueChangedEvent(int pinNumber, PinChangeEventHandler callback)
        {
            if (_pinNumberToEventHandler.TryGetValue(pinNumber, out LibGpiodDriverEventHandler eventHandler))
            {
                eventHandler.ValueFalling -= callback;
                eventHandler.ValueRising -= callback;
                if (eventHandler.IsCallbackListEmpty())
                {
                    _pinNumberToEventHandler.TryRemove(pinNumber, out eventHandler);
                    eventHandler.Dispose();
                }
            }
            else
            {
                throw ExceptionHelper.GetInvalidOperationException(ExceptionResource.NotListeningForEventError);
            }
        }

        protected internal override void SetPinMode(int pinNumber, PinMode mode)
        {
            int requestResult  = -1;
            if (_pinNumberToSafeLineHandle.TryGetValue(pinNumber, out SafeLineHandle pinHandle))
            {
                string consumer = pinNumber.ToString();
                if (mode == PinMode.Input)
                {
                    requestResult = Interop.RequestLineInput(pinHandle, consumer);
                }
                else
                {
                    requestResult = Interop.RequestLineOutput(pinHandle, consumer);
                }

                pinHandle.PinMode = mode;
            }

            if (requestResult == -1)
            {
                throw ExceptionHelper.GetIOException(ExceptionResource.SetPinModeError, Marshal.GetLastWin32Error(), pinNumber);
            } 
        }

        protected internal override WaitForEventResult WaitForEvent(int pinNumber, PinEventTypes eventTypes, CancellationToken cancellationToken)
        {
            if (eventTypes.HasFlag(PinEventTypes.Rising) || eventTypes.HasFlag(PinEventTypes.Falling))
            {
                LibGpiodDriverEventHandler eventHandler = _pinNumberToEventHandler.GetOrAdd(pinNumber, PopulateEventHandler);

                if (eventTypes.HasFlag(PinEventTypes.Rising))
                {
                    eventHandler.ValueRising += callback;
                }
                else if (eventTypes.HasFlag(PinEventTypes.Falling))
                {
                    eventHandler.ValueFalling += callback;
                }

                bool eventOccured = false;
                void callback(object o, PinValueChangedEventArgs e)
                {
                    eventOccured = true;
                }

                WaitForEventResult(cancellationToken, eventHandler.CancellationTokenSource.Token, ref eventOccured);
                RemoveCallbackForPinValueChangedEvent(pinNumber, callback);

                return new WaitForEventResult
                {
                    TimedOut = !eventOccured,
                    EventTypes = eventTypes
                };
            }
            else
            {
                throw ExceptionHelper.GetArgumentException(ExceptionResource.InvalidEventType);
            }
        }

        private void WaitForEventResult(CancellationToken sourceToken, CancellationToken parentToken, ref bool eventOccured)
        {
            while (!(sourceToken.IsCancellationRequested || parentToken.IsCancellationRequested || eventOccured))
            {
                Thread.Sleep(1);
            }
        }

        protected internal override void Write(int pinNumber, PinValue value)
        {
            if (!_pinNumberToSafeLineHandle.TryGetValue(pinNumber, out SafeLineHandle pinHandle))
            {
                throw ExceptionHelper.GetInvalidOperationException(ExceptionResource.PinNotOpenedError, pin: pinNumber);
            }

            Interop.SetGpiodLineValue(pinHandle, (value == PinValue.High) ? 1 : 0);
        }

        protected override void Dispose(bool disposing)
        {
            if (_pinNumberToEventHandler != null)
            {
                foreach (KeyValuePair<int, LibGpiodDriverEventHandler> kv in _pinNumberToEventHandler)
                {
                    int pin = kv.Key;
                    LibGpiodDriverEventHandler eventHandler = kv.Value;
                    eventHandler.Dispose();
                }

                _pinNumberToEventHandler = null;
            }

            
            if (_pinNumberToSafeLineHandle != null)
            {
                foreach (int pin in _pinNumberToSafeLineHandle.Keys)
                {
                    if (_pinNumberToSafeLineHandle.TryGetValue(pin, out SafeLineHandle pinHandle))
                    {
                        pinHandle?.Dispose();
                    }
                }

                _pinNumberToSafeLineHandle = null;
            }

            _chip?.Dispose();
            _chip = null;

            base.Dispose(disposing);
        }
    }
}
