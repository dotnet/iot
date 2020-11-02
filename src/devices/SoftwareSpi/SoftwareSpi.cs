// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System;
using System.Threading;
using System.Device.Gpio;
using System.Device.Spi;

namespace Iot.Device.Spi
{
    /// <summary>
    /// Software SPI implementation
    /// </summary>
    public class SoftwareSpi : SpiDevice
    {
        private readonly int _clk;
        private readonly int _miso;
        private readonly int _mosi;
        private readonly int _cs;
        private readonly SpiConnectionSettings _settings;
        private readonly bool _shouldDispose;
        private GpioController _controller;

        /// <summary>
        /// Software implementation of the SPI.
        /// </summary>
        /// <remarks>Note that there is a ChipSelectLine in the SPIConnectionSettings as well, either that or the cs property will be used.</remarks>
        /// <param name="clk">Clock pin.</param>
        /// <param name="miso">Master Input Slave Output pin. Optional, set to -1 to ignore</param>
        /// <param name="mosi">Master Output Slave Input pin.</param>
        /// <param name="cs">Chip select pin (or negated chip select). Optional, set to -1 to ignore.</param>
        /// <param name="settings">Settings of the SPI connection.</param>
        /// <param name="controller">GPIO controller used for pins.</param>
        /// <param name="shouldDispose">True to dispose the Gpio Controller</param>
        public SoftwareSpi(int clk, int miso, int mosi, int cs = -1, SpiConnectionSettings settings = null, GpioController controller = null, bool shouldDispose = true)
        {
            _controller = controller ?? new GpioController();
            _shouldDispose = controller == null ? true : shouldDispose;

            _settings = settings ?? new SpiConnectionSettings(-1, -1);

            _clk = clk;
            _miso = miso;
            _mosi = mosi;

            if (_cs != -1 && _settings.ChipSelectLine != -1 && _cs != _settings.ChipSelectLine)
            {
                throw new ArgumentException("Both cs and settings.ChipSelectLine can't both be set");
            }

            if (cs != -1)
            {
                _cs = cs;
            }
            else
            {
                _cs = _settings.ChipSelectLine;
            }

            _controller.OpenPin(_clk, PinMode.Output);
            if (_miso != -1)
            {
                _controller.OpenPin(_miso, PinMode.Input);
            }

            _controller.OpenPin(_mosi, PinMode.Output);
            if (_cs != -1)
            {
                _controller.OpenPin(_cs, PinMode.Output);
            }

            // aka. CPOL - tells us which state of the clock means idle (false means 'low' or 'ground' or '0')
            bool idle = ((int)_settings.Mode & 0b10) == 0b10;

            // aka. CPHA - tells us when read/write is 'captured'
            bool onPulseEnd = ((int)_settings.Mode & 1) == 1;

            if (_cs != -1)
            {
                _controller.Write(_cs, !(bool)_settings.ChipSelectLineActiveState);
            }

            _controller.Write(_clk, idle);

            // TODO: To respect ClockFrequency we need to inject the right delays here
            //       and have some very accurate way to measure time.
            //       Ideally we should verify the output with an oscilloscope.

            // pulse start   pulse end
            //       v       v
            //       ---------
            //       |       |
            // ------         ------
            //  idle   !idle   idle
            // note: vertical axis represents idle or !idle (pulse) state and is orthogonal
            //       to low/high related with GPIO - that part is defined by SPI Mode which
            //       tells us what GPIO state represents idle and also if the measurement happens
            //       on pulse start or end
            if (onPulseEnd)
            {
                // When we capture onPulseEnd then we need to start pulse before we send the data
                // and then trigger the capture on exit
                _bitTransfer = new ScopeData(
                    enter: () =>
                    {
                        _controller.Write(_clk, !idle);
                    },
                    exit: () =>
                    {
                        controller.Write(_clk, idle);
                    });
            }
            else
            {
                _bitTransfer = new ScopeData(
                    exit: () =>
                    {
                        _controller.Write(_clk, !idle);
                        _controller.Write(_clk, idle);
                    });
            }

            if (_cs != -1)
            {
                _chipSelect = new ScopeData(
                    enter: () =>
                    {
                        _controller.Write(_cs, _settings.ChipSelectLineActiveState);
                    },
                    exit: () =>
                    {
                        _controller.Write(_cs, !(bool)_settings.ChipSelectLineActiveState);
                    });
            }
            else
            {
                _chipSelect = new ScopeData();
            }
        }

