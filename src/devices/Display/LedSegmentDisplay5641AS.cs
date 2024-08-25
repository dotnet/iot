// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System;
using System.Collections.Generic;
using System.Device.Gpio;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Iot.Device.Display;

namespace Display
{
    /// <summary>
    /// LED 4-digit 7-segment display, model 5641AS. This class handles driverless, direct-pin access over GPIO.
    /// </summary>
    /// <remarks>
    /// Data Sheet:
    /// http://www.xlitx.com/datasheet/5641AS.pdf
    /// </remarks>
    public class LedSegmentDisplay5641AS : ISevenSegmentDisplay, IDisposable
    {
        private readonly LedSegmentDisplay5641ASPinScheme _pinScheme;
        private readonly GpioController _gpioController;
        private readonly bool _shouldDispose;
        private bool _disposedValue;

        private List<GpioPin> _segments;
        private List<GpioPin> _digits;

        /// <inheritdoc />
        public int NumberOfDigits => _pinScheme.DigitCount;

        /// <inheritdoc />
        public Segment this[int address] { get => throw new NotImplementedException(); set => throw new NotImplementedException(); }

        /// <summary>
        /// Initializes a new instance of the <see cref="LedSegmentDisplay5641AS"/> class.
        /// </summary>
        public LedSegmentDisplay5641AS(LedSegmentDisplay5641ASPinScheme pinScheme, GpioController? gpioController = null, bool shouldDispose = true)
        {
            _pinScheme = pinScheme;
            _gpioController = gpioController ?? new GpioController();
            _shouldDispose = gpioController is null || shouldDispose;

            _segments = new(pinScheme.SegmentCountPerDigit);
            _digits = new(pinScheme.DigitCount);
        }

        /// <summary>
        /// Opens GPIO pins defined in <see cref="LedSegmentDisplay5641ASPinScheme"/> as <see cref="PinMode.Output"/>
        /// </summary>
        public void Open()
        {
            foreach (var segmentPinNumBoard in _pinScheme.Segments)
            {
                _segments.Add(_gpioController.OpenPin(segmentPinNumBoard, PinMode.Output));
            }

            foreach (var digitPinNumBoard in _pinScheme.Digits)
            {
                _digits.Add(_gpioController.OpenPin(digitPinNumBoard, PinMode.Output));
            }
        }

        /// <summary>
        /// Closes GPIO pins defined in <see cref="LedSegmentDisplay5641ASPinScheme"/>
        /// </summary>
        public void Close()
        {
            foreach (var segmentPinNumBoard in _pinScheme.Segments)
            {
                _gpioController.ClosePin(segmentPinNumBoard);
            }

            foreach (var digitPinNumBoard in _pinScheme.Digits)
            {
                _gpioController.ClosePin(digitPinNumBoard);
            }

            _segments = new(_pinScheme.SegmentCountPerDigit);
            _digits = new(_pinScheme.DigitCount);
        }

        /// <summary>
        /// Clears display by writing <see cref="PinValue.Low"/> to all segments
        /// </summary>
        public void Clear()
        {
            foreach (var seg in _segments)
            {
                seg?.Write(PinValue.Low);
            }
        }

        /// <summary>
        /// Writes a series of characters to the display, with optional support for dots/decimals.
        /// </summary>
        /// <param name="characters">A list of characters represented in Fonts</param>
        /// <param name="decimalsEnabled">Array indicating displayed (true) or not displayed (false) decimal point in each position</param>
        /// <exception cref="ArgumentException"></exception>
        /// <remarks>The 5641AS display has four dots in the bottom right corner of each digit
        /// and are handled separately from the characters to support changes at runtime.</remarks>
        public void Write(ReadOnlySpan<Font> characters, bool[]? decimalsEnabled = null)
        {
            if (characters.Length > NumberOfDigits)
            {
                throw new ArgumentException($"Too many characters specified (max {NumberOfDigits} supported)", nameof(characters));
            }

            if (decimalsEnabled?.Length > _digits.Count)
            {
                throw new ArgumentException($"Too many decimal points specified (max {_pinScheme.DigitCount} supported)", nameof(decimalsEnabled));
            }

            // Characters must be written to the device in reverse order to be displayed in the intended order
            //  Let's be nice and handle this for the caller
            Font[] charToWrite = characters.ToArray().Reverse().ToArray();
            bool[] decimalsToWrite = decimalsEnabled?.Reverse().ToArray() ?? Enumerable.Repeat(false, _pinScheme.DigitCount).ToArray();

            for (int idx = 0; idx < _digits.Count; idx++)
            {
                GpioPin digit = _digits[idx];
                WriteAll(_digits, _pinScheme.DigitDisplayOff);
                digit.Write(!_pinScheme.DigitDisplayOff);

                // write the number
                Font currentNumToWrite = charToWrite[idx];
                var mapping = SegmentHelpers.GetPinValuesFromFont(currentNumToWrite);
                for (int i = 0; i < mapping.Length; i++)
                {
                    PinValue val = mapping[i];
                    _segments[i].Write(val);
                }

                // DP handling per digit
                _segments.Last().Write(decimalsToWrite?[idx] ?? false);

                Thread.Sleep(5);
            }

            static void WriteAll(List<GpioPin> pins, PinValue pinValue) => pins.ForEach(p => p.Write(pinValue));
        }

        /// <inheritdoc/>
        public void Write(ReadOnlySpan<Segment> digits, int startAddress = 0)
        {
            throw new NotImplementedException();
        }

        /// <inheritdoc/>
        public void Write(ReadOnlySpan<Font> characters, int startAddress = 0)
        {
            Write(characters, decimalsEnabled: null);
        }

        /// <summary>
        /// Dispose pattern
        /// </summary>
        /// <param name="disposing">Indicates type is under active disposal</param>
        protected virtual void Dispose(bool disposing)
        {
            if (!_disposedValue)
            {
                if (disposing)
                {
                    Clear();
                    if (_shouldDispose)
                    {
                        _gpioController?.Dispose();
                    }
                    else
                    {
                        Close();
                    }
                }

                _disposedValue = true;
            }
        }

        /// <inheritdoc />
        public void Dispose()
        {
            Dispose(disposing: true);
            GC.SuppressFinalize(this);
        }
    }
}
