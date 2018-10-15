// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using System.Collections.Generic;
using System.Device.Gpio;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Threading;

using GpioPinValueChangedHandler = global::Windows.Foundation.TypedEventHandler<global::Windows.Devices.Gpio.GpioPin, global::Windows.Devices.Gpio.GpioPinValueChangedEventArgs>;
using WinGpio = global::Windows.Devices.Gpio;

namespace System.Devices.Gpio
{
    public class Windows10Driver : GpioDriver
    {
        private readonly WinGpio.GpioController _winGpioController;
        private readonly Dictionary<int, PinItem> _openPins = new Dictionary<int, PinItem>();
        private readonly object _openPinsLock = new object();

        public Windows10Driver()
            : this(null)
        {
        }

        public Windows10Driver(WinGpio.GpioController winGpioController)
        {
            _winGpioController = winGpioController ?? WinGpio.GpioController.GetDefault();
            PinCount =  _winGpioController.PinCount;
        }

        protected internal override int PinCount { get; }

        public override void Dispose()
        {
            lock (_openPinsLock)
            {
                foreach (var openPin in _openPins)
                    openPin.Value.Dispose();
                _openPins.Clear();
            }
        }

        protected internal override void ClosePin(int gpioPinNumber)
        {
            VerifyPinIsInValidRange(gpioPinNumber, nameof(gpioPinNumber));

            lock (_openPinsLock)
            {
                VerifyPinIsOpen(gpioPinNumber, nameof(gpioPinNumber)).Dispose();
                _openPins.Remove(gpioPinNumber);
            }
        }

        protected internal override int ConvertPinNumber(int pinNumber, PinNumberingScheme from, PinNumberingScheme to)
        {
            VerifyPinIsInValidRange(pinNumber, nameof(pinNumber));
            if (from == PinNumberingScheme.Gpio && to == PinNumberingScheme.Gpio)
                return pinNumber;

            throw new NotSupportedException("Only PinNumberingScheme.Gpio numbering scheme is supported");
        }

        protected internal override TimeSpan GetDebounce(int gpioPinNumber)
        {
            VerifyPinIsInValidRange(gpioPinNumber, nameof(gpioPinNumber));

            lock (_openPinsLock)
            {
                var pinItem = VerifyPinIsOpen(gpioPinNumber, nameof(gpioPinNumber));
                return pinItem.Pin.DebounceTimeout;
            }
        }

        protected internal override bool GetEnableRaisingPinEvents(int gpioPinNumber)
        {
            VerifyPinIsInValidRange(gpioPinNumber, nameof(gpioPinNumber));

            // This method doesn't really make sense for WinGpio, since a pin is enabled when its ValueChanged is subscribed to
            // Since we can't hook the WinGpio.GpioPin's event adder/remover, nor access its delegate value, we will just
            // assume, for the sake of satisfying this abstract method, that the pin is enabled for events if it's open.
            lock (_openPinsLock)
                return _openPins.ContainsKey(gpioPinNumber);
        }

        protected internal override PinEvent GetPinEventsToDetect(int gpioPinNumber)
        {
            VerifyPinIsInValidRange(gpioPinNumber, nameof(gpioPinNumber));

            lock (_openPinsLock)
                return VerifyPinIsOpen(gpioPinNumber, nameof(gpioPinNumber)).PinEventsToDetect;
        }

        protected internal override PinMode GetPinMode(int gpioPinNumber)
        {
            VerifyPinIsInValidRange(gpioPinNumber, nameof(gpioPinNumber));

            lock (_openPinsLock)
            {
                var pinItem = VerifyPinIsOpen(gpioPinNumber, nameof(gpioPinNumber));
                return GpioDriveModeToPinMode(pinItem.Pin.GetDriveMode());
            }
        }

        protected internal override PinValue Input(int gpioPinNumber)
        {
            VerifyPinIsInValidRange(gpioPinNumber, nameof(gpioPinNumber));

            lock (_openPinsLock)
            {
                var pinItem = VerifyPinIsOpen(gpioPinNumber, nameof(gpioPinNumber));
                return GpioPinValueToPinValue(pinItem.Pin.Read());
            }
        }

