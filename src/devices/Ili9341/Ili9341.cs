// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using System;
using System.Device;
using System.Device.Gpio;
using System.Device.I2c;
using System.Device.I2c.Drivers;
using System.Device.Spi;
using System.Device.Spi.Drivers;
using System.Drawing;

namespace Iot.Device.Ili9341
{
    public class Ili9341 : IDisposable
    {
        public const int Width = 240;
        public const int Height = 320;
        private SpiDevice _spi;
        private IGpioController _controller;
        private int _dc;
        private int _reset;

        public Ili9341(SpiDevice spiDevice, int dc, int reset, IGpioController gpioController = null)
        {
            _spi = spiDevice;
            _dc = dc;
            _reset = reset;
            _controller = gpioController ?? new GpioController();
            _controller.OpenPin(_dc, PinMode.Output);
            _controller.OpenPin(_reset, PinMode.Output);

            HardwareReset();

            // init
            // SendCommand((Command)0xEF);
            // SendData(0x03);
            // SendData(0x80);
            // SendData(0x02);
            // SendCommand((Command)0xCF);
            // SendData(0x00);
            // SendData(0XC1);
            // SendData(0X30);
            // SendCommand((Command)0xED);
            // SendData(0x64);
            // SendData(0x03);
            // SendData(0X12);
            // SendData(0X81);
            // SendCommand((Command)0xE8);
            // SendData(0x85);
            // SendData(0x00);
            // SendData(0x78);
            // SendCommand((Command)0xCB);
            // SendData(0x39);
            // SendData(0x2C);
            // SendData(0x00);
            // SendData(0x34);
            // SendData(0x02);
            // SendCommand((Command)0xF7);
            // SendData(0x20);
            // SendCommand((Command)0xEA);
            // SendData(0x00);
            // SendData(0x00);
            SendCommand(Command.PowerControl1);
            SendData(0x23);
            SendCommand(Command.PowerControl2);
            SendData(0x10);
            SendCommand(Command.VcomControl1);
            SendData(0x3e);
            SendData(0x28);
            SendCommand(Command.VcomControl2);
            SendData(0x86);
            SendCommand(Command.MemoryAccessControl);
            SendData(0x48);
            SendCommand(Command.ColModPixelFormatSet);
            SendData(0x55);
            SendCommand(Command.FrameRateControlInNormalMode);
            SendData(0x00);
            SendData(0x18);
            SendCommand(Command.DisplayFunctionControl);
            SendData(0x08);
            SendData(0x82);
            SendData(0x27);
            // SendCommand((Command)0xF2);
            // SendData(0x00);
            SendCommand(Command.GammaSet);
            SendData(0x01);
            SendCommand(Command.PositiveGammaCorrection);
            SendData(0x0F);
            SendData(0x31);
            SendData(0x2B);
            SendData(0x0C);
            SendData(0x0E);
            SendData(0x08);
            SendData(0x4E);
            SendData(0xF1);
            SendData(0x37);
            SendData(0x07);
            SendData(0x10);
            SendData(0x03);
            SendData(0x0E);
            SendData(0x09);
            SendData(0x00);
            SendCommand(Command.NegativeGammaCorrection);
            SendData(0x00);
            SendData(0x0E);
            SendData(0x14);
            SendData(0x03);
            SendData(0x11);
            SendData(0x07);
            SendData(0x31);
            SendData(0xC1);
            SendData(0x48);
            SendData(0x08);
            SendData(0x0F);
            SendData(0x0C);
            SendData(0x31);
            SendData(0x36);
            SendData(0x0F);
            SendCommand(Command.SleepOut);
            DelayHelper.DelayMilliseconds(120, allowThreadYield: true);
            SendCommand(Command.DisplayOn);
        }

        public void Draw(Bitmap bitmap)
        {
            SetWindow();
            byte[] buffer = new byte[Width * Height * 2];

            // byte r = (byte)(color.R >> 3);
            // byte g = (byte)(color.G >> 2);
            // byte b = (byte)(color.B >> 3);
            // ushort col = (ushort)((r << 11) | (g << 5) | b);

            for (int i = 0; i < buffer.Length; i++)
            {
                int p = i / 2;
                int x = p % Width;
                int y = p / Width;
                if (i % 2 == 0)
                {
                    float fx = (x - Width / 2);
                    float fy = (y - Height / 2);
                    float r = (float)Math.Sqrt(fx * fx + fy * fy);
                    //float a = (float)Math.Atan2(fy, fx);

                    if (r < 50)
                    {
                        buffer[i] = 0x78;
                    }
                }
                // if (i % 4 < 2)
                // {
                    // buffer[i] = 0x70;
                // }
            }

            Span<byte> sp = new Span<byte>(buffer);
            SendData(sp);
        }

        private void HardwareReset()
        {
            _controller.Write(_reset, PinValue.High);
            DelayHelper.DelayMilliseconds(5, allowThreadYield: true);
            _controller.Write(_reset, PinValue.Low);
            DelayHelper.DelayMilliseconds(20, allowThreadYield: true);
            _controller.Write(_reset, PinValue.High);
            DelayHelper.DelayMilliseconds(150, allowThreadYield: true);
        }

        private void SetWindow(uint x0 = 0, uint y0 = 0, uint x1 = Width - 1, uint y1 = Height - 1)
        {
            SendCommand(Command.ColumnAddressSet);
            Span<byte> data = stackalloc byte[4]
            {
                (byte)(x0 >> 8),
                (byte)x0,
                (byte)(x1 >> 8),
                (byte)x1,
            };
            SendData(data);
            SendCommand(Command.PageAddressSet);
            Span<byte> data2 = stackalloc byte[4]
            {
                (byte)(y0 >> 8),
                (byte)y0,
                (byte)(y1 >> 8),
                (byte)y1,
            };
            SendData(data2);
            SendCommand(Command.MemoryWrite);
        }

        private void SendCommand(Command command)
        {
            Span<byte> buff = stackalloc byte[1] { (byte)command };
            Send(buff, isData: false);
        }

        private void SendData(ReadOnlySpan<byte> buffer)
        {
            Send(buffer, isData: true);
        }

        private void SendData(byte data)
        {
            Span<byte> buff = stackalloc byte[1] { data };
            SendData(buff);
        }

        private void Send(ReadOnlySpan<byte> buffer, bool isData)
        {
            _controller.Write(_dc, isData);

            int bufLen = 4096 >> 1;
            while (buffer.Length > bufLen)
            {
                _spi.Write(buffer.Slice(0, bufLen));
                buffer = buffer.Slice(bufLen);
            }

            if (buffer.Length > 0)
            {
                _spi.Write(buffer);
            }
        }

        public void Dispose()
        {
            _spi?.Dispose();
            _spi = null;
            //_controller?.Dispose();
            //_controller = null;
        }
    }
}
