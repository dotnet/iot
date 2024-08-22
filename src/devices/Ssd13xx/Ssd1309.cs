// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System;
using System.Collections.Generic;
using System.Device.Gpio;
using System.Device.Spi;
using System.Linq;
using System.Threading;
using Iot.Device.Graphics;
using Iot.Device.Ssd13xx.Commands;
using Iot.Device.Ssd13xx.Commands.Ssd1306Commands;

namespace Iot.Device.Ssd13xx
{
    /// <summary>
    /// A single-chip CMOS OLED/PLED driver with controller for organic/polymer
    /// light emitting diode dot-matrix graphic display system.
    /// </summary>
    public class Ssd1309 : Ssd13xx
    {
        private readonly GpioPin _csGpioPin;
        private readonly GpioPin _dcGpioPin;
        private readonly GpioPin _rstGpioPin;

        private GpioController _gpioController;

        /// <summary>
        /// Initializes new instance of Ssd13069 device that will communicate using SPI
        /// in a non-traditional "4-wire SPI" mode that uses DC and RST GPIO pins to switch
        /// between Data/Command instructions and reset behavior.
        /// A single-chip CMOS OLED/PLED driver with controller for organic/polymer
        /// light emitting diode dot-matrix graphic display system.
        /// </summary>
        /// <param name="spiDevice">The SPI device used for communication.</param>
        /// <param name="width">Width of the display. Typically 128 pixels</param>
        /// <param name="height">Height of the display</param>
        /// <param name="gpioController">Instance of the boards GpioController</param>
        /// <param name="csGpioPin">GPIO pin for chip-select. Active state is LOW. Does not guarantee support for multiple SPI bus devices.</param>
        /// <param name="dcGpioPin">GPIO pin for Data/Command control</param>
        /// <param name="rstGpioPin">GPIO pin for Reset behavior</param>
        public Ssd1309(SpiDevice spiDevice, GpioController gpioController, int csGpioPin, int dcGpioPin, int rstGpioPin, int width, int height)
        : base(spiDevice, width, height)
        {
            _gpioController = gpioController;

            _csGpioPin = _gpioController.OpenPin(csGpioPin, PinMode.Output, PinValue.High);
            _dcGpioPin = _gpioController.OpenPin(dcGpioPin, PinMode.Output, PinValue.Low);
            _rstGpioPin = _gpioController.OpenPin(rstGpioPin, PinMode.Output, PinValue.High);

            Reset();
            Initialize();
        }

        /// <summary>
        /// Sends command to the device
        /// </summary>
        /// <param name="command">Command being sent</param>
        public virtual void SendCommand(ISsd1309Command command) => SendCommand((ICommand)command);

        /// <summary>
        /// Sends command to the device
        /// </summary>
        /// <param name="command">Command being sent</param>
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

            // TODO: Is this needed at all if data/command is controlled via GPIO instead of a control byte?
            Span<byte> writeBuffer = SliceGenericBuffer(commandBytes.Length);

            commandBytes.CopyTo(writeBuffer.Slice(0));

            if (SpiDevice != null)
            {
                // Begin consuming SPI bus data
                // Because the timing is not perfect, this may have adverse side-effects when using multiple SPI bus devices
                _csGpioPin.Write(PinValue.Low);

                // Enable command mode
                _dcGpioPin.Write(PinValue.Low);
                SpiDevice.Write(writeBuffer);

                // Stop consuming SPI bus data
                _csGpioPin.Write(PinValue.High);
            }
            else
            {
                throw new InvalidOperationException("No SPI device available or it has been disposed.");
            }
        }

        /// <summary>
        /// Sends data to the device
        /// </summary>
        /// <param name="data">Data being sent</param>
        public override void SendData(Span<byte> data)
        {
            if (data.IsEmpty)
            {
                throw new ArgumentNullException(nameof(data));
            }

            if (SpiDevice != null)
            {
                // Begin consuming SPI bus data
                // Because the timing is not perfect, this may have adverse side-effects when using multiple SPI bus devices
                _csGpioPin.Write(PinValue.Low);

                // Enable data mode
                _dcGpioPin.Write(PinValue.High);
                SpiDevice.Write(data);

                // Stop consuming SPI bus data
                _csGpioPin.Write(PinValue.High);
            }
            else
            {
                throw new InvalidOperationException("No SPI device available or it has been disposed.");
            }
        }

        /// <summary>
        /// Init 128x64
        /// </summary>
        protected virtual void Initialize()
        {
            SendCommand(new SetDisplayOff());
            SendCommand(new SetDisplayClockDivideRatioOscillatorFrequency());
            SendCommand(new SetMultiplexRatio());
            SendCommand(new SetDisplayOffset());
            SendCommand(new SetDisplayStartLine());
            SendCommand(new SetMemoryAddressingMode(SetMemoryAddressingMode.AddressingMode.Horizontal));
            SetStartAddress();
            SendCommand(new SetSegmentReMap(true));
            SendCommand(new SetComOutputScanDirection(false));
            SendCommand(new SetComPinsHardwareConfiguration());
            SendCommand(new SetContrastControlForBank0());
            SendCommand(new SetPreChargePeriod());
            SendCommand(new SetVcomhDeselectLevel());
            SendCommand(new EntireDisplayOn(false));
            SendCommand(new SetNormalDisplay());
            SendCommand(new SetDisplayOn());
            Thread.Sleep(200);
            ClearScreen();
        }

        /// <summary>
        /// Set the start address for the display
        /// </summary>
        protected override void SetStartAddress()
        {
            SendCommand(new SetColumnAddress());

            if (ScreenHeight == 32)
            {
                SendCommand(new SetPageAddress(PageAddress.Page0, PageAddress.Page3));
            }
            else
            {
                SendCommand(new SetPageAddress());
            }
        }

        /// <summary>
        /// Reset device by cycling RST pin to LOW and resting at HIGH.
        /// This will not re-initialize the device.
        /// </summary>
        protected virtual void Reset()
        {
            _rstGpioPin.Write(PinValue.Low);
            Thread.Sleep(100);
            _rstGpioPin.Write(PinValue.High);
            Thread.Sleep(100);
        }

        /// <summary>
        /// Sends the image to the display
        /// </summary>
        /// <param name="image">Image to send to display</param>
        public override void DrawBitmap(BitmapImage image)
        {
            if (!CanConvertFromPixelFormat(image.PixelFormat))
            {
                throw new InvalidOperationException($"{image.PixelFormat} is not a supported pixel format");
            }

            SetStartAddress();

            int width = ScreenWidth;
            int pages = image.Height / 8;
            List<byte> buffer = new();

            for (int page = 0; page < pages; page++)
            {
                for (int x = 0; x < width; x++)
                {
                    int bits = 0;
                    for (byte bit = 0; bit < 8; bit++)
                    {
                        bits = bits << 1;
                        bits |= image[x, page * 8 + 7 - bit].GetBrightness() > BrightnessThreshold ? 1 : 0;
                    }

                    buffer.Add((byte)bits);
                }
            }

            int chunk_size = 16;
            for (int i = 0; i < buffer.Count; i += chunk_size)
            {
                SendData(buffer.Skip(i).Take(chunk_size).ToArray());
            }
        }
    }
}