        protected internal override bool IsPinModeSupported(PinMode mode)
        {
            lock (_openPinsLock)
            {
                // Since this API doesn't specify a gpioPinNumber argument, just use any open pin,
                // or -1 if none are open, which will cause VerifyPinIsOpen to throw.
                var pinItem = (_openPins.Count != 0) ? _openPins.First().Value : VerifyPinIsOpen(-1, null);
                return pinItem.Pin.IsDriveModeSupported(PinModeToGpioDriveMode(mode));
            }
        }

        protected internal override void OpenPin(int gpioPinNumber)
        {
            VerifyPinIsInValidRange(gpioPinNumber, nameof(gpioPinNumber));

            lock (_openPinsLock)
            {
                VerifyPinIsClosed(gpioPinNumber, nameof(gpioPinNumber));

                var pinItem = new PinItem { Pin = _winGpioController.OpenPin(gpioPinNumber) };
                _openPins[gpioPinNumber] = pinItem;
            }
        }

        protected internal override void OpenPWMPin(int chip, int channel)
        {
            // Add validation and required code to open the pin
        }

        protected internal override void Output(int gpioPinNumber, PinValue value)
        {
            VerifyPinIsInValidRange(gpioPinNumber, nameof(gpioPinNumber));

            lock (_openPinsLock)
            {
                var pinItem = VerifyPinIsOpen(gpioPinNumber, nameof(gpioPinNumber));
                pinItem.Pin.Write(PinValueToGpioPinValue(value));
            }
        }

        protected internal override void SetDebounce(int gpioPinNumber, TimeSpan timeout)
        {
            VerifyPinIsInValidRange(gpioPinNumber, nameof(gpioPinNumber));

            lock (_openPinsLock)
            {
                var pinItem = VerifyPinIsOpen(gpioPinNumber, nameof(gpioPinNumber));
                pinItem.Pin.DebounceTimeout = timeout;
            }
        }

        protected internal override void SetEnableRaisingPinEvents(int gpioPinNumber, bool enable)
        {
            VerifyPinIsInValidRange(gpioPinNumber, nameof(gpioPinNumber));

            // This method doesn't really make sense for WinGpio, since a pin is enabled when its ValueChanged is subscribed to.
            // Since we can't hook the WinGpio.GpioPin's event adder/remover, nor access its delegate value, we will just
            // assume, for the sake of satisfying this abstract method, that only an open pin can be enabled for events.
            lock (_openPinsLock)
                VerifyPinIsOpen(gpioPinNumber, nameof(gpioPinNumber));
        }

        protected internal override void SetPinEventsToDetect(int gpioPinNumber, PinEvent events)
        {
            VerifyPinIsInValidRange(gpioPinNumber, nameof(gpioPinNumber));

            lock (_openPinsLock)
                VerifyPinIsOpen(gpioPinNumber, nameof(gpioPinNumber)).PinEventsToDetect = events;
        }

        protected internal override void SetPinMode(int gpioPinNumber, PinMode mode)
        {
            VerifyPinIsInValidRange(gpioPinNumber, nameof(gpioPinNumber));

            lock (_openPinsLock)
            {
                var pinItem = VerifyPinIsOpen(gpioPinNumber, nameof(gpioPinNumber));
                pinItem.Pin.SetDriveMode(PinModeToGpioDriveMode(mode));
            }
        }

        protected internal override bool WaitForPinEvent(int gpioPinNumber, TimeSpan timeout)
        {
            VerifyPinIsInValidRange(gpioPinNumber, nameof(gpioPinNumber));

            var waitEvent = new ManualResetEventSlim();
            GpioPinValueChangedHandler handler = (s, a) => waitEvent.Set();

            WinGpio.GpioPin pin = null;
            lock (_openPinsLock)
            {
                pin = VerifyPinIsOpen(gpioPinNumber, nameof(gpioPinNumber)).Pin;
                pin.ValueChanged += handler;
            }

            var result = waitEvent.Wait(timeout);
            pin.ValueChanged -= handler;

            return result;
        }

        protected internal override void ClosePWMPin(int chip, int channel)
        {
            throw new NotImplementedException();
        }

