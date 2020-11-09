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
        private readonly int _serial;
        private readonly int _clock;
        private readonly int _latch;
        private readonly int _bitLength;
        private readonly bool _shouldDispose;
        private GpioController? _controller;
        private SpiDevice? _spiDevice;

        /// <summary>
        /// Initialize a new shift register connected through GPIO.
        /// </summary>
        /// <param name="pinMapping">The pin mapping to use by the binding.</param>
        /// <param name="bitLength">Bit length of register, including chained registers.</param>
        /// <param name="gpioController">The GPIO Controller used for interrupt handling.</param>
        /// <param name="shouldDispose">True (the default) if the GPIO controller shall be disposed when disposing this instance.</param>
        public ShiftRegister(ShiftRegisterPinMapping pinMapping, int bitLength, GpioController? gpioController = null,  bool shouldDispose = true)
        {
            _shouldDispose = shouldDispose || gpioController is null;
            _controller = gpioController ?? new GpioController();
            _pinMapping = pinMapping;
            _serial = _pinMapping.SerialDataInput;
            _clock = _pinMapping.Clock;
            _latch = _pinMapping.LatchEnable;
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
            _spiDevice = spiDevice ?? throw new ArgumentNullException(nameof(spiDevice));
            _bitLength = bitLength;
        }

        /// <summary>
        /// GPIO controller.
        /// </summary>
        protected GpioController? GpioController => _controller;

        /// <summary>
        /// SPI device.
        /// </summary>
        protected SpiDevice? SpiDevice => _spiDevice;

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
                throw new ArgumentNullException(nameof(ShiftClear), "Only supported for registers with bit lengths evenly divisible by 8.");
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
            if (_controller is null || _pinMapping.SerialDataInput < 0)
            {
                throw new ArgumentNullException(nameof(ShiftBit), "GpioController was not provided or {nameof(_pinMapping.SerialDataInput)} not mapped to pin");
            }

            // writes value to serial data pin
            _controller.Write(_serial, value);
            // data is written to the storage register on the rising edge of the storage register clock
            _controller.Write(_clock, 1);
            // values are reset to low
            _controller.Write(_serial, 0);
            _controller.Write(_clock, 0);
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
            if (_controller is null || _pinMapping.LatchEnable < 0)
            {
                throw new Exception($"{nameof(Latch)}: GpioController was not provided or {nameof(_pinMapping.LatchEnable)} not mapped to pin");
            }

            // latches value on rising edge of register clock (LE)
            _controller.Write(_latch, 1);
            // value reset to low in preparation for next use.
            _controller.Write(_latch, 0);
        }

        /// <summary>
        /// Switch output register to high or low-impedance state.
        /// Enables or disables register outputs, but does not delete values.
        /// Requires use of GPIO controller.
        /// </summary>
        public bool OutputEnable
        {
            set
            {
                if (_controller is null || _pinMapping.OutputEnable < 0)
                {
                    throw new Exception($"{nameof(OutputEnable)}: {nameof(_pinMapping.OutputEnable)} not mapped to non-zero pin value");
                }

                _controller.Write(_pinMapping.OutputEnable, value ? 0 : 1);
            }
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
            if (_serial >= 0 &&
                _latch >= 0 &&
                _clock >= 0)
            {
                OpenPinAndWrite(_serial, 0);
                OpenPinAndWrite(_latch, 0);
                OpenPinAndWrite(_clock, 0);
            }
            else
            {
                throw new Exception($"{nameof(ShiftRegister)} -- {nameof(ShiftRegisterPinMapping)} values must be non-zero; Values: {nameof(ShiftRegisterPinMapping.SerialDataInput)}: {_serial}; {nameof(ShiftRegisterPinMapping.LatchEnable)}: {_latch}; {nameof(ShiftRegisterPinMapping.Clock)}: {_clock};.");
            }

            // this pin assignment is optional
            // if not assigned, must be tied to ground
            if (_pinMapping.OutputEnable > 0)
            {
                OpenPinAndWrite(_pinMapping.OutputEnable, 0);
            }
        }

        private void OpenPinAndWrite(int pin, PinValue value)
        {
            _controller?.OpenPin(pin, PinMode.Output);
            _controller?.Write(pin, value);
        }
    }
}
