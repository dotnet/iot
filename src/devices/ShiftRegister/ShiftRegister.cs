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
        // Spec: https://www.ti.com/lit/ds/symlink/sn74hc595.pdf
        // Tutorial: https://www.youtube.com/watch?v=6fVbJbNPrEU
        // Using with SPI:
        // https://forum.arduino.cc/index.php?topic=571144.0
        // http://www.cupidcontrols.com/2013/12/turn-on-the-spi-lights-spi-output-shift-registers-and-leds/
        private readonly bool _shouldDispose;
        private readonly int _data;
        private readonly int _srclk;
        private readonly int _rclk;
        private readonly int _bitLength;
        private readonly ShiftRegisterPinMapping _pinMapping;
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
            _data = _pinMapping.Data;
            _srclk = _pinMapping.SrClk;
            _rclk = _pinMapping.RClk;
            _bitLength = bitLength;
            SetupPins();
        }

        /// <summary>
        /// Initialize a new shift register device connected through SPI.
        /// Uses 3 pins (MOSI -> Data, SCLK -> SCLK, CE0 -> RCLK)
        /// </summary>
        /// <param name="spiDevice">SpiDevice used for serial communication.</param>
        /// <param name="bitLength">Bit length of register, including chained registers.</param>
        public ShiftRegister(SpiDevice spiDevice, int bitLength)
        {
            _spiDevice = spiDevice;
            _bitLength = bitLength;
        }

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
        /// Writes high or low value to storage register.
        /// This will shift the existing values to the next storage slot.
        /// Does not perform latch.
        /// Requires use of GPIO controller.
        /// </summary>
        public void ShiftBit(PinValue value)
        {
            if (_controller is null || _pinMapping.Data == 0)
            {
                throw new ArgumentNullException($"{nameof(ShiftBit)}: GpioController was not provided or {nameof(_pinMapping.Data)} not mapped to pin");
            }

            _controller.Write(_data, value);
            // data is written to the storage register on the rising edge of the storage register clock.
            _controller.Write(_srclk, 1);
            // values are reset to low in preparation for next use.
            _controller.Write(_data, 0);
            _controller.Write(_srclk, 0);
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
                // logical equivalent of value[i] (which isn't supported in C#)
                // starts left-most and ends up right-most
                int data = (0b_1000_0000 >> i) & value;
                // writes value to register
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
            if (_controller is null || _pinMapping.RClk == 0)
            {
                throw new ArgumentNullException($"{nameof(Latch)}: GpioController was not provided or {nameof(_pinMapping.RClk)} not mapped to pin");
            }

            // latches value on rising edge of register clock
            _controller.Write(_rclk, 1);
            // value reset to low in preparation for next use.
            _controller.Write(_rclk, 0);
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
            if (_data > 0 &&
                _rclk > 0 &&
                _srclk > 0)
            {
                OpenPinAndWrite(_data, 0);
                OpenPinAndWrite(_rclk, 0);
                OpenPinAndWrite(_srclk, 0);
            }
            else
            {
                throw new ArgumentException($"{nameof(ShiftRegister)} -- {nameof(ShiftRegisterPinMapping)} values must be non-zero; Values: {nameof(ShiftRegisterPinMapping.Data)}: {_data}; {nameof(ShiftRegisterPinMapping.RClk)}: {_rclk}; {nameof(ShiftRegisterPinMapping.SrClk)}: {_srclk};.");
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
