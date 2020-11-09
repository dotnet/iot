// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System;
using System.Device;
using System.Device.Gpio;
using System.Device.Spi;
using System.Linq;

namespace Iot.Device.MemoryLcd
{
    /// <summary>
    /// Memory LCD
    /// </summary>
    public abstract class LSxxxB7DHxx : IDisposable
    {
        #region  Delay constants for LCD timing

        // see datasheet 6-2 Power Supply Sequence
        private const int T3 = 30; // Release time for initialization of TCOM latch (>30us)
        private const int T4 = 30; // TCOM polarity initialization time (>30us)

        // see datasheet 6-3 Input Signal Basic Characteristics
        private const int TsScs = 3; // SCS setup time (>3us)
        private const int ThScs = 1; // SCS hold time (>1us)
        #endregion

        #region Screen specification

        /// <summary>
        /// Memory LCD width pixels
        /// </summary>
        public abstract int PixelWidth { get; }

        /// <summary>
        /// Memory LCD height pixels
        /// </summary>
        public abstract int PixelHeight { get; }

        /// <summary>
        /// Memory LCD bytes per line
        /// </summary>
        public int BytesPerLine { get; }
        #endregion

        #region GPIO config

        /// <summary>
        /// Chip select signal
        /// </summary>
        private readonly int _scs;

        /// <summary>
        /// Display ON/OFF signal<br/>The display ON/OFF signal is only for display. Data in the memory will be saved at the time of ON/OFF.<br/>When it’s "H", data in the memory will display, when it’s "L", white color will diaplay and data in the memory will be saved.
        /// </summary>
        private readonly int _disp;

        /// <summary>
        /// External COM inversion signal input (H: enable)<br/>When EXTMODE is "Lo", connect the EXTCOMIN to VSS.
        /// </summary>
        private readonly int _extcomin;
        #endregion

        private readonly bool _shouldDispose;
        private GpioController? _gpio;

        private SpiDevice _spi;

        internal byte[] _lineNumberBuffer;
        internal byte[] _frameBuffer;

        /// <summary>
        /// Create a memory LCD device
        /// </summary>
        /// <param name="spi">SPI controller</param>
        /// <param name="gpio"><see cref="GpioController"/> related with operations on pins</param>
        /// <param name="shouldDispose">True to dispose the Gpio Controller</param>
        /// <param name="scs">Chip select signal</param>
        /// <param name="disp">Display ON/OFF signal</param>
        /// <param name="extcomin">External COM inversion signal input</param>
        internal LSxxxB7DHxx(SpiDevice spi, GpioController? gpio = null, bool shouldDispose = true, int scs = -1, int disp = -1, int extcomin = -1)
        {
            _spi = spi ?? throw new ArgumentNullException(nameof(spi));

            if (scs != -1 || disp != -1 || extcomin != -1)
            {
                _shouldDispose = gpio == null || shouldDispose;
                _gpio = gpio;
            }
            else
            {
                _shouldDispose = false;
                _gpio = null;
            }

            _scs = scs;
            _disp = disp;
            _extcomin = extcomin;

            BytesPerLine = (PixelWidth + 7) / 8;

            _lineNumberBuffer = Enumerable.Range(0, PixelHeight).Select(m => (byte)m).ToArray();
            _frameBuffer = new byte[BytesPerLine * PixelHeight];

            Init();
        }

        /// <summary>
        /// Updates data of only one specified line.
        /// </summary>
        public void DataUpdate1Line(byte lineIndex, Span<byte> data, bool frameInversion = false)
        {
            if (data.Length != BytesPerLine)
            {
                throw new Exception($"Data writing period must be {PixelWidth}ck ({BytesPerLine} bytes)");
            }

            // m(1), ag(1), d(18), dummy(2)
            Span<byte> outputBuffer = stackalloc byte[BytesPerLine + 4];

            outputBuffer[0] = (byte)(ModeSelectionPeriodByte.Mode | (frameInversion ? ModeSelectionPeriodByte.FrameInversion : ModeSelectionPeriodByte.Dummy));
            outputBuffer[1] = AgBytes.Table[lineIndex];
            data.CopyTo(outputBuffer.Slice(2, BytesPerLine));

            WriteSpi(outputBuffer);
        }

