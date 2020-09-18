// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System;
using System.Collections.Generic;
using System.Device.Gpio;
using System.Device.Spi;

namespace Iot.Device.Multiplexing
{
    /// <summary>
    /// MBI5027 16-Bit shift register With 3-State output register and error detection.
    /// Supports SPI and GPIO control.
    /// </summary>
    public class Mbi5027 : ShiftRegister
    {
        // Datasheet: http://archive.fairchip.com/pdf/MACROBLOCK/MBI5027.pdf
        private Mbi5027PinMapping _pinMapping;

        /// <summary>
        /// Initialize a new shift register connected through GPIO.
        /// </summary>
        /// <param name="pinMapping">The pin mapping to use by the binding.</param>
        /// <param name="bitLength">Bit length of register, including chained registers. Default is 16 bits.</param>
        /// <param name="gpioController">The GPIO Controller used for interrupt handling.</param>
        /// <param name="shouldDispose">Option (true by default) to dispose the GPIO controller when disposing this instance.</param>
        public Mbi5027(Mbi5027PinMapping pinMapping, int bitLength = 16, GpioController gpioController = null,  bool shouldDispose = true)
        : base(new ShiftRegisterPinMapping(pinMapping.Sdi, pinMapping.Clk, pinMapping.LE, pinMapping.OE), bitLength, gpioController, shouldDispose)
        {
            _pinMapping = pinMapping;
            SetupPins();
        }

        /// <summary>
        /// Initializes a new shift register device connected through SPI.
        /// Uses 3 pins (SDI -> SDI, SCLK -> SCLK, CE0 -> LE)
        /// </summary>
        /// <param name="spiDevice">SpiDevice used for serial communication.</param>
        /// <param name="bitLength">Bit length of register, including chained registers. Default is 8 bits.</param>
        public Mbi5027(SpiDevice spiDevice, int bitLength = 16)
        : base(spiDevice, bitLength)
        {
        }

        /// <summary>
        /// Enable open/short eror detection mode.
        /// Requires use of GPIO controller.
        /// </summary>
        public void EnableDetectionMode()
        {
            if (GpioController is null || _pinMapping.Clk < 0 || _pinMapping.OE < 0 || _pinMapping.LE < 0)
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
        }

        /// <summary>
        /// Read output error status.
        /// Requires use of GPIO controller.
        /// </summary>
        public IEnumerable<PinValue> ReadOutputErrorStatus()
        {
            /*  Required timing waveform

            **Phase 1: Load test data
            n = BitLength (continue to n; 16 in the default case)
            L = latch
                  1   2   3   n   L
            CLK _↑‾|_↑‾|_↑‾|_↑‾|_↑‾|

            OE  ‾‾‾‾‾‾‾‾‾‾‾‾‾‾‾‾‾‾‾‾
                 1   1   1   1   1
            SDI ‾‾‾‾‾‾‾‾‾‾‾‾‾‾‾‾‾|__
                 1   1   1   1   0
            LE  _________________↑‾|
                 0   0   0   0   1

            **Phase 2: Signal to detect error status
                  1   2   3
            CLK _↑‾|_↑‾|_↑‾|
            OE  ___________
                 0   0   0

            **Phase 3: Read status for each output (normal or error codes)
                  1   2   3   n
            CLK _↑‾|_↑‾|_↑‾|_↑‾|

            OE  ‾‾‾‾‾‾‾‾‾‾‾‾‾‾‾‾
                 1   1   1   1
            SDO  ?   ?   ?   ?
            Read error codes from SDO -- starts with BitLength - 1
            */

            // Phase 1: Load test data
            // n cycles with OE high
            for (int i = 0; i < BitLength; i++)
            {
                ShiftBit(1);
            }

            // latch test data
            Latch();

            // Phase 2: Signal to detect error status
            // three clock cycles, with OE low
            GpioController.Write(_pinMapping.OE, 0);
            for (int i = 0; i < 3; i++)
            {
                GpioController.Write(_pinMapping.Clk, 1);
                GpioController.Write(_pinMapping.Clk, 0);
            }

            // Phase 3: Read status for each output (normal or error codes)
            // read error codes from SDO, with OE high
            GpioController.Write(_pinMapping.OE, 1);
            for (int i = 0; i < BitLength; i++)
            {
                PinValue sdo = GpioController.Read(_pinMapping.Sdo);
                yield return sdo;
                GpioController.Write(_pinMapping.Clk, 1);
                GpioController.Write(_pinMapping.Clk, 0);
            }
        }

        /// <summary>
        /// Enable normal mode.
        /// Requires use of GPIO controller.
        /// </summary>
        public void EnableNormalMode()
        {
            if (GpioController is null || _pinMapping.Clk < 0 || _pinMapping.OE < 0 || _pinMapping.LE < 0)
            {
                throw new ArgumentNullException($"{nameof(EnableDetectionMode)}: GpioController was not provided or {nameof(_pinMapping.Clk)}, {nameof(_pinMapping.LE)}, or {nameof(_pinMapping.OE)} not mapped to pin");
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

        private void SetupPins()
        {
            if (_pinMapping.Sdo >= 0)
            {
                GpioController.OpenPin(_pinMapping.Sdo, PinMode.Input);
            }
        }
    }
}
