// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using System.Collections.Generic;
using System.Disposables;
using System.Threading;
using System.Threading.Tasks;

namespace System.Device.Gpio
{
    /// <summary>
    /// Represents a shared view of general-purpose I/O (GPIO) controller.
    /// </summary>
    public sealed class SharedGpioController : IGpioController
    {
        private readonly List<int> _openPins = new List<int>();
        private readonly IGpioController _controller;

        /// <summary>
        /// Initializes a new instance of the <see cref="GpioController"/> class that will use the logical pin numbering scheme as default.
        /// </summary>
        public SharedGpioController(IGpioController controller)
        {
            _controller = controller;
        }

        /// <inheritdoc/>
        public void Dispose()
        {
            foreach (var openPin in _openPins)
            {
                _controller.ClosePin(openPin);
            }

            _openPins.Clear();
        }

        /// <inheritdoc/>
        public PinNumberingScheme NumberingScheme => _controller.NumberingScheme;

        /// <inheritdoc/>
        public int PinCount => _controller.PinCount;

        /// <inheritdoc/>
        public void OpenPin(int pinNumber)
        {
            _controller.OpenPin(pinNumber);
            _openPins.Add(pinNumber);
        }

        /// <inheritdoc/>
        public void OpenPin(int pinNumber, PinMode mode)
        {
            _controller.OpenPin(pinNumber, mode);
            _openPins.Add(pinNumber);
        }

        /// <inheritdoc/>
        public void ClosePin(int pinNumber)
        {
            if (_openPins.Remove(pinNumber))
            {
                _controller.ClosePin(pinNumber);
            }
        }

        /// <inheritdoc/>
        public void SetPinMode(int pinNumber, PinMode mode)
        {
            _controller.SetPinMode(pinNumber, mode);
        }

        /// <inheritdoc/>
        public PinMode GetPinMode(int pinNumber)
        {
            return _controller.GetPinMode(pinNumber);
        }

        /// <inheritdoc/>
        public bool IsPinOpen(int pinNumber)
        {
            return _openPins.Contains(pinNumber);
        }

        /// <inheritdoc/>
        public bool IsPinModeSupported(int pinNumber, PinMode mode)
        {
            return _controller.IsPinModeSupported(pinNumber, mode);
        }

        /// <inheritdoc/>
        public PinValue Read(int pinNumber)
        {
            return _controller.Read(pinNumber);
        }

        /// <inheritdoc/>
        public void Write(int pinNumber, PinValue value)
        {
            _controller.Write(pinNumber, value);
        }

        /// <inheritdoc/>
        public WaitForEventResult WaitForEvent(int pinNumber, PinEventTypes eventTypes, TimeSpan timeout)
        {
            return _controller.WaitForEvent(pinNumber, eventTypes, timeout);
        }

        /// <inheritdoc/>
        public WaitForEventResult WaitForEvent(int pinNumber, PinEventTypes eventTypes, CancellationToken cancellationToken)
        {
            return _controller.WaitForEvent(pinNumber, eventTypes, cancellationToken);
        }

        /// <inheritdoc/>
        public ValueTask<WaitForEventResult> WaitForEventAsync(int pinNumber, PinEventTypes eventTypes, TimeSpan timeout)
        {
            return _controller.WaitForEventAsync(pinNumber, eventTypes, timeout);
        }

        /// <inheritdoc/>
        public ValueTask<WaitForEventResult> WaitForEventAsync(int pinNumber, PinEventTypes eventTypes, CancellationToken token)
        {
            return _controller.WaitForEventAsync(pinNumber, eventTypes, token);
        }

        /// <inheritdoc/>
        public void RegisterCallbackForPinValueChangedEvent(int pinNumber, PinEventTypes eventTypes, PinChangeEventHandler callback)
        {
            _controller.RegisterCallbackForPinValueChangedEvent(pinNumber, eventTypes, callback);
        }

        /// <inheritdoc/>
        public void UnregisterCallbackForPinValueChangedEvent(int pinNumber, PinChangeEventHandler callback)
        {
            _controller.UnregisterCallbackForPinValueChangedEvent(pinNumber, callback);
        }

        /// <inheritdoc/>
        public void Write(ReadOnlySpan<PinValuePair> pinValuePairs)
        {
            _controller.Write(pinValuePairs);
        }

        /// <inheritdoc/>
        public void Read(Span<PinValuePair> pinValuePairs)
        {
            _controller.Read(pinValuePairs);
        }
    }
}
