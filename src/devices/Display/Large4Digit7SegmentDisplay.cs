// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using System;
using System.Device.I2c;
using System.Runtime.InteropServices;

namespace Iot.Device.Display
{
    /// <summary>
    /// Adafruit 1.2" 4-Digit 7-Segment Display w/I2C Backpack
    /// </summary>
    /// <remarks>
    /// Comes in yellow, green and red colors:
    /// https://www.adafruit.com/product/1268
    /// https://www.adafruit.com/product/1269
    /// https://www.adafruit.com/product/1270
    /// Sources:
    /// https://github.com/adafruit/Adafruit_LED_Backpack/blob/master/Adafruit_LEDBackpack.cpp
    /// https://github.com/sobek1985/Adafruit_LEDBackpack/blob/master/Adafruit_LEDBackpack/AlphaNumericFourCharacters.cs
    /// </remarks>
    public partial class Large4Digit7SegmentDisplay : Ht16k33, ISevenSegmentDisplay
    {
        #region Private members
        #region Constants
        /// <summary>
        /// Number of digits supported by display
        /// </summary>
        private const int MaxNumberOfDigits = 4;

        /// <summary>
        /// This display does not support dot bits for each digit,
        /// so the first bit should be masked before flushing to
        /// the device
        /// </summary>
        private const byte SegmentMask = 0b0111_1111;
        #endregion

        #region Enums
        /// <summary>
        /// Digit address within display buffer
        /// </summary>
        private enum Address
        {
            /// <summary>
            /// First digit
            /// </summary>
            Digit1 = 1,
            /// <summary>
            /// Second digit
            /// </summary>
            Digit2 = 3,
            /// <summary>
            /// Dot setting bits
            /// </summary>
            Dots = 5,
            /// <summary>
            /// Third digit
            /// </summary>
            Digit3 = 7,
            /// <summary>
            /// Fourth digit
            /// </summary>
            Digit4 = 9
        }
        #endregion

        /// <summary>
        /// List of digit addresses for sequential writing
        /// </summary>
        private static readonly Address[] s_digitAddressList = { Address.Digit1, Address.Digit2, Address.Digit3, Address.Digit4 };

        /// <summary>
        /// Empty display buffer
        /// </summary>
        private static readonly byte[] s_clearBuffer =
        {
            0x00, 0b0000_0000,
            0x01, 0b0000_0000,
            0x02, 0b0000_0000,
            0x03, 0b0000_0000,
            0x04, 0b0000_0000
        };

        /// <summary>
        /// Display buffer
        /// </summary>
        private readonly byte[] _displayBuffer =
        {
            0x00, 0b0000_0000,
            0x01, 0b0000_0000,
            0x02, 0b0000_0000,
            0x03, 0b0000_0000,
            0x04, 0b0000_0000
        };

        /// <summary>
        /// Translate digit number to buffer address
        /// </summary>
        /// <param name="digit">digit to translate</param>
        /// <exception cref="ArgumentOutOfRangeException"><paramref name="digit"/></exception>
        private static int TranslateDigitToBufferAddress(int digit) => digit switch
        {
            0 => (int)Address.Digit1,
            1 => (int)Address.Digit2,
            2 => (int)Address.Digit3,
            3 => (int)Address.Digit4,
            _ => throw new ArgumentOutOfRangeException(nameof(digit)),
        };
        #endregion

        #region Public members
        /// <summary>
        /// Initialize 7-Segment display
        /// </summary>
        /// <param name="i2cDevice">The <see cref="I2cDevice"/> to create with.</param>
        public Large4Digit7SegmentDisplay(I2cDevice i2cDevice) : base(i2cDevice) { }

        /// <inheritdoc/>
        public int NumberOfDigits { get; } = MaxNumberOfDigits;

        /// <summary>
        /// Gets or sets a single digit's segments by id
        /// </summary>
        /// <param name="address">digit address (0..3)</param>
        /// <returns>Segment in display buffer for the given address</returns>
        public Segment this[int address]
        {
            get => (Segment)_displayBuffer[TranslateDigitToBufferAddress(address)];
            set
            {
                _displayBuffer[TranslateDigitToBufferAddress(address)] = (byte)value;
                AutoFlush();
            }
        }

        /// <inheritdoc/>
        /// <remarks>Write clears dots, you'll have to reset them afterwards</remarks>
        /// <exception cref="ArgumentOutOfRangeException"><paramref name="startAddress"/></exception>
        /// <exception cref="ArgumentOutOfRangeException"><see cref="Large4Digit7SegmentDisplay"/> only supports <see cref="MaxNumberOfDigits"/> digits</exception>
        public override void Write(ReadOnlySpan<byte> digits, int startAddress = 0)
        {
            if (digits.Length == 0)
            {
                // Nothing to write
                return;
            }

            if (startAddress < 0 || startAddress >= MaxNumberOfDigits)
            {
                throw new ArgumentOutOfRangeException(nameof(startAddress));
            }

            if (digits.Length + startAddress > MaxNumberOfDigits)
            {
                throw new ArgumentOutOfRangeException($"{nameof(Large4Digit7SegmentDisplay)} only supports {MaxNumberOfDigits - startAddress} digits starting from address {startAddress}");
            }

            foreach (byte digit in digits)
            {
                _displayBuffer[(int)s_digitAddressList[startAddress++]] = (byte)(digit & SegmentMask);
            }

            AutoFlush();
        }

