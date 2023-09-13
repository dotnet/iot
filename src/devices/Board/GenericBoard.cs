// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System;
using System.Collections.Generic;
using System.Device;
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
        /// <summary>
        /// Creates a generic board instance with auto-detection of the best drivers for GPIO, I2c, SPI, etc.
        /// The drivers are late-bound however, so whether it works or not can only be determined after pins are opened.
        /// </summary>
        public GenericBoard()
        {
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
        protected override I2cBusManager CreateI2cBusCore(int busNumber, int[]? pins)
        {
            return new I2cBusManager(this, busNumber, pins, I2cBus.Create(busNumber));
        }

        /// <inheritdoc />
        public override int GetDefaultI2cBusNumber()
        {
            throw new NotSupportedException("The generic board has no default I2C bus");
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
