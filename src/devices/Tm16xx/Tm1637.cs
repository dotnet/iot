// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System;
using System.Collections.Generic;
using System.Device.Gpio;

namespace Iot.Device.Tm16xx
{
    /// <summary>
    /// Represents Titanmec TM1637 device.
    /// </summary>
    public class Tm1637 : Tm16xxI2CLikeBase
    {
        #region Const

        // According to the doc, the clock pulse width minimum is 400 ns
        // And waiting time between clk up and down is 1 µs
        private const byte ClockWidthMicroseconds = 1;

        private const byte MemoryAddressPrefix = 0b1100_0000;
        private const byte FixedAddressWritingCommand = 0b0100_0100;
        private const byte SequencedWritingCommand = 0b0100_0000;

        // To store what has been displayed last. Used when change on brightness or screen on/off is used.
        private byte _lastDisplay = 0;
        private byte _lastDisplayIndex = 0;
        private byte _displayState = 0;

        #endregion

        #region State

        /// <summary>
        /// Sets the screen on or off.
        /// </summary>
        [Obsolete($"This is kept for code backward compatible. Uses IsScreenOn instead.")]
        public bool ScreenOn
        {
            get => IsScreenOn;
            set => IsScreenOn = value;
        }

        /// <summary>
        /// Gets or sets the screen brightness. Value must be in range from 0 to 7. For TM1637, 7 is the brightest.
        /// </summary>
        [Obsolete($"This is kept for code backward compatible. Uses ScreenBrightness instead.")]
        public byte Brightness
        {
            get => ScreenBrightness;
            set => ScreenBrightness = value;
        }

        /// <summary>
        /// Gets or sets the segment mode. Due to the Led7Segment is the only supported, this property could be ignored for Tm1637.
        /// </summary>
        public override LedSegment LedSegment
        {
            get => LedSegment.Led7Segment;
            set
            {
                if (value == LedSegment.Led7Segment)
                {
                    return;
                }

                throw new ArgumentOutOfRangeException(nameof(value), value, $"{nameof(Tm1637)} can only support 7-segment mode.");
            }
        }

        /// <summary>
        /// Updates all screen settings at once to reduce communication.
        /// </summary>
        /// <param name="brightness">Screen brightness. Value must be in range from 0 to 7. Default value is 7.</param>
        /// <param name="screenOn">Whether the screen is on. Default value is <see langword="true"/>, aka screen on.</param>
        /// <param name="waitForNextDisplay">Set to <see langword="true"/> to leave the update command with the next display command, in order to save once communication. Default value is <see langword="false"/>. If a Display method is called next, this can be set to <see langword="true"/>.</param>
        public void SetScreen(byte brightness = 7, bool screenOn = true, bool waitForNextDisplay = false)
        {
            if (brightness > 7)
            {
                throw new ArgumentException("Value must be less than 8.", nameof(brightness));
            }

            _brightness = brightness;
            _screenOn = screenOn;
            if (waitForNextDisplay)
            {
                return;
            }

            OnStateChanged();
        }

        private protected override void OnStateChanged()
        {
            // Generates the state code for this time and future.
            _displayState = (byte)((_screenOn ? 0b1000_1000 : 0b1000_0000) + _brightness);

            // Sends display command.
            DisplayOneInternal(_lastDisplayIndex, _lastDisplay);
        }

        #endregion

        #region Constructors

        /// <summary>
        /// Initializes an instance of TM1637 with Gpio controller specified.
        /// </summary>
        /// <param name="pinClk">The clock pin.</param>
        /// <param name="pinDio">The data pin.</param>
        /// <param name="controller">The instance of the Gpio controller which will not be disposed with this object.</param>
        public Tm1637(int pinClk, int pinDio, GpioController controller)
            : this(pinClk, pinDio, gpioController: controller)
        {
        }

        /// <summary>
        /// Initializes an instance of TM1637 with new Gpio controller which will be disposed with this object. Pin numbering scheme is set to logical.
        /// </summary>
        /// <param name="pinClk">The clock pin.</param>
        /// <param name="pinDio">The data pin.</param>
        public Tm1637(int pinClk, int pinDio)
            : this(pinClk, pinDio, null, true)
        {
        }

