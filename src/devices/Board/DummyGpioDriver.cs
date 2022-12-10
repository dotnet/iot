// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System;
using System.Collections.Generic;
using System.Device.Gpio;
using System.Text;
using System.Threading;

namespace Iot.Device.Board
{
    /// <summary>
    /// A GPIO driver that has zero pins. Use to fulfill the interface.
    /// </summary>
    public class DummyGpioDriver : GpioDriver
    {
        /// <summary>
        /// Returns 0
        /// </summary>
        protected override int PinCount => 0;

        /// <inheritdoc />
        protected override int ConvertPinNumberToLogicalNumberingScheme(int pinNumber)
        {
            return 0;
        }

        /// <summary>
        /// There are no pins on this board, so this always throws an exception
        /// </summary>
        /// <param name="pinNumber">Pin number</param>
        /// <exception cref="NotSupportedException">Always thrown: This board has no pins</exception>
        protected override void OpenPin(int pinNumber)
        {
            throw new NotSupportedException("No such pin");
        }

        /// <inheritdoc />
        protected override void ClosePin(int pinNumber)
        {
            throw new NotSupportedException("No such pin");
        }

        /// <inheritdoc />
        protected override void SetPinMode(int pinNumber, PinMode mode)
        {
            throw new NotSupportedException("No such pin");
        }

        /// <inheritdoc />
        protected override PinMode GetPinMode(int pinNumber)
        {
            throw new NotSupportedException("No such pin");
        }

        /// <inheritdoc />
        protected override bool IsPinModeSupported(int pinNumber, PinMode mode)
        {
            throw new NotSupportedException("No such pin");
        }

        /// <inheritdoc />
        protected override PinValue Read(int pinNumber)
        {
            throw new NotSupportedException("No such pin");
        }

        /// <inheritdoc />
        protected override void Toggle(int pinNumber)
        {
            throw new NotSupportedException("No such pin");
        }

        /// <inheritdoc />
        protected override void Write(int pinNumber, PinValue value)
        {
            throw new NotSupportedException("No such pin");
        }

        /// <inheritdoc />
        protected override WaitForEventResult WaitForEvent(int pinNumber, PinEventTypes eventTypes, CancellationToken cancellationToken)
        {
            throw new NotSupportedException("No such pin");
        }

        /// <inheritdoc />
        protected override void AddCallbackForPinValueChangedEvent(int pinNumber, PinEventTypes eventTypes, PinChangeEventHandler callback)
        {
            throw new NotSupportedException("No such pin");
        }

        /// <inheritdoc />
        protected override void RemoveCallbackForPinValueChangedEvent(int pinNumber, PinChangeEventHandler callback)
        {
            throw new NotSupportedException("No such pin");
        }
    }
}
