// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System;
using System.Collections.Generic;
using System.Device.Gpio;
using System.Device.Spi;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Iot.Device.Ili934x
{
    /// <summary>
    /// Ili9342 QVGA display
    /// </summary>
    public class Ili9342 : Ili9341
    {
        /// <summary>
        /// Initializes new instance of ILI9342 device that will communicate using SPI bus.
        /// </summary>
        /// <param name="spiDevice">The SPI device used for communication. This Spi device will be displayed along with the ILI9341 device.</param>
        /// <param name="dataCommandPin">The id of the GPIO pin used to control the DC line (data/command). This pin must be provided.</param>
        /// <param name="resetPin">The id of the GPIO pin used to control the /RESET line (RST). Can be -1 if not connected</param>
        /// <param name="backlightPin">The pin for turning the backlight on and off, or -1 if not connected.</param>
        /// <param name="spiBufferSize">The size of the SPI buffer. If data larger than the buffer is sent then it is split up into multiple transmissions. The default value is 4096.</param>
        /// <param name="gpioController">The GPIO controller used for communication and controls the the <paramref name="resetPin"/> and the <paramref name="dataCommandPin"/>
        /// If no Gpio controller is passed in then a default one will be created and disposed when ILI9341 device is disposed.</param>
        /// <param name="shouldDispose">True to dispose the Gpio Controller when done</param>
        public Ili9342(SpiDevice spiDevice, int dataCommandPin, int resetPin, int backlightPin = -1, int spiBufferSize = 4096, GpioController? gpioController = null, bool shouldDispose = true)
            : base(spiDevice, dataCommandPin, resetPin, backlightPin, spiBufferSize, gpioController, shouldDispose)
        {
        }

        /// <inheritdoc />
        public override int ScreenHeight => 240;

        /// <inheritdoc />
        public override int ScreenWidth => 320;

        /// <summary>
        /// Configure the Ili9342 (it uses a different color format than the 9341 and by default is used in landscape mode)
        /// </summary>
        protected override void InitDisplayParameters()
        {
            SendCommand(Ili9341Command.MemoryAccessControl, 0b1100); // Landscape orientation, inverted color order
            SendCommand(Ili9341Command.ColModPixelFormatSet, 0x55); // 16-bits per pixel
            SendCommand(Ili9341Command.FrameRateControlInNormalMode, 0x00, 0x1B);
            SendCommand(Ili9341Command.GammaSet, 0x01);
            SendCommand(Ili9341Command.ColumnAddressSet, 0x00, 0x00, 0x01, 0x3F); // 319 width of the screen
            SendCommand(Ili9341Command.PageAddressSet, 0x00, 0x00, 0x00, 0xEF); // 239 height of the screen
            SendCommand(Ili9341Command.EntryModeSet, 0x07);
            SendCommand(Ili9341Command.DisplayFunctionControl, 0x0A, 0x82, 0x27, 0x00);
            SendCommand(Ili9341Command.DisplayInversionOn); // When enabling display inversion, the colors work the same as for the ILI9341
        }
    }
}
