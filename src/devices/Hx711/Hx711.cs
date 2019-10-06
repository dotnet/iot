// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using System;
using System.Device.Gpio;

namespace Iot.Device.Hx711
{
    /// <summary>
    /// Controller for Hx711 - 24-bit Analog-to-Digital Converter for Weigh Scales
    /// </summary>
    /// <remarks>
    /// Data sheet available here: https://cdn.sparkfun.com/datasheets/Sensors/ForceFlex/hx711_english.pdf
    /// Based off ms-iot implementation here: https://github.com/ms-iot/hx711/blob/master/HX711.cs 
    /// </remarks>
    public class Hx711 : IDisposable
    {
        private readonly object _syncRoot = new object();

        private readonly GpioController _gpioController;
        private readonly int _dataPinNumber;
        private readonly int _clockPinNumber;

        private bool _enabled;

        /// <summary>
        /// Creates a new instance of the HC-SCR501.
        /// </summary>
        /// <param name="gpioController">The <see cref="GpioController"/> for the clock and data pins</param>
        /// <param name="dataPin">The number of the pin to use for data</param>
        /// <param name="clockPin">The number of the pin to use for clock</param>
        /// <param name="pinNumberingScheme">Pin Numbering Scheme</param>
        public Hx711(GpioController gpioController, int dataPin, int clockPin, PinNumberingScheme pinNumberingScheme = PinNumberingScheme.Logical)
        {
            _gpioController = gpioController;
            _dataPinNumber = dataPin;
            _clockPinNumber = clockPin;
        }
        
        /// <summary>
        /// Releases any resources
        /// </summary>
        public void Dispose()
        {
            Disable();
        }

        private byte ShiftInByte()
        {
            byte value = 0x00;

            // Convert "GpioPinValue.High" and "GpioPinValue.Low" to 1 and 0, respectively.
            // NOTE: Loop is unrolled for performance
            _gpioController.Write(_clockPinNumber, PinValue.High);
            value |= (byte)((byte)(_gpioController.Read(_dataPinNumber)) << 7);
            _gpioController.Write(_clockPinNumber, PinValue.Low);
            _gpioController.Write(_clockPinNumber, PinValue.High);
            value |= (byte)((byte)(_gpioController.Read(_dataPinNumber)) << 6);
            _gpioController.Write(_clockPinNumber, PinValue.Low);
            _gpioController.Write(_clockPinNumber, PinValue.High);
            value |= (byte)((byte)(_gpioController.Read(_dataPinNumber)) << 5);
            _gpioController.Write(_clockPinNumber, PinValue.Low);
            _gpioController.Write(_clockPinNumber, PinValue.High);
            value |= (byte)((byte)(_gpioController.Read(_dataPinNumber)) << 4);
            _gpioController.Write(_clockPinNumber, PinValue.Low);
            _gpioController.Write(_clockPinNumber, PinValue.High);
            value |= (byte)((byte)(_gpioController.Read(_dataPinNumber)) << 3);
            _gpioController.Write(_clockPinNumber, PinValue.Low);
            _gpioController.Write(_clockPinNumber, PinValue.High);
            value |= (byte)((byte)(_gpioController.Read(_dataPinNumber)) << 2);
            _gpioController.Write(_clockPinNumber, PinValue.Low);
            _gpioController.Write(_clockPinNumber, PinValue.High);
            value |= (byte)((byte)(_gpioController.Read(_dataPinNumber)) << 1);
            _gpioController.Write(_clockPinNumber, PinValue.Low);
            _gpioController.Write(_clockPinNumber, PinValue.High);
            value |= (byte)_gpioController.Read(_dataPinNumber);
            _gpioController.Write(_clockPinNumber, PinValue.Low);

            return value;
        }

        // Byte:     0        1        2        3
        // Bits:  76543210 76543210 76543210 76543210
        // Data: |--------|--------|--------|--------|
        // Bit#:  33222222 22221111 11111100 00000000
        //        10987654 32109876 54321098 76543210
        private int ReadData()
        {
            uint value = 0;
            byte[] data = new byte[4];

            // Wait for chip to become ready
            while (_gpioController.Read(_dataPinNumber) != PinValue.Low) ;
            
            // Clock in data
            data[1] = ShiftInByte();
            data[2] = ShiftInByte();
            data[3] = ShiftInByte();

            // Clock in gain of 128 for next reading
            _gpioController.Write(_clockPinNumber, PinValue.High);
            _gpioController.Write(_clockPinNumber, PinValue.Low);

            // Replicate the most significant bit to pad out a 32-bit signed integer
            if (0x80 == (data[1] & 0x80))
            {
                data[0] = 0xFF;
            }
            else
            {
                data[0] = 0x00;
            }

            // Construct a 32-bit signed integer
            value = (uint)((data[0] << 24) | (data[1] << 16) | (data[2] << 8) | data[3]);

            // Datasheet indicates the value is returned as a two's complement value so
            // flip all the bits
            value = ~value;

            // ... and add 1
            return (int)(++value);
        }

        /// <summary>
        /// Enables the device by setting the reset pin high.
        /// </summary>
        public void Enable()
        {
            _gpioController.OpenPin(_clockPinNumber, PinMode.Output);

            try
            {
                _gpioController.OpenPin(_dataPinNumber, PinMode.Input);
            }
            catch
            {
                _gpioController.ClosePin(_clockPinNumber);
                throw;
            }

            _gpioController.Write(_clockPinNumber, PinValue.Low);
            _enabled = true;

        }

        /// <summary>
        /// Disables the device by setting the reset pin low.
        /// </summary>
        public void Disable()
        {
            if (_enabled)
            {
                _gpioController.ClosePin(_clockPinNumber);
                _gpioController.ClosePin(_dataPinNumber);
                _enabled = false;
            }
        }

        /// <summary>
        /// Reads the current value by waiting for the data
        /// pin to be ready (low) then clocking in three bytes
        /// of data from MSB to LSB
        /// </summary>
        public int ReadValue()
        {
            if (_enabled)
            {
                lock (_syncRoot)
                {
                    return ReadData();
                }
            }
            else
            {
                throw new InvalidOperationException("Hx711 hasn't been enabled yet. Call `Enable()` before `ReadValue()`");
            }
        }
    }
}
