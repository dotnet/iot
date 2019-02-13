// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information

using System.Collections.Generic;
using System.Threading;
using System.Runtime.InteropServices;
using System.IO;

namespace System.Device.Gpio.Drivers
{
    public class LibGpiodDriver : UnixDriver
    {
        private SafeChipHandle _chip;

        private Dictionary<int, SafeLineHandle> _pinNumberToSafeLineHandle;

        protected internal override int PinCount => Interop.GetNumberOfLines(_chip);

        public LibGpiodDriver()
        {
            SafeChipIteratorHandle iterator = Interop.GetChipIterator();
            if (iterator == null)
            {
                throw new IOException($"Unable to find a chip iterator, error code: {Marshal.GetLastWin32Error()}");
            }

            _chip = Interop.GetNextChipFromChipIterator(iterator);
            if (_chip == null)
            {
                throw new IOException($"Unable to find a chip, error code: {Marshal.GetLastWin32Error()}");
            }

            // Freeing other chips opened
            Interop.FreeChipIteratorNoCloseCurrentChip(iterator);
            _pinNumberToSafeLineHandle = new Dictionary<int, SafeLineHandle>(PinCount);
        }

        protected internal override void AddCallbackForPinValueChangedEvent(int pinNumber, PinEventTypes eventType, PinChangeEventHandler callback)
        {
            throw new NotImplementedException();
        }
        
        protected internal override void ClosePin(int pinNumber)
        {
            if (_pinNumberToSafeLineHandle.TryGetValue(pinNumber, out SafeLineHandle pinHandle))
            {
                Interop.ReleaseGpiodLine(pinHandle);
                _pinNumberToSafeLineHandle.Remove(pinNumber);
            }
        }

        protected internal override int ConvertPinNumberToLogicalNumberingScheme(int pinNumber) => throw new
            PlatformNotSupportedException("This driver is generic so it cannot perform conversions between pin numbering schemes.");

        protected internal override PinMode GetPinMode(int pinNumber)
        {
            if (_pinNumberToSafeLineHandle.TryGetValue(pinNumber, out SafeLineHandle pinHandle))
            {
                return (Interop.GetLineDirection(pinHandle) == 1) ? PinMode.Input : PinMode.Output;
            }
            throw new IOException($"Error while reading pin mode for pin: {pinNumber}");
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
                throw new IOException($"Pin number {pinNumber} not available, error: {Marshal.GetLastWin32Error()}");
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
                    throw new IOException($"Error while reading value from pin number: {pinNumber}, error: {Marshal.GetLastWin32Error()}");
                }
                return result;
            }
            throw new IOException($"Error while reading value from pin number: {pinNumber}");
        }

        protected internal override void RemoveCallbackForPinValueChangedEvent(int pinNumber, PinChangeEventHandler callback)
        {
            throw new NotImplementedException();
        }

        protected internal override void SetPinMode(int pinNumber, PinMode mode)
        {
            bool failed  = false;
            if (_pinNumberToSafeLineHandle.TryGetValue(pinNumber, out SafeLineHandle pinHandle))
            {
                if (mode == PinMode.Input)
                {
                    failed = Interop.RequestLineInput(pinHandle, pinNumber.ToString()) == -1;
                }
                else
                {
                    failed = Interop.RequestLineOutput(pinHandle, pinNumber.ToString()) == -1;
                }
            }

            if (failed)
            {
                throw new IOException($"Error setting pin mode, pin:{pinNumber}, error: {Marshal.GetLastWin32Error()}");
            } 
        }

        protected internal override WaitForEventResult WaitForEvent(int pinNumber, PinEventTypes eventType, CancellationToken cancellationToken)
        {
            throw new NotImplementedException();
        }

        protected internal override void Write(int pinNumber, PinValue value)
        {
            if (_pinNumberToSafeLineHandle.TryGetValue(pinNumber, out SafeLineHandle pinHandle))
            {
                Interop.SetGpiodLineValue(pinHandle, (value == PinValue.High) ? 1 : 0);
            }
        }

        protected override void Dispose(bool disposing)
        {
            foreach (int pin in _pinNumberToSafeLineHandle.Keys)
            {
                if (_pinNumberToSafeLineHandle.TryGetValue(pin, out SafeLineHandle pinHandle))
                {
                    Interop.ReleaseGpiodLine(pinHandle);
                }

                _pinNumberToSafeLineHandle.Remove(pin);
            }

            if (_chip != null)
            {
                _chip.Dispose();
                _chip = null;
            }

            base.Dispose(disposing);
        }
    }
}
