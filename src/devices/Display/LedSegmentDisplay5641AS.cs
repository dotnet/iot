// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System;
using System.Collections.Generic;
using System.Device.Gpio;
using System.Linq;
using System.Threading;
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

        private List<GpioPin> _segmentPins;
        private List<GpioPin> _digitPins;

        // address, Segment(s)
        private Dictionary<int, Segment> _displayBuffer;

        /// <inheritdoc />
        public int NumberOfDigits => _pinScheme.DigitCount;

        /// <inheritdoc />
        public Segment this[int address]
        {
            get => _displayBuffer[address];
            set => WriteDigitAtAddress(address, value);
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="LedSegmentDisplay5641AS"/> class.
        /// </summary>
        public LedSegmentDisplay5641AS(LedSegmentDisplay5641ASPinScheme pinScheme, GpioController? gpioController = null, bool shouldDispose = true)
        {
            _pinScheme = pinScheme;
            _gpioController = gpioController ?? new GpioController();
            _shouldDispose = gpioController is null || shouldDispose;

            _segmentPins = new(pinScheme.SegmentCountPerDigit);
            _digitPins = new(pinScheme.DigitCount);
            _displayBuffer = new Dictionary<int, Segment>(pinScheme.DigitCount);

            Open();
        }

        /// <summary>
        /// Opens GPIO pins defined in <see cref="LedSegmentDisplay5641ASPinScheme"/> as <see cref="PinMode.Output"/>
        /// </summary>
        private void Open()
        {
            foreach (var segmentPinNumBoard in _pinScheme.Segments)
            {
                _segmentPins.Add(_gpioController.OpenPin(segmentPinNumBoard, PinMode.Output));
            }

            foreach (var digitPinNumBoard in _pinScheme.Digits)
            {
                _digitPins.Add(_gpioController.OpenPin(digitPinNumBoard, PinMode.Output));
            }
        }

        /// <summary>
        /// Closes GPIO pins defined in <see cref="LedSegmentDisplay5641ASPinScheme"/>
        /// </summary>
        private void Close()
        {
            foreach (var segmentPinNumBoard in _pinScheme.Segments)
            {
                _gpioController.ClosePin(segmentPinNumBoard);
            }

            foreach (var digitPinNumBoard in _pinScheme.Digits)
            {
                _gpioController.ClosePin(digitPinNumBoard);
            }

            _segmentPins = new(_pinScheme.SegmentCountPerDigit);
            _digitPins = new(_pinScheme.DigitCount);
        }

        /// <summary>
        /// Clears display by writing <see cref="PinValue.Low"/> to all segments
        /// </summary>
        public void Clear()
        {
            foreach (var seg in _segmentPins)
            {
                seg?.Write(PinValue.Low);
            }
        }

        /// <inheritdoc/>
        public void Write(ReadOnlySpan<Segment> digits, int startAddress = 0)
        {
            if (digits.Length > NumberOfDigits)
            {
                throw new ArgumentOutOfRangeException(nameof(digits), $"Too many digits specified (max {NumberOfDigits} supported");
            }

            if (startAddress > NumberOfDigits - 1)
            {
                throw new ArgumentOutOfRangeException(nameof(startAddress), $"Specified digit address out of range (max {NumberOfDigits} supported");
            }

            // Characters must be written to the device in reverse order to be displayed in the intended order
            Segment[] digitsToWrite = digits.ToArray().Reverse().ToArray();

            for (int idx = startAddress; idx < NumberOfDigits; idx++)
            {
                WriteDigitAtAddress(idx, digitsToWrite[idx]);
                Thread.Sleep(5);
            }
        }

        /// <inheritdoc/>
        public void Write(ReadOnlySpan<Font> characters, int startAddress = 0)
        {
            if (characters.Length > NumberOfDigits)
            {
                throw new ArgumentException($"Too many characters specified (max {NumberOfDigits} supported)", nameof(characters));
            }

            Write(characters.ToArray().Select(c => (Segment)c).ToArray(), startAddress);
        }

        private void WriteDigitAtAddress(int digitAddress, Segment segmentsToWrite)
        {
            ActivateDigit(digitAddress);

            ReadOnlySpan<PinValue> mapping = SegmentHelpers.GetPinValuesFromSegment(segmentsToWrite);
            SetPinValues(mapping);

            _displayBuffer[digitAddress] = segmentsToWrite;
        }

        private void ActivateDigit(int idx)
        {
            GpioPin digit = _digitPins[idx];
            WriteAll(_digitPins, _pinScheme.DigitDisplayOff);
            digit.Write(!_pinScheme.DigitDisplayOff);

            static void WriteAll(List<GpioPin> pins, PinValue pinValue) => pins.ForEach(p => p.Write(pinValue));
        }

        private void SetPinValues(ReadOnlySpan<PinValue> pinValues)
        {
            for (int i = 0; i < pinValues.Length; i++)
            {
                PinValue val = pinValues[i];
                _segmentPins[i].Write(val);
            }
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
