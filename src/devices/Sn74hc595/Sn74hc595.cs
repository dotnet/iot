// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System;
using System.Device.Gpio;
using System.Device.Spi;

namespace Iot.Device.Multiplexing
{
    /// <summary>
    /// SN74HC595 8-Bit Shift Registers With 3-State Output Registers
    /// Supports SPI and GPIO control
    /// </summary>
    public class Sn74hc595 : ShiftRegister
    {
        // Datasheet: https://www.ti.com/lit/ds/symlink/sn74hc595.pdf
        private Sn74hc595PinMapping _pinMapping;

        /// <summary>
        /// Initialize a new shift register connected through GPIO.
        /// </summary>
        /// <param name="pinMapping">The pin mapping to use by the binding.</param>
        /// <param name="bitLength">Bit length of register, including chained registers. Default is 8 bits.</param>
        /// <param name="gpioController">The GPIO Controller used for interrupt handling.</param>
        /// <param name="shouldDispose">True (the default) if the GPIO controller shall be disposed when disposing this instance.</param>
        public Sn74hc595(Sn74hc595PinMapping pinMapping, int bitLength = 8, GpioController? gpioController = null,  bool shouldDispose = true)
        : base(new ShiftRegisterPinMapping(pinMapping.Ser, pinMapping.SrClk, pinMapping.RClk, pinMapping.OE), bitLength, gpioController, shouldDispose)
        {
            _pinMapping = pinMapping;
            SetupPins();
        }

        /// <summary>
        /// Clear storage register.
        /// Requires use of GPIO controller.
        /// </summary>
        public void ClearStorage(bool latch = true)
        {
            if (GpioController is null || _pinMapping.SrClr == 0)
            {
                throw new Exception($"{nameof(ClearStorage)}: GpioController was not provided or {nameof(_pinMapping.SrClr)} not mapped to pin");
            }

            GpioController.Write(_pinMapping.SrClr, 0);
            GpioController.Write(_pinMapping.SrClr, 1);

            if (latch)
            {
                Latch();
            }
        }

        private void SetupPins()
        {
            if (_pinMapping.SrClr > 0 && GpioController is object)
            {
                GpioController.OpenPin(_pinMapping.SrClr, PinMode.Output);
                GpioController.Write(_pinMapping.SrClr, 1);
            }
        }
    }
}
