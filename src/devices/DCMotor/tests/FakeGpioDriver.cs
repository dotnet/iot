// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System;
using System.Collections.Concurrent;
using System.Device.Gpio;
using System.Threading;

namespace Iot.Device.DCMotor.Tests
{
    /// <summary>
    /// A GPIO driver that keeps the last value written to each pin. Like the Raspberry Pi drivers,
    /// closing a pin does not change its value.
    /// </summary>
    internal sealed class FakeGpioDriver : GpioDriver
    {
        private readonly ConcurrentDictionary<int, PinValue> _values = new();
        private readonly ConcurrentDictionary<int, PinMode> _modes = new();

        public bool IsDisposed { get; private set; }

        public PinValue GetValue(int pinNumber) => _values.TryGetValue(pinNumber, out PinValue value) ? value : PinValue.Low;

        /// <summary>
        /// Sets the value that the next reads of an input pin return, as if a device drove it
        /// </summary>
        public void SetInputValue(int pinNumber, PinValue value) => _values[pinNumber] = value;

        protected override int PinCount => 28;

        protected override void OpenPin(int pinNumber)
        {
        }

        protected override void ClosePin(int pinNumber)
        {
        }

        protected override void SetPinMode(int pinNumber, PinMode mode) => _modes[pinNumber] = mode;

        protected override PinMode GetPinMode(int pinNumber) => _modes.TryGetValue(pinNumber, out PinMode mode) ? mode : PinMode.Input;

        protected override bool IsPinModeSupported(int pinNumber, PinMode mode) => true;

        protected override PinValue Read(int pinNumber) => GetValue(pinNumber);

        protected override void Write(int pinNumber, PinValue value) => _values[pinNumber] = value;

        protected override WaitForEventResult WaitForEvent(int pinNumber, PinEventTypes eventTypes, CancellationToken cancellationToken) => throw new NotSupportedException();

        protected override void AddCallbackForPinValueChangedEvent(int pinNumber, PinEventTypes eventTypes, PinChangeEventHandler callback) => throw new NotSupportedException();

        protected override void RemoveCallbackForPinValueChangedEvent(int pinNumber, PinChangeEventHandler callback) => throw new NotSupportedException();

        protected override void Dispose(bool disposing)
        {
            IsDisposed = true;
            base.Dispose(disposing);
        }
    }
}