        /// <inheritdoc/>
        public void Write(ReadOnlySpan<Segment> digits, int startAddress = 0) =>
            Write(MemoryMarshal.Cast<Segment, byte>(digits), startAddress);

        /// <inheritdoc/>
        public void Write(ReadOnlySpan<Font> characters, int startAddress = 0) =>
            Write(MemoryMarshal.Cast<Font, byte>(characters), startAddress);

        /// <inheritdoc/>
        public override void Clear() =>
            s_clearBuffer.CopyTo(_displayBuffer, 0);

        /// <inheritdoc/>
        public override void Flush() =>
            _i2cDevice.Write(_displayBuffer);

        #region Write overloads
        /// <summary>
        /// Write integer value as decimal digits
        /// </summary>
        /// <param name="value">integer value</param>
        /// <param name="alignment">alignment on display (left or right, right is default)</param>
        /// <exception cref="ArgumentOutOfRangeException"><paramref name="value"/> must be between -999..9999</exception>
        public void Write(int value, Alignment alignment = Alignment.Right)
        {
            if (value > 9999 || value < -999)
            {
                throw new ArgumentOutOfRangeException(nameof(value), $"{nameof(value)} must be between -999..9999");
            }

            Write(value.ToString(), alignment);
        }

        /// <summary>
        /// Write string value to display
        /// </summary>
        /// <param name="value">value to display, max 4 characters, or 5 characters if the 3rd character is ':' (this also turns on the center colon), or 6 characters if 1st character is also ':'</param>
        /// <remarks>
        /// * Unsupported characters will be replaced as whitespace
        /// * This method clears the buffer before writing, so dots have to be reset afterwards
        /// </remarks>
        /// <param name="alignment">alignment on display (left or right, right is default)</param>
        /// <exception cref="ArgumentException"><paramref name="value"/>[2] must be a ':'</exception>
        /// <exception cref="ArgumentOutOfRangeException"><paramref name="value"/> can contain maximum 5 characters</exception>
        public void Write(string value, Alignment alignment = Alignment.Left)
        {
            Clear();

            if (string.IsNullOrWhiteSpace(value))
            {
                return;
            }

            // Show two left dots as colon if first character is : and aligment is left
            if (value[0] == ':' && alignment == Alignment.Left)
            {
                Dots = Dot.LeftColon;
                if (value.Length == 1)
                {
                    return;
                }
                value = value.Substring(1);
            }

            switch (value.Length)
            {
                case 3 when alignment == Alignment.Left && value[2] == ':':
                    Dots |= Dot.CenterColon;
                    value = value.Substring(0, 2);
                    break;
                case 3 when alignment == Alignment.Right && value[0] == ':':
                    Dots |= Dot.CenterColon;
                    value = "  " + value.Substring(1);
                    break;
                case 4 when alignment == Alignment.Left && value[2] == ':':
                    Dots |= Dot.CenterColon;
                    value = value.Substring(0, 2) + value[3];
                    break;
                case 4 when alignment == Alignment.Right && value[1] == ':':
                    Dots |= Dot.CenterColon;
                    value = " " + value[0] + value.Substring(2, 2);
                    break;
                case 5 when value[2] != ':':
                    throw new ArgumentException(nameof(value), $"{nameof(value)}[2] must be a ':'");
                case 5:
                    Dots |= Dot.CenterColon;
                    value = value.Substring(0, 2) + value.Substring(3, 2);
                    break;
                case 1 when alignment == Alignment.Right:
                case 2 when alignment == Alignment.Right:
                case 3 when alignment == Alignment.Right:
                    value = value.PadLeft(MaxNumberOfDigits);
                    break;
                case 1 when alignment == Alignment.Left:
                case 2 when alignment == Alignment.Left:
                case 3 when alignment == Alignment.Left:
                case 4:
                    // Nothing to do
                    break;
                default:
                    throw new ArgumentOutOfRangeException(nameof(value), $"{nameof(value)} can contain maximum 5 characters");
            }

            Write(FontHelper.GetString(value));
        }
        #endregion

        /// <summary>
        /// Gets or sets dot configuration
        /// </summary>
        /// <remarks>The <see cref="Clear"/> method also clears the dots as well.</remarks>
        public Dot Dots
        {
            get => (Dot)_displayBuffer[(int)Address.Dots];
            set
            {
                _displayBuffer[(int)Address.Dots] = (byte)value;
                AutoFlush();
            }
        }
        #endregion
    }
}
