// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information

using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using System.Runtime.InteropServices;
using System.IO;
using System.Threading.Tasks;
using System.Collections.Concurrent;

namespace System.Device.Gpio.Drivers
{
    public class LibGpiodDriver : UnixDriver
    {
        private SafeChipHandle _chip;

        private Dictionary<int, SafeLineHandle> _pinNumberToSafeLineHandle;

        private ConcurrentDictionary<int, LibGpiodDriverEventHandler> _pinNumberToEventHandler = new ConcurrentDictionary<int, LibGpiodDriverEventHandler>();

        protected internal override int PinCount => Interop.GetNumberOfLines(_chip);

        public LibGpiodDriver()
        {
            OpenChip();
            _pinNumberToSafeLineHandle = new Dictionary<int, SafeLineHandle>(PinCount);
        }

        private void OpenChip()
        {
            SafeChipIteratorHandle iterator = Interop.GetChipIterator();
            if (iterator == null)
            {
                throw new IOException($"No any chip available, error code: {Marshal.GetLastWin32Error()}");
            }
            _chip = Interop.GetNextChipFromChipIterator(iterator);
            if (_chip == null)
            {
                throw new IOException($"No chip found, error code: {Marshal.GetLastWin32Error()}");
            }
            // Freeing other chips opened
            Interop.FreeChipIteratorNoCloseCurrentChip(iterator);
        }

        private SafeLineHandle RequestEvent(int pinNumber, PinEventTypes eventType)
        {
            int eventSuccess = -1;
            SafeLineHandle pin = Interop.GetChipLineByOffset(_chip, pinNumber);
            if (pin != null)
            {
                if (eventType == PinEventTypes.Rising || eventType == PinEventTypes.Falling)
                {
                    eventSuccess = Interop.RequestBothEdgeEventForLine(pin, $"Listen {pinNumber} for falling edge event");
                }
                else
                {
                    throw new ArgumentException("Invalid or not supported event type requested");
                }
            }
            if (eventSuccess < 0)
            {
                throw new IOException($"Error while requesting event listener for pin {pinNumber}, error code: {Marshal.GetLastWin32Error()}");
            }
            return pin;
        }

        protected internal override void AddCallbackForPinValueChangedEvent(int pinNumber, PinEventTypes eventType, PinChangeEventHandler callback)
        {
            if (!_pinNumberToEventHandler.TryGetValue(pinNumber, out LibGpiodDriverEventHandler eventHandler))
            {
                if (_pinNumberToSafeLineHandle.TryGetValue(pinNumber, out SafeLineHandle pin))
                    Interop.ReleaseGpiodLine(pin);
                eventHandler = new LibGpiodDriverEventHandler(pinNumber, new CancellationTokenSource());
                _pinNumberToEventHandler.TryAdd(pinNumber, eventHandler);
                eventHandler.PinHandle = RequestEvent(pinNumber, eventType);
                InitializeEventDetectionTask(eventHandler);
            }
            if (eventType == PinEventTypes.Rising)
            {
                eventHandler.ValueRising += callback;
            }
            else if (eventType == PinEventTypes.Falling)
            {
                eventHandler.ValueFalling += callback;
            }
        }

        private void InitializeEventDetectionTask(LibGpiodDriverEventHandler eventHandler)
        {
            Task pinListeningThread = Task.Run(() =>
            {
                int waitResult = -2;
                while (!eventHandler.CancellationTokenSource.IsCancellationRequested)
                {
                    waitResult = Interop.WaitForEventOnLine(eventHandler.PinHandle);
                    if (waitResult == -1)
                    {
                        throw new IOException($"Error while waiting for event, error code: {Marshal.GetLastWin32Error()}");
                    }
                    if (waitResult == 1)
                    {
                        int readResult = Interop.ReadEventForLine(eventHandler.PinHandle);
                        if (readResult == -1)
                        {
                            throw new IOException($"Error while trying to read pin event result, error code: {Marshal.GetLastWin32Error()}");
                        }
                        PinEventTypes eventType = (readResult == 1) ? PinEventTypes.Rising : PinEventTypes.Falling;
                        var args = new PinValueChangedEventArgs(eventType, eventHandler.PinNumber);
                        eventHandler?.OnPinValueChanged(args, eventType);
                    }
                }
            });
        }

        protected internal override void ClosePin(int pinNumber)
        {
            if (_pinNumberToSafeLineHandle.TryGetValue(pinNumber, out SafeLineHandle pin))
            {
                Interop.ReleaseGpiodLine(pin);
                _pinNumberToSafeLineHandle.Remove(pinNumber);
            }
        }

        protected internal override int ConvertPinNumberToLogicalNumberingScheme(int pinNumber) => throw new
            PlatformNotSupportedException("This driver is generic so it cannot perform conversions between pin numbering schemes.");

        protected internal override PinMode GetPinMode(int pinNumber)
        {
            if (_pinNumberToSafeLineHandle.TryGetValue(pinNumber, out SafeLineHandle pin))
            {
                return (Interop.GetLineDirection(pin) == 1) ? PinMode.Input : PinMode.Output;
            }
            return PinMode.Input;
        }

