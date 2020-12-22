// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

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
    /// <summary>
    /// A generic board class. Uses generic implementations for GPIO, I2C etc
    /// </summary>
    public class GenericBoard : Board
    {
        private Dictionary<int, PinUsage> _knownUsages;

        /// <summary>
        /// Creates a generic board instance
        /// </summary>
        /// <param name="defaultNumberingScheme">Default pin numbering scheme</param>
        public GenericBoard(PinNumberingScheme defaultNumberingScheme)
            : base(defaultNumberingScheme)
        {
            _knownUsages = new Dictionary<int, PinUsage>();
        }

        /// <inheritdoc />
        public override int ConvertPinNumber(int pinNumber, PinNumberingScheme inputScheme, PinNumberingScheme outputScheme)
        {
            if (inputScheme == outputScheme)
            {
                return pinNumber;
            }

            throw new NotSupportedException("This board only supports logical pin numbering");
        }

        /// <inheritdoc />
        protected override SpiDevice CreateSimpleSpiDevice(SpiConnectionSettings settings, int[] pins)
        {
            return SpiDevice.Create(settings);
        }

        /// <inheritdoc />
        protected override PwmChannel CreateSimplePwmChannel(int chip, int channel, int frequency, double dutyCyclePercentage)
        {
            return PwmChannel.Create(chip, channel, frequency, dutyCyclePercentage);
        }

        /// <inheritdoc />
        protected override I2cDevice CreateI2cDeviceCore(I2cConnectionSettings connectionSettings)
        {
            return I2cDevice.Create(connectionSettings);
        }

        /// <inheritdoc />
        public override I2cBusManager CreateOrGetDefaultI2cBus()
        {
            throw new NotSupportedException("For the generic board, you need to specify the pins to use for I2C by explicitly specifying them");
        }

        /// <summary>
        /// Check whether the given pin is usable for the given purpose.
        /// This implementation always returns unknown, since the generic board requires the user to know what he's doing.
        /// </summary>
        public override AlternatePinMode GetHardwareModeForPinUsage(int pinNumber, PinUsage usage, PinNumberingScheme pinNumberingScheme = PinNumberingScheme.Logical, int bus = 0)
        {
            return AlternatePinMode.Unknown;
        }

        /// <inheritdoc />
        protected override void ActivatePinMode(int pinNumber, PinUsage usage)
        {
            int pinNumber2 = RemapPin(pinNumber, DefaultPinNumberingScheme);
            _knownUsages[pinNumber2] = usage;
            base.ActivatePinMode(pinNumber, usage);
        }

        /// <inheritdoc />
        public override PinUsage DetermineCurrentPinUsage(int pinNumber)
        {
            PinUsage usage;
            pinNumber = RemapPin(pinNumber, DefaultPinNumberingScheme);
            if (_knownUsages.TryGetValue(pinNumber, out usage))
            {
                return usage;
            }

            // The generic board only knows the usage if it has been explicitly set before
            return PinUsage.Unknown;
        }

        /// <inheritdoc />
        public override int[] GetDefaultPinAssignmentForI2c(int busId)
        {
            throw new NotSupportedException("For the generic board, you need to specify the pins to use for I2C");
        }

        /// <inheritdoc />
        public override int GetDefaultPinAssignmentForPwm(int chip, int channel)
        {
            throw new NotSupportedException("For the generic board, you need to specify the pin to use for pwm");
        }

        /// <inheritdoc />
        public override int[] GetDefaultPinAssignmentForSpi(SpiConnectionSettings connectionSettings)
        {
            throw new NotSupportedException("For the generic board, you need to specify the pins to use for SPI");
        }
    }
}
