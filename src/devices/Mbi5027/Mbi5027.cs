// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using System;
using System.Device.Gpio;
using System.Device.Spi;

namespace Iot.Device.Multiplexing
{
    /// <summary>
    /// MBI5027 16-Bit Shift Registers With 3-State Output Registers
    /// Supports SPI and GPIO control
    /// </summary>
    public class Mbi5027 : ShiftRegister
    {
        // Datasheet: http://archive.fairchip.com/pdf/MACROBLOCK/MBI5027.pdf
        private Mbi5027PinMapping _pinMapping;

        /// <summary>
        /// Initialize a new shift register connected through GPIO.
        /// </summary>
        /// <param name="pinMapping">The pin mapping to use by the binding.</param>
        /// <param name="bitLength">Bit length of register, including chained registers. Default is 8 bits.</param>
        /// <param name="gpioController">The GPIO Controller used for interrupt handling.</param>
        /// <param name="shouldDispose">True (the default) if the GPIO controller shall be disposed when disposing this instance.</param>
        public Mbi5027(Mbi5027PinMapping pinMapping, int bitLength = 8, GpioController gpioController = null,  bool shouldDispose = true)
        : base(new ShiftRegisterPinMapping(pinMapping.Sdi, pinMapping.OE, pinMapping.LE, pinMapping.Clk), bitLength, gpioController, shouldDispose)
        {
            _pinMapping = pinMapping;
        }

        /// <summary>
        /// Initialize a new shift register device connected through SPI.
        /// Uses 3 pins (MOSI -> Data, SCLK -> SCLK, CE0 -> RCLK)
        /// </summary>
        /// <param name="spiDevice">SpiDevice used for serial communication.</param>
        /// <param name="bitLength">Bit length of register, including chained registers. Default is 8 bits.</param>
        public Mbi5027(SpiDevice spiDevice, int bitLength = 8)
        : base(spiDevice, bitLength)
        {
        }

        /// <summary>
        /// Clear storage registers.
        /// Requires use of GPIO controller.
        /// </summary>
        public void EnableDetectionMode()
        {
            if (GpioController is null || _pinMapping.Clk == 0 || _pinMapping.OE == 0 || _pinMapping.LE == 0)
            {
                throw new ArgumentNullException($"{nameof(EnableDetectionMode)}: GpioController was not provided or {nameof(_pinMapping.Clk)}, {nameof(_pinMapping.LE)}, or {nameof(_pinMapping.OE)} not mapped to pin");
            }

            /*  Required timing waveform
                  1   2   3   4   5
            CLK _↑‾|_↑‾|_↑‾|_↑‾|_↑‾|
            
            OE  ‾‾‾|___|‾‾‾‾‾‾‾‾‾‾‾‾
                 1   0   1   1   1 
            LE  ___________|‾‾‾|____
                 0   0   0   1   0
            */

            ChangeModeSignal(1, 0);
            ChangeModeSignal(0, 0);
            ChangeModeSignal(1, 0);
            ChangeModeSignal(1, 1);
            ChangeModeSignal(1, 0);
            GpioController.Write(_pinMapping.OE, 0);
        }

        /// <summary>
        /// Clear storage registers.
        /// Requires use of GPIO controller.
        /// </summary>
        public void EnableNormalMode()
        {
            if (GpioController is null || _pinMapping.Clk == 0 || _pinMapping.OE == 0 || _pinMapping.LE == 0)
            {
                throw new ArgumentNullException($"{nameof(EnableDetectionMode)}: GpioController was not provided or {nameof(_pinMapping.SrClk)}, {nameof(_pinMapping.RClk)}, or {nameof(_pinMapping.OE)} not mapped to pin");
            }

            /*  Required timing waveform
                  1   2   3   4   5
            CLK _↑‾|_↑‾|_↑‾|_↑‾|_↑‾|
            
            OE  ‾‾‾|___|‾‾‾‾‾‾‾‾‾‾‾‾
                 1   0   1   1   1 
            LE  ____________________
                 0   0   0   0   0
            */

            ChangeModeSignal(1, 0);
            ChangeModeSignal(0, 0);
            ChangeModeSignal(1, 0);
            ChangeModeSignal(1, 0);
            ChangeModeSignal(1, 0);
            GpioController.Write(_pinMapping.OE, 0);
        }

        private void ChangeModeSignal(PinValue oe, PinValue le)
        {
            GpioController.Write(_pinMapping.OE, oe);
            GpioController.Write(_pinMapping.LE, le);
            GpioController.Write(_pinMapping.Clk, 1);
            GpioController.Write(_pinMapping.Clk, 0);
        }

        /// <summary>
        /// Clear storage registers.
        /// Requires use of GPIO controller.
        /// </summary>
        public void ReadErrorStatus()
        {
            /*  Required timing waveform
                  1   2   3   4   5   6   7   8   9   10
            CLK _↑‾|_↑‾|_↑‾|_↑‾|_↑‾|_↑‾|_↑‾|_↑‾|_↑‾|_↑‾|⋯
            
            OE  ‾‾‾|___________________|‾‾‾‾‾‾‾‾‾‾‾‾‾‾‾‾⋯
                 1   0   0   0   0   0   1   1   1   1  ⋯
            SDO                          Read error codes starting with bit 15
            */

            // one clock cycle with OE high
            GpioController.Write(_pinMapping.OE, 1);
            GpioController.Write(_pinMapping.Clk, 1);
            GpioController.Write(_pinMapping.Clk, 0);

            // 
        }


    }
}
