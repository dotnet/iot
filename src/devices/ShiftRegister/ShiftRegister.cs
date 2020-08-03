// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System;
using System.Device.Gpio;
using System.Device.Spi;

namespace Iot.Device.Multiplexing
{
    /// <summary>
    /// Generic shift register implementation. Supports multiple register lengths.
    /// Compatible with SN74HC595, MBI5027 and MBI5168, for example.
    /// Supports SPI and GPIO control.
    /// </summary>
    public class ShiftRegister : IDisposable
    {
        // Datasheet: https://www.ti.com/lit/ds/symlink/sn74hc595.pdf
        // Datasheet: http://archive.fairchip.com/pdf/MACROBLOCK/MBI5168.pdf
        // Tutorial: https://www.youtube.com/watch?v=6fVbJbNPrEU
        // Using with SPI:
        // https://forum.arduino.cc/index.php?topic=571144.0
        // http://www.cupidcontrols.com/2013/12/turn-on-the-spi-lights-spi-output-shift-registers-and-leds/
        private readonly ShiftRegisterPinMapping _pinMapping;
        private readonly int _sdi;
        private readonly int _clk;
        private readonly int _latch;
        private readonly int _bitLength;
        private readonly bool _shouldDispose;
        private GpioController _controller;
        private SpiDevice _spiDevice;

        /// <summary>
        /// Initialize a new shift register connected through GPIO.
        /// </summary>
        /// <param name="pinMapping">The pin mapping to use by the binding.</param>
        /// <param name="bitLength">Bit length of register, including chained registers.</param>
        /// <param name="gpioController">The GPIO Controller used for interrupt handling.</param>
        /// <param name="shouldDispose">True (the default) if the GPIO controller shall be disposed when disposing this instance.</param>
        public ShiftRegister(ShiftRegisterPinMapping pinMapping, int bitLength, GpioController gpioController = null,  bool shouldDispose = true)
        {
            if (gpioController == null)
            {
                gpioController = new GpioController();
            }

            _controller = gpioController;
            _shouldDispose = shouldDispose;
            _pinMapping = pinMapping;
            _sdi = _pinMapping.Sdi;
            _clk = _pinMapping.Clk;
            _latch = _pinMapping.LE;
            _bitLength = bitLength;
            SetupPins();
        }

        /// <summary>
        /// Initialize a new shift register device connected through SPI.
        /// Uses 3 pins (SDI -> SDI, SCLK -> SCLK, CE0 -> LE)
        /// </summary>
        /// <param name="spiDevice">SpiDevice used for serial communication.</param>
        /// <param name="bitLength">Bit length of register, including chained registers.</param>
        public ShiftRegister(SpiDevice spiDevice, int bitLength)
        {
            _spiDevice = spiDevice;
            _bitLength = bitLength;
        }

        /// <summary>
        /// GPIO controller.
        /// </summary>
        protected GpioController GpioController => _controller;

        /// <summary>
        /// SPI device.
        /// </summary>
        protected SpiDevice SpiDevice => _spiDevice;

        /// <summary>
        /// Bit length across all connected registers.
        /// </summary>
        public int BitLength => _bitLength;

        /// <summary>
        /// Reports if shift register is connected with SPI.
        /// </summary>
        public bool UsesSpi => _spiDevice is object;

        /// <summary>
        /// Reports if shift register is connected with GPIO.
        /// </summary>
        public bool UsesGpio => _controller is object;

        /// <summary>
        /// Shifts zeros.
        /// Will dim all connected LEDs, for example.
        /// Assumes register bit length evenly divisible by 8.
        /// Supports GPIO controller or SPI device.
        /// </summary>
        public void ShiftClear()
        {
            if (_bitLength % 8 > 0)
            {
                throw new ArgumentNullException($"{nameof(ShiftClear)}: Only supported for registers with bit lengths evenly divisible by 8.");
            }

            for (int i = 0; i < _bitLength / 8; i++)
            {
                ShiftByte(0b_0000_0000);
            }
        }

        /// <summary>
        /// Writes PinValue value to storage register.
        /// This will shift existing values to the next storage slot.
        /// Does not latch.
        /// Requires use of GPIO controller.
        /// </summary>
        public void ShiftBit(PinValue value)
        {
            if (_controller is null || _pinMapping.Sdi == 0)
            {
                throw new ArgumentNullException($"{nameof(ShiftBit)}: GpioController was not provided or {nameof(_pinMapping.Sdi)} not mapped to pin");
            }

            // writes value to serial data pin
            _controller.Write(_sdi, value);
            // data is written to the storage register on the rising edge of the storage register clock
            _controller.Write(_clk, 1);
            // values are reset to low
            _controller.Write(_sdi, 0);
            _controller.Write(_clk, 0);
        }

        /// <summary>
        /// Shifts a byte -- 8 bits -- to the storage register.
        /// Assumes register bit length evenly divisible by 8.
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
                // 0b_1000_0000 (same as integer 128) used as input to create mask
                // determines value of i bit in byte value
                // logical equivalent of value[i] (which isn't supported for byte type in C#)
                // starts left-most and ends up right-most
                int data = (0b_1000_0000 >> i) & value;
                // writes value to storage register
                ShiftBit(data);
            }

            if (latch)
            {
                Latch();
            }
        }

        /// <summary>
        /// Latches values in data register to output pi.
        /// Requires use of GPIO controller.
        /// </summary>
        public void Latch()
        {
            if (_controller is null || _pinMapping.LE == 0)
            {
                throw new ArgumentNullException($"{nameof(Latch)}: GpioController was not provided or {nameof(_pinMapping.LE)} not mapped to pin");
            }

            // latches value on rising edge of register clock (LE)
            _controller.Write(_latch, 1);
            // value reset to low in preparation for next use.
            _controller.Write(_latch, 0);
        }

        /// <summary>
        /// Switch output register to high-impedance state.
        /// Temporarily disables register outputs, but does not delete values.
        /// Requires use of GPIO controller.
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
        /// Enables register outputs with existing values.
        /// Requires use of GPIO controller.
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
            // this condition only applies to GPIO devices
            if (_shouldDispose)
            {
                _controller?.Dispose();
                _controller = null;
            }

            // SPI devices are always disposed
            _spiDevice?.Dispose();
            _spiDevice = null;
        }

        private void SetupPins()
        {
            // these three pins are required
            if (_sdi > 0 &&
                _latch > 0 &&
                _clk > 0)
            {
                OpenPinAndWrite(_sdi, 0);
                OpenPinAndWrite(_latch, 0);
                OpenPinAndWrite(_clk, 0);
            }
            else
            {
                throw new ArgumentException($"{nameof(ShiftRegister)} -- {nameof(ShiftRegisterPinMapping)} values must be non-zero; Values: {nameof(ShiftRegisterPinMapping.Sdi)}: {_sdi}; {nameof(ShiftRegisterPinMapping.LE)}: {_latch}; {nameof(ShiftRegisterPinMapping.Clk)}: {_clk};.");
            }

            // this pin assignment is optional
            // if not assigned, must be tied to ground
            if (_pinMapping.OE > 0)
            {
                OpenPinAndWrite(_pinMapping.OE, 0);
            }
        }

        private void OpenPinAndWrite(int pin, PinValue value)
        {
            _controller.OpenPin(pin, PinMode.Output);
            _controller.Write(pin, value);
        }
    }
}
