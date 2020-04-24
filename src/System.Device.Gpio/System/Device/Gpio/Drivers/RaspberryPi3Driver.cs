// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using System.Runtime.CompilerServices;
using System.Threading;
using System.Threading.Tasks;

namespace System.Device.Gpio.Drivers
{
    /// <summary>
    /// A GPIO driver for the Raspberry Pi 3 or 4, running Raspbian (or, with some limitations, ubuntu)
    /// </summary>
    public class RaspberryPi3Driver : GpioDriver
    {
        private RaspberryPiRegisterDriver _internalDriver;

        public RaspberryPi3Driver()
        {
            if (Environment.OSVersion.Platform == PlatformID.Unix)
            {
                _internalDriver = new RaspberryPi3LinuxDriver();
            }
            else
            {
                _internalDriver = new RaspberryPiRegisterDriver();
            }
        }

        /// <inheritdoc/>
        protected internal override int PinCount => 28;

        /// <inheritdoc/>
        protected internal override void AddCallbackForPinValueChangedEvent(int pinNumber, PinEventTypes eventTypes, PinChangeEventHandler callback) => _internalDriver.AddCallbackForPinValueChangedEvent(pinNumber, eventTypes, callback);

        /// <inheritdoc/>
        protected internal override void ClosePin(int pinNumber) => _internalDriver.ClosePin(pinNumber);

        /// <inheritdoc/>
        protected internal override int ConvertPinNumberToLogicalNumberingScheme(int pinNumber)
        {
            return pinNumber switch
            {
                3 => 2,
                5 => 3,
                7 => 4,
                8 => 14,
                10 => 15,
                11 => 17,
                12 => 18,
                13 => 27,
                15 => 22,
                16 => 23,
                18 => 24,
                19 => 10,
                21 => 9,
                22 => 25,
                23 => 11,
                24 => 8,
                26 => 7,
                27 => 0,
                28 => 1,
                29 => 5,
                31 => 6,
                32 => 12,
                33 => 13,
                35 => 19,
                36 => 16,
                37 => 26,
                38 => 20,
                40 => 21,
                _ => throw new ArgumentException($"Board (header) pin {pinNumber} is not a GPIO pin on the {GetType().Name} device.", nameof(pinNumber))
            };
        }

        /// <inheritdoc/>
        protected internal override PinMode GetPinMode(int pinNumber) => _internalDriver.GetPinMode(pinNumber);

        /// <inheritdoc/>
        protected internal override bool IsPinModeSupported(int pinNumber, PinMode mode) => _internalDriver.IsPinModeSupported(pinNumber, mode);

        /// <inheritdoc/>
        protected internal override void OpenPin(int pinNumber) => _internalDriver.OpenPin(pinNumber);

        /// <inheritdoc/>
        protected internal override PinValue Read(int pinNumber) => _internalDriver.Read(pinNumber);

        /// <inheritdoc/>
        protected internal override void RemoveCallbackForPinValueChangedEvent(int pinNumber, PinChangeEventHandler callback) => _internalDriver.RemoveCallbackForPinValueChangedEvent(pinNumber, callback);

        /// <inheritdoc/>
        protected internal override void SetPinMode(int pinNumber, PinMode mode) => _internalDriver.SetPinMode(pinNumber, mode);

        /// <inheritdoc/>
        protected internal override WaitForEventResult WaitForEvent(int pinNumber, PinEventTypes eventTypes, CancellationToken cancellationToken) => _internalDriver.WaitForEvent(pinNumber, eventTypes, cancellationToken);

        /// <inheritdoc/>
        protected internal override ValueTask<WaitForEventResult> WaitForEventAsync(int pinNumber, PinEventTypes eventTypes, CancellationToken cancellationToken) => _internalDriver.WaitForEventAsync(pinNumber, eventTypes, cancellationToken);

        /// <inheritdoc/>
        protected internal override void Write(int pinNumber, PinValue value) => _internalDriver.Write(pinNumber, value);

        protected ulong SetRegister
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get => _internalDriver.SetRegister;
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            set => _internalDriver.SetRegister = value;
        }

        protected ulong ClearRegister
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get => _internalDriver.ClearRegister;
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            set => _internalDriver.ClearRegister = value;
        }

        protected override void Dispose(bool disposing)
        {
            _internalDriver?.Dispose();
            _internalDriver = null;
            base.Dispose(disposing);
        }
    }
}
