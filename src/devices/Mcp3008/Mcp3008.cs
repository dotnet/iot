// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using System;
using System.Device.Gpio;
using System.Device.Spi;

namespace Iot.Device
{
    public class Mcp3008 : IDisposable
    {
        private readonly SpiDevice _spiDevice;
        private GpioController _controller;
        private readonly CommunicationProtocol _protocol;
        private readonly int _clk;
        private readonly int _miso;
        private readonly int _mosi;
        private readonly int _cs;

        private enum CommunicationProtocol
        {
            Gpio,
            Spi
        }

        public enum InputConfiguration
        {
            Differential = 0,
            SingleEnded = 1
        }

        public Mcp3008(SpiDevice spiDevice)
        {
            _spiDevice = spiDevice;
            _protocol = CommunicationProtocol.Spi;
        }

        public Mcp3008(int clk, int miso, int mosi, int cs)
        {
            _controller = new GpioController();
            _clk = clk;
            _miso = miso;
            _mosi = mosi;
            _cs = cs;

            _controller.OpenPin(_clk, PinMode.Output);
            _controller.OpenPin(_miso, PinMode.Input);
            _controller.OpenPin(_mosi, PinMode.Output);
            _controller.OpenPin(_cs, PinMode.Output);
            _protocol = CommunicationProtocol.Gpio;
        }

        public void Dispose()
        {
            if (_controller != null)
            {
                _controller.Dispose();
                _controller = null;
            }
        }

        public int Read(int channel, InputConfiguration inputConfiguration = InputConfiguration.SingleEnded)
        {
            if (channel < 0 || channel > 7)
            {
                throw new ArgumentException("ADC channel must be within 0-7 range.");
            }

            if (_protocol == CommunicationProtocol.Spi)
            {
                return ReadSpi(channel, inputConfiguration);
            }
            else
            {
                return ReadGpio(channel, inputConfiguration);
            }
        }

        private static byte GetConfigurationBits(int channel, InputConfiguration inputConfiguration)
        {
            int configurationBits = (0x18 | channel) << 3;

            if (inputConfiguration == InputConfiguration.Differential)
            {
                configurationBits &= 0xBF;  // Clear mode bit.
            }

            return (byte)configurationBits;
        }

        // Ported: https://gist.github.com/ladyada/3151375
        private int ReadGpio(int channel, InputConfiguration inputConfiguration)
        {
            while (true)
            {
                int result = 0;
                byte command = GetConfigurationBits(channel, inputConfiguration);

                _controller.Write(_cs, PinValue.High);
                _controller.Write(_clk, PinValue.Low);
                _controller.Write(_cs, PinValue.Low);

                for (int cnt = 0; cnt < 5; cnt++)
                {
                    if ((command & 0x80) > 0)
                    {
                        _controller.Write(_mosi, PinValue.High);
                    }
                    else
                    {
                        _controller.Write(_mosi, PinValue.Low);
                    }

                    command <<= 1;
                    _controller.Write(_clk, PinValue.High);
                    _controller.Write(_clk, PinValue.Low);
                }

                for (int cnt = 0; cnt < 12; cnt++)
                {
                    _controller.Write(_clk, PinValue.High);
                    _controller.Write(_clk, PinValue.Low);
                    result <<= 1;

                    if (_controller.Read(_miso) == PinValue.High)
                    {
                        result |= 0x1;
                    }
                }

                _controller.Write(_cs, PinValue.High);

                result >>= 1;
                return result;
            }
        }

        // Ported: https://github.com/adafruit/Adafruit_Python_MCP3008/blob/master/Adafruit_MCP3008/MCP3008.py
        private int ReadSpi(int channel, InputConfiguration inputConfiguration)
        {
            byte configurationBits = GetConfigurationBits(channel, inputConfiguration);
            byte[] input = new byte[] { configurationBits, 0, 0 };
            byte[] output = new byte[3];

            _spiDevice.TransferFullDuplex(input, output);

            int result = (output[0] & 0x01) << 9;
            result |= (output[1] & 0xFF) << 1;
            result |= (output[2] & 0x80) >> 7;
            result = result & 0x3FF;
            return result;
        }
    }
}
