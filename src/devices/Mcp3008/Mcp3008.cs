// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using System;
using System.Device.Gpio;
using System.Device.Spi;

namespace Iot.Device.Mcp3008
{
    public class Mcp3008 : IDisposable
    {
        private SpiDevice _spiDevice;

        internal enum InputConfiguration
        {
            Differential = 0,
            SingleEnded = 1
        }

        public Mcp3008(SpiDevice spiDevice)
        {
            _spiDevice = spiDevice;
        }

        public void Dispose()
        {
            _spiDevice?.Dispose();
            _spiDevice = null;
        }

        public int Read(int channel)
        {
            if (channel < 0 || channel > 7)
            {
                throw new ArgumentException("ADC channel must be within 0-7 range.");
            }

            return ReadSpi(channel, InputConfiguration.SingleEnded);
        }

        private static byte GetConfigurationBits(int channel, InputConfiguration inputConfiguration)
        {
            int configurationBits = (0b0001_1000 | channel) << 3;

            if (inputConfiguration == InputConfiguration.Differential)
            {
                configurationBits &= 0b1011_1111; // Clear mode bit.
            }

            return (byte)configurationBits;
        }

        // Ported: https://github.com/adafruit/Adafruit_Python_MCP3008/blob/master/Adafruit_MCP3008/MCP3008.py
        private int ReadSpi(int channel, InputConfiguration inputConfiguration)
        {
            byte configurationBits = GetConfigurationBits(channel, inputConfiguration);
            byte[] writeBuffer = new byte[] { configurationBits, 0, 0 };
            byte[] readBuffer = new byte[3];

            _spiDevice.TransferFullDuplex(writeBuffer, readBuffer);

            int result = (readBuffer[0] & 0b0000_0001) << 9;
            result |= (readBuffer[1] & 0b1111_1111) << 1;
            result |= (readBuffer[2] & 0b1000_0000) >> 7;
            result = result & 0b0011_1111_1111;
            return result;
        }
    }
}
