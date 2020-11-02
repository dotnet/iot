// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System;
using System.Device.I2c;
using UnitsNet;

namespace Iot.Device.RadioReceiver
{
    /// <summary>
    /// FM Stereo Radio TEA5767
    /// </summary>
    public class Tea5767 : RadioReceiverBase
    {
        /// <summary>
        /// TEA5767 default I2C address.
        /// </summary>
        public const byte DefaultI2cAddress = 0x60;

        private I2cDevice _i2cDevice;
        // TEA5767 has no register. Setting TEA5767 requires sending 5 bytes of commands to it.
        // Default values: disable mute, XTAL is 13MHz, disable standby, enable SNC
        private byte[] _registers = new byte[5] { 0, 0, 0b_0001_0000, 0b_0001_0010, 0 };

        /// <summary>
        /// TEA5767 mute.
        /// </summary>
        public bool Mute { get => GetMute(); set => SetMute(value); }

        /// <summary>
        /// TEA5767 standby.
        /// </summary>
        public bool Standby { get => GetStandby(); set => SetStandby(value); }

        /// <summary>
        /// TEA5767 FM frequency range.
        /// </summary>
        public FrequencyRange FrequencyRange { get => GetFrequencyRange(); set => SetFrequencyRange(value); }

        /// <summary>
        /// TEA5767 FM frequency.
        /// </summary>
        public override Frequency Frequency { get => GetFrequency(); set => SetFrequency(value); }

        /// <summary>
        /// Create a new instance of the TEA5767.
        /// </summary>
        /// <param name="i2cDevice">The I2C device used for communication.</param>
        /// <param name="frequencyRange">FM frequency range.</param>
        /// <param name="frequency">FM frequency.</param>
        public Tea5767(I2cDevice i2cDevice, FrequencyRange frequencyRange, Frequency frequency)
        {
            _i2cDevice = i2cDevice;

            FrequencyRange = frequencyRange;
            Frequency = frequency;

            SaveRegisters();
        }

        /// <summary>
        /// Set whether TEA5767 is mute.
        /// </summary>
        /// <param name="isMute">Mute if the value is true.</param>
        private void SetMute(bool isMute)
        {
            if (isMute)
            {
                _registers[0] |= 0b_1000_0000;
            }
            else
            {
                _registers[0] &= 0b_0111_1111;
            }

            SaveRegisters();
        }

        /// <summary>
        /// Get whether TEA5767 is mute.
        /// </summary>
        /// <returns>Mute if the value is true.</returns>
        private bool GetMute()
        {
            return _registers[0] >> 7 == 1 ? true : false;
        }

        /// <summary>
        /// Set TEA5767 FM frequency range.
        /// </summary>
        /// <param name="bandRange">FM frequency range.</param>
        private void SetFrequencyRange(FrequencyRange bandRange)
        {
            switch (bandRange)
            {
                case FrequencyRange.Japan:
                    _registers[3] |= 0b_0010_0000;
                    break;
                case FrequencyRange.Other:
                    _registers[3] &= 0b_1101_1111;
                    break;
                default:
                    throw new ArgumentOutOfRangeException(nameof(bandRange), bandRange, null);
            }

            SaveRegisters();
        }

        /// <summary>
        /// Get TEA5767 FM frequency range.
        /// </summary>
        /// <returns>FM frequency range.</returns>
        private FrequencyRange GetFrequencyRange()
        {
            return (FrequencyRange)((_registers[3] & 0b_1101_1111) >> 5);
        }

