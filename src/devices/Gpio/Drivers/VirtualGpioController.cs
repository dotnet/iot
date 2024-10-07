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
            if (numberingScheme != PinNumberingScheme.Logical)
            {
                throw new ArgumentException("Only PinNumberingScheme Logical is supported.");
            }
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
        /// This removes the pin for the virtual controller. It does not close the pin as the pin has been opened by another controller.
        /// </summary>
        /// <param name="pinNumber">The pin number in the controller's numbering scheme.</param>
        public override void ClosePin(int pinNumber)
        {
            if (!IsPinOpen(pinNumber))
            {
                throw new InvalidOperationException($"Can not close pin {pinNumber} because it is not open.");
            }

            GpioPins.TryRemove(pinNumber, out _);
        }

        /// <summary>
        /// Disposes this instance and removes all pins associated with this virtual controller.
        /// </summary>
        /// <param name="disposing">True to dispose all instances, false to dispose only unmanaged resources</param>
        protected override void Dispose(bool disposing)
        {
            foreach (int pin in GpioPins.Keys)
            {
                // The list contains the pin in the current NumberingScheme
                ClosePin(pin);
            }

            // We're just emptying the lists
            _pins.Clear();
            GpioPins.Clear();
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

        /// <summary>
        /// Opens a pin in order for it to be ready to use.
        /// The driver attempts to open the pin without changing its mode or value.
        /// </summary>
        /// <param name="pinNumber">The pin number in the controller's numbering scheme.</param>
        public override GpioPin OpenPin(int pinNumber)
        {
            if (IsPinOpen(pinNumber))
            {
                return GpioPins[pinNumber];
            }

            VirtualGpioPin pin = new VirtualGpioPin(_pins[pinNumber], pinNumber);

            GpioPins.TryAdd(pinNumber, pin);
            return GpioPins[pinNumber];
        }

        /// <inheritdoc/>
        public override ComponentInformation QueryComponentInformation()
        {
            ComponentInformation self = new ComponentInformation(this, "Virtual GPIO Controller");

            foreach (var cp in self.SubComponents)
            {
                self.AddSubComponent(cp);
            }

            self.Properties["OpenPins"] = string.Join(", ", GpioPins.Select(x => x.Key));

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