        /// <summary>
        /// Updates arbitrary multiple lines data.
        /// </summary>
        public void DataUpdateMultipleLines(Span<byte> lineIndex, Span<byte> data, bool frameInversion = false)
        {
            if (data.Length != lineIndex.Length * BytesPerLine)
            {
                throw new Exception($"Data writing period must be {lineIndex.Length * PixelWidth}ck ({lineIndex.Length * BytesPerLine} bytes)");
            }

            // m(1), ag1(1), d1(18), dummy2(1), ag2(1), d2(18), ... , dummyn(1), agn(1), dn(18), dummy(2)
            Span<byte> outputBuffer = stackalloc byte[2 + (2 + BytesPerLine) * lineIndex.Length];

            outputBuffer[0] = (byte)(ModeSelectionPeriodByte.Mode | (frameInversion ? ModeSelectionPeriodByte.FrameInversion : ModeSelectionPeriodByte.Dummy));

            for (int i = 0; i < lineIndex.Length; i++)
            {
                int from = BytesPerLine * i;
                int to = 1 + (2 + BytesPerLine) * i;

                outputBuffer[to] = AgBytes.Table[lineIndex[i] + 1];

                data.Slice(from, BytesPerLine).CopyTo(outputBuffer.Slice(to + 1, BytesPerLine));
            }

            WriteSpi(outputBuffer);
        }

        /// <summary>
        /// Maintains memory internal data (maintains current display).
        /// </summary>
        public void Display(bool frameInversion = false)
        {
            // m(1), dummy(1)
            Span<byte> outputBuffer = stackalloc byte[2];

            outputBuffer[0] = (byte)(frameInversion ? ModeSelectionPeriodByte.FrameInversion : ModeSelectionPeriodByte.Dummy);

            WriteSpi(outputBuffer);
        }

        /// <summary>
        /// Clears memory internal data and writes white.
        /// </summary>
        public void AllClear(bool frameInversion = false)
        {
            // m(1), dummy(1)
            Span<byte> outputBuffer = stackalloc byte[2];

            outputBuffer[0] = (byte)(ModeSelectionPeriodByte.AllClear | (frameInversion ? ModeSelectionPeriodByte.FrameInversion : ModeSelectionPeriodByte.Dummy));

            WriteSpi(outputBuffer);
        }

        private void Init()
        {
            if (_gpio != null)
            {
                if (_scs > -1)
                {
                    _gpio.OpenPin(_scs, PinMode.Output);
                }

                if (_disp > -1)
                {
                    _gpio.OpenPin(_disp, PinMode.Output);
                    _gpio.Write(_disp, PinValue.High);
                    DelayHelper.DelayMicroseconds(T3, true);
                }

                if (_extcomin > -1)
                {
                    _gpio.OpenPin(_extcomin, PinMode.Output);
                    _gpio.Write(_extcomin, PinValue.Low);
                    DelayHelper.DelayMicroseconds(T4, true);
                }
            }
        }

        private void WriteSpi(ReadOnlySpan<byte> bytes)
        {
            if (_gpio != null && _scs > -1)
            {
                _gpio.Write(_scs, PinValue.High);
                DelayHelper.DelayMicroseconds(TsScs, true);
                _spi.Write(bytes);
                DelayHelper.DelayMicroseconds(ThScs, true);
                _gpio.Write(_scs, PinValue.Low);
            }
            else
            {
                _spi.Write(bytes);
            }
        }

        /// <inheritdoc/>
        public void Dispose()
        {
            _spi.Dispose();

            if (_gpio != null)
            {
                if (_shouldDispose)
                {
                    _gpio.Dispose();
                }
                else
                {
                    if (_scs > -1)
                    {
                        _gpio.ClosePin(_scs);
                    }

                    if (_disp > -1)
                    {
                        _gpio.ClosePin(_disp);
                    }

                    if (_extcomin > -1)
                    {
                        _gpio.ClosePin(_extcomin);
                    }
                }

                _gpio = null;
            }
        }
    }
}
