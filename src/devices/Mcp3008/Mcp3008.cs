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

        private SpiDevice _spiDevice;
        private GpioController _controller;
        private CommunicationProtocol _protocol;
        private int _CLK;
        private int _MISO;
        private int _MOSI;
        private int _CS;

        private enum CommunicationProtocol
        {
            Gpio,
            Spi
        }


        public Mcp3008(SpiDevice spiDevice)
        {
            _spiDevice = spiDevice;
            _protocol = CommunicationProtocol.Spi;
        }

        public Mcp3008(int CLK, int MISO, int MOSI, int CS)
        {
            _controller = new GpioController();
            _CLK = CLK;
            _MISO = MISO;
            _MOSI = MOSI;
            _CS = CS;

            _controller.OpenPin(_CLK, PinMode.Output);
            _controller.OpenPin(_MISO, PinMode.Input);
            _controller.OpenPin(_MOSI, PinMode.Output);
            _controller.OpenPin(_CS, PinMode.Output);
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

        public int Read(int adc_channel)
        {
            if (adc_channel < 0 || adc_channel > 7)
            {
                throw new ArgumentException("ADC channel must be within 0-7 range.");
            }

            if (_protocol == CommunicationProtocol.Spi)
            {
                return ReadSpi(adc_channel);
            }
            else
            {
                return ReadGpio(adc_channel);
            }
        }

        // port of https://gist.github.com/ladyada/3151375
        private int ReadGpio(int adc_channel)
        {
            while (true)
            {
                var commandout = 0;
                var trim_pot = 0;

                _controller.Write(_CS, PinValue.High);
                _controller.Write(_CLK, PinValue.Low);
                _controller.Write(_CS, PinValue.Low);

                commandout |= 0x18;
                commandout <<= 3;

                for (var i = 0; i < 5; i++)
                {
                    if ((commandout & 0x80) > 0)
                    {
                        _controller.Write(_MOSI, PinValue.High);
                    }
                    else
                    {
                        _controller.Write(_MOSI, PinValue.Low);
                    }

                    commandout <<= 1;
                    _controller.Write(_CLK, PinValue.High);
                    _controller.Write(_CLK, PinValue.Low);
                }

                for (var i = 0; i < 12; i++)
                {
                    _controller.Write(_CLK, PinValue.High);
                    _controller.Write(_CLK, PinValue.Low);
                    trim_pot <<= 1;

                    if (_controller.Read(_MISO) == PinValue.High)
                    {
                        trim_pot |= 0x1;
                    }
                }

                _controller.Write(_CS, PinValue.High);

                trim_pot >>= 1;
                return trim_pot;
            }
        }

        // ported code from:
        // https://github.com/adafruit/Adafruit_Python_MCP3008/blob/master/Adafruit_MCP3008/MCP3008.py
        private int ReadSpi(int adc_channel)
        {
            int command = 0b11 << 6;                  //Start bit, single channel read
            command |= (adc_channel & 0x07) << 3;  // Channel number (in 3 bits)
            var input = new Byte[] { (Byte)command, 0, 0 };
            var output = new Byte[3];
            _spiDevice.TransferFullDuplex(input, output);

            var result = (output[0] & 0x01) << 9;
            result |= (output[1] & 0xFF) << 1;
            result |= (output[2] & 0x80) >> 7;
            result = result & 0x3FF;
            return result;
        }
    }    
}
