// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using System;
using System.Device.Gpio;
using System.Device.Spi;
using System.Drawing;
using System.Threading.Tasks;

namespace Iot.Device.Ssd1351
{
    /// <summary>
    /// A single-chip CMOS OLED/PLED driver with controller for organic/polymer
    /// light emitting diode dot-matrix graphic display system.
    /// </summary>
    public partial class Ssd1351 : IDisposable
    {
        private const byte ScreenWidthPx = 128;
        private const byte ScreenHeightPx = 128;
        private const int DefaultSPIBufferSize = 0x1000;

        private readonly int _dcPinId;
        private readonly int _resetPinId;
        private readonly int _spiBufferSize;
        private readonly bool _disposeGpioController = false;

        private SpiDevice _spiDevice;
        private GpioController _gpioDevice;
        private ColorDepth _colorDepth;
        private ColorSequence _colorSequence;

        /// <summary>
        /// Initializes new instance of Ssd1351 device that will communicate using SPI bus.
        /// A single-chip CMOS OLED/PLED driver with controller for organic/polymer
        /// light emitting diode dot-matrix graphic display system.
        /// </summary>
        /// <param name="spiDevice">The SPI device used for communication. This Spi device will be displed along with the Ssd1351 device.</param>
        /// <param name="gpioController">The GPIO controller used for communication and controls the the <paramref name="resetPin"/> and the <paramref name="dataCommandPin"/>
        /// If no Gpio controller is passed in then a default one will be created and disposed when Ssd1351 device is disposed.</param>
        /// <param name="dataCommandPin">The id of the GPIO pin used to control the DC line (data/command).</param>
        /// <param name="resetPin">The id of the GPIO pin used to control the /RESET line (data/command).</param>
        /// <param name="spiBufferSize">The size of the SPI buffer. If data larger than the buffer is sent then it is split up into multiple transmissions. The default value is 4096.</param>
        public Ssd1351(SpiDevice spiDevice, int dataCommandPin, int resetPin, int spiBufferSize = DefaultSPIBufferSize, GpioController gpioController = null)
        {
            if (!InRange((uint)spiBufferSize, 0x1000, 0x10000))
            {
                throw new ArgumentException($"SPI Buffer Size must be between 4096 and 65536.", nameof(spiBufferSize));
            }

            _gpioDevice = gpioController;

            _spiDevice = spiDevice ?? throw new ArgumentNullException(nameof(spiDevice));

            _dcPinId = dataCommandPin;
            _resetPinId = resetPin;

            if (_gpioDevice == null)
            {
                _gpioDevice = new GpioController();
                _disposeGpioController = true;
            }

            _gpioDevice.OpenPin(_dcPinId, PinMode.Output);
            _gpioDevice.OpenPin(_resetPinId, PinMode.Output);

            _spiBufferSize = spiBufferSize;

            SetSegmentReMapColorDepth();
        }

        /// <summary>
        /// Convert a color structure to a byte tuple represening the colour in 565 format.
        /// </summary>
        /// <param name="color">The color to be converted.</param>
        /// <returns>
        /// This method returns the low byte and the high byte of the 16bit value representing RGB565 or BGR565 value
        ///
        /// byte    11111111 00000000
        /// bit     76543210 76543210
        ///
        /// For ColorSequence.RGB
        ///         RRRRRGGG GGGBBBBB
        ///         43210543 21043210
        ///
        /// For ColorSequence.BGR
        ///         BBBBBGGG GGGRRRRR
        ///         43210543 21043210
        /// </returns>
        private (byte, byte) Color565(Color color)
        {
            // get the top 5 MSB of the blue or red value
            UInt16 retval = (UInt16)((_colorSequence == ColorSequence.BGR ? color.B : color.R) >> 3);
            // shift right to make room for the green Value
            retval <<= 6;
            // combine with the 6 MSB if the green value
            retval |= (UInt16)(color.G >> 2);
            // shift right to make room for the red or blue Value
            retval <<= 5;
            // combine with the 6 MSB if the red or blue value
            retval |= (UInt16)((_colorSequence == ColorSequence.BGR ? color.R : color.B) >> 3);
            return ((byte)(retval >> 8), (byte)(retval & 0xFF));
        }

