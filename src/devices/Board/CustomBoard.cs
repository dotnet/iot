using System;
using System.Collections.Generic;
using System.Device.Gpio;
using System.Device.I2c;
using System.Device.Pwm;
using System.Device.Spi;
using System.Text;

namespace Iot.Device.Board
{
    /// <summary>
    /// A board that can be customized with user-specified drivers.
    /// This should only be used if the drivers can't be auto-detected properly.
    /// </summary>
    public class CustomBoard : GenericBoard
    {
        private readonly GpioDriver _gpioDriver;
        private readonly Func<int, I2cBus> _i2CBusCreator;
        private readonly Func<SpiConnectionSettings, SpiDevice> _spiDeviceCreator;
        private readonly Func<int, PwmChannel> _pwmChannelCreator;

        /// <summary>
        /// Creates a new custom board.
        /// </summary>
        /// <param name="gpioDriver">GPIO driver to use</param>
        /// <param name="i2cBusCreator">Function to create an I2C bus instance</param>
        /// <param name="spiDeviceCreator">Function to create an SPI device</param>
        /// <param name="pwmChannelCreator">Function to create a PWM channel</param>
        public CustomBoard(GpioDriver gpioDriver, Func<int, I2cBus> i2cBusCreator,
            Func<SpiConnectionSettings, SpiDevice> spiDeviceCreator, Func<int, PwmChannel> pwmChannelCreator)
        : base(PinNumberingScheme.Logical)
        {
            _gpioDriver = gpioDriver;
            _i2CBusCreator = i2cBusCreator;
            _spiDeviceCreator = spiDeviceCreator;
            _pwmChannelCreator = pwmChannelCreator;
        }

        /// <inheritdoc />
        public override int ConvertPinNumber(int pinNumber, PinNumberingScheme inputScheme, PinNumberingScheme outputScheme)
        {
            if (inputScheme != outputScheme)
            {
                throw new NotSupportedException("Only logical pin numbering supported");
            }

            return pinNumber;
        }

        /// <summary>
        /// Returns the GPIO driver
        /// </summary>
        /// <returns></returns>
        protected override GpioDriver? TryCreateBestGpioDriver()
        {
            return _gpioDriver ?? throw new NotSupportedException("No GPIO Driver specified");
        }

        /// <inheritdoc />
        protected override I2cBusManager CreateI2cBusCore(int busNumber, int[]? pins)
        {
            return new I2cBusManager(this, busNumber, pins, _i2CBusCreator(busNumber));
        }

        /// <inheritdoc />
        public override int GetDefaultI2cBusNumber()
        {
            throw new NotSupportedException("The custom board does not have a default bus number");
        }

        /// <inheritdoc />
        protected override SpiDevice CreateSimpleSpiDevice(SpiConnectionSettings settings, int[] pins)
        {
            return _spiDeviceCreator(settings);
        }

        /// <inheritdoc />
        protected override PwmChannel CreateSimplePwmChannel(int chip, int channel, int frequency, double dutyCyclePercentage)
        {
            return _pwmChannelCreator(channel);
        }
    }
}
