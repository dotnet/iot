// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System;
using System.Collections.Generic;
using System.Device.Gpio;

namespace Iot.Device.Tm16xx
{
    /// <summary>
    /// Represents Titanmec TM1650 device.
    /// </summary>
    public class Tm1650 : Tm16xxI2CLikeBase
    {
        #region Const

        private const int MemoryAddressPrefix = 0b0110_1000;
        private const byte ModeCommand = 0b0100_1000;

        // According to the doc, the clock pulse width minimum is 400 ns
        // And waiting time between clk up and down is 1 µs
        private const byte ClockWidthMicroseconds = 1;
        private bool _use7Segment;
        private byte[] _characterMemoryAddress = new byte[0];
        #endregion

        #region Character memory address

        private protected override void OnCharacterOrderChanged()
        {
            _characterMemoryAddress = Array.ConvertAll(_characterOrder, i => (byte)(MemoryAddressPrefix | (i << 1)));
        }

        #endregion

        #region State

        /// <summary>
        /// Gets or sets the segment mode. Led7Segment and Led8Segment are supported.
        /// </summary>
        public override LedSegment LedSegment
        {
            get => _use7Segment ? LedSegment.Led7Segment : LedSegment.Led8Segment;

            set
            {
                switch (value)
                {
                    case LedSegment.Led7Segment:
                        _use7Segment = true;
                        break;
                    case LedSegment.Led8Segment:
                        _use7Segment = false;
                        break;
                    default:
                        throw new ArgumentOutOfRangeException(nameof(value), value, $"{nameof(Tm1650)} can only support 7-segment and 8-segment modes.");
                }

                OnStateChanged();
            }
        }

        /// <summary>
        /// Updates all screen settings at once to reduce communication.
        /// </summary>
        /// <param name="brightness">Screen brightness. Value must be in range from 0 to 7. Default value is 0.</param>
        /// <param name="use7Segment">Whether use 7 segment or 8. Default value is <see langword="false"/>, aka 8 segment.</param>
        /// <param name="screenOn">Whether the screen is on. Default value is <see langword="true"/>, aka screen on.</param>
        public void SetScreen(byte brightness = 0, bool use7Segment = false, bool screenOn = true)
        {
            if (brightness > 7)
            {
                throw new ArgumentException("Value must be less than 8.", nameof(brightness));
            }

            _brightness = brightness;
            _use7Segment = use7Segment;
            _screenOn = screenOn;
            OnStateChanged();
        }

        private protected override void OnStateChanged()
        {
            // Sends display command.
            var displayCommand = (byte)((_brightness << 4) |
                                        (_use7Segment ? 8 : 0) |
                                        (_screenOn ? 1 : 0));

            StartTransmission();
            WriteByteAndWaitAcknowledge(ModeCommand);
            WriteByteAndWaitAcknowledge(displayCommand);
            StopTransmission();
        }

        #endregion

        #region Constructors

        /// <summary>
        /// Initializes an instance of TM1650 with Gpio controller specified.
        /// </summary>
        /// <param name="pinClk">The clock pin.</param>
        /// <param name="pinDio">The data pin.</param>
        /// <param name="controller">The instance of the Gpio controller which will not be disposed with this object.</param>
        public Tm1650(int pinClk, int pinDio, GpioController controller)
            : this(pinClk, pinDio, gpioController: controller)
        {
        }

        /// <summary>
        /// Initializes an instance of TM1650 with new Gpio controller which will be disposed with this object. Pin numbering scheme is set to logical.
        /// </summary>
        /// <param name="pinClk">The clock pin.</param>
        /// <param name="pinDio">The data pin.</param>
        public Tm1650(int pinClk, int pinDio)
            : this(pinClk, pinDio, null, true)
        {
        }

        /// <summary>
        /// Initializes an instance of TM1650.
        /// </summary>
        /// <param name="pinClk">The clock pin.</param>
        /// <param name="pinDio">The data pin.</param>
        /// <param name="gpioController">The instance of the gpio controller. Set to <see langword="null" /> to create a new one.</param>
        /// <param name="shouldDispose">Sets to <see langword="true" /> to dispose the Gpio controller with this object. If the <paramref name="gpioController"/> is set to <see langword="null"/>, this parameter will be ignored and the new created Gpio controller will always be disposed with this object.</param>
        public Tm1650(int pinClk, int pinDio, GpioController? gpioController = null, bool shouldDispose = true)
            : base(pinClk, pinDio, ClockWidthMicroseconds, gpioController, shouldDispose)
        {
            _maxCharacters = 4;
            _characterOrder = new byte[4] { 0, 1, 2, 3 };
            // ReSharper disable once VirtualMemberCallInConstructor
            OnCharacterOrderChanged();
            SetScreen();
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
        /// <param name="rawData">The raw data array to display, size of the array has to be 4 maximum.</param>
        public override void Display(ReadOnlySpan<byte> rawData)
        {
            if (rawData.Length > _maxCharacters)
            {
                throw new ArgumentException($"Maximum number of characters for TM1650 is {_maxCharacters}.", nameof(rawData));
            }

            for (int i = 0; i < rawData.Length; i++)
            {
                DisplayOneInternal(i, rawData[i]);
            }

            for (int j = rawData.Length; j < _maxCharacters; j++)
            {
                DisplayOneInternal(j, (byte)Character.Nothing);
            }
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
        /// <param name="characterPosition">The character position from 0 to 3.</param>
        /// <param name="rawData">The segment character to display.</param>
        public override void Display(byte characterPosition, byte rawData)
        {
            if (characterPosition > _maxCharacters)
            {
                throw new ArgumentException($"Maximum number of characters for TM1650 is {_maxCharacters}.", nameof(characterPosition));
            }

            DisplayOneInternal(characterPosition, rawData);
        }

        /// <inheritdoc />
        public override void ClearDisplay()
        {
            DisplayOneInternal(0, 0);
            DisplayOneInternal(1, 0);
            DisplayOneInternal(2, 0);
            DisplayOneInternal(3, 0);
        }

        private void DisplayOneInternal(int index, byte rawData)
        {
            StartTransmission();
            WriteByteAndWaitAcknowledge(_characterMemoryAddress[index]);
            WriteByteAndWaitAcknowledge(rawData);
            StopTransmission();
        }
        #endregion

        #region Low level IO

        private protected override IEnumerable<bool> ByteToBitConverter(byte data)
        {
            for (var i = 0; i < 7; i++)
            {
                yield return (data & 0b1000_0000) != 0;
                data <<= 1;
            }

            yield return (data & 0b1000_0000) != 0;
        }

        #endregion
    }
}
