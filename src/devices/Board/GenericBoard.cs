using System;
using System.Collections.Generic;
using System.Device.Gpio;
using System.Device.Gpio.Drivers;
using System.Device.I2c;
using System.Device.Pwm;
using System.Device.Spi;
using System.Text;

namespace Iot.Device.Board
{
    public class GenericBoard : Board
    {
        private Dictionary<int, PinUsage> _knownUsages;

        public GenericBoard(PinNumberingScheme defaultNumberingScheme)
            : base(defaultNumberingScheme)
        {
            _knownUsages = new Dictionary<int, PinUsage>();
        }

        public override int ConvertPinNumber(int pinNumber, PinNumberingScheme inputScheme, PinNumberingScheme outputScheme)
        {
            if (inputScheme == outputScheme)
            {
                return pinNumber;
            }

            throw new NotSupportedException("This board only supports logical pin numbering");
        }

        protected override SpiDevice CreateSimpleSpiDevice(SpiConnectionSettings settings, int[] pins)
        {
            return SpiDevice.Create(settings);
        }

        protected override PwmChannel CreateSimplePwmChannel(int chip, int channel, int frequency, double dutyCyclePercentage)
        {
            return PwmChannel.Create(chip, channel, frequency, dutyCyclePercentage);
        }

        protected override I2cDevice CreateSimpleI2cDevice(I2cConnectionSettings connectionSettings, int[] pins)
        {
            return I2cDevice.Create(connectionSettings);
        }

        /// <summary>
        /// Check whether the given pin is usable for the given purpose.
        /// This implementation always returns unknown, since the generic board requires the user to know what he's doing.
        /// </summary>
        public override AlternatePinMode GetHardwareModeForPinUsage(int pinNumber, PinUsage usage, PinNumberingScheme pinNumberingScheme = PinNumberingScheme.Logical, int bus = 0)
        {
            return AlternatePinMode.Unknown;
        }

        protected override void ActivatePinMode(int pinNumber, PinUsage usage)
        {
            _knownUsages[pinNumber] = usage;
            base.ActivatePinMode(pinNumber, usage);
        }

        public override PinUsage DetermineCurrentPinUsage(int pinNumber)
        {
            PinUsage usage;
            if (_knownUsages.TryGetValue(pinNumber, out usage))
            {
                return usage;
            }

            // The generic board only knows the usage if it has been explicitly set before
            return PinUsage.Unknown;
        }

        public override int[] GetDefaultPinAssignmentForI2c(I2cConnectionSettings connectionSettings)
        {
            throw new NotSupportedException("For the generic board, you need to specify the pins to use for I2C");
        }

        public override int GetDefaultPinAssignmentForPwm(int chip, int channel)
        {
            throw new NotSupportedException("For the generic board, you need to specify the pin to use for pwm");
        }

        public override int[] GetDefaultPinAssignmentForSpi(SpiConnectionSettings connectionSettings)
        {
            throw new NotSupportedException("For the generic board, you need to specify the pins to use for SPI");
        }
    }
}
