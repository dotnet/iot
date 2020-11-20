using System;
using System.Collections.Generic;
using System.Device.I2c;
using System.Text;

namespace Iot.Device.Board
{
    internal class I2cDeviceManager : I2cDevice
    {
        private readonly Board _board;
        private readonly int _sdaPin;
        private readonly int _sclPin;
        private I2cDevice _i2cDeviceImplementation;

        public I2cDeviceManager(Board board, I2cConnectionSettings settings, int[]? pins, Func<I2cConnectionSettings, int[], I2cDevice> creationOperation)
        {
            if (pins == null || pins.Length != 2)
            {
                throw new ArgumentException("Must provide a valid set of 2 pins", nameof(pins));
            }

            _board = board;
            _sdaPin = pins[0];
            _sclPin = pins[1];
            try
            {
                _board.ReservePin(_sdaPin, PinUsage.I2c, this);
                _board.ReservePin(_sclPin, PinUsage.I2c, this);
                _i2cDeviceImplementation = creationOperation(settings, pins);
            }
            catch (Exception)
            {
                _board.ReleasePin(_sdaPin, PinUsage.I2c, this);
                _board.ReleasePin(_sclPin, PinUsage.I2c, this);
                throw;
            }
        }

        public override I2cConnectionSettings ConnectionSettings
        {
            get
            {
                if (_i2cDeviceImplementation == null)
                {
                    throw new ObjectDisposedException("I2c Device");
                }

                return _i2cDeviceImplementation.ConnectionSettings;
            }
        }

        internal I2cDevice RawDevice
        {
            get
            {
                return _i2cDeviceImplementation;
            }
        }

        public override byte ReadByte()
        {
            if (_i2cDeviceImplementation == null)
            {
                throw new ObjectDisposedException("I2c Device");
            }

            return _i2cDeviceImplementation.ReadByte();
        }

        public override void Read(Span<byte> buffer)
        {
            if (_i2cDeviceImplementation == null)
            {
                throw new ObjectDisposedException("I2c Device");
            }

            _i2cDeviceImplementation.Read(buffer);
        }

        public override void WriteByte(byte value)
        {
            if (_i2cDeviceImplementation == null)
            {
                throw new ObjectDisposedException("I2c Device");
            }

            _i2cDeviceImplementation.WriteByte(value);
        }

        public override void Write(ReadOnlySpan<byte> buffer)
        {
            if (_i2cDeviceImplementation == null)
            {
                throw new ObjectDisposedException("I2c Device");
            }

            _i2cDeviceImplementation.Write(buffer);
        }

        public override void WriteRead(ReadOnlySpan<byte> writeBuffer, Span<byte> readBuffer)
        {
            if (_i2cDeviceImplementation == null)
            {
                throw new ObjectDisposedException("I2c Device");
            }

            _i2cDeviceImplementation.WriteRead(writeBuffer, readBuffer);
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                if (_i2cDeviceImplementation != null)
                {
                    _i2cDeviceImplementation.Dispose();
                    _board.ReleasePin(_sdaPin, PinUsage.I2c, this);
                    _board.ReleasePin(_sclPin, PinUsage.I2c, this);
                }

                // So we don't release pins a second time
                _i2cDeviceImplementation = null!;
            }

            base.Dispose(disposing);
        }
    }
}