        protected internal override void PWMWrite(int chip, int channel, PWMMode mode, int period, int dutyCycle)
        {
            throw new NotImplementedException();
        }

        #region Private Implementation

        private int VerifyPinIsInValidRange(int gpioPinNumber, string argumentName)
        {
            // Assumes that the collection is locked
            return ValidateArgument.IsInHalfOpenRange(gpioPinNumber, 0, PinCount, argumentName);
        }

        private PinItem VerifyPinIsOpen(int gpioPinNumber, string argumentName)
        {
            // Assumes that the collection is locked
            if (!_openPins.TryGetValue(gpioPinNumber, out var pinItem))
                throw new ArgumentException($"Specified GPIO pin is not open: {gpioPinNumber}", argumentName);

            return pinItem;
        }

        private void VerifyPinIsClosed(int gpioPinNumber, string argumentName)
        {
            // Assumes that the collection is locked
            if (_openPins.ContainsKey(gpioPinNumber))
                throw new ArgumentException($"Specified GPIO pin is already open: {gpioPinNumber}", argumentName);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static WinGpio.GpioPinDriveMode PinModeToGpioDriveMode(PinMode mode)
        {
            switch (mode)
            {
                case PinMode.Input:
                    return WinGpio.GpioPinDriveMode.Input;
                case PinMode.Output:
                    return WinGpio.GpioPinDriveMode.Output;
                case PinMode.InputPullDown:
                    return WinGpio.GpioPinDriveMode.InputPullDown;
                case PinMode.InputPullUp:
                    return WinGpio.GpioPinDriveMode.InputPullUp;
                default:
                    throw new NotSupportedException($"GPIO pin mode not supported: {mode}");
            }
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static PinMode GpioDriveModeToPinMode(WinGpio.GpioPinDriveMode mode)
        {
            switch (mode)
            {
                case WinGpio.GpioPinDriveMode.Input:
                    return PinMode.Input;
                case WinGpio.GpioPinDriveMode.Output:
                    return PinMode.Output;
                case WinGpio.GpioPinDriveMode.InputPullDown:
                    return PinMode.InputPullDown;
                case WinGpio.GpioPinDriveMode.InputPullUp:
                    return PinMode.InputPullUp;
                default:
                    throw new NotSupportedException($"GPIO pin mode not supported: {mode}");
            }
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static PinValue GpioPinValueToPinValue(WinGpio.GpioPinValue value)
        {
            switch (value)
            {
                case WinGpio.GpioPinValue.Low:
                    return PinValue.Low;
                case WinGpio.GpioPinValue.High:
                    return PinValue.High;
                default:
                    throw new NotSupportedException($"GPIO pin value not supported: {value}");
            }
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        WinGpio.GpioPinValue PinValueToGpioPinValue(PinValue value)
        {
            switch (value)
            {
                case PinValue.Low:
                    return WinGpio.GpioPinValue.Low;
                case PinValue.High:
                    return WinGpio.GpioPinValue.High;
                default:
                    throw new NotSupportedException($"GPIO pin value not supported: {value}");
            }
        }

        #endregion Private Implementation

        #region Nested Types

        private class PinItem : IDisposable
        {
            public WinGpio.GpioPin Pin { get; set; }
            public PinEvent PinEventsToDetect { get; set; } = PinEvent.Any;

            ~PinItem()
            {
                Dispose();
                GC.SuppressFinalize(this);
            }

            public void Dispose()
            {
                Pin?.Dispose();
                Pin = null;
                PinEventsToDetect = PinEvent.Any;
            }
        }

        /// <summary>
        /// Note that many of these could/should be made generic common across the entire framework
        /// </summary>
        private static class ValidateArgument
        {
            public static T IsInHalfOpenRange<T>(T argument, T minValue, T endValue, string argumentName, string message = null)
                where T : struct, IComparable
            {
                if (argument.CompareTo(minValue) < 0 || endValue.CompareTo(argument) <= 0)
                {
                    throw new ArgumentOutOfRangeException(argumentName, message);
                }

                return argument;
            }

        }

        #endregion Nested Types
    }
}