                /// <summary>
        /// Initializes an instance of TM1637.
        /// </summary>
        /// <param name="pinClk">The clock pin.</param>
        /// <param name="pinDio">The data pin.</param>
        /// <param name="gpioController">The instance of the gpio controller. Set to <see langword="null" /> to create a new one.</param>
        /// <param name="shouldDispose">Sets to <see langword="true" /> to dispose the Gpio controller with this object. If the <paramref name="gpioController"/> is set to <see langword="null"/>, this parameter will be ignored and the new created Gpio controller will always be disposed with this object.</param>
        public Tm1637(int pinClk, int pinDio, GpioController? gpioController = null, bool shouldDispose = true)
            : base(pinClk, pinDio, ClockWidthMicroseconds, gpioController, shouldDispose)
        {
            _maxCharacters = 6;
            _brightness = 7;
            _screenOn = true;
            _characterOrder = new byte[6] { 0, 1, 2, 3, 4, 5 };
        }
        #endregion

        #region Display

        /// <summary>
        /// Displays segments starting at first segment with byte array containing raw data for each segment including the dot.
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
        /// Representation of the number 0 so lighting segments a, b, c, d, e and F is then 0x3f.
        /// </remarks>
        /// </summary>
        /// <param name="rawData">The raw data array to display, size of the array has to be 6 maximum.</param>
        public override void Display(ReadOnlySpan<byte> rawData)
        {
            if (rawData.Length > _maxCharacters)
            {
                throw new ArgumentException($"Maximum number of characters for TM1637 is {_maxCharacters}.", nameof(rawData));
            }

            // Prepare the buffer with the right order to transfer
            byte[] toTransfer = new byte[_maxCharacters];

            for (int i = 0; i < rawData.Length; i++)
            {
                toTransfer[_characterOrder[i]] = rawData[i];
            }

            for (int j = rawData.Length; j < _maxCharacters; j++)
            {
                toTransfer[_characterOrder[j]] = 0;
            }

            _lastDisplayIndex = 0;
            _lastDisplay = toTransfer[0];

            DisplayMultipleInternal(toTransfer);
        }

        /// <summary>
        /// Displays a raw data at a specific segment position from 0 to 3.
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
        /// Representation of the number 0 so lighting segments a, b, c, d, e and F is then 0x3f.
        /// </remarks>
        /// <param name="characterPosition">The character position from 0 to 5.</param>
        /// <param name="rawData">The segment character to display.</param>
        public override void Display(byte characterPosition, byte rawData)
        {
            if (characterPosition > _maxCharacters)
            {
                throw new ArgumentException($"Maximum number of characters for TM1637 is {_maxCharacters}.", nameof(characterPosition));
            }

            _lastDisplay = rawData;
            _lastDisplayIndex = characterPosition;
            DisplayOneInternal(characterPosition, rawData);
        }

        /// <inheritdoc />
        public override void ClearDisplay()
        {
            _lastDisplay = 0;
            _lastDisplayIndex = 0;
            DisplayMultipleInternal(0, 0, 0, 0, 0, 0);
        }

        // Displays without updating _lastDisplayIndex and _lastDisplay.
        private void DisplayOneInternal(int index, byte rawData)
        {
            StartTransmission();
            // First command for fixed address mode
            WriteByteAndWaitAcknowledge(FixedAddressWritingCommand);
            StopTransmission();
            StartTransmission();
            // Set the address to transfer
            WriteByteAndWaitAcknowledge((byte)(MemoryAddressPrefix + index));
            // Transfer the byte
            WriteByteAndWaitAcknowledge(rawData);
            StopTransmission();
            StartTransmission();
            // Set the display on/off and the brightness
            WriteByteAndWaitAcknowledge(_displayState);
            StopTransmission();
        }

        private void DisplayMultipleInternal(params byte[] rawData)
        {
            StartTransmission();
            // First command for sequence mode
            WriteByteAndWaitAcknowledge(SequencedWritingCommand);
            StopTransmission();
            StartTransmission();
            // Set the address to transfer
            WriteByteAndWaitAcknowledge(MemoryAddressPrefix);
            // Transfer the data
            for (int i = 0; i < _maxCharacters; i++)
            {
                WriteByteAndWaitAcknowledge(rawData[i]);
            }

            StopTransmission();
            StartTransmission();
            // Set the display on/off and the brightness
            WriteByteAndWaitAcknowledge(_displayState);
            StopTransmission();
        }
        #endregion

        #region Low level IO

        private protected override IEnumerable<bool> ByteToBitConverter(byte data)
        {
            for (var i = 0; i < 7; i++)
            {
                yield return (data & 0b0000_0001) != 0;
                data >>= 1;
            }

            yield return (data & 0b0000_0001) != 0;
        }

        #endregion
    }
}
