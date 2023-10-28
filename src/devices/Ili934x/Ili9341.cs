// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System;
using System.Device.Gpio;
using System.Device.Spi;
using System.Drawing;
using System.Threading;
using System.Threading.Tasks;
using Iot.Device.Graphics;

namespace Iot.Device.Ili934x
{
    /// <summary>
    /// The ILI9341 is a QVGA (Quarter VGA) driver integrated circuit that is used to control 240×320 VGA LCD screens.
    /// </summary>
    public partial class Ili9341 : GraphicDisplay
    {
        /// <summary>
        /// Default frequency for SPI
        /// </summary>
        public const int DefaultSpiClockFrequency = 12_000_000;

        /// <summary>
        /// Default mode for SPI
        /// </summary>
        public const SpiMode DefaultSpiMode = SpiMode.Mode3;

        private const int DefaultSPIBufferSize = 0x1000;
        internal const byte LcdPortraitConfig = 8 | 0x40;
        internal const byte LcdLandscapeConfig = 44;

        private readonly int _dcPinId;
        private readonly int _resetPinId;
        private readonly int _backlightPin;
        private readonly int _spiBufferSize;
        private readonly bool _shouldDispose;

        private SpiDevice _spiDevice;
        private GpioController _gpioDevice;

        private Rgb565[] _screenBuffer;
        private Rgb565[] _previousBuffer;

        private double _fps;
        private DateTimeOffset _lastUpdate;

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
        public Ili9341(SpiDevice spiDevice, int dataCommandPin, int resetPin, int backlightPin = -1, int spiBufferSize = DefaultSPIBufferSize, GpioController? gpioController = null, bool shouldDispose = true)
        {
            if (spiBufferSize <= 0)
            {
                throw new ArgumentException("Buffer size must be larger than 0.", nameof(spiBufferSize));
            }

            _spiDevice = spiDevice;
            _dcPinId = dataCommandPin;
            _resetPinId = resetPin;
            _backlightPin = backlightPin;
            _gpioDevice = gpioController ?? new GpioController();
            _shouldDispose = shouldDispose || gpioController is null;
            _fps = 0;
            _lastUpdate = DateTimeOffset.UtcNow;

            _gpioDevice.OpenPin(_dcPinId, PinMode.Output);
            if (_resetPinId >= 0)
            {
                _gpioDevice.OpenPin(_resetPinId, PinMode.Output);
            }

            _spiBufferSize = spiBufferSize;

            if (_backlightPin != -1)
            {
                _gpioDevice.OpenPin(_backlightPin, PinMode.Output);
                TurnBacklightOn();
            }

            ResetDisplayAsync().Wait();

            SendCommand(Ili9341Command.SoftwareReset);
            SendCommand(Ili9341Command.DisplayOff);
            Thread.Sleep(10);
            InitDisplayParameters();
            SendCommand(Ili9341Command.SleepOut);
            Thread.Sleep(120);
            SendCommand(Ili9341Command.DisplayOn);
            Thread.Sleep(100);
            SendCommand(Ili9341Command.MemoryWrite);

            _screenBuffer = new Rgb565[ScreenWidth * ScreenHeight];
            _previousBuffer = new Rgb565[ScreenWidth * ScreenHeight];

            // And clear the display
            SendFrame(true);
        }

        /// <summary>
        /// Width of the screen, in pixels
        /// </summary>
        /// <remarks>This is of type int, because all image sizes use int, even though this can never be negative</remarks>
        public override int ScreenWidth => 240;

        /// <summary>
        /// Height of the screen, in pixels
        /// </summary>
        /// <remarks>This is of type int, because all image sizes use int, even though this can never be negative</remarks>
        public override int ScreenHeight => 320;

        /// <inheritdoc />
        public override PixelFormat NativePixelFormat => PixelFormat.Format16bppRgb565;

        /// <summary>
        /// Returns the last FPS value (frames per second).
        /// The value is unfiltered.
        /// </summary>
        public double Fps => _fps;

