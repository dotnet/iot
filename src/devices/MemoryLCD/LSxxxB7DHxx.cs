// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using System;
using System.Device.Gpio;
using System.Device.Spi;
using System.Threading;

namespace Iot.Device.MemoryLcd
{
    /// <summary>
    /// Memory LCD
    /// </summary>
    public abstract class LSxxxB7DHxx
    {
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
        private int _scs;

        /// <summary>
        /// Display ON/OFF signal<br/>The display ON/OFF signal is only for display. Data in the memory will be saved at the time of ON/OFF.<br/>When it’s "H", data in the memory will display, when it’s "L", white color will diaplay and data in the memory will be saved.
        /// </summary>
        private int _disp;

        /// <summary>
        /// External COM inversion signal input (H: enable)<br/>When EXTMODE is "Lo", connect the EXTCOMIN to VSS.
        /// </summary>
        private int _extcomin;
        #endregion

        #region  Delay constants for LCD timing

        private static readonly int s_powerup_disp_delay = 1; // (>30us)
        private static readonly int s_powerup_extcomin_delay = 1; // (>30us)
        // private static readonly int s_scs_high_delay = 0; // (>3us)
        // private static readonly int s_scs_low_delay = 0; // (>1us)
        // private static readonly int s_interframe_delay = 0; // (>1us)
        private static readonly int s_ts_scs = 0; // >6us
        private static readonly int s_th_scs = 0; // >2us
        #endregion

        private readonly GpioController _gpio;
        private readonly SpiDevice _spi;

        /// <summary>
        /// Create a memory LCD device
        /// </summary>
        /// <param name="spi">SPI controller</param>
        /// <param name="gpio">GPIO controller</param>
        /// <param name="scs">Chip select signal</param>
        /// <param name="disp">Display ON/OFF signal</param>
        /// <param name="extcomin">External COM inversion signal input</param>
        internal LSxxxB7DHxx(SpiDevice spi, GpioController gpio, int scs, int disp, int extcomin)
        {
            _spi = spi ?? throw new ArgumentNullException(nameof(spi));
            _gpio = gpio;
            _scs = scs;
            _disp = disp;
            _extcomin = extcomin;

            BytesPerLine = (PixelWidth + 7) / 8;

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
            var buffer = new byte[BytesPerLine + 4];

            buffer[0] = (byte)(ModeSelectionPeriodByte.Mode | (frameInversion ? ModeSelectionPeriodByte.FrameInversion : ModeSelectionPeriodByte.Dummy));
            buffer[1] = Utility.GetAgByte(lineIndex);

            Span<byte> line = buffer.AsSpan(2, BytesPerLine);
            data.CopyTo(line);

            WriteSpi(buffer);
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
            var buffer = new byte[2 + (2 + BytesPerLine) * lineIndex.Length];

            buffer[0] = (byte)(ModeSelectionPeriodByte.Mode | (frameInversion ? ModeSelectionPeriodByte.FrameInversion : ModeSelectionPeriodByte.Dummy));

            for (var i = 0; i < lineIndex.Length; i++)
            {
                var from = BytesPerLine * i;
                var to = 1 + (2 + BytesPerLine) * i;

                buffer[to] = Utility.GetAgByte(lineIndex[i] + 1);

                Span<byte> fromLine = data.Slice(from, BytesPerLine);
                Span<byte> toLine = buffer.AsSpan(to + 1, BytesPerLine);

                fromLine.CopyTo(toLine);
            }

            WriteSpi(buffer);
        }

        /// <summary>
        /// Maintains memory internal data (maintains current display).
        /// </summary>
        public void Display(bool frameInversion = false)
        {
            // m(1), dummy(1)
            var buffer = new byte[2];

            buffer[0] = (byte)(frameInversion ? ModeSelectionPeriodByte.FrameInversion : ModeSelectionPeriodByte.Dummy);

            WriteSpi(buffer);
        }

        /// <summary>
        /// Clears memory internal data and writes white.
        /// </summary>
        public void AllClear(bool frameInversion = false)
        {
            // m(1), dummy(1)
            var buffer = new byte[2];

            buffer[0] = (byte)(ModeSelectionPeriodByte.AllClear | (frameInversion ? ModeSelectionPeriodByte.FrameInversion : ModeSelectionPeriodByte.Dummy));

            WriteSpi(buffer);
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
                    Thread.Sleep(s_powerup_disp_delay);
                }

                if (_extcomin > -1)
                {
                    _gpio.OpenPin(_extcomin, PinMode.Output);
                    _gpio.Write(_extcomin, PinValue.Low);
                    Thread.Sleep(s_powerup_extcomin_delay);
                }
            }
        }

        private void WriteSpi(ReadOnlySpan<byte> bytes)
        {
            if (_gpio != null && _scs > -1)
            {
                _gpio.Write(_scs, PinValue.High);
                Thread.Sleep(s_ts_scs);
                _spi.Write(bytes);
                Thread.Sleep(s_th_scs);
                _gpio.Write(_scs, PinValue.Low);
            }
            else
            {
                _spi.Write(bytes);
            }
        }
    }
}
