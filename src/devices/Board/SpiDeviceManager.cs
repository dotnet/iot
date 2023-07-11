// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System;
using System.Collections.Generic;
using System.Device;
using System.Device.Spi;
using System.Diagnostics;
using System.Linq;

namespace Iot.Device.Board
{
    internal class SpiDeviceManager : SpiDevice, IDeviceManager
    {
        private readonly Board _board;
        private readonly int[] _pins;
        private SpiDevice _device;

        public SpiDeviceManager(Board board, SpiConnectionSettings connectionSettings, int[]? pins, Func<SpiConnectionSettings, int[], SpiDevice> createOperation)
        {
            if (pins == null || pins.Length < 3 || pins.Length > 4)
            {
                throw new ArgumentException("Must provide a valid set of 3 or 4 pins", nameof(pins));
            }

            _board = board;
            ConnectionSettings = connectionSettings;
            try
            {
                _board.ReservePin(pins[0], PinUsage.Spi, this);
                _board.ReservePin(pins[1], PinUsage.Spi, this);
                _board.ReservePin(pins[2], PinUsage.Spi, this);
                if (pins.Length == 4)
                {
                    // The fourth pin, if provided, is the CS line. This may be handled manually, but if done in hardware, SPI
                    // numbering must be used here I think. So the value would be 0 or 1 for CE0 and CE1 in ALT 0 mode, which are
                    // the logical pins 7 and 8.
                    _board.ReservePin(pins[3], PinUsage.Spi, this);
                }

                _device = createOperation(connectionSettings, pins);
            }
            catch (Exception x)
            {
                // TODO: Replace with logging
                Debug.WriteLine($"Exception: {x}");
                _board.ReleasePin(pins[0], PinUsage.Spi, this);
                _board.ReleasePin(pins[1], PinUsage.Spi, this);
                _board.ReleasePin(pins[2], PinUsage.Spi, this);
                if (pins.Length == 4)
                {
                    _board.ReleasePin(pins[3], PinUsage.Spi, this);
                }

                throw;
            }

            _pins = pins;
        }

        public override SpiConnectionSettings ConnectionSettings
        {
            get;
        }

        internal SpiDevice RawDevice
        {
            get
            {
                return _device;
            }
        }

        public bool IsDisposed
        {
            get
            {
                return _device == null;
            }
        }

        public IReadOnlyCollection<int> GetActiveManagedPins()
        {
            return _pins.ToList();
        }

        public override byte ReadByte()
        {
            if (_device == null)
            {
                throw new ObjectDisposedException("SPI Device");
            }

            return _device.ReadByte();
        }

        public override void Read(Span<byte> buffer)
        {
            if (_device == null)
            {
                throw new ObjectDisposedException("SPI Device");
            }

            _device.Read(buffer);
        }

        public override void WriteByte(byte value)
        {
            if (_device == null)
            {
                throw new ObjectDisposedException("SPI Device");
            }

            _device.WriteByte(value);
        }

        public override void Write(ReadOnlySpan<byte> buffer)
        {
            if (_device == null)
            {
                throw new ObjectDisposedException("SPI Device");
            }

            _device.Write(buffer);
        }

        public override void TransferFullDuplex(ReadOnlySpan<byte> writeBuffer, Span<byte> readBuffer)
        {
            if (_device == null)
            {
                throw new ObjectDisposedException("SPI Device");
            }

            _device.TransferFullDuplex(writeBuffer, readBuffer);
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                if (_device != null)
                {
                    _device.Dispose();
                    foreach (int pin in _pins)
                    {
                        _board.ReleasePin(pin, PinUsage.Spi, this);
                    }
                }

                // Do not release pins a second time
                _device = null!;
            }

            base.Dispose(disposing);
        }

        /// <summary>
        /// Query the component information (the tree of active drivers) for diagnostic purposes.
        /// </summary>
        /// <returns>A <see cref="ComponentInformation"/> instance</returns>
        public ComponentInformation QueryComponentInformation()
        {
            return new ComponentInformation(this, $"SPI Bus Manager, Bus number {_device.ConnectionSettings.BusId}");
        }
    }
}
