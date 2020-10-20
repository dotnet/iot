// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using System.Threading;

namespace System.Device.Gpio.Drivers
{
    /// <summary>
    /// A GPIO driver for Windows 10 IoT.
    /// </summary>
    public class Windows10Driver : GpioDriver
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="Windows10Driver"/> class.
        /// </summary>
        public Windows10Driver()
        {
            // If we land in this method it means the console application is running on Windows and targetting net5.0 (without specifying Windows platform)
            // In order to call WinRT code in net5.0 it is required for the application to target the specific platform
            // so we throw the bellow exception with a detailed message in order to instruct the consumer on how to move forward.
            throw new PlatformNotSupportedException(CommonHelpers.GetFormattedWindowsPlatformTargetingErrorMessage(nameof(Windows10Driver)));
        }

        /// <inheritdoc />
        protected internal override int PinCount => throw new PlatformNotSupportedException();

        /// <inheritdoc />
        protected internal override void AddCallbackForPinValueChangedEvent(int pinNumber, PinEventTypes eventTypes, PinChangeEventHandler callback)
        {
            throw new PlatformNotSupportedException();
        }

        /// <inheritdoc />
        protected internal override void ClosePin(int pinNumber)
        {
            throw new PlatformNotSupportedException();
        }

        /// <inheritdoc />
        protected internal override int ConvertPinNumberToLogicalNumberingScheme(int pinNumber)
        {
            throw new PlatformNotSupportedException();
        }

        /// <inheritdoc />
        protected internal override PinMode GetPinMode(int pinNumber)
        {
            throw new PlatformNotSupportedException();
        }

        /// <inheritdoc />
        protected internal override bool IsPinModeSupported(int pinNumber, PinMode mode)
        {
            throw new PlatformNotSupportedException();
        }

        /// <inheritdoc />
        protected internal override void OpenPin(int pinNumber)
        {
            throw new PlatformNotSupportedException();
        }

        /// <inheritdoc />
        protected internal override PinValue Read(int pinNumber)
        {
            throw new PlatformNotSupportedException();
        }

        /// <inheritdoc />
        protected internal override void RemoveCallbackForPinValueChangedEvent(int pinNumber, PinChangeEventHandler callback)
        {
            throw new PlatformNotSupportedException();
        }

        /// <inheritdoc />
        protected internal override void SetPinMode(int pinNumber, PinMode mode)
        {
            throw new PlatformNotSupportedException();
        }

        /// <inheritdoc />
        protected internal override WaitForEventResult WaitForEvent(int pinNumber, PinEventTypes eventTypes, CancellationToken cancellationToken)
        {
            throw new PlatformNotSupportedException();
        }

        /// <inheritdoc />
        protected internal override void Write(int pinNumber, PinValue value)
        {
            throw new PlatformNotSupportedException();
        }
    }
}
