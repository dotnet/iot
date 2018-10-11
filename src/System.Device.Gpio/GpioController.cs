// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using System.Collections.Generic;
using System.Device.Gpio;
using System.Linq;
using System.Runtime.InteropServices;

namespace System.Devices.Gpio
{
    public class GpioController : IDisposable
    {
        private IDictionary<int, GpioPin> _pins;
        private IDictionary<Tuple<int, int>, GpioPWMPin> _pwmPins;

        public GpioController(PinNumberingScheme numbering = PinNumberingScheme.Gpio)
        {
            GpioDriver driver;
            bool isLinux = RuntimeInformation.IsOSPlatform(OSPlatform.Linux);

            if (isLinux)
            {
                driver = new UnixDriver();
            }
            else if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
            {
                driver = new Windows10Driver();
            }
            else
            {
                throw new NotSupportedException($"Platform '{RuntimeInformation.OSDescription}' not supported");
            }

            Initialize(driver, numbering);
        }

        public GpioController(GpioDriver driver, PinNumberingScheme numbering = PinNumberingScheme.Gpio)
        {
            Initialize(driver, numbering);
        }

        private void Initialize(GpioDriver driver, PinNumberingScheme numbering)
        {
            Driver = driver;
            Numbering = numbering;
            _pins = new Dictionary<int, GpioPin>();
            _pwmPins = new Dictionary<Tuple<int, int>, GpioPWMPin>();

            driver.ValueChanged += OnPinValueChanged;
        }

        public void Dispose()
        {
            while (_pins.Count > 0)
            {
                GpioPin pin = _pins.Values.First();
                pin.Dispose();
            }

            while (_pwmPins.Count > 0)
            {
                GpioPWMPin pin = _pwmPins.Values.First();
                pin.Dispose();
            }

            Driver.Dispose();
        }

        internal GpioDriver Driver { get; private set; }

        public PinNumberingScheme Numbering { get; set; }

        public int PinCount => Driver.PinCount;

        public IEnumerable<GpioPin> OpenPins => _pins.Values;

        public bool IsPinOpen(int pinNumber)
        {
            int gpioNumber = Driver.ConvertPinNumber(pinNumber, Numbering, PinNumberingScheme.Gpio);
            return _pins.ContainsKey(gpioNumber);
        }

        public GpioPin this[int pinNumber]
        {
            get
            {
                int gpioNumber = Driver.ConvertPinNumber(pinNumber, Numbering, PinNumberingScheme.Gpio);
                bool isOpen = _pins.TryGetValue(gpioNumber, out GpioPin pin);

                if (!isOpen)
                {
                    throw new GpioException("The pin must be already open");
                }

                return pin;
            }
        }

        public GpioPin OpenPin(int pinNumber)
        {
            int gpioNumber = Driver.ConvertPinNumber(pinNumber, Numbering, PinNumberingScheme.Gpio);
            bool isOpen = _pins.TryGetValue(gpioNumber, out GpioPin pin);

            if (isOpen)
            {
                throw new GpioException("Pin already open");
            }

            Driver.OpenPin(gpioNumber);
            pin = new GpioPin(this, gpioNumber);
            _pins[gpioNumber] = pin;
            return pin;
        }

        public GpioPWMPin OpenPWMPin(int chip, int channel, PWMMode mode = PWMMode.Balanced, int period = 0, int dutyCycle = 0)
        {
            GpioPWMPin pin;
            bool isOpen = _pwmPins.TryGetValue(new Tuple<int, int>(chip, channel), out pin);

            if (isOpen)
            {
                throw new GpioException("PWM channel is already opened");
            }

            Driver.OpenPWMPin(chip, channel);
            pin = new GpioPWMPin(this, chip, channel, mode, period, dutyCycle);
            _pwmPins[new Tuple<int, int>(chip, channel)] = pin;
            return pin;
        }

        public void ClosePin(int pinNumber)
        {
            int gpioNumber = Driver.ConvertPinNumber(pinNumber, Numbering, PinNumberingScheme.Gpio);
            bool isOpen = _pins.TryGetValue(gpioNumber, out GpioPin pin);

            if (isOpen)
            {
                InternalClosePin(pin);
            }
        }

        public void ClosePin(GpioPin pin)
        {
            if (pin == null)
            {
                throw new ArgumentNullException(nameof(pin));
            }

            if (pin.Controller != this)
            {
                throw new ArgumentException("The given pin does not belong to this controller");
            }

            InternalClosePin(pin);
        }

        public void ClosePWMPin(GpioPWMPin pin)
        {
            if (pin == null)
            {
                throw new ArgumentNullException(nameof(pin));
            }

            if (pin.Controller != this)
            {
                throw new ArgumentException("The given PWM pin does not belong to this controller");
            }
            _pwmPins.Remove(new Tuple<int, int>(pin.Chip, pin.Channel));
            Driver.ClosePWMPin(pin.Chip, pin.Channel);
        }

        private void InternalClosePin(GpioPin pin)
        {
            int gpioNumber = pin.GpioNumber;
            _pins.Remove(gpioNumber);
            Driver.ClosePin(gpioNumber);
        }

        private void OnPinValueChanged(object sender, PinValueChangedEventArgs e)
        {
            GpioPin pin = _pins[e.GpioPinNumber];
            pin?.OnValueChanged(e);
        }
    }
}
