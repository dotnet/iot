// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using System;
using System.Device.Gpio;
using System.Device.Spi;

namespace Iot.Device.ShiftRegister
{
    /// <summary>
    /// SN74HC595 8-Bit Shift Registers With 3-State Output Registers
    /// Supports SPI and GPIO control
    /// </summary>
    public class Sn74hc595 : IDisposable
    {
        // Spec: https://www.ti.com/lit/ds/symlink/sn74hc595.pdf
        // Tutorial: https://www.youtube.com/watch?v=6fVbJbNPrEU
        // Using with SPI:
        // https://forum.arduino.cc/index.php?topic=571144.0
        // http://www.cupidcontrols.com/2013/12/turn-on-the-spi-lights-spi-output-shift-registers-and-leds/
        private readonly bool _shouldDispose;
        private readonly int _data;
        private readonly int _srclk;
        private readonly int _rclk;
        private GpioController _controller;
        private SpiDevice _spiDevice;
        private PinMapping _pinMapping;

        private int _count;

        /// <summary>
        /// Initialize a new Sn74hc595 device connected through GPIO (uses 3-5 pins)
        /// </summary>
        /// <param name="pinMapping">The pin mapping to use by the binding.</param>
        /// <param name="gpioController">The GPIO Controller used for interrupt handling.</param>
        /// <param name="shouldDispose">True (the default) if the GPIO controller shall be disposed when disposing this instance.</param>
        /// <param name="count">Count of (daisy-chained) shift registers. Default/minimum is 1.</param>
        public Sn74hc595(PinMapping pinMapping, GpioController gpioController = null,  bool shouldDispose = true, int count = 1)
        {
            pinMapping.Validate();
            if (gpioController == null)
            {
                gpioController = new GpioController();
            }

            _controller = gpioController;
            _shouldDispose = shouldDispose;
            _pinMapping = pinMapping;
            _data = _pinMapping.Data;
            _srclk = _pinMapping.SrClk;
            _rclk = _pinMapping.RClk;
            _count = count;
            SetupPins();
        }

        /// <summary>
        /// Initialize a new Sn74hc595 device connected through SPI (uses 3 pins)
        /// </summary>
        /// <param name="spiDevice">SpiDevice used for serial communication.</param>
        /// <param name="count">Count of (daisy-chained) shift registers. Default/minimum is 1.</param>
        public Sn74hc595(SpiDevice spiDevice, int count = 1)
        {
            _spiDevice = spiDevice;
            _count = count;
        }

        /// <summary>
        /// Initialize a new Sn74hc595 device connected through both SPI and GPIO (SPI for writing data; GPIO for configuration; use 4-5 pins)
        /// </summary>
        /// <param name="spiDevice">SpiDevice used for serial communication.</param>
        /// <param name="pinMapping">The pin mapping to use by the binding</param>
        /// <param name="gpioController">The GPIO Controller used for interrupt handling</param>
        /// <param name="shouldDispose">True (the default) if the GPIO controller shall be disposed when disposing this instance</param>
        /// <param name="count">Count of (daisy-chained) shift registers. Default/minimum is 1.</param>
        public Sn74hc595(SpiDevice spiDevice, PinMapping pinMapping, GpioController gpioController = null, bool shouldDispose = true, int count = 1)
                  : this(pinMapping, gpioController, shouldDispose, count)
        {
            _spiDevice = spiDevice;
        }

        /// <summary>
        /// Count of shift units.
        /// The number of (daisy-chained) shift registers. Minimum is 1.
        /// Not the count of registers on a single unit.
        /// </summary>
        public int Count => _count;

        /// <summary>
        /// Count of total bits / registers across all (daisy-chained) shift registers.
        /// Minimum is 8.
        /// </summary>
        public int Bits => _count * 8;

        /// <summary>
        /// Reports if Sn74hc595 is controlled with SPI.
        /// </summary>
        public bool UsesSpi => _spiDevice is object;

