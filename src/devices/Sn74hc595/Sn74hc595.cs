using System;
using System.Device.Gpio;

namespace Iot.Device.ShiftRegister
{
    /// <summary>
    /// SN74HC595 8-Bit Shift Registers With 3-State Output Registers
    /// </summary>
    public class Sn74hc595
    {
        // Spec: https://www.ti.com/lit/ds/symlink/sn74hc595.pdf
        // Tutorial: https://www.youtube.com/watch?v=6fVbJbNPrEU
        private GpioController _controller;
        private bool _shouldDispose;
        private PinMapping _mapping;
        private int _ser;
        private int _srclk;
        private int _rclk;

        private int _count;

        /// <summary>
        /// Initialize a new Sn74hc595 device connected through GPIO (up to 5 pins)
        /// </summary>
        /// <param name="pinMapping">The pin mapping to use by the binding</param>
        /// <param name="gpioController">The GPIO Controller used for interrupt handling</param>
        /// <param name="shouldDispose">True (the default) if the GPIO controller shall be disposed when disposing this instance</param>
        /// <param name="count">Count of (daisy-chained) shift registers. Minimum is 1.</param>
        public Sn74hc595(PinMapping pinMapping, GpioController gpioController = null,  bool shouldDispose = true, int count = 1)
        {
            if (gpioController == null)
            {
                gpioController = new GpioController();
            }

            _controller = gpioController;
            _shouldDispose = shouldDispose;
            if (!pinMapping.Validate())
            {
                throw new ArgumentException(nameof(Sn74hc595));
            }

            _mapping = pinMapping;
            _ser = _mapping.SER;
            _srclk = _mapping.SRCLK;
            _rclk = _mapping.RCLK;
            _count = count;
            Setup();
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
        /// Cleanup.
        /// Failing to dispose this class, especially when callbacks are active, may lead to undefined behavior.
        /// </summary>
        public void Dispose()
        {
            if (_shouldDispose)
            {
                _controller?.Dispose();
                _controller = null;
            }
        }

        /// <summary>
        /// Clear storage registers.
        /// </summary>
        public void ClearStorage()
        {
            if (_mapping.SRCLR > 0)
            {
                _controller.WriteValuesToPin(_mapping.SRCLR, 0, 1);
            }
            else
            {
                throw new ArgumentNullException($"{nameof(ClearStorage)}: {nameof(_mapping.SRCLR)} not mapped to non-zero pin value");
            }
        }

        /// <summary>
        /// Shift zeros and latch.
        /// Will dim all connected LEDs, for example.
        /// </summary>
        public void ShiftClear()
        {
            for (int i = 0; i < Bits; i++)
            {
                Shift(0);
            }

            Latch();
        }

        /// <summary>
        /// Shift single value to next register
        /// Does not perform latch.
        /// </summary>
        public void Shift(int value)
        {
            _controller.Write(_ser, value);
            _controller.Write(_srclk, 1);
            _controller.WriteValueToPins(0, _ser, _srclk);
        }

        /// <summary>
        /// Latches values in registers.
        /// Essentially a "publish" command.
        /// </summary>
        public void Latch()
        {
            _controller.WriteValuesToPin(_rclk, 1, 0);
        }

        /// <summary>
        /// Shift a byte -- 8 bits -- to the 8 registers.
        /// Pushes / overwrites any existing values.
        /// Does not perform latch.
        /// </summary>
        public void ShiftByte(byte value)
        {
            for (int i = 0; i < 8; i++)
            {
                var data = (128 >> i) & value;
                Shift(data);
            }
        }

        /// <summary>
        /// Switch output register to high-impedance state.
        /// Disables current output register.
        /// </summary>
        public void OutputDisable()
        {
            if (_mapping.OE > 0)
            {
                _controller.Write(_mapping.OE, 1);
            }
            else
            {
                throw new ArgumentNullException($"{nameof(OutputDisable)}: {nameof(_mapping.OE)} not mapped to non-zero pin value");
            }
        }

        /// <summary>
        /// Switch output register low-impedance state.
        /// Enables current register.
        /// </summary>
        public void OutputEnable()
        {
            if (_mapping.OE > 0)
            {
                _controller.Write(_mapping.OE, 0);
            }
            else
            {
                throw new ArgumentNullException($"{nameof(OutputEnable)}: {nameof(_mapping.OE)} not mapped to non-zero pin value");
            }
        }

        private void Setup()
        {
            _controller.OpenPins(PinMode.Output, _ser, _rclk, _srclk, _mapping.SRCLR);
            _controller.WriteValueToPins(0, _ser, _rclk, _rclk);
            _controller.Write(_mapping.SRCLR, 1);

            if (_mapping.OE > 0)
            {
                _controller.OpenPinAndWrite(_mapping.OE, 0);
            }
        }

        /// <summary>
        /// Represents pin bindings for the Sn74hc595.
        /// </summary>
        public struct PinMapping
        {
            // 5 pins

            /// <param name="ser">Data pin</param>
            /// <param name="oe">Output enable pin</param>
            /// <param name="rclk">Register clock pin (latch)</param>
            /// <param name="srclk">Shift register pin (shift to data register)</param>
            /// <param name="srclr">Shift register clear pin (shift register is cleared)</param>
            public PinMapping(int ser, int oe, int rclk, int srclk, int srclr)
            {
                SER = ser;              // data in;     SR pin 14
                OE = oe;                // blank;       SR pin 13
                RCLK = rclk;            // latch;       SR pin 12
                SRCLK = srclk;          // clock;       SR pin 11
                SRCLR = srclr;          // clear;       SR pin 10
            }

            /// <summary>
            /// Standard pin bindings for the Sn74hc595.
            /// </summary>
            public static PinMapping Standard => new PinMapping(25, 12, 16, 20, 21);

            /// <summary>
            /// Matching pin bindings for the Sn74hc595 (Pi and shift register pin numbers match).
            /// </summary>
            public static PinMapping Matching => new PinMapping(14, 13, 12, 11, 10);

            /// <summary>
            /// SER (data) pin number.
            /// </summary>
            public int SER { get; set; }

            /// <summary>
            /// OE (output enable) pin number.
            /// </summary>
            public int OE { get; set; }

            /// <summary>
            /// RCLK (latch) pin number.
            /// </summary>
            public int RCLK { get; set; }

            /// <summary>
            /// SRCLK (shift) pin number.
            /// </summary>
            public int SRCLK { get; set; }

            /// <summary>
            /// SRCLR (clear register) pin number.
            /// </summary>
            public int SRCLR { get; set; }

            internal bool Validate()
            {
                if (SER > 0 &&
                    RCLK > 0 &&
                    SRCLK > 0)
                {
                    return true;
                }

                return false;
            }
        }
    }
}