        /// <summary>
        /// Configure memory and orientation parameters
        /// </summary>
        protected virtual void InitDisplayParameters()
        {
            SendCommand(Ili9341Command.MemoryAccessControl, LcdPortraitConfig);
            SendCommand(Ili9341Command.ColModPixelFormatSet, 0x55); // 16-bits per pixel
            SendCommand(Ili9341Command.FrameRateControlInNormalMode, 0x00, 0x1B);
            SendCommand(Ili9341Command.GammaSet, 0x01);
            SendCommand(Ili9341Command.ColumnAddressSet, 0x00, 0x00, 0x00, 0xEF); // width of the screen
            SendCommand(Ili9341Command.PageAddressSet, 0x00, 0x00, 0x01, 0x3F); // height of the screen
            SendCommand(Ili9341Command.EntryModeSet, 0x07);
            SendCommand(Ili9341Command.DisplayFunctionControl, 0x0A, 0x82, 0x27, 0x00);
        }

        /// <summary>
        /// This device supports standard 32 bit formats as input
        /// </summary>
        /// <param name="format">The format to query</param>
        /// <returns>True if it is supported, false if not</returns>
        public override bool CanConvertFromPixelFormat(PixelFormat format)
        {
            return format == PixelFormat.Format32bppXrgb || format == PixelFormat.Format32bppArgb;
        }

        /// <summary>
        /// Fill rectangle to the specified color
        /// </summary>
        /// <param name="color">The color to fill the rectangle with.</param>
        /// <param name="x">The x co-ordinate of the point to start the rectangle at in pixels.</param>
        /// <param name="y">The y co-ordinate of the point to start the rectangle at in pixels.</param>
        /// <param name="w">The width of the rectangle in pixels.</param>
        /// <param name="h">The height of the rectangle in pixels.</param>
        public void FillRect(Color color, int x, int y, int w, int h)
        {
            FillRect(color, x, y, w, h, false);
        }

        /// <summary>
        /// Fill rectangle to the specified color
        /// </summary>
        /// <param name="color">The color to fill the rectangle with.</param>
        /// <param name="x">The x co-ordinate of the point to start the rectangle at in pixels.</param>
        /// <param name="y">The y co-ordinate of the point to start the rectangle at in pixels.</param>
        /// <param name="w">The width of the rectangle in pixels.</param>
        /// <param name="h">The height of the rectangle in pixels.</param>
        /// <param name="doRefresh">True to immediately update the screen, false to only update the back buffer</param>
        private void FillRect(Color color, int x, int y, int w, int h, bool doRefresh)
        {
            Span<byte> colourBytes = stackalloc byte[2]; // create a short span that holds the colour data to be sent to the display

            // set the colourbyte array to represent the fill colour
            var c = Rgb565.FromRgba32(color);

            // set the pixels in the array representing the raw data to be sent to the display
            // to the fill color
            for (int j = y; j < y + h; j++)
            {
                for (int i = x; i < x + w; i++)
                {
                    _screenBuffer[i + j * ScreenWidth] = c;
                }
            }

            if (doRefresh)
            {
                SendFrame(false);
            }
        }

        /// <summary>
        /// Clears the screen to a specific color
        /// </summary>
        /// <param name="color">The color to clear the screen to</param>
        /// <param name="doRefresh">Immediately force an update of the screen. If false, only the backbuffer is cleared.</param>
        public void ClearScreen(Color color, bool doRefresh)
        {
            FillRect(color, 0, 0, ScreenWidth, ScreenHeight, doRefresh);
        }

        /// <summary>
        /// Clears the screen to black
        /// </summary>
        /// <param name="doRefresh">Immediately force an update of the screen. If false, only the backbuffer is cleared.</param>
        public void ClearScreen(bool doRefresh)
        {
            FillRect(Color.FromArgb(0, 0, 0), 0, 0, ScreenWidth, ScreenHeight, doRefresh);
        }

        /// <summary>
        /// Immediately clears the screen to black.
        /// </summary>
        public override void ClearScreen()
        {
            ClearScreen(true);
        }

        /// <summary>
        /// Resets the display.
        /// </summary>
        public async Task ResetDisplayAsync()
        {
            if (_resetPinId < 0)
            {
                return;
            }

            _gpioDevice.Write(_resetPinId, PinValue.High);
            await Task.Delay(20).ConfigureAwait(false);
            _gpioDevice.Write(_resetPinId, PinValue.Low);
            await Task.Delay(20).ConfigureAwait(false);
            _gpioDevice.Write(_resetPinId, PinValue.High);
            await Task.Delay(20).ConfigureAwait(false);
        }

