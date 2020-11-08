// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System;
using System.Device;
using System.Device.Gpio;
using System.Diagnostics;
using System.Linq;
using System.Runtime.InteropServices;
// using System.Runtime.InteropServices.WindowsRuntime;
namespace Iot.Device.Tm1637
{
    /// <summary>
    /// Represents Tm1637 segment display
    /// </summary>
    public sealed class Tm1637 : IDisposable
    {
        /// <summary>
        /// The number of characters that the TM1637 can handle
        /// </summary>
        public static byte MaxCharacters => 6;

        // According to the doc, the clock pulse width minimum is 400 ns
        // And waiting time between clk up and down is 1 µs
        private const byte ClockWidthMicroseconds = 1;

        private readonly int _pinClk;
        private readonly int _pinDio;
        private GpioController _controller;
        private bool _shouldDispose;

        private byte _brightness;

        // Default character order is from 0 to 5
        private byte[] _charactersOrder = new byte[6] { 0, 1, 2, 3, 4, 5 };

        // To store what has been displayed last. Used when change on brightness or
        // screen on/off is used
        private byte[] _lastDisplay = new byte[6] { 0x00, 0x00, 0x00, 0x00, 0x00, 0x00 };
        private bool _screenOn;

        /// <summary>
        /// Initialize a TM1637
        /// </summary>
        /// <param name="pinClk">The clock pin</param>
        /// <param name="pinDio">The data pin</param>
        /// <param name="pinNumberingScheme">Use the logical or physical pin layout</param>
        /// <param name="gpioController">A Gpio Controller if you want to use a specific one</param>
        /// <param name="shouldDispose">True to dispose the Gpio Controller</param>
        public Tm1637(int pinClk, int pinDio, PinNumberingScheme pinNumberingScheme = PinNumberingScheme.Logical,
            GpioController? gpioController = null, bool shouldDispose = true)
        {
            _pinClk = pinClk;
            _pinDio = pinDio;
            _controller = gpioController ?? new GpioController(pinNumberingScheme);
            _shouldDispose = shouldDispose || gpioController is null;
            _controller.OpenPin(_pinClk, PinMode.Output);
            _controller.OpenPin(_pinDio, PinMode.Output);
            _brightness = 7;
        }

        /// <summary>
        /// Order of characters, expect a 6 length byte array
        /// 0 to 5, any order. Most of the time 4 segments do
        /// not need to be changed but the 6 ones may be in different order
        /// like 0 1 2 5 4 3. In this case, this byte array has be be in this order
        /// </summary>
        public byte[] CharacterOrder
        {
            get => _charactersOrder;
            set
            {
                if (value.Length != MaxCharacters)
                {
                    throw new ArgumentException($"Size of {nameof(CharacterOrder)} can only be 6 length");
                }

                // Check if we have all values from 0 to 5
                bool allExist = true;
                for (int i = 0; i < MaxCharacters; i++)
                {
                    allExist &= Array.Exists(value, e => e == i);
                }

                if (!allExist)
                {
                    throw new ArgumentException(
                        $"{nameof(CharacterOrder)} needs to have all existing characters from 0 to 5");
                }

                value.CopyTo(_charactersOrder, 0);
            }
        }

        /// <summary>
        /// Set the screen on or off
        /// </summary>
        public bool ScreenOn
        {
            get => _screenOn;
            set
            {
                _screenOn = value;
                DisplayRaw(0, _lastDisplay[0]);
            }
        }

        /// <summary>
        /// Adjust the screen brightness from 0 to 7
        /// </summary>
        public byte Brightness
        {
            get => _brightness;
            set
            {
                if (value > 7)
                {
                    throw new ArgumentException($"{nameof(Brightness)} can't be more than 7");
                }

                _brightness = value;
                DisplayRaw(0, _lastDisplay[0]);
            }
        }

        private PinValue WriteByte(byte data)
        {
            // We send data by 8 bits
            for (byte i = 0; i < 8; i++)
            {
                _controller.Write(_pinClk, PinValue.Low);
                DelayHelper.DelayMicroseconds(ClockWidthMicroseconds, true);
                // LSB first
                if ((data & 0x01) == 0x01)
                {
                    _controller.Write(_pinDio, PinValue.High);
                }
                else
                {
                    _controller.Write(_pinDio, PinValue.Low);
                }

                // LSB first
                data >>= 1;
                _controller.Write(_pinClk, PinValue.High);
                DelayHelper.DelayMicroseconds(ClockWidthMicroseconds, true);
            }

            // Wait for the acknowledge
            _controller.Write(_pinClk, PinValue.Low);
            _controller.Write(_pinDio, PinValue.High);
            _controller.Write(_pinClk, PinValue.High);
            DelayHelper.DelayMicroseconds(ClockWidthMicroseconds, true);
            _controller.SetPinMode(_pinDio, PinMode.Input);

            // Wait 1 µs, it's the waiting time between clk up and down
            // That's according to the documentation
            DelayHelper.DelayMicroseconds(ClockWidthMicroseconds, true);

            var ack = _controller.Read(_pinDio);
            if (ack == PinValue.Low)
            {
                // We get acknowledge from the device
                _controller.SetPinMode(_pinDio, PinMode.Output);
                _controller.Write(_pinDio, PinValue.Low);
            }

            _controller.Write(_pinClk, PinValue.High);
            DelayHelper.DelayMicroseconds(ClockWidthMicroseconds, true);
            _controller.Write(_pinClk, PinValue.Low);
            DelayHelper.DelayMicroseconds(ClockWidthMicroseconds, true);

            _controller.SetPinMode(_pinDio, PinMode.Output);
            return ack;
        }

