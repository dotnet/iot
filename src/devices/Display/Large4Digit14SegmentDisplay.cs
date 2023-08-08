// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System;
using System.Device.I2c;
using System.Runtime.InteropServices;

namespace Iot.Device.Display
{
    /// <summary>
    /// PIMORONI FOUR LETTER PHAT 14 segment display
    /// </summary>
    /// <remarks>
    /// https://shop.pimoroni.com/products/four-letter-phat
    /// Sources:
    /// Derived from /src/devices/Display/Large4Digit7SegmentDisplay.cs
    /// https://github.com/adafruit/Adafruit_LED_Backpack/blob/master/Adafruit_LEDBackpack.cpp
    /// https://github.com/sobek1985/Adafruit_LEDBackpack/blob/master/Adafruit_LEDBackpack/AlphaNumericFourCharacters.cs
    /// </remarks>
    public partial class Large4Digit14SegmentDisplay : Ht16k33
    {
        #region Private members
        #region Constants

        /// <summary>
        /// Number of digits supported by display
        /// </summary>
        private const int MaxNumberOfDigits = 4;

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
            /// Third digit
            /// </summary>
            Digit3 = 5,

            /// <summary>
            /// Fourth digit
            /// </summary>
            Digit4 = 7
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
            0b0000_0000, // Command
            0b0000_0000, // Data Disp 1
            0b0000_0000,
            0b0000_0000, // Data Disp 2
            0b0000_0000,
            0b0000_0000, // Data Disp 3
            0b0000_0000,
            0b0000_0000, // Data Disp 4
            0b0000_0000
        };

        /// <summary>
        /// Display buffer
        /// </summary>
        private readonly byte[] _displayBuffer =
        {
            0b0000_0000, // Command
            0b0000_0000, // Data Disp 1
            0b0000_0000,
            0b0000_0000, // Data Disp 2
            0b0000_0000,
            0b0000_0000, // Data Disp 3
            0b0000_0000,
            0b0000_0000, // Data Disp 4
            0b0000_0000
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
        public Large4Digit14SegmentDisplay(I2cDevice i2cDevice)
            : base(i2cDevice)
        {
        }

        /// <summary>
        /// Gets the number of digits supported by the display
        /// </summary>
        public int NumberOfDigits { get; } = MaxNumberOfDigits;

        /// <summary>
        /// Gets or sets a single digit's segments by id
        /// </summary>
        /// <param name="address">digit address (0..3)</param>
        /// <returns>Segment in display buffer for the given address</returns>
        public Segment14 this[int address]
        {
            get => (Segment14)_displayBuffer[TranslateDigitToBufferAddress(address)];
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

            if (startAddress < 0 || startAddress >= (MaxNumberOfDigits * 2))
            {
                throw new ArgumentOutOfRangeException(nameof(startAddress));
            }

            if (digits.Length + startAddress > (MaxNumberOfDigits * 2))
            {
                throw new ArgumentOutOfRangeException(nameof(digits), $"{nameof(Large4Digit7SegmentDisplay)} only supports {MaxNumberOfDigits - startAddress} digits starting from address {startAddress}");
            }

            foreach (byte digit in digits)
            {
                _displayBuffer[(int)s_digitAddressList[startAddress++]] = (byte)(digit);
            }

            AutoFlush();
        }

        /// <summary>
        /// Write raw data to display buffer
        /// </summary>
        /// <param name="digits">Array of ushort to write to the display</param>
        /// <param name="startAddress">Address to start writing from</param>
        private void Write(ReadOnlySpan<ushort> digits, int startAddress = 0)
        {
            if (digits.Length == 0)
            {
                // Nothing to write
                return;
            }

            if (startAddress < 0 || startAddress >= (MaxNumberOfDigits * 2))
            {
                throw new ArgumentOutOfRangeException(nameof(startAddress));
            }

            if (digits.Length + startAddress > (MaxNumberOfDigits * 2))
            {
                throw new ArgumentOutOfRangeException(nameof(digits), $"{nameof(Large4Digit14SegmentDisplay)} only supports {MaxNumberOfDigits - startAddress} digits starting from address {startAddress}");
            }

            foreach (ushort digit in digits)
            {
                var asBytes = BitConverter.GetBytes(digit);

                _displayBuffer[(int)s_digitAddressList[startAddress]] = (byte)(asBytes[1]);
                _displayBuffer[(int)s_digitAddressList[startAddress] + 1] = (byte)(asBytes[0]);
                startAddress++;
            }

            AutoFlush();
        }

        /// <summary>
        /// Write a series of digits to the display buffer
        /// </summary>
        /// <param name="digits">a list of digits represented in segments</param>
        /// <param name="startAddress">Address to start writing from</param>
        public void Write(ReadOnlySpan<Segment14> digits, int startAddress = 0) =>
            Write(MemoryMarshal.Cast<Segment14, ushort>(digits), startAddress);

        /// <summary>
        /// Write a series of characters to the display buffer
        /// </summary>
        /// <param name="characters">a list of characters represented in fonts</param>
        /// <param name="startAddress">Address to start writing from</param>
        public void Write(ReadOnlySpan<Font14> characters, int startAddress = 0) =>
            Write(MemoryMarshal.Cast<Font14, ushort>(characters), startAddress);

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

            switch (value.Length)
            {
                case 1 when alignment == Alignment.Right:
                case 2 when alignment == Alignment.Right:
                case 3 when alignment == Alignment.Right:
                    value = value.PadLeft(MaxNumberOfDigits);
                    break;
                case 1 when alignment == Alignment.Left:
                case 2 when alignment == Alignment.Left:
                case 3 when alignment == Alignment.Left:
                case 4:
                case 5:
                    // Nothing to do
                    break;
                default:
                    throw new ArgumentOutOfRangeException(nameof(value), $"{nameof(value)} can contain maximum 5 characters");
            }

            Write(FontHelper14.GetString(value));
        }

        /// <summary>
        /// Write an array of up to 4 bytes as hex
        /// </summary>
        /// <param name="values">Array of bytes</param>
        /// <exception cref="ArgumentOutOfRangeException"><paramref name="values"/>Length of array must be greater 0 and less than  MaxNumberOfDigits</exception>
        public void WriteHex(byte[] values)
        {
            if (values.Length > MaxNumberOfDigits)
            {
                throw new ArgumentOutOfRangeException(nameof(values), $"{nameof(values)} can contain maximum {MaxNumberOfDigits} bytes");
            }

            Write(FontHelper14.GetHexDigits(values));
        }

        /// <summary>
        /// Write a single char to a spcific display digit
        /// </summary>
        /// <param name="c">The character to display</param>
        /// <param name="pos">zero-based index of character position from the left</param>
        /// <exception cref="ArgumentOutOfRangeException"><paramref name="pos"/>position must be between 0 and MaxNumberOfDigits-1</exception>
        public void WriteChar(char c, int pos)
        {
            if (pos >= MaxNumberOfDigits || pos < 0)
            {
                throw new ArgumentOutOfRangeException(nameof(pos), $"{nameof(pos)} must be between 0 and  {MaxNumberOfDigits - 1} bytes");
            }

            Font14[] ch = { FontHelper14.GetCharacter(c) };

            var tp = new ReadOnlySpan<Font14>(ch);

            Write(tp, pos);
        }

        #endregion
        #endregion
    }
}
