// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using System;
using System.Device;
using System.Device.Gpio;
using System.Diagnostics;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;

namespace Iot.Device.Tm1637
{
    public class Tm1637 : IDisposable
    {
        /// <summary>
        /// The number of segments that the TM1637 can handle
        /// </summary>
        public byte MaxSegments => 6;
        // Used for the space or to display blank on a segment
        private const int Nothing = 16;
        // According to the doc, the clock pulse width minimum is 400 ns
        // And waiting time between clk up and down is 1 µs
        private const byte ClockWidthMicroseconds = 1;

        private readonly int _pinClk;
        private readonly int _pinDio;
        private GpioController _controller;
        private byte _brightness;
        // Default segment order is from 0 to 5
        private byte[] _segmentOrder = new byte[6] { 0, 1, 2, 3, 4, 5 };
        // To store what has been displayed last. Used when change on brightness or
        // screen on/off is used
        private byte[] _lastDisplay = new byte[6] { 0x00, 0x00, 0x00, 0x00, 0x00, 0x00 };
        private bool _secrrenOn;

        // All numbers 0~9, plus letters "A","b","C","d","E","F", nothing/space and "-"
        // Prebuild characters
        private static byte[] _displayMatrix = {0x3f, 0x06, 0x5b, 0x4f,
                            0x66, 0x6d, 0x7d, 0x07,
                            0x7f, 0x6f, 0x77, 0x7c,
                            0x39, 0x5e, 0x79, 0x71,
                            0x00, 0x40};

        /// <summary>
        /// Initialize a TM1637
        /// </summary>
        /// <param name="pinClk">The clock pin</param>
        /// <param name="pinDio">The data pin</param>
        /// <param name="pinNumberingScheme">Use the logical or physical pin layout</param>
        public Tm1637(int pinClk, int pinDio, PinNumberingScheme pinNumberingScheme = PinNumberingScheme.Logical)
        {
            _pinClk = pinClk;
            _pinDio = pinDio;
            _controller = new GpioController(pinNumberingScheme);
            _controller.OpenPin(_pinClk, PinMode.Output);
            _controller.OpenPin(_pinDio, PinMode.Output);
            _brightness = 7;

        }

        /// <summary>
        /// Order of segments, expect a 6 length byte array
        /// 0 to 5, any order. Most of the time 4 segments do
        /// not need to be changed but the 6 ones may be in different order
        /// like 0 1 2 5 4 3. In this case, this byte array has be be in this order
        /// </summary>
        public byte[] SegmentOrder
        {
            get { return _segmentOrder; }
            set
            {
                if (value.Length != MaxSegments)
                    throw new ArgumentException($"Size of {nameof(SegmentOrder)} can only be 6 length");
                // Check if we have all values from 0 to 5
                bool allExist = true;
                for (int i = 0; i < MaxSegments; i++)
                {
                    allExist &= Array.Exists(value, e => e == i);
                }
                if (!allExist)
                    throw new ArgumentException($"{nameof(SegmentOrder)} needs to have all existing segments from 0 to 5");
                _segmentOrder = value;
            }
        }

        /// <summary>
        /// Set the screen on or off
        /// </summary>
        public bool ScreenOn
        {
            get { return _secrrenOn; }

            set
            {
                _secrrenOn = value;
                DisplayRaw(0, _lastDisplay[0]);
            }
        }

        /// <summary>
        /// Adjust the screen brightness from 0 to 7
        /// </summary>
        public byte Brightness
        {
            get { return _brightness; }
            set
            {
                if (value > 7)
                    throw new ArgumentException($"{nameof(Brightness)} can't be more than 7");
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
                    _controller.Write(_pinDio, PinValue.High);
                else
                    _controller.Write(_pinDio, PinValue.Low);
                // LSB first
                data >>= 1;
                _controller.Write(_pinClk, PinValue.High);
                DelayHelper.DelayMicroseconds(ClockWidthMicroseconds, true);
            }

            // Wait for the acknoledge
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
                _controller.SetPinMode(_pinDio, PinMode.Output);
                _controller.Write(_pinDio, PinValue.Low);
            }

            _controller.Write(_pinClk, PinValue.High);
            DelayHelper.DelayMicroseconds(ClockWidthMicroseconds, true);
            _controller.Write(_pinClk, PinValue.Low);
            DelayHelper.DelayMicroseconds(ClockWidthMicroseconds, true);

            // Wait 50 Microseconds
            //DelayHelper.DelayMicroseconds(50, true);
            _controller.SetPinMode(_pinDio, PinMode.Output);
            //DelayHelper.DelayMicroseconds(50, true);

            return ack;
        }