        /// <summary>
        /// This command turns the backlight panel off.
        /// </summary>
        public void TurnBacklightOn()
        {
            if (_backlightPin == -1)
            {
                throw new InvalidOperationException("Backlight pin not set");
            }

            _gpioDevice.Write(_backlightPin, PinValue.High);
        }

        /// <summary>
        /// This command turns the backlight panel off.
        /// </summary>
        public void TurnBacklightOff()
        {
            if (_backlightPin == -1)
            {
                throw new InvalidOperationException("Backlight pin not set");
            }

            _gpioDevice.Write(_backlightPin, PinValue.Low);
        }

        private void SetWindow(int x0, int y0, int x1, int y1)
        {
            SendCommand(Ili9341Command.ColumnAddressSet);
            Span<byte> data = stackalloc byte[4]
            {
                (byte)(x0 >> 8),
                (byte)x0,
                (byte)(x1 >> 8),
                (byte)x1,
            };
            SendData(data);
            SendCommand(Ili9341Command.PageAddressSet);
            Span<byte> data2 = stackalloc byte[4]
            {
                (byte)(y0 >> 8),
                (byte)y0,
                (byte)(y1 >> 8),
                (byte)y1,
            };
            SendData(data2);
            SendCommand(Ili9341Command.MemoryWrite);
        }

        /// <summary>
        /// Send a command to the the display controller along with associated parameters.
        /// </summary>
        /// <param name="command">Command to send.</param>
        /// <param name="commandParameters">parameteters for the command to be sent</param>
        internal void SendCommand(Ili9341Command command, params byte[] commandParameters)
        {
            SendCommand(command, commandParameters.AsSpan());
        }

        /// <summary>
        /// Send a command to the the display controller along with parameters.
        /// </summary>
        /// <param name="command">Command to send.</param>
        /// <param name="data">Span to send as parameters for the command.</param>
        internal void SendCommand(Ili9341Command command, Span<byte> data)
        {
            Span<byte> commandSpan = stackalloc byte[]
            {
                (byte)command
            };

            SendSPI(commandSpan, true);

            if (data != null && data.Length > 0)
            {
                SendSPI(data);
            }
        }

        /// <summary>
        /// Send data to the display controller.
        /// </summary>
        /// <param name="data">The data to send to the display controller.</param>
        private void SendData(Span<byte> data)
        {
            SendSPI(data, blnIsCommand: false);
        }

        /// <summary>
        /// Write a block of data to the SPI device
        /// </summary>
        /// <param name="data">The data to be sent to the SPI device</param>
        /// <param name="blnIsCommand">A flag indicating that the data is really a command when true or data when false.</param>
        private void SendSPI(Span<byte> data, bool blnIsCommand = false)
        {
            int index = 0;
            int len;

            // set the DC pin to indicate if the data being sent to the display is DATA or COMMAND bytes.
            _gpioDevice.Write(_dcPinId, blnIsCommand ? PinValue.Low : PinValue.High);

            // write the array of bytes to the display. (in chunks of SPI Buffer Size)
            do
            {
                // calculate the amount of spi data to send in this chunk
                len = Math.Min(data.Length - index, _spiBufferSize);
                // send the slice of data off set by the index and of length len.
                _spiDevice.Write(data.Slice(index, len));
                // add the length just sent to the index
                index += len;
            }
            while (index < data.Length); // repeat until all data sent.
        }

        /// <inheritdoc />
        public override BitmapImage GetBackBufferCompatibleImage()
        {
            return BitmapImage.CreateBitmap(ScreenWidth, ScreenHeight, PixelFormat.Format32bppArgb);
        }

        /// <inheritdoc />
        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                if (_gpioDevice != null)
                {
                    if (_resetPinId >= 0)
                    {
                        _gpioDevice.ClosePin(_resetPinId);
                    }

                    if (_backlightPin >= 0)
                    {
                        _gpioDevice.ClosePin(_backlightPin);
                    }

                    if (_dcPinId >= 0)
                    {
                        _gpioDevice.ClosePin(_dcPinId);
                    }

                    if (_shouldDispose)
                    {
                        _gpioDevice?.Dispose();
                    }

                    _gpioDevice = null!;
                }

                _spiDevice?.Dispose();
                _spiDevice = null!;
            }
        }
    }
}
