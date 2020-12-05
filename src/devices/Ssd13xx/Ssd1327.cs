// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System;
using System.Device.I2c;
using Iot.Device.Ssd13xx.Commands;
using Ssd1327Cmnds = Iot.Device.Ssd13xx.Commands.Ssd1327Commands;

namespace Iot.Device.Ssd13xx
{
    /// <summary>
    /// Represents SSD1327 OLED display
    /// </summary>
    public class Ssd1327 : Ssd13xx
    {
        private const byte Command_Mode = 0x80;
        private const byte Data_Mode = 0x40;
        private const int CleaningBufferSize = 48 * 96;

        /// <summary>
        /// Initializes new instance of Ssd1327 device that will communicate using I2C bus.
        /// </summary>
        /// <param name="i2cDevice">The I2C device used for communication.</param>
        public Ssd1327(I2cDevice i2cDevice)
            : base(i2cDevice)
        {
        }

        /// <summary>
        /// Sets column address
        /// </summary>
        /// <param name="startAddress">Start address</param>
        /// <param name="endAddress">End address</param>
        public void SetColumnAddress(byte startAddress = 0x08, byte endAddress = 0x37) => SendCommand(new Ssd1327Cmnds.SetColumnAddress(startAddress, endAddress));

        /// <summary>
        /// Sets row address
        /// </summary>
        /// <param name="startAddress">Start address</param>
        /// <param name="endAddress">End address</param>
        public void SetRowAddress(byte startAddress = 0x00, byte endAddress = 0x5f) => SendCommand(new Ssd1327Cmnds.SetRowAddress(startAddress, endAddress));

        /// <summary>
        /// Clears the display
        /// </summary>
        public void ClearDisplay()
        {
            SendCommand(new SetDisplayOff());
            SetColumnAddress();
            SetRowAddress();
            byte[] data = new byte[CleaningBufferSize];
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

        /// <summary>
        /// Sends command to the device
        /// </summary>
        /// <param name="command">Command being send</param>
        public void SendCommand(ISsd1327Command command) => SendCommand((ICommand)command);

        /// <summary>
        /// Sends command to the device
        /// </summary>
        /// <param name="command">Command being send</param>
        public override void SendCommand(ISharedCommand command) => SendCommand(command);

        /// <summary>
        /// Send data to the display controller.
        /// </summary>
        /// <param name="data">The data to send to the display controller.</param>
        public void SendData(byte data)
        {
            Span<byte> writeBuffer = new byte[] { Data_Mode, data };

            _i2cDevice.Write(writeBuffer);
        }

        private void SendCommand(ICommand command)
        {
            byte[]? commandBytes = command?.GetBytes();

            if (commandBytes is not { Length: >0 })
            {
                throw new ArgumentException(nameof(command), "Argument is either null or there were no bytes to send.");
            }

            foreach (var item in commandBytes)
            {
                SendCommand(item);
            }
        }
    }
}