        /// <summary>
        /// Set TEA5767 FM frequency.
        /// </summary>
        /// <param name="frequency">FM frequency.</param>
        private void SetFrequency(Frequency frequency)
        {
            double frequencyMhz = frequency.Megahertz;
            switch (FrequencyRange)
            {
                case FrequencyRange.Japan:
                    if (frequencyMhz < 76 || frequencyMhz > 90)
                    {
                        throw new ArgumentOutOfRangeException(nameof(frequency), $"{nameof(frequency)} needs to be in the range of 76 to 90.");
                    }

                    break;
                case FrequencyRange.Other:
                    if (frequencyMhz < 87 || frequencyMhz > 108)
                    {
                        throw new ArgumentOutOfRangeException(nameof(frequency), $"{nameof(frequency)} needs to be in the range of 87 to 108.");
                    }

                    break;
                default:
                    throw new ArgumentOutOfRangeException(nameof(FrequencyRange), FrequencyRange, null);
            }

            int f = (int)((frequencyMhz * 1000000 + 225000) / 8192);

            byte high = (byte)((f & 0b_0011_1111_0000_0000) >> 8);
            byte low = (byte)(f & 0b_1111_1111);

            _registers[0] &= 0b_1100_0000;
            _registers[0] |= high;
            _registers[1] = low;

            SaveRegisters();
        }

        /// <summary>
        /// Get TEA5767 FM frequency.
        /// </summary>
        /// <returns>FM frequency.</returns>
        private Frequency GetFrequency()
        {
            byte[] readBuffer = ReadRegisters();

            int f = ((readBuffer[0] & 0b_0011_1111) << 8) | readBuffer[1];

            return Frequency.FromMegahertz(Math.Round((f * 8192 - 225000) / 1000000.0, 1));
        }

        /// <summary>
        /// Set whether TEA5767 is standby.
        /// </summary>
        /// <param name="isStandby">Standby if the value is true.</param>
        private void SetStandby(bool isStandby)
        {
            if (isStandby)
            {
                _registers[3] |= 0b_0100_0000;
            }
            else
            {
                _registers[3] &= 0b_1011_1111;
            }

            SaveRegisters();
        }

        /// <summary>
        /// Get whether TEA5767 is standby.
        /// </summary>
        /// <returns>Standby if the value is true.</returns>
        private bool GetStandby()
        {
            return (_registers[3] & 0b_0100_0000) > 0 ? true : false;
        }

        /// <summary>
        /// Automatic search for effective radio.
        /// </summary>
        /// <param name="searchDirection">Search up or down from the current frequency.</param>
        /// <param name="stopLevel">Stop search condition (range from 1 to 3).</param>
        public void Search(SearchDirection searchDirection = SearchDirection.Up, int stopLevel = 3)
        {
            if (stopLevel < 1 || stopLevel > 3)
            {
                throw new ArgumentOutOfRangeException(nameof(stopLevel), $"{nameof(stopLevel)} needs to be in the range of 1 to 3.");
            }

            // enable search mode
            _registers[0] |= 0b_0100_0000;

            // clear search standard bit
            _registers[2] &= 0b_0001_1111;
            // set search stop level
            _registers[2] |= (byte)(stopLevel << 6);

            switch (searchDirection)
            {
                case SearchDirection.Up:
                    _registers[2] |= 0b_1000_0000;
                    break;
                case SearchDirection.Down:
                    _registers[2] &= 0b_01111_1111;
                    break;
                default:
                    throw new ArgumentOutOfRangeException(nameof(searchDirection), searchDirection, null);
            }

            SaveRegisters();

            // check whether the search is over
            byte[] readBuffer;
            do
            {
                readBuffer = ReadRegisters();
            }
            while (readBuffer[0] >> 7 != 1);

            // disable search mode
            _registers[0] &= 0b_1011_1111;
        }

        /// <summary>
        /// Cleanup
        /// </summary>
        protected override void Dispose(bool disposing)
        {
            _i2cDevice?.Dispose();
            _i2cDevice = null;

            base.Dispose(disposing);
        }

        private byte[] ReadRegisters()
        {
            Span<byte> readBuffer = stackalloc byte[4];

            _i2cDevice.Read(readBuffer);

            return readBuffer.ToArray();
        }

        private void SaveRegisters()
        {
            _i2cDevice.Write(_registers);
        }
    }
}