        protected internal override bool IsPinModeSupported(int pinNumber, PinMode mode)
        {
            // Libgpiod Api do not support pull up or pull down resistors for now.
            return mode != PinMode.InputPullDown && mode != PinMode.InputPullUp;
        }

        protected internal override void OpenPin(int pinNumber)
        {
            SafeLineHandle pin = Interop.GetChipLineByOffset(_chip, pinNumber);
            if (pin == null)
            {
                throw new IOException($"Pin number {pinNumber} not available for chip: {_chip} error: {Marshal.GetLastWin32Error()}");
            }
            _pinNumberToSafeLineHandle.Add(pinNumber, pin);
        }

        protected internal override PinValue Read(int pinNumber)
        {
            if (_pinNumberToSafeLineHandle.TryGetValue(pinNumber, out SafeLineHandle pin))
            {
                int value = Interop.GetGpiodLineValue(pin);
                if (value == -1)
                {
                    throw new IOException($"Error while reading value from pin number: {pinNumber}, error: {Marshal.GetLastWin32Error()}");
                }
                return (value == 1) ? PinValue.High : PinValue.Low;
            }
            return PinValue.Low;
        }

        protected internal override void RemoveCallbackForPinValueChangedEvent(int pinNumber, PinChangeEventHandler callback)
        {
            if (_pinNumberToEventHandler.TryGetValue(pinNumber, out LibGpiodDriverEventHandler eventHandler))
            {
                eventHandler.ValueFalling -= callback;
                eventHandler.ValueRising -= callback;
                if (eventHandler.IsCallbackListEmpty())
                {
                    eventHandler.CancellationTokenSource.Cancel();
                    Interop.ReleaseGpiodLine(eventHandler.PinHandle);
                    eventHandler.Dispose();
                    _pinNumberToEventHandler.TryRemove(pinNumber, out eventHandler);
                 }
            }
            else
            {
                throw new InvalidOperationException("Attempted to remove a callback for a pin that is not listening for events.");
            }
        }

        protected internal override void SetPinMode(int pinNumber, PinMode mode)
        {
            int success = -1;
            if (_pinNumberToSafeLineHandle.TryGetValue(pinNumber, out SafeLineHandle pin))
            {
                if (mode == PinMode.Input)
                {
                    success = Interop.RequestLineInput(pin, pinNumber.ToString());
                }
                else
                {
                    success = Interop.RequestLineOutput(pin, pinNumber.ToString());
                }
            }
            if (success == -1) {
                throw new IOException($"Error setting pin mode, pin:{pinNumber}, error: {Marshal.GetLastWin32Error()}");
            } 
        }

        protected internal override WaitForEventResult WaitForEvent(int pinNumber, PinEventTypes eventType, CancellationToken cancellationToken)
        {          
            if (_pinNumberToSafeLineHandle.TryGetValue(pinNumber, out SafeLineHandle pin))
            {
                Interop.ReleaseGpiodLine(pin);
            }
            pin = RequestEvent(pinNumber, eventType);
            bool eventOccured = WaitForEvent(cancellationToken, pin, eventType);
            return new WaitForEventResult
            {
                TimedOut = eventOccured,
                EventType = eventType
            };
        }

        private bool WaitForEvent(CancellationToken cancellationToken, SafeLineHandle pin, PinEventTypes eventType)
        {
            int waitResult = -2;
            while (!cancellationToken.IsCancellationRequested)
            {
                waitResult = Interop.WaitForEventOnLine(pin);
                if (waitResult == -1)
                {
                    throw new IOException($"Error while waiting for event, error code: {Marshal.GetLastWin32Error()}");
                }
                if (waitResult == 1)
                {
                    int readResult = Interop.ReadEventForLine(pin);
                    if (readResult == -1)
                    {
                        throw new IOException($"Error while trying to read pin event result, error code: {Marshal.GetLastWin32Error()}");
                    }
                    if (readResult == (int)eventType)
                    {
                        return true;
                    }
                }
            }
            return false;
        }

        protected internal override void Write(int pinNumber, PinValue value)
        {
            if (_pinNumberToSafeLineHandle.TryGetValue(pinNumber, out SafeLineHandle pin))
            {
                Interop.SetGpiodLineValue(pin, (value == PinValue.High) ? 1 : 0);
            }
        }

        protected override void Dispose(bool disposing)
        {
            foreach (SafeLineHandle handle in _pinNumberToSafeLineHandle.Values)
            {
                if (handle != null)
                {
                    Interop.ReleaseGpiodLine(handle);
                }
            }
            foreach (LibGpiodDriverEventHandler devicePin in _pinNumberToEventHandler.Values)
            {
                devicePin.Dispose();
            }
            _pinNumberToEventHandler.Clear();
            if (_chip != null) 
                _chip.Dispose();
            base.Dispose(disposing);
        }
    }
}
