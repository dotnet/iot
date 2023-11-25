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

namespace Iot.Device.Gpio
{
    /// <summary>
    /// An implementation of a Virtual <see cref="GpioController"/>.
    /// </summary>
    public class VirtualGpioController : GpioController
    {
        private readonly ConcurrentDictionary<int, GpioPin> _pins = new ConcurrentDictionary<int, GpioPin>();
        private readonly ConcurrentDictionary<int, VirtualGpioPin> _openPins = new ConcurrentDictionary<int, VirtualGpioPin>();
        private readonly ConcurrentDictionary<int, PinEvent> _callbackEvents = new ConcurrentDictionary<int, PinEvent>();

        private record PinEvent(PinEventTypes PinEventTypes, PinChangeEventHandler? Callbacks);

        /// <summary>
        /// Initializes a new instance of the <see cref="VirtualGpioController"/> class.
        /// </summary>
        public VirtualGpioController()
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="VirtualGpioController"/> class.
        /// </summary>
        /// <param name="numberingScheme">The numbering scheme used to represent pins provided by the controller.</param>
        public VirtualGpioController(PinNumberingScheme numberingScheme)
        {
            // Nothing on purpose, as only logical is suported
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="VirtualGpioController"/> class.
        /// </summary>
        /// <param name="numberingScheme">The numbering scheme used to represent pins provided by the controller.</param>
        /// <param name="driver">The driver that manages all of the pin operations for the controller.</param>
        public VirtualGpioController(PinNumberingScheme numberingScheme, GpioDriver driver)
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="VirtualGpioController"/> class.
        /// </summary>
        public VirtualGpioController(Dictionary<int, GpioPin> pins)
        {
            foreach (var pin in pins)
            {
                _pins.TryAdd(pin.Key, pin.Value);
            }
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="VirtualGpioController"/> class.
        /// </summary>
        public VirtualGpioController(IEnumerable<GpioPin> pins)
        {
            int inc = 0;
            foreach (var pin in pins)
            {
                _pins.TryAdd(inc++, pin);
            }
        }

        /// <summary>
        /// Adds a new pin number with an associated <see cref="GpioPin"/>.
        /// </summary>
        /// <param name="pinNumber">The pin number.</param>
        /// <param name="gpioPin">The <see cref="GpioPin"/> to add.</param>
        public void Add(int pinNumber, GpioPin gpioPin) => _pins.TryAdd(pinNumber, gpioPin);

        /// <summary>
        /// Adds a new <see cref="GpioPin"/>. The number is done automatically by adding after the last element.
        /// </summary>
        /// <param name="gpioPin">The <see cref="GpioPin"/> to add.</param>
        public void Add(GpioPin gpioPin)
        {
            // Find the last number of the pin and assign the new number to it
            int newPin = _pins.Count > 0 ? _pins.Keys.Max() + 1 : 0;
            _pins.TryAdd(newPin, gpioPin);
        }

        /// <inheritdoc/>
        public override int PinCount => _pins.Count;

        /// <summary>
        /// Closes an open pin.
        /// If allowed by the driver, the state of the pin is not changed.
        /// </summary>
        /// <param name="pinNumber">The pin number in the controller's numbering scheme.</param>
        public new void ClosePin(int pinNumber)
        {
            if (!IsPinOpen(pinNumber))
            {
                throw new InvalidOperationException($"Can not close pin {pinNumber} because it is not open.");
            }

            _openPins.TryRemove(pinNumber, out _);
        }

        /// <summary>
        /// Disposes this instance and closes all open pins associated with this controller.
        /// </summary>
        public new void Dispose()
        {
            foreach (int pin in _openPins.Keys)
            {
                // The list contains the pin in the current NumberingScheme
                ClosePin(pin);
            }

            // We're just emptying the lists
            _pins.Clear();
            _openPins.Clear();
        }

        /// <inheritdoc/>
        public override PinMode GetPinMode(int pinNumber)
        {
            if (!IsPinOpen(pinNumber))
            {
                throw new InvalidOperationException($"Can not set a mode to pin {pinNumber} because it is not open.");
            }

            return _pins[pinNumber].GetPinMode();
        }

        /// <inheritdoc/>
        public override bool IsPinModeSupported(int pinNumber, PinMode mode)
        {
            return _pins[pinNumber].IsPinModeSupported(mode);
        }

        /// <summary>
        /// Checks if a specific pin is open.
        /// </summary>
        /// <param name="pinNumber">The pin number in the controller's numbering scheme.</param>
        /// <returns>The status if the pin is open or closed.</returns>
        public new bool IsPinOpen(int pinNumber)
        {
            return _openPins.ContainsKey(pinNumber);
        }

        /// <summary>
        /// Opens a pin and sets it to a specific mode.
        /// </summary>
        /// <param name="pinNumber">The pin number in the controller's numbering scheme.</param>
        /// <param name="mode">The mode to be set.</param>
        public new GpioPin OpenPin(int pinNumber, PinMode mode)
        {
            var pin = OpenPin(pinNumber);
            _pins[pinNumber].SetPinMode(mode);
            return pin;
        }

        /// <summary>
        /// Opens a pin and sets it to a specific mode and value.
        /// </summary>
        /// <param name="pinNumber">The pin number in the controller's numbering scheme.</param>
        /// <param name="mode">The mode to be set.</param>
        /// <param name="initialValue">The initial value to be set if the mode is output. The driver will attempt to set the mode without causing glitches to the other value.
        /// (if <paramref name="initialValue"/> is <see cref="PinValue.High"/>, the pin should not glitch to low during open)</param>
        public new GpioPin OpenPin(int pinNumber, PinMode mode, PinValue initialValue)
        {
            var pin = OpenPin(pinNumber, mode);
            _pins[pinNumber].Write(initialValue);
            return pin;
        }

        /// <summary>
        /// Opens a pin in order for it to be ready to use.
        /// The driver attempts to open the pin without changing its mode or value.
        /// </summary>
        /// <param name="pinNumber">The pin number in the controller's numbering scheme.</param>
        public new GpioPin OpenPin(int pinNumber)
        {
            if (IsPinOpen(pinNumber))
            {
                return _openPins[pinNumber];
            }

            _openPins.TryAdd(pinNumber, new VirtualGpioPin(_pins[pinNumber], pinNumber));
            return _openPins[pinNumber];
        }

        /// <inheritdoc/>
        public override ComponentInformation QueryComponentInformation()
        {
            ComponentInformation self = new ComponentInformation(this, "Virtual GPIO Controller");

            // PinCount is not added on purpose, because the property throws NotSupportedException on some hardware
            self.Properties["OpenPins"] = string.Join(", ", _openPins.Select(x => x.Key));

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

                    if (pinEvent.PinEventTypes == pinValueChangedEventArgs.ChangeType)
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

        /// <inheritdoc/>
        public override WaitForEventResult WaitForEvent(int pinNumber, PinEventTypes eventTypes, CancellationToken cancellationToken)
        {
            throw new NotImplementedException();
        }

        /// <inheritdoc/>
        public override ValueTask<WaitForEventResult> WaitForEventAsync(int pinNumber, PinEventTypes eventTypes, CancellationToken token)
        {
            throw new NotImplementedException();
        }
    }
}