        private void StartTransmission()
        {
            _controller.Write(_pinClk, PinValue.High);
            _controller.Write(_pinDio, PinValue.High);
            DelayHelper.DelayMicroseconds(ClockWidthMicroseconds, true);
            _controller.Write(_pinDio, PinValue.Low);
            //_controller.Write(_pinClk, PinValue.Low);
            //DelayHelper.DelayMicroseconds(5, true);
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
        /// </summary>
        /// <param name="rawData">The raw data array to display, size of the array has to be 6 maximum</param>
        public void Display(ReadOnlySpan<byte> rawData)
        {
            if (rawData.Length > MaxSegments)
                throw new ArgumentException($"Maximum number of segments for TM1637 is {MaxSegments}");

            // Prepare the buffer with the right order to transfer
            byte[] toTransfer = new byte[MaxSegments];

            for (int i = 0; i < rawData.Length; i++)
                toTransfer[_segmentOrder[i]] = rawData[i];

            for (int j = rawData.Length; j < MaxSegments; j++)
                toTransfer[_segmentOrder[j]] = _displayMatrix[Nothing];
            _lastDisplay = toTransfer;

            StartTransmission();
            // First command is set data
            WriteByte((byte)DataCommand.DataCommandSetting);
            StopTransmission();
            StartTransmission();
            // Second command is set address to automatic
            WriteByte((byte)DataCommand.DataCommandSetting);
            // Transfer the data
            for (int i = 0; i < MaxSegments; i++)
                WriteByte(toTransfer[i]);

            StopTransmission();
            StartTransmission();
            // Set the display on/off and the brightness
            WriteByte((byte)((ScreenOn ? DisplayCommand.DisplayOn : DisplayCommand.DisplayOff) + _brightness));
            StopTransmission();
        }

        /// <summary>
        /// Displays a series of prebuild characters including the dot or not
        /// </summary>
        /// <param name="character">The Character to display</param>
        public void Display(Character[] character)
        {
            Span<byte> segData = stackalloc byte[character.Length];
            for (int i = 0; i < segData.Length; i++)
                segData[i] = Encode((byte)character[i].Char, character[i].Dot);
            Display(segData);
        }

        /// <summary>
        /// Displays a raw data at a specific segment address from 0 to 6
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
        /// </summary>
        /// <param name="segmentAddress">The segment address from 0 to 6</param>
        /// <param name="rawData">The raw data to display</param>
        public void Display(byte segmentAddress, byte rawData)
        {
            if (segmentAddress > MaxSegments)
                throw new ArgumentException($"Maximum number of segments for TM1637 is {MaxSegments}");

            // Recreate the buffer in correct order
            _lastDisplay[_segmentOrder[segmentAddress]] = rawData;
            DisplayRaw(_segmentOrder[segmentAddress], rawData);
        }

        private void DisplayRaw(byte segmentAddress, byte rawData)
        {
            StartTransmission();
            // First command for fix address
            WriteByte((byte)DataCommand.FixAddress);
            StopTransmission();
            StartTransmission();
            // Fix address with the address
            WriteByte((byte)(DataCommand.FixAddress + segmentAddress));
            // Transfer the byte
            WriteByte(rawData);
            StopTransmission();
            StartTransmission();
            // Set the display on/off and the brightness
            WriteByte((byte)((ScreenOn ? DisplayCommand.DisplayOn : DisplayCommand.DisplayOff) + _brightness));
            StopTransmission();
        }

        /// <summary>
        /// Displays a prebuild characters including the dot or not
        /// </summary>
        /// <param name="segmentAddress">The segment address from 0 to 6</param>
        /// <param name="charecter">The Character to display</param>
        public void Display(byte segmentAddress, Character charecter)
        {
            byte data = Encode((byte)charecter.Char, charecter.Dot);
            Display(segmentAddress, data);
        }

        /// <summary>
        /// Clear the display
        /// </summary>
        public void ClearDisplay()
        {
            // 6 segments with nothing/space displayed
            Span<byte> clearDisplay = stackalloc byte[]
            {
                _displayMatrix[Nothing],
                _displayMatrix[Nothing],
                _displayMatrix[Nothing],
                _displayMatrix[Nothing],
                _displayMatrix[Nothing],
                _displayMatrix[Nothing]
            };
            Display(clearDisplay);
        }

        private byte Encode(byte data, bool dot)
        {
            data = _displayMatrix[data];
            data += dot ? (byte)0x80 : (byte)0x00;
            return data;
        }

        /// <summary>
        /// Cleanup
        /// </summary>
        public void Dispose()
        {
            _controller?.Dispose();
            _controller = null;
        }
    }
}