        /// <summary>
        /// Send filled rectangle to the ssd1351 display.
        /// </summary>
        /// <param name="color">The color to fill the rectangle with.</param>
        /// <param name="x">The x co-ordinate of the point to start the rectangle at in pixels (0..126 for the Ssd1351 where 0 represents the leftmost pixel).</param>
        /// <param name="y">The y co-ordinate of the point to start the rectangle at in pixles (0..126 for the Ssd1351 where 0 represents the topmost pixel).</param>
        /// <param name="w">The width of the rectangle in pixels (1..127 for the Ssd1351).</param>
        /// <param name="h">The height of the rectangle in pixels (1..127 for the Ssd1351).</param>
        public void FillRect(Color color, byte x, byte y, byte w, byte h)
        {
            Span<byte> colourBytes = stackalloc byte[_colorDepth == ColorDepth.ColourDepth65K ? 2 : 3]; // create a short span that holds the colour data to be sent to the display
            Span<byte> displayBytes = stackalloc byte[w * h * (_colorDepth == ColorDepth.ColourDepth65K ? 2 : 3)]; // span used to form the data to be written out to the SPI interface

            // set the colourbyte array to represent the fill colour
            if (_colorDepth == ColorDepth.ColourDepth65K)
            {
                (colourBytes[0], colourBytes[1]) = Color565(color);
            }
            else
            {
                colourBytes[0] = (byte)((_colorSequence == ColorSequence.BGR ? color.B : color.R) >> 2);
                colourBytes[1] = (byte)(color.G >> 2);
                colourBytes[2] = (byte)((_colorSequence == ColorSequence.BGR ? color.R : color.B) >> 2);
            }

            // set the pixels in the array representing the raw data to be sent to the display
            // to the fill color
            for (int i = 0; i < w * h; i++)
            {
                if (_colorDepth == ColorDepth.ColourDepth65K)
                {
                    displayBytes[i * 2 + 0] = colourBytes[0];
                    displayBytes[i * 2 + 1] = colourBytes[1];
                }
                else
                {
                    displayBytes[i * 3 + 0] = colourBytes[0];
                    displayBytes[i * 3 + 1] = colourBytes[1];
                    displayBytes[i * 3 + 2] = colourBytes[2];
                }
            }

            // specifiy a location for the rows and columns on the display where the data is to be written
            SetColumnAddress(x, (byte)(x + w - 1));
            SetRowAddress(y, (byte)(y + h - 1));

            // write out the pixel data
            SendCommand(Ssd1351Command.WriteRam, displayBytes);
        }

        /// <summary>
        /// Clears screen
        /// </summary>
        public void ClearScreen()
        {
            FillRect(Color.Black, 0, 0, ScreenHeightPx, ScreenWidthPx);
        }

        /// <summary>
        /// Resets the display.
        /// </summary>
        public async Task ResetDisplayAsync()
        {
            _gpioDevice.Write(_dcPinId, PinValue.Low);
            _gpioDevice.Write(_resetPinId, PinValue.High);

            await Task.Delay(20);

            _gpioDevice.Write(_resetPinId, PinValue.Low);
            await Task.Delay(20);

            _gpioDevice.Write(_resetPinId, PinValue.High);
            await Task.Delay(20);
        }

        /// <summary>
        /// Verifies value is within a specific range.
        /// </summary>
        /// <param name="value">Value to check.</param>
        /// <param name="start">Starting value of range.</param>
        /// <param name="end">Ending value of range.</param>
        /// <returns>Determines if value is within range.</returns>
        internal static bool InRange(uint value, uint start, uint end)
        {
            return value >= start && value <= end;
        }

        /// <summary>
        /// Send a command to the the display controller along with associated parameters.
        /// </summary>
        /// <param name="command">Command to send.</param>
        /// <param name="commandParameters">parameteters for the command to be sent</param>
        private void SendCommand(Ssd1351Command command, params byte[] commandParameters)
        {
            Span<byte> paramSpan = stackalloc byte[commandParameters.Length];
            for (int i = 0; i < commandParameters.Length; paramSpan[i] = commandParameters[i], i++)
            {
            }

            SendCommand(command, paramSpan);
        }

        /// <summary>
        /// Send a command to the the display controller along with parameters.
        /// </summary>
        /// <param name="command">Command to send.</param>
        /// <param name="data">Span to send as parameters for the command.</param>
        private void SendCommand(Ssd1351Command command, Span<byte> data)
        {
            Span<byte> commandSpan = stackalloc byte[]
            {
                (byte)command
            };

            SendSPI(commandSpan, true);

            if (data != null && data.Length > 0)
            {
                SendSPI(data);

                // detect certain commands that may alter the state of the device. This is done as the
                // SPI device cannot read registers from the ssd1351 and so changes need to be captured
                switch (command)
                {
                    // capture changes to the colour depth and colour sequence
                    case Ssd1351Command.SetRemap:
                        _colorSequence = (ColorSequence)((data[0] >> 2) & 0x01);
                        _colorDepth = (ColorDepth)((data[0] >> 6) & 0x03);
                        break;
                }
            }
        }

        /// <summary>
        /// Send data to the display controller.
        /// </summary>
        /// <param name="data">The data to send to the display controller.</param>
        public void SendData(Span<byte> data)
        {
            if (data == null)
            {
                throw new ArgumentNullException(nameof(data));
            }

            // create a buffer to contain the data plus pseudo command to indicate data.
            Span<byte> buffer = stackalloc byte[data.Length + 1];

            buffer[0] = 0x40; // Control byte indicating that the following bytes are data and not command parameters.
            data.CopyTo(buffer.Slice(1));
            SendSPI(buffer);
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

        /// <inheritdoc/>
        public void Dispose()
        {
            if (_disposeGpioController)
            {
                _gpioDevice?.Dispose();
                _gpioDevice = null;
            }

            _spiDevice?.Dispose();
            _spiDevice = null;
        }
    }
}
