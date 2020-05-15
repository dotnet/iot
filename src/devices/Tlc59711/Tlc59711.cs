// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using System;
using System.Collections.Generic;
using System.Device.Gpio;
using System.Device.Spi;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using Iot.Device.Spi;

namespace Iot.Device.Tlc59711
{
    /// <summary>
    /// On-board LED on the device.
    /// </summary>
    public class Tlc59711 : IDisposable
    {
        private readonly ushort[] _pwmBuffer;
        private readonly byte _numberOfDrivers;
        private SpiDevice _spiDevice;
        private byte _brightnessRed;
        private byte _brightnessGreen;
        private byte _brightnessBlue;
        private byte[] _dataToWrite;

        /// <summary>
        /// Creates a new instance of the Tlc59711.
        /// </summary>
        public Tlc59711(byte numberOfDrivers, SpiDevice spiDevice)
        {
            _numberOfDrivers = numberOfDrivers;

            _spiDevice = spiDevice ?? throw new ArgumentNullException(nameof(spiDevice));

            // default 100% brigthness
            _brightnessRed = _brightnessGreen = _brightnessBlue = 0x7f;

            // 12 channels per driver
            _pwmBuffer = new ushort[numberOfDrivers * 12];

            _dataToWrite = new byte[_numberOfDrivers * 28];
        }

        /// <summary>
        /// Disposes SpiDevice instances
        /// </summary>
        public void Dispose()
        {
            _spiDevice?.Dispose();
            _spiDevice = null;
        }

        /// <summary>
        /// Call this to set the PWM level for a channel.
        /// </summary>
        /// <param name="channel">Channel, 0-based</param>
        /// <param name="pwm">PWM level (0 = minimum, 65535 = maximum)</param>
        public void SetPWM(int channel, ushort pwm)
        {
            if (channel > 12 * _numberOfDrivers)
            {
                throw new ArgumentOutOfRangeException("channel");
            }

            _pwmBuffer[channel] = pwm;

        }

        /// <summary>
        /// Call this to set the RGB value for a group of 3 channels
        /// </summary>
        /// <param name="ledNumber">LED number (channel number of the "red" pin divided by 3)</param>
        /// <param name="red">Red level (0 = minimum, 65535 = maximum)</param>
        /// <param name="green">Green level (0 = minimum, 65535 = maximum)</param>
        /// <param name="blue">Blue level (0 = minimum, 65535 = maximum)</param>
        public void SetLED(int ledNumber, ushort red, ushort green, ushort blue)
        {
            SetPWM((byte)(ledNumber * 3), red);
            SetPWM((byte)(ledNumber * 3 + 1), green);
            SetPWM((byte)(ledNumber * 3 + 2), blue);
        }

        /// <summary>
        /// Call this after every change to write the new PWM levels to the device.
        /// </summary>
        public void Write()
        {
            int pos = 0;

            uint command;

            // Magic word for write
            command = 0x25;

            command <<= 5;
            // OUTTMG = 1, EXTGCK = 0, TMGRST = 1, DSPRPT = 1, BLANK = 0 -> 0x16
            command |= 0x16;

            command <<= 7;
            command |= _brightnessRed;

            command <<= 7;
            command |= _brightnessGreen;

            command <<= 7;
            command |= _brightnessBlue;

            for (byte n = 0; n < _numberOfDrivers; n++)
            {
                _dataToWrite[pos++] = (byte)(command >> 24);
                _dataToWrite[pos++] = (byte)(command >> 16);
                _dataToWrite[pos++] = (byte)(command >> 8);
                _dataToWrite[pos++] = (byte)(command);

                // 12 channels per TLC59711
                for (int c = 11; c >= 0; c--)
                {
                    // 16 bits per channel, send MSB first
                    _dataToWrite[pos++] = (byte)(_pwmBuffer[n * 12 + c] >> 8);
                    _dataToWrite[pos++] = (byte)(_pwmBuffer[n * 12 + c]);
                }
            }

            _spiDevice.Write(_dataToWrite);
        }

        /// <summary>
        /// Set the brightness of all LED channels to same value
        /// </summary>
        /// <param name="brightness">Brightness between 0 and 127</param>
        public void SetBrightness(byte brightness)
        {
            brightness = Math.Min(brightness, (byte)127);

            _brightnessRed = _brightnessGreen = _brightnessBlue = brightness;
        }

        /// <summary>
        /// Set the brightness of LED channels to specific value
        /// </summary>
        /// <param name="brightnessRed">Brightness between 0 and 127</param>
        /// <param name="brightnessGreen">Brightness between 0 and 127</param>
        /// <param name="brightnessBlue">Brightness between 0 and 127</param>
        public void SetBrightness(byte brightnessRed, byte brightnessGreen, byte brightnessBlue)
        {
            _brightnessRed = Math.Min(brightnessRed, (byte)127);
            _brightnessGreen = Math.Min(brightnessGreen, (byte)127);
            _brightnessBlue = Math.Min(brightnessBlue, (byte)127);
        }
    }
}
