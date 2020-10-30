using System;
using System.Collections.Generic;
using System.Device.Gpio.I2c;
using System.Device.I2c;
using System.Linq;
using System.Text;

#pragma warning disable CS1591
namespace Iot.Device.Arduino
{
    /// <summary>
    /// Implementation of an I2C device connected to an arduino using the Firmata protocol
    /// </summary>
    internal class ArduinoI2cDevice : I2cDevice
    {
        private readonly ArduinoBoard _board;

        public ArduinoI2cDevice(ArduinoBoard board, I2cConnectionSettings connectionSettings)
        {
            _board = board ?? throw new ArgumentNullException(nameof(board));

            if (connectionSettings == null)
            {
                throw new ArgumentNullException(nameof(connectionSettings));
            }

            if (connectionSettings.BusId != 0)
            {
                throw new NotSupportedException("Only I2C Bus 0 is supported");
            }

            if (connectionSettings.DeviceAddress > 127)
            {
                throw new NotSupportedException("The device address must be less than 128.");
            }

            ConnectionSettings = connectionSettings;

            // Ensure the corresponding pins are set to I2C (not strictly necessary, but consistent)
            foreach (SupportedPinConfiguration supportedPinConfiguration in _board.SupportedPinConfigurations.Where(x => x.PinModes.Contains(SupportedMode.I2C)))
            {
                _board.Firmata.SetPinMode(supportedPinConfiguration.Pin, SupportedMode.I2C);
            }

            _board.Firmata.SendI2cConfigCommand();

            // Sometimes, the very first I2C command fails (nothing happens), so try reading a byte
            int retries = 3;
            while (retries-- > 0)
            {
                try
                {
                    ReadByte();
                    break;
                }
                catch (Exception x) when (x is TimeoutException || x is I2cCommunicationException)
                {
                }
            }

            // If the above still failed, there's probably no device on the other end. But this shall not throw here but only if
            // the client calls ReadByte.
        }

        public override I2cConnectionSettings ConnectionSettings
        {
            get;
        }

        public override byte ReadByte()
        {
            Span<byte> bytes = stackalloc byte[1];
            bytes[0] = 0;
            _board.Firmata.WriteReadI2cData(ConnectionSettings.DeviceAddress, null, bytes);
            return bytes[0];
        }

        public override void Read(Span<byte> buffer)
        {
            _board.Firmata.WriteReadI2cData(ConnectionSettings.DeviceAddress, null, buffer);
        }

        public override void WriteByte(byte value)
        {
            Span<byte> bytes = stackalloc byte[1];
            bytes[0] = value;
            _board.Firmata.WriteReadI2cData(ConnectionSettings.DeviceAddress, bytes, null);
        }

        public override void Write(ReadOnlySpan<byte> buffer)
        {
            _board.Firmata.WriteReadI2cData(ConnectionSettings.DeviceAddress, buffer, null);
        }

        public override void WriteRead(ReadOnlySpan<byte> writeBuffer, Span<byte> readBuffer)
        {
            _board.Firmata.WriteReadI2cData(ConnectionSettings.DeviceAddress, writeBuffer, readBuffer);
        }
    }
}
