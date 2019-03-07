// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using Iot.Device.Ssd13xx.Commands;
using Ssd1327Cmnds = Iot.Device.Ssd13xx.Commands.Ssd1327Commands;
using System;
using System.Device.I2c;

namespace Iot.Device.Ssd13xx
{
    public class Ssd1327 : Ssd13xx
    {
        private const byte Command_Mode = 0x80;
        private const byte Data_Mode = 0x40;

        /// <summary>
        /// Initializes new instance of Ssd1327 device that will communicate using I2C bus.
        /// </summary>
        /// <param name="i2cDevice">>The I2C device used for communication.</param>
        public Ssd1327(I2cDevice i2cDevice)
            : base(i2cDevice)
        {
        }

        public void SetColumnAddress(byte startAddress = 0x08, byte endAddress = 0x37)
        {
            SendCommand(new Ssd1327Cmnds.SetColumnAddress(startAddress, endAddress));
        }

        public void SetRowAddress(byte startAddress = 0x00, byte endAddress = 0x5f)
        {
            SendCommand(new Ssd1327Cmnds.SetRowAddress(startAddress, endAddress));
        }

        public void ClearDisplay()
        {
            SendCommand(new SetDisplayOff());
            SetColumnAddress();
            SetRowAddress();
            byte[] data = new byte[48 * 96];
            SendData(data);
            SendCommand(new SetDisplayOn());
        }
        
        /// <summary>
        /// Send a command to the display controller.
        /// </summary>
        /// <param name="command">The command to send to the display controller.</param>
        public void SendCommand(byte command)
        {
            Span<byte> writeBuffer = new byte[] { Command_Mode, command };

            _i2cDevice.Write(writeBuffer);
        }

        public void SendCommand(ISsd1327Command command)
        {
            SendCommand((ICommand)command);
        }

        public override void SendCommand(ISharedCommand command)
        {
            SendCommand((ICommand)command);
        }

        protected override void SendCommand(ICommand command)
        {
            byte[] commandBytes = command.GetBytes();

            if (commandBytes == null)
            {
                throw new ArgumentNullException(nameof(commandBytes));
            }

            if (commandBytes.Length == 0)
            {
                throw new ArgumentException("The command did not contain any bytes to send.");
            }

            foreach (var item in commandBytes)
            {
                SendCommand(item);
            }
        }

        /// <summary>
        /// Send data to the display controller.
        /// </summary>
        /// <param name="data">The data to send to the display controller.</param>
        public void SendData(byte data)
        {
            Span<byte> writeBuffer = new byte[] { Data_Mode, data };

            _i2cDevice.Write(writeBuffer);
        }
    }
}