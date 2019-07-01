// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using System;
using System.Device.I2c.Devices;
using Iot.Device.Ssd13xx.Commands;

namespace Iot.Device.Ssd13xx
{
    /// <summary>
    /// A single-chip CMOS OLED/PLED driver with controller for organic/polymer
    /// light emitting diode dot-matrix graphic display system. 
    /// </summary>
    public class Ssd1306 : Ssd13xx
    {
        /// <summary>
        /// Initializes new instance of Ssd1306 device that will communicate using I2C bus.
        /// A single-chip CMOS OLED/PLED driver with controller for organic/polymer
        /// light emitting diode dot-matrix graphic display system. 
        /// </summary>
        /// <param name="i2cDevice">>The I2C device used for communication.</param>
        public Ssd1306(I2cDevice i2cDevice)
        : base(i2cDevice)
        {
        }

        public void SendCommand(ISsd1306Command command)
        {
            SendCommand((ICommand)command);
        }

        public override void SendCommand(ISharedCommand command)
        {
            SendCommand(command);
        }

        /// <summary>
        /// Send a command to the display controller.
        /// </summary>
        /// <param name="command">The command to send to the display controller.</param>
        private void SendCommand(ICommand command)
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

            Span<byte> writeBuffer = SliceGenericBuffer(commandBytes.Length + 1);

            commandBytes.CopyTo(writeBuffer.Slice(1));

            // Be aware there is a Continuation Bit in the Control byte and can be used
            // to state (logic LOW) if there is only data bytes to follow.
            // This binding separates commands and data by using SendCommand and SendData.

            _i2cDevice.Write(writeBuffer);
        }
    }
}
