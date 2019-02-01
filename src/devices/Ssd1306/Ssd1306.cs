// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using Iot.Device.Ssd1306.Command;
using System;
using System.Device.I2c;

namespace Iot.Device.Ssd1306
{
    public class Ssd1306 : IDisposable
    {
        private I2cDevice _i2cDevice;

        public Ssd1306(I2cDevice i2cDevice)
        {
            _i2cDevice = i2cDevice;
        }

        public void SendCommand(ICommand command, bool continuation = false)
        {
            byte[] commandBytes = command.GetBytes();
            byte[] writeBuffer = new byte[commandBytes.Length + 1];
            commandBytes.CopyTo(writeBuffer, 1);

            if (continuation)
            {
                writeBuffer[0] = 0x80;  // Update Control byte.
            }

            _i2cDevice.Write(writeBuffer);
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
            _i2cDevice.Write(writeBuffer);
        }

        public void Dispose()
        {
            _i2cDevice?.Dispose();
            _i2cDevice = null;
        }
    }
}
