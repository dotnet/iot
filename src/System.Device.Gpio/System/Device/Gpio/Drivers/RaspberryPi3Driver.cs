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
        private GpioDriver _internalDriver;

        /* private delegates for register Properties */
        private delegate void Set_Register(ulong value);
        private delegate ulong Get_Register();

        private readonly Set_Register _setSetRegister;
        private readonly Get_Register _getSetRegister;
        private readonly Set_Register _setClearRegister;
        private readonly Get_Register _getClearRegister;

        /// <summary>
        /// Creates an instance of the RaspberryPi3Driver.
        /// This driver works on Raspberry 3 or 4, both on Linux and on Windows
        /// </summary>
        public RaspberryPi3Driver()
        {
            if (Environment.OSVersion.Platform == PlatformID.Unix)
            {
                RaspberryPi3LinuxDriver linuxDriver = CreateInternalRaspberryPi3LinuxDriver();

                if (linuxDriver == null)
                {
                    throw new PlatformNotSupportedException("Not a supported Raspberry Pi type");
                }

                _setSetRegister = (value) => linuxDriver.SetRegister = value;
                _setClearRegister = (value) => linuxDriver.ClearRegister = value;
                _getSetRegister = () => linuxDriver.SetRegister;
                _getClearRegister = () => linuxDriver.ClearRegister;
                _internalDriver = linuxDriver;
            }
            else
            {
                _internalDriver = CreateWindows10GpioDriver();
                _setSetRegister = (value) => throw new PlatformNotSupportedException();
                _setClearRegister = (value) => throw new PlatformNotSupportedException();
                _getSetRegister = () => throw new PlatformNotSupportedException();
                _getClearRegister = () => throw new PlatformNotSupportedException();
            }
        }

        internal RaspberryPi3Driver(RaspberryPi3LinuxDriver linuxDriver)
        {
            if (Environment.OSVersion.Platform == PlatformID.Unix)
            {
                _setSetRegister = (value) => linuxDriver.SetRegister = value;
                _setClearRegister = (value) => linuxDriver.ClearRegister = value;
                _getSetRegister = () => linuxDriver.SetRegister;
                _getClearRegister = () => linuxDriver.ClearRegister;
                _internalDriver = linuxDriver;
            }
            else
            {
                throw new NotSupportedException("This ctor is for internal use only");
            }
        }

        internal static RaspberryPi3LinuxDriver CreateInternalRaspberryPi3LinuxDriver()
        {
            RaspberryBoardIdentification identification = RaspberryBoardIdentification.LoadBoard();
            RaspberryPi3LinuxDriver linuxDriver;
            switch (identification.GetBoardModel())
            {
                case RaspberryBoardIdentification.Model.RaspberryPi3B:
                case RaspberryBoardIdentification.Model.RaspberryPi3BPlus:
                case RaspberryBoardIdentification.Model.RaspberryPi4:
                    linuxDriver = new RaspberryPi3LinuxDriver();
                    break;
                case RaspberryBoardIdentification.Model.RaspberryPiComputeModule3:
                    linuxDriver = new RaspberryPiCm3Driver();
                    break;
                default:
                    linuxDriver = null;
                    break;
            }

            return linuxDriver;
        }

        [MethodImpl(MethodImplOptions.NoInlining)]
        private static GpioDriver CreateWindows10GpioDriver()
        {
            // This wrapper is needed to prevent Mono from loading Windows10Driver
            // which causes all fields to be loaded - one of such fields is WinRT type which does not
            // exist on Linux which causes TypeLoadException.
            // Using NoInlining and no explicit type prevents this from happening.
            return new Windows10Driver();
        }

        /// <inheritdoc/>
        protected internal override int PinCount => _internalDriver.PinCount;

        /// <inheritdoc/>
        protected internal override void AddCallbackForPinValueChangedEvent(int pinNumber, PinEventTypes eventTypes, PinChangeEventHandler callback) => _internalDriver.AddCallbackForPinValueChangedEvent(pinNumber, eventTypes, callback);

        /// <inheritdoc/>
        protected internal override void ClosePin(int pinNumber) => _internalDriver.ClosePin(pinNumber);

        /// <inheritdoc/>
        protected internal override int ConvertPinNumberToLogicalNumberingScheme(int pinNumber)
        {
            return _internalDriver.ConvertPinNumberToLogicalNumberingScheme(pinNumber);
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

        /// <summary>
        /// Allows directly setting the "Set pin high" register. Used for special applications only
        /// </summary>
        protected ulong SetRegister
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get => _getSetRegister();
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            set => _setSetRegister(value);
        }

        /// <summary>
        /// Allows directly setting the "Set pin low" register. Used for special applications only
        /// </summary>
        protected ulong ClearRegister
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get => _getClearRegister();
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            set => _setClearRegister(value);
        }

        /// <inheritdoc />
        protected override void Dispose(bool disposing)
        {
            _internalDriver?.Dispose();
            _internalDriver = null;
            base.Dispose(disposing);
        }
    }
}
