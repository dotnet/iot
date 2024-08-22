// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System;
using System.Device.I2c;
using Iot.Device.Ssd13xx.Commands;
using Ssd1306Cmnds = Iot.Device.Ssd13xx.Commands.Ssd1306Commands;

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
        /// <param name="i2cDevice">The I2C device used for communication.</param>
        /// <param name="width">Width of the display. Typically 128 pixels</param>
        /// <param name="height">Height of the display, variants with 32 or 64 rows exist</param>
        public Ssd1306(I2cDevice i2cDevice, int width, int height)
        : base(i2cDevice, width, height)
        {
            Initialize();
        }

        /// <summary>
        /// Sends command to the device
        /// </summary>
        /// <param name="command">Command being send</param>
        public void SendCommand(ISsd1306Command command) => SendCommand((ICommand)command);

        /// <summary>
        /// Sends command to the device
        /// </summary>
        /// <param name="command">Command being send</param>
        public override void SendCommand(ISharedCommand command) => SendCommand(command);

        /// <summary>
        /// Send a command to the display controller.
        /// </summary>
        /// <param name="command">The command to send to the display controller.</param>
        private void SendCommand(ICommand command)
        {
            Span<byte> commandBytes = command?.GetBytes();

            if (commandBytes is not { Length: >0 })
            {
                throw new ArgumentNullException(nameof(command), "Argument is either null or there were no bytes to send.");
            }

            Span<byte> writeBuffer = SliceGenericBuffer(commandBytes.Length + 1);
            writeBuffer[0] = 0x00; // Control byte
            commandBytes.CopyTo(writeBuffer.Slice(1));

            // Be aware there is a Continuation Bit in the Control byte and can be used
            // to state (logic LOW) if there is only data bytes to follow.
            // This binding separates commands and data by using SendCommand and SendData.
            I2cDevice?.Write(writeBuffer);
        }

        // Display size 128x32 or 128x64
        private void Initialize()
        {
            SendCommand(new SetDisplayOff());
            SendCommand(new Ssd1306Cmnds.SetDisplayClockDivideRatioOscillatorFrequency(0x00, 0x08));
            SendCommand(new SetMultiplexRatio(0x1F));
            SendCommand(new Ssd1306Cmnds.SetDisplayOffset(0x00));
            SendCommand(new Ssd1306Cmnds.SetDisplayStartLine(0x00));
            SendCommand(new Ssd1306Cmnds.SetChargePump(true));
            SendCommand(new Ssd1306Cmnds.SetMemoryAddressingMode(Ssd1306Cmnds.SetMemoryAddressingMode.AddressingMode.Horizontal));
            SendCommand(new Ssd1306Cmnds.SetSegmentReMap(true));
            SendCommand(new Ssd1306Cmnds.SetComOutputScanDirection(false));
            SendCommand(new Ssd1306Cmnds.SetComPinsHardwareConfiguration(false, false));
            SendCommand(new SetContrastControlForBank0(0x8F));
            SendCommand(new Ssd1306Cmnds.SetPreChargePeriod(0x01, 0x0F));
            SendCommand(new Ssd1306Cmnds.SetVcomhDeselectLevel(Ssd1306Cmnds.SetVcomhDeselectLevel.DeselectLevel.Vcc1_00));
            SendCommand(new Ssd1306Cmnds.EntireDisplayOn(false));
            SendCommand(new Ssd1306Cmnds.SetNormalDisplay());
            SendCommand(new SetDisplayOn());
            SendCommand(new Ssd1306Cmnds.SetColumnAddress());
            SendCommand(new Ssd1306Cmnds.SetPageAddress(Ssd1306Cmnds.PageAddress.Page1, Ssd1306Cmnds.PageAddress.Page3));
            ClearScreen();
        }

        /// <inheritdoc />
        protected override void SetStartAddress()
        {
            SendCommand(new Ssd1306Cmnds.SetColumnAddress());
            SendCommand(new Ssd1306Cmnds.SetPageAddress(Ssd1306Cmnds.PageAddress.Page0,
                Ssd1306Cmnds.PageAddress.Page3));
        }
    }
}
