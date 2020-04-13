using System;
using System.Collections.Generic;
using System.Device.Analog;
using System.Device.Gpio;
using System.Device.Gpio.Drivers;
using System.Device.I2c;
using System.Device.Pwm;
using System.Device.Pwm.Channels;
using System.Device.Spi;
using System.Text;

namespace System.Device.Boards
{
    /// <summary>
    /// A generic board for Unix platforms
    /// </summary>
    public class UnixBoard : Board
    {
        private GpioDriver _internalDriver;
        private bool _useLibgpiod;

        public UnixBoard(PinNumberingScheme defaultNumberingScheme, bool useLibgpiod = true)
            : base(defaultNumberingScheme)
        {
            _internalDriver = null;
            _useLibgpiod = useLibgpiod;
        }

        /// <summary>
        /// True if the Libgpiod driver is used, false if SysFs is used.
        /// Returns false until <seealso cref="Initialize"/> is called.
        /// </summary>
        public bool LibGpiodDriverUsed
        {
            get
            {
                return _internalDriver is LibGpiodDriver;
            }
        }

        public override GpioController CreateGpioController(PinNumberingScheme pinNumberingScheme)
        {
            var driver = CreateGpioDriver();
            return new GpioController(DefaultPinNumberingScheme, driver, this);
        }

        protected virtual GpioDriver CreateGpioDriver()
        {
            if (_useLibgpiod == false)
            {
                return new SysFsDriver(this);
            }

            UnixDriver driver = null;
            try
            {
                driver = new LibGpiodDriver(this);
            }
            catch (PlatformNotSupportedException)
            {
                driver = new SysFsDriver(this);
            }

            return driver;
        }

        public override int ConvertPinNumberToLogicalNumberingScheme(int pinNumber)
        {
            return pinNumber;
        }

        public override int ConvertLogicalNumberingSchemeToPinNumber(int pinNumber)
        {
            return pinNumber;
        }

        public override void Initialize()
        {
            base.Initialize();

            if (Environment.OSVersion.Platform != PlatformID.Unix)
            {
                // Something is really wrong here.
                throw new PlatformNotSupportedException("This board type is only supported on linux/unix.");
            }

            // Try to create a GpioController - if that succeeds, we're probably on compatible hardware
            try
            {
                UnixDriver driver = UnixDriver.Create(this);
                driver.Initialize();
                _internalDriver = driver;
            }
            catch (Exception x) when (!(x is NullReferenceException))
            {
                throw new PlatformNotSupportedException($"Unable to open GPIO device: {x.Message}", x);
            }
        }

        public override I2cDevice CreateI2cDevice(I2cConnectionSettings connectionSettings)
        {
            return new UnixI2cDevice(connectionSettings, this);
        }

        public override SpiDevice CreateSpiDevice(SpiConnectionSettings settings)
        {
            return new UnixSpiDevice(settings, this);
        }

        public override PwmChannel CreatePwmChannel(int chip, int channel, int frequency = 400, double dutyCyclePercentage = 0.5)
        {
            return new UnixPwmChannel(this, chip, channel, frequency, dutyCyclePercentage);
        }

        public override AnalogController CreateAnalogController(int chip)
        {
            throw new NotSupportedException("The Raspberry Pi has no on-board analog inputs.");
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                if (_internalDriver != null)
                {
                    _internalDriver.Dispose();
                    _internalDriver = null;
                }
            }

            base.Dispose(disposing);
        }
    }
}
