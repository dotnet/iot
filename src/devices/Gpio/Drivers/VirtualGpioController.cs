// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System.Threading.Tasks;
using System.Threading;
using System.Collections.Generic;
using System.Linq;
using System.Collections.Concurrent;
using System.Device.Gpio;
using System;
using System.Device;
using Iot.Device.Board;

namespace Iot.Device.Gpio
{
    /// <summary>
    /// An implementation of a Virtual <see cref="GpioController"/>.
    /// </summary>
    public class VirtualGpioController : GpioController
    {
        private readonly ConcurrentDictionary<int, VirtualGpioPin> _pins = new ConcurrentDictionary<int, VirtualGpioPin>();
        private readonly ConcurrentDictionary<int, PinEvent> _callbackEvents = new ConcurrentDictionary<int, PinEvent>();

        private record PinEvent(PinEventTypes PinEventTypes, PinChangeEventHandler? Callbacks);

        /// <summary>
        /// Initializes a new empty instance of the <see cref="VirtualGpioController"/> class.
        /// </summary>
        public VirtualGpioController()
            : base(new DummyGpioDriver())
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="VirtualGpioController"/> class.
        /// </summary>
        public VirtualGpioController(Dictionary<int, GpioPin> pins)
            : this()
        {
            foreach (var pin in pins)
            {
                if (!_pins.TryAdd(pin.Key, new VirtualGpioPin(pin.Value, pin.Key, this)))
                {
                    throw new ArgumentException($"Duplicate virtual pin number in collection: {pin.Key}.");
                }
            }
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="VirtualGpioController"/> class.
        /// </summary>
        public VirtualGpioController(IEnumerable<GpioPin> pins)
            : this()
        {
            int inc = 0;
            foreach (var pin in pins)
            {
                _pins.TryAdd(inc, new VirtualGpioPin(pin, inc, this));
                inc++;
            }
        }

        /// <summary>
        /// Adds a new pin number with an associated <see cref="GpioPin"/>.
        /// </summary>
        /// <param name="pinNumber">The pin number.</param>
        /// <param name="gpioPin">The <see cref="GpioPin"/> to add.</param>
        /// <returns>True on success, false if the pin number is in use already</returns>
        public bool Add(int pinNumber, GpioPin gpioPin) => _pins.TryAdd(pinNumber, new VirtualGpioPin(gpioPin, pinNumber, this));

        /// <summary>
        /// Adds a new <see cref="GpioPin"/>. The number is done automatically by adding after the last element.
        /// </summary>
        /// <param name="gpioPin">The <see cref="GpioPin"/> to add.</param>
        public void Add(GpioPin gpioPin)
        {
            // Find the last number of the pin and assign the new number to it
            int newPin = _pins.Count > 0 ? _pins.Keys.Max() + 1 : 0;
            _pins.TryAdd(newPin, new VirtualGpioPin(gpioPin, newPin, this));
        }

        /// <inheritdoc/>
        public override int PinCount => _pins.Count;

        /// <summary>
        /// Disposes this instance and removes all pins associated with this virtual controller.
        /// </summary>
        /// <param name="disposing">True to dispose all instances, false to dispose only unmanaged resources</param>
        protected override void Dispose(bool disposing)
        {
            base.Dispose(disposing);
            // We're just emptying the lists
            _pins.Clear();
        }

        /// <inheritdoc />
        protected override void OpenPinCore(int pinNumber)
        {
            // Not clear what this should do
            throw new InvalidOperationException("Cannot open a pin on the VirtualGpioController, as they're already open when constructed");
        }

        /// <summary>
        /// Closes the given pin
        /// </summary>
        /// <param name="pinNumber">The pin number</param>
        /// <exception cref="InvalidOperationException">There is no such virtual pin number registered or it was already closed</exception>
        protected override void ClosePinCore(int pinNumber)
        {
            if (!_pins.TryGetValue(pinNumber, out var pin))
            {
                throw new InvalidOperationException($"Virtual pin {pinNumber} does not exist");
            }

            pin.Close();
            _pins.TryRemove(pinNumber, out _);
        }

        /// <inheritdoc />
        public override bool IsPinOpen(int pinNumber)
        {
            return _pins.ContainsKey(pinNumber);
        }

        /// <inheritdoc/>
        public override PinMode GetPinMode(int pinNumber)
        {
            if (!IsPinOpen(pinNumber))
            {
                throw new InvalidOperationException($"Can not get a mode to pin {pinNumber} because it is not open.");
            }

            return _pins[pinNumber].GetPinMode();
        }

        /// <inheritdoc/>
        public override bool IsPinModeSupported(int pinNumber, PinMode mode)
        {
            return _pins[pinNumber].IsPinModeSupported(mode);
        }

        /// <inheritdoc/>
        public override ComponentInformation QueryComponentInformation()
        {
            ComponentInformation self = new ComponentInformation(this, "Virtual GPIO Controller");

            HashSet<GpioController> controllers = new HashSet<GpioController>();
            foreach (var pin in _pins)
            {
                controllers.Add(pin.Value.OldController);
                self.Properties.Add($"PinMapping{pin.Key}", pin.Value.OldPinNumber.ToString());
            }

            foreach (var c in controllers)
            {
                self.AddSubComponent(c.QueryComponentInformation());
            }

            return self;
        }

        /// <inheritdoc/>
        public override PinValue Read(int pinNumber)
        {
            if (!IsPinOpen(pinNumber))
            {
                throw new InvalidOperationException($"Can not read from pin {pinNumber} because it is not open.");
            }

            return _pins[pinNumber].Read();
        }

        /// <summary>
        /// Sets the mode to a pin.
        /// </summary>
        /// <param name="pinNumber">The pin number in the controller's numbering scheme.</param>
        /// <param name="mode">The mode to be set.</param>
        public override void SetPinMode(int pinNumber, PinMode mode)
        {
            if (!IsPinOpen(pinNumber))
            {
                throw new InvalidOperationException($"Can not set a mode to pin {pinNumber} because it is not open.");
            }

            if (!IsPinModeSupported(pinNumber, mode))
            {
                throw new InvalidOperationException($"Pin {pinNumber} does not support mode {mode}.");
            }

            _pins[pinNumber].SetPinMode(mode);
        }

        /// <inheritdoc/>
        public override void Write(int pinNumber, PinValue value)
        {
            if (!IsPinOpen(pinNumber))
            {
                throw new InvalidOperationException($"Can not write to pin {pinNumber} because it is not open.");
            }

            _pins[pinNumber].Write(value);
        }

        /// <inheritdoc/>
        public override void Toggle(int pinNumber)
        {
            if (!IsPinOpen(pinNumber))
            {
                throw new InvalidOperationException($"Can not read from pin {pinNumber} because it is not open.");
            }

            _pins[pinNumber].Toggle();
        }

        /// <inheritdoc/>
        public override void RegisterCallbackForPinValueChangedEvent(int pinNumber, PinEventTypes eventTypes, PinChangeEventHandler callback)
        {
            if (_callbackEvents.Count == 0)
            {
                _pins[pinNumber].ValueChanged += PinValueChanged;
            }

            _callbackEvents.TryAdd(pinNumber, new PinEvent(eventTypes, callback));
        }

        private void PinValueChanged(object sender, PinValueChangedEventArgs pinValueChangedEventArgs)
        {
            // Get GpioPin associated with the real life pin
            // Note that if we have multiple pins with the same pin number, we will only take the first one in the list
            var pins = _pins.Where(m => m.Value.PinNumber == pinValueChangedEventArgs.PinNumber);
            foreach (var pin in pins)
            {
                if (_callbackEvents.ContainsKey(pin.Key))
                {
                    var pinEvent = _callbackEvents[pin.Key];
                    if (pinEvent == null)
                    {
                        return;
                    }

                    if ((pinEvent.PinEventTypes & pinValueChangedEventArgs.ChangeType) != 0)
                    {
                        pinEvent.Callbacks?.Invoke(this, new PinValueChangedEventArgs(pinValueChangedEventArgs.ChangeType, pin.Key));
                    }

                    break;
                }
            }
        }

        /// <inheritdoc/>
        public override void UnregisterCallbackForPinValueChangedEvent(int pinNumber, PinChangeEventHandler callback)
        {
            _callbackEvents.TryRemove(pinNumber, out _);
            if (_callbackEvents.Count == 0)
            {
                _pins[pinNumber].ValueChanged -= PinValueChanged;
            }
        }

        /// <summary>
        /// Gets a reference to an open pin
        /// </summary>
        /// <param name="pinNumber">The pin number</param>
        /// <returns>A <see cref="GpioPin"/> instance representing the given pin</returns>
        /// <exception cref="InvalidOperationException">The pin number is invalid</exception>
        public GpioPin GetOpenPin(int pinNumber)
        {
            if (_pins.TryGetValue(pinNumber, out var pin))
            {
                return pin;
            }

            throw new InvalidOperationException($"No such pin: {pinNumber}");
        }

        /// <inheritdoc/>
        public override WaitForEventResult WaitForEvent(int pinNumber, PinEventTypes eventTypes, CancellationToken cancellationToken)
        {
            var pin = _pins[pinNumber];
            return pin.OldController.WaitForEvent(pin.OldPinNumber, eventTypes, cancellationToken);
        }

        /// <inheritdoc/>
        public override async ValueTask<WaitForEventResult> WaitForEventAsync(int pinNumber, PinEventTypes eventTypes, CancellationToken token)
        {
            var pin = _pins[pinNumber];
            return await pin.OldController.WaitForEventAsync(pin.OldPinNumber, eventTypes, token);
        }
    }
}