        /// <inheritdoc />
        public override SpiConnectionSettings ConnectionSettings => _settings;

        /// <inheritdoc />
        public override void TransferFullDuplex(ReadOnlySpan<byte> dataToWrite, Span<byte> dataToRead)
        {
            if (dataToWrite.Length != dataToRead.Length)
            {
                throw new ArgumentException(nameof(dataToWrite));
            }

            if (_miso == -1)
            {
                throw new ArgumentException("Cannot read without a miso pin specified");
            }

            int bitLen = _settings.DataBitLength;
            int lastBit = bitLen - 1;

            using (StartChipSelect())
            {
                for (int i = 0; i < dataToRead.Length; i++)
                {
                    byte readByte = 0;
                    for (int j = 0; j < bitLen; j++)
                    {
                        using (StartBitTransfer())
                        {
                            int bit = _settings.DataFlow == DataFlow.MsbFirst ? lastBit - j : j;
                            bool bitToWrite = ((dataToWrite[i] >> bit) & 1) == 1;
                            if (ReadWriteBit(bitToWrite))
                            {
                                readByte |= (byte)(1 << bit);
                            }
                        }
                    }

                    dataToRead[i] = readByte;
                }
            }
        }

        private void TransferWriteOnly(ReadOnlySpan<byte> dataToWrite)
        {
            int bitLen = _settings.DataBitLength;
            int lastBit = bitLen - 1;

            using (StartChipSelect())
            {
                for (int i = 0; i < dataToWrite.Length; i++)
                {
                    for (int j = 0; j < bitLen; j++)
                    {
                        using (StartBitTransfer())
                        {
                            int bit = _settings.DataFlow == DataFlow.MsbFirst ? lastBit - j : j;
                            bool bitToWrite = ((dataToWrite[i] >> bit) & 1) == 1;
                            WriteBit(bitToWrite);
                        }
                    }
                }
            }
        }

        private bool ReadWriteBit(bool bitToWrite)
        {
            // _mosi is checked higher up the call path
            _controller.Write(_mosi, bitToWrite);
            return (bool)_controller.Read(_miso);
        }

        private void WriteBit(bool bitToWrite)
        {
            _controller.Write(_mosi, bitToWrite);
        }

        /// <inheritdoc />
        public override void Read(Span<byte> data)
        {
            Span<byte> dataToWrite = stackalloc byte[data.Length];
            TransferFullDuplex(dataToWrite, data);
        }

        /// <inheritdoc />
        public override void Write(ReadOnlySpan<byte> data)
        {
            TransferWriteOnly(data);
        }

        /// <inheritdoc />
        public override void WriteByte(byte data)
        {
            Span<byte> outData = stackalloc byte[1];
            outData[0] = data;
            Write(outData);
        }

        /// <inheritdoc />
        public override byte ReadByte()
        {
            Span<byte> data = stackalloc byte[1];
            Read(data);
            return data[0];
        }

        /// <inheritdoc />
        protected override void Dispose(bool disposing)
        {
            if (_shouldDispose)
            {
                _controller?.Dispose();
                _controller = null;
            }

            base.Dispose(disposing);
        }

        private readonly ScopeData _bitTransfer;
        private readonly ScopeData _chipSelect;
        private Scope StartBitTransfer() => new Scope(_bitTransfer);
        private Scope StartChipSelect() => new Scope(_chipSelect);

        private class ScopeData
        {
            internal Action _enter;
            internal Action _exit;

            public ScopeData(Action enter = null, Action exit = null)
            {
                _enter = enter ?? new Action(() => { });
                _exit = exit ?? new Action(() => { });
            }

            public void Enter()
            {
                _enter.Invoke();
            }

            public void Exit()
            {
                _exit.Invoke();
            }
        }

        private struct Scope : IDisposable
        {
            internal readonly ScopeData _data;

            public Scope(ScopeData data)
            {
                _data = data ?? throw new ArgumentNullException(nameof(data));
                data.Enter();
            }

            public void Dispose()
            {
                _data.Exit();
            }
        }
    }
}
