// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using Iot.Device.Ssd1306.Command;
using System;
using System.Device.I2c;
using System.Device.Spi;

namespace Iot.Device.Ssd1306
{
    public class Ssd1306 : IDisposable
    {
        private readonly I2cDevice _i2cDevice;
        private readonly SpiDevice _spiDevice;
        private readonly ConnectionType _connectionType;

        public Ssd1306(I2cDevice i2cDevice)
        {
            _i2cDevice = i2cDevice;
            _connectionType = ConnectionType.I2c;
        }

        public Ssd1306(SpiDevice spiDevice)
        {
            _spiDevice = spiDevice;
            _connectionType = ConnectionType.Spi;
        }

        public void Dispose()
        {
        }

        public void SendCommand(ICommand command, bool continuation = false)
        {
            byte controlByte = 0x00;
            byte[] commandBytes = command.GetBytes();
            byte[] writeBuffer = new byte[commandBytes.Length + 1];
            commandBytes.CopyTo(writeBuffer, 1);

            if (continuation)
            {
                controlByte |= 0x80;
            }

            writeBuffer[0] = controlByte;

            if (_connectionType == ConnectionType.I2c)
            {
                _i2cDevice.Write(writeBuffer);
            }
            else
            {
                _spiDevice.Write(writeBuffer);
            }
        }

        public void SendData(byte[] data, bool continuation = false)
        {
            byte controlByte = 0x40;
            byte[] writeBuffer = new byte[data.Length + 1];
            data.CopyTo(writeBuffer, 1);

            if (continuation)
            {
                controlByte |= 0x80;
            }

            writeBuffer[0] = controlByte;

            if (_connectionType == ConnectionType.I2c)
            {
                _i2cDevice.Write(writeBuffer);
            }
            else
            {
                _spiDevice.Write(writeBuffer);
            }
        }
    }
}