        /// <summary>
        /// Reports if Sn74hc595 is controlled with GPIO.
        /// </summary>
        public bool UsesGpio => _controller is object;

        /// <summary>
        /// Clear storage registers.
        /// </summary>
        public void ClearStorage()
        {
            if (_controller is null || _pinMapping.SrClr == 0)
            {
                throw new ArgumentNullException($"{nameof(ClearStorage)}: GpioController was not provided or {nameof(_pinMapping.SrClr)} not mapped to pin");
            }

            _controller.Write(_pinMapping.SrClr, 0);
            _controller.Write(_pinMapping.SrClr, 1);
        }

        /// <summary>
        /// Shifts zeros.
        /// Will dim all connected LEDs, for example.
        /// </summary>
        public void ShiftClear()
        {
            for (int i = 0; i < Count; i++)
            {
                ShiftByte(0);
            }
        }

        /// <summary>
        /// Shifts single value to next register
        /// Does not perform latch.
        /// </summary>
        public void Shift(PinValue value)
        {
            if (_controller is null || _pinMapping.Data == 0)
            {
                throw new ArgumentNullException($"{nameof(Shift)}: GpioController was not provided or {nameof(_pinMapping.Data)} not mapped to pin");
            }

            _controller.Write(_data, value);
            _controller.Write(_srclk, 1);
            _controller.Write(_data, 0);
            _controller.Write(_srclk, 0);
        }

        /// <summary>
        /// Shifts a byte -- 8 bits -- to the 8 registers.
        /// Pushes / overwrites any existing values.
        /// Latches by default.
        /// </summary>
        public void ShiftByte(byte value, bool latch = true)
        {
            if (_spiDevice is object)
            {
                _spiDevice.WriteByte(value);
                return;
            }

            for (int i = 0; i < 8; i++)
            {
                int data = (128 >> i) & value;
                Shift(data);
            }

            if (latch)
            {
                Latch();
            }
        }

        /// <summary>
        /// Shifts multiple bytes to multiple daisy-chained shift registers
        /// Latches by default.
        /// </summary>
        public void ShiftBytes(Sn74hc595 sr, int value, int count = 0)
        {
            if (count == 0)
            {
                count = sr.Count;
            }

            if (count == 1 || count > 4)
            {
                throw new ArgumentException($"{nameof(ShiftBytes)}: count must be  2, 3, or 4.");
            }

            for (int i = count - 1; i > 0; i--)
            {
                int shift = i * 8;
                int downShiftedValue = value >> shift;
                ShiftByte((byte)downShiftedValue);
            }

            ShiftByte((byte)value);
        }

        /// <summary>
        /// Latches values in data register.
        /// Essentially a "publish" command.
        /// </summary>
        public void Latch()
        {
            _controller.Write(_rclk, 1);
            _controller.Write(_rclk, 0);
        }

        /// <summary>
        /// Switch output register to high-impedance state.
        /// Disables current output register.
        /// </summary>
        public void OutputDisable()
        {
            if (_controller is null || _pinMapping.OE == 0)
            {
                throw new ArgumentNullException($"{nameof(OutputDisable)}: {nameof(_pinMapping.OE)} not mapped to non-zero pin value");
            }

            _controller.Write(_pinMapping.OE, 1);
        }

        /// <summary>
        /// Switch output register low-impedance state.
        /// Enables current register.
        /// </summary>
        public void OutputEnable()
        {
            if (_controller is null || _pinMapping.OE == 0)
            {
                throw new ArgumentNullException($"{nameof(OutputEnable)}: {nameof(_pinMapping.OE)} not mapped to non-zero pin value");
            }

            _controller.Write(_pinMapping.OE, 0);
        }