        private void StartTransmission()
        {
            _controller.Write(_pinClk, PinValue.High);
            _controller.Write(_pinDio, PinValue.High);
            DelayHelper.DelayMicroseconds(ClockWidthMicroseconds, true);
            _controller.Write(_pinDio, PinValue.Low);
        }

        private void StopTransmission()
        {
            _controller.Write(_pinClk, PinValue.Low);
            _controller.Write(_pinDio, PinValue.Low);
            DelayHelper.DelayMicroseconds(ClockWidthMicroseconds, true);
            _controller.Write(_pinClk, PinValue.High);
            _controller.Write(_pinDio, PinValue.High);
        }

        /// <summary>
        /// Displays segments starting at first segment with byte array containing raw data for each segment including the dot
        /// <remarks>
        /// Segment representation:
        ///
        /// bit 0 = a       _a_
        /// bit 1 = b      |   |
        /// bit 2 = c      f   b
        /// bit 3 = d      |_g_|
        /// bit 4 = e      |   |
        /// bit 5 = f      e   c
        /// bit 6 = g      |_d_|  .dp
        /// bit 7 = dp
        ///
        /// Representation of the number 0 so lighting segments a, b, c, d, e and F is then 0x3f
        /// </remarks>
        /// </summary>
        /// <param name="rawData">The raw data array to display, size of the array has to be 6 maximum</param>
        private void Display(ReadOnlySpan<byte> rawData)
        {
            if (rawData.Length > MaxCharacters)
            {
                throw new ArgumentException($"Maximum number of segments for TM1637 is {MaxCharacters}");
            }

            // Prepare the buffer with the right order to transfer
            byte[] toTransfer = new byte[MaxCharacters];

            for (int i = 0; i < rawData.Length; i++)
            {
                toTransfer[_charactersOrder[i]] = rawData[i];
            }

            for (int j = rawData.Length; j < MaxCharacters; j++)
            {
                toTransfer[_charactersOrder[j]] = (byte)Character.Nothing;
            }

            _lastDisplay = toTransfer;

            StartTransmission();
            // First command is set data
            WriteByte((byte)DataCommand.DataCommandSetting);
            StopTransmission();
            StartTransmission();
            // Second command is set address to automatic
            WriteByte((byte)DataCommand.DataCommandSetting);
            // Transfer the data
            for (int i = 0; i < MaxCharacters; i++)
            {
                WriteByte(toTransfer[i]);
            }

            StopTransmission();
            StartTransmission();
            // Set the display on/off and the brightness
            WriteByte((byte)((ScreenOn ? DisplayCommand.DisplayOn : DisplayCommand.DisplayOff) + _brightness));
            StopTransmission();
        }

        /// <summary>
        /// Displays a series of prebuild characters including the dot or not
        /// You can build your won characters with the primitives like Bottom, Top, Dot
        /// </summary>
        /// <param name="rawData">The Character to display</param>
        public void Display(ReadOnlySpan<Character> rawData)
        {
            Display(MemoryMarshal.AsBytes(rawData));
        }

        /// <summary>
        /// Displays a raw data at a specific segment position from 0 to 5
        /// </summary>
        /// <remarks>
        /// Segment representation:
        ///
        /// bit 0 = a       _a_
        /// bit 1 = b      |   |
        /// bit 2 = c      f   b
        /// bit 3 = d      |_g_|
        /// bit 4 = e      |   |
        /// bit 5 = f      e   c
        /// bit 6 = g      |_d_|  .dp
        /// bit 7 = dp
        ///
        /// Representation of the number 0 so lighting segments a, b, c, d, e and F is then 0x3f
        /// </remarks>
        /// <param name="characterPosition">The character position from 0 to 5</param>
        /// <param name="rawData">The segment characters to display</param>
        public void Display(byte characterPosition, Character rawData)
        {
            if (characterPosition > MaxCharacters)
            {
                throw new ArgumentException($"Maximum number of characters for TM1637 is {MaxCharacters}");
            }

            // Recreate the buffer in correct order
            _lastDisplay[_charactersOrder[characterPosition]] = (byte)rawData;
            DisplayRaw(_charactersOrder[characterPosition], (byte)rawData);
        }

        private void DisplayRaw(byte characterAddress, byte rawData)
        {
            StartTransmission();
            // First command for fix address
            WriteByte((byte)DataCommand.FixAddress);
            StopTransmission();
            StartTransmission();
            // Set the address to transfer
            WriteByte((byte)(DataCommand.AddressCommandSetting + characterAddress));
            // Transfer the byte
            WriteByte(rawData);
            StopTransmission();
            StartTransmission();
            // Set the display on/off and the brightness
            WriteByte((byte)((ScreenOn ? DisplayCommand.DisplayOn : DisplayCommand.DisplayOff) + _brightness));
            StopTransmission();
        }

        /// <summary>
        /// Clear the display
        /// </summary>
        public void ClearDisplay()
        {
            // 6 segments with nothing/space displayed
            Span<byte> clearDisplay = stackalloc byte[]
            {
                (byte)Character.Nothing,
                (byte)Character.Nothing,
                (byte)Character.Nothing,
                (byte)Character.Nothing,
                (byte)Character.Nothing,
                (byte)Character.Nothing,
            };
            Display(clearDisplay);
        }

        /// <summary>
        /// Cleanup
        /// </summary>
        public void Dispose()
        {
            if (_shouldDispose)
            {
                _controller?.Dispose();
                _controller = null!;
            }
        }
    }
}