        /// <summary>
        /// Cleanup.
        /// Failing to dispose this class, especially when callbacks are active, may lead to undefined behavior.
        /// </summary>
        public void Dispose()
        {
            if (_shouldDispose)
            {
                _controller?.Dispose();
                _controller = null;
                _spiDevice?.Dispose();
                _spiDevice = null;
            }
        }

        private void SetupPins()
        {
            if (_spiDevice is null)
            {
                OpenPinAndWrite(_data, 0);
                OpenPinAndWrite(_rclk, 0);
                OpenPinAndWrite(_srclk, 0);
            }

            if (_pinMapping.OE > 0)
            {
                OpenPinAndWrite(_pinMapping.OE, 0);
            }

            if (_pinMapping.SrClr > 0)
            {
                OpenPinAndWrite(_pinMapping.SrClr, 1);
            }
        }

        private void OpenPinAndWrite(int pin, PinValue value)
        {
            _controller.OpenPin(pin, PinMode.Output);
            _controller.Write(pin, value);
        }

        /// <summary>
        /// Represents pin bindings for the Sn74hc595.
        /// </summary>
        public struct PinMapping
        {
            /// <param name="data">Data pin</param>
            /// <param name="oe">Output enable pin</param>
            /// <param name="rclk">Register clock pin (latch)</param>
            /// <param name="srclk">Shift register pin (shift to data register)</param>
            /// <param name="srclr">Shift register clear pin (shift register is cleared)</param>
            public PinMapping(int data, int oe, int rclk, int srclk, int srclr)
            {
                Data = data;            // data in;     SR pin 14
                OE = oe;                // blank;       SR pin 13
                RClk = rclk;            // latch;       SR pin 12
                SrClk = srclk;          // clock;       SR pin 11
                SrClr = srclr;          // clear;       SR pin 10
                                        // daisy chain  SR pin 9 (QH` not mapped; for SR -> SR communication)
            }

            /// <summary>
            /// Standard pin bindings for the Sn74hc595.
            /// </summary>
            public static PinMapping Standard => new PinMapping(25, 12, 16, 20, 21);
            /*
                Data    = 25    // data
                OE      = 12    // blank
                RClk    = 16    // latch / publish storage register
                SrClk   = 20    // storage register clock
                SrClr   = 21    // clear
            */

            /// <summary>
            /// Matching pin bindings for the Sn74hc595 (Pi and shift register pin numbers match).
            /// </summary>
            public static PinMapping Matching => new PinMapping(14, 13, 12, 11, 10);
            /*
                Data    = 14    // data
                OE      = 13    // blank
                RClk    = 12    // latch / publish storage register
                SrClk   = 11    // storage register clock
                SrClr   = 10    // clear
            */

            /// <summary>
            /// SER (data) pin number.
            /// </summary>
            public int Data { get; set; }

            /// <summary>
            /// OE (output enable) pin number.
            /// </summary>
            public int OE { get; set; }

            /// <summary>
            /// RCLK (latch) pin number.
            /// </summary>
            public int RClk { get; set; }

            /// <summary>
            /// SRCLK (shift) pin number.
            /// </summary>
            public int SrClk { get; set; }

            /// <summary>
            /// SRCLR (clear register) pin number.
            /// </summary>
            public int SrClr { get; set; }

            /// <summary>
            /// Validate that mapping is correct. Only relevant if GPIO is used as a serial protocol.
            /// Data, RClk, and SrClk must be set (non-zero). SrClr and OE are optional.
            /// </summary>
            public bool Validate(bool throwOnError = true)
            {
                if (Data > 0 &&
                    RClk > 0 &&
                    SrClk > 0)
                {
                    return true;
                }

                if (throwOnError)
                {
                    throw new ArgumentException($"{nameof(PinMapping.Validate)} -- PinMapping values should be non-zero; Values: {nameof(Data)}: {Data}; {nameof(RClk)}: {RClk}; {nameof(SrClk)}: {SrClk};.");
                }

                return false;
            }
        }
    }
}
