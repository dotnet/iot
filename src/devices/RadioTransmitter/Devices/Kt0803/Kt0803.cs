// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using System;
using System.Device.I2c;

namespace Iot.Device.RadioTransmitter
{
    /// <summary>
    /// FM radio transmitter module KT0803.
    /// </summary>
    public class Kt0803 : RadioTransmitterBase
    {
        private I2cDevice _i2cDevice;

        /// <summary>
        /// Kt0803 default I2C address.
        /// </summary>
        public const byte DefaultI2cAddress = 0x3E;

        /// <summary>
        /// Kt0803 FM frequency (range from 70Mhz to 108Mhz).
        /// </summary>
        public override double Frequency { get => GetFrequency(); set => SetFrequency(value); }

        /// <summary>
        /// Kt0803 standby.
        /// </summary>
        public bool Standby { get => GetStandby(); set => SetStandby(value); }

        /// <summary>
        /// Kt0803 mute.
        /// </summary>
        public bool Mute { get => GetMute(); set => SetMute(value); }
        
        /// <summary>
        /// Kt0803 PGA ( Programmable Gain Amplifier ) gain.
        /// </summary>
        public PgaGain PgaGain { get => GetPga(); set => SetPga(value); }

        /// <summary>
        /// Kt0803 transmission power.
        /// </summary>
        public TransmissionPower TransmissionPower { get => GetTransmissionPower(); set => SetTransmissionPower(value); }

        private Region _region;
        /// <summary>
        /// Kt0803 region.
        /// </summary>
        public Region Region { get => _region; set { SetRegion(value); _region = value; } }

        /// <summary>
        /// Kt0803 bass boost.
        /// </summary>
        public BassBoost BassBoost { get => GetBassBoost(); set => SetBassBoost(value); }

        /// <summary>
        /// Creates a new instance of the Kt0803.
        /// </summary>
        /// <param name="i2cDevice">The I2C device used for communication.</param>
        /// <param name="frequency">FM frequency (range from 70MHz to 108MHz).</param>
        /// <param name="region">Region.</param>
        /// <param name="power">Transmission power.</param>
        /// <param name="pga">PGA (Programmable Gain Amplifier) gain.</param>
        public Kt0803(I2cDevice i2cDevice, double frequency, Region region, TransmissionPower power = TransmissionPower.Power_108dBuV, PgaGain pga = PgaGain.PGA_00dB)
        {
            _i2cDevice = i2cDevice;
            Frequency = frequency;
            TransmissionPower = power;
            PgaGain = pga;
            Region = region;
            Mute = false;
            Standby = false;
        }

        /// <summary>
        /// Cleanup
        /// </summary>
        protected override void Dispose(bool disposing)
        {
            _i2cDevice?.Dispose();
            _i2cDevice = null;
        }

        /// <summary>
        /// Set Kt0803 FM frequency.
        /// </summary>
        /// <param name="frequency">FM frequency (range from 70MHz to 108MHz).</param>
        private void SetFrequency(double frequency)
        {
            // Details in Datasheet P7
            if (frequency < 70 || frequency > 108)
            {
                throw new ArgumentOutOfRangeException("Range from 70MHz to 108MHz.");
            }

            int freq, reg0, reg1, reg2;

            reg1 = ReadByte(Register.KT_CONFIG01);
            reg2 = ReadByte(Register.KT_CONFIG02);

            // 3 bytes
            freq = (int)(frequency * 20);
            freq &= 0b_1111_1111_1111;

            if ((freq & 0b_0001) > 0)
            {
                reg2 |= 0b_1000_0000;
            }
            else
            {
                reg2 &= ~0b_1000_0000;
            }
            reg0 = freq >> 1;
            reg1 = (reg1 & 0b_1111_1000) | (freq >> 9);

            WriteByte(Register.KT_CONFIG02, (byte)reg2);
            WriteByte(Register.KT_CHSEL, (byte)reg0);
            WriteByte(Register.KT_CONFIG01, (byte)reg1);
        }

        /// <summary>
        /// Get Kt0803 FM frequency.
        /// </summary>
        /// <returns>FM frequency.</returns>
        private double GetFrequency()
        {
            int reg0 = ReadByte(Register.KT_CHSEL);
            int reg1 = ReadByte(Register.KT_CONFIG01);
            int reg2 = ReadByte(Register.KT_CONFIG02);

            int freq = ((reg1 & 0b_0111) << 9) | (reg0 << 1) | (reg2 & 0b_1000_0000 >> 7);

            return Math.Round(freq / 20.0, 1);
        }

        /// <summary>
        /// Set Kt0803 PGA (Programmable Gain Amplifier) gain.
        /// </summary>
        /// <param name="pgaGain">PGA gain.</param>
        private void SetPga(PgaGain pgaGain)
        {
            // Details in Datasheet P9
            int reg1 = ReadByte(Register.KT_CONFIG01);
            int reg3 = ReadByte(Register.KT_CONFIG04);

            int pgaVal = (byte)pgaGain << 3;

            reg1 = (reg1 & 0b_1100_0111) | pgaVal;

            switch (pgaGain)
            {
                case PgaGain.PGA_00dB:
                case PgaGain.PGA_04dB:
                case PgaGain.PGA_08dB:
                case PgaGain.PGA_12dB:
                    reg3 = (reg3 & 0b_1100_1111) | (3 << 4);
                    break;
                case PgaGain.PGA_N04dB:
                case PgaGain.PGA_N08dB:
                case PgaGain.PGA_N12dB:
                    reg3 = (reg3 & 0b_1100_1111) | (0 << 4);
                    break;
                default:
                    break;
            }

            WriteByte(Register.KT_CONFIG01, (byte)reg1);
            WriteByte(Register.KT_CONFIG04, (byte)reg3);
        }

        /// <summary>
        /// Get Kt0803 PGA (Programmable Gain Amplifier) gain.
        /// </summary>
        /// <returns>PGA gain.</returns>
        private PgaGain GetPga()
        {
            int reg1 = ReadByte(Register.KT_CONFIG01);

            return (PgaGain)((reg1 & 0b_0011_1000) >> 3);
        }

        /// <summary>
        /// Set Kt0803 transmission power.
        /// </summary>
        /// <param name="power">Transmission power.</param>
        private void SetTransmissionPower(TransmissionPower power)
        {
            // Details in Datasheet P8 Table4
            int reg1 = ReadByte(Register.KT_CONFIG01);
            int reg2 = ReadByte(Register.KT_CONFIG02);
            int reg10 = ReadByte(Register.KT_CONFIG13);

            int powerVal = (byte)power;

            reg1 = (reg1 & 0b_0011_1111) | (powerVal << 6);

            if ((powerVal & 0b_0100) > 0)
            {
                reg10 |= 0b_1000_0000;
            }
            else
            {
                reg10 &= ~0b_1000_0000;
            }

            if ((powerVal & 0b_1000) > 0)
            {
                reg2 |= 0b_0100_0000;
            }
            else
            {
                reg2 &= ~0b_0100_0000;
            }

            if (powerVal >= 8)
            {
                WriteByte(Register.KT_CONFIG0E, 0b_0010);
            }
            else
            {
                WriteByte(Register.KT_CONFIG0E, 0b_0000);
            }

            WriteByte(Register.KT_CONFIG01, (byte)reg1);
            WriteByte(Register.KT_CONFIG02, (byte)reg2);
            WriteByte(Register.KT_CONFIG13, (byte)reg10);
        }

        /// <summary>
        /// Get Kt0803 transmission power.
        /// </summary>
        /// <returns>Transmission power.</returns>
        private TransmissionPower GetTransmissionPower()
        {
            int reg1 = ReadByte(Register.KT_CONFIG01);
            int reg2 = ReadByte(Register.KT_CONFIG02);
            int reg10 = ReadByte(Register.KT_CONFIG13);

            return (TransmissionPower)(((reg2 & 0b_0100_0000) >> 3) | (reg10 >> 5) | ((reg1 & 0b_1100_0000) >> 6));
        }

        /// <summary>
        /// Set Kt0803 region.
        /// </summary>
        /// <param name="region">region.</param>
        private void SetRegion(Region region)
        {
            // Details in Datasheet P8
            int reg2 = ReadByte(Register.KT_CONFIG02);

            switch (region)
            {
                case Region.America:
                case Region.Japan:
                    reg2 &= ~0b_0001;
                    break;
                case Region.Europe:
                case Region.Australia:
                case Region.China:
                case Region.Other:
                    reg2 |= 0b_0001;
                    break;
                default:
                    break;
            }

            WriteByte(Register.KT_CONFIG02, (byte)reg2);
        }

        /// <summary>
        /// Set Kt0803 mute.
        /// </summary>
        /// <param name="isMute">Mute when value is true.</param>
        private void SetMute(bool isMute)
        {
            // Details in Datasheet P8
            int reg2 = ReadByte(Register.KT_CONFIG02);

            if (isMute)
            {
                reg2 |= 0b_1000;
            }
            else
            {
                reg2 &= ~0b_1000;
            }

            WriteByte(Register.KT_CONFIG02, (byte)reg2);
        }

        /// <summary>
        /// Get Kt0803 mute.
        /// </summary>
        /// <returns>Mute when value is true.</returns>
        private bool GetMute()
        {
            int reg2 = ReadByte(Register.KT_CONFIG02);

            return (reg2 & 0b_1000) >> 3 == 1 ? true : false;
        }

        /// <summary>
        /// Set Kt0803 standby.
        /// </summary>
        /// <param name="isStandby">Standby when value is true.</param>
        private void SetStandby(bool isStandby)
        {
            // Details in Datasheet P10
            int reg4 = ReadByte(Register.KT_CONFIG0B);

            if (isStandby)
            {
                reg4 |= 0b_1000_0000;
            }
            else
            {
                reg4 &= ~0b_1000_0000;
            }

            WriteByte(Register.KT_CONFIG0B, (byte)reg4);
        }

        /// <summary>
        /// Get Kt0803 standby.
        /// </summary>
        /// <returns>Standby when value is true.</returns>
        private bool GetStandby()
        {
            int reg4 = ReadByte(Register.KT_CONFIG0B);

            return reg4  >> 7 == 1 ? true : false;
        }

        /// <summary>
        /// Set Kt0803 bass boost.
        /// </summary>
        /// <param name="bassBoost">Boost mode.</param>
        private void SetBassBoost(BassBoost bassBoost)
        {
            // Details in Datasheet P9
            int reg3 = ReadByte(Register.KT_CONFIG04);

            reg3 &= 0b_1111_1100;
            reg3 |= (byte)bassBoost;

            WriteByte(Register.KT_CONFIG04, (byte)reg3);
        }

        /// <summary>
        /// Get Kt0803 bass boost.
        /// </summary>
        /// <returns>Boost mode.</returns>
        private BassBoost GetBassBoost()
        {
            byte reg3 = ReadByte(Register.KT_CONFIG04);

            return (BassBoost)((reg3 << 6) >> 6);
        }

        private void WriteByte(Register register, byte value)
        {
            Span<byte> writeBuffer = stackalloc byte[] { (byte)register, value };

            _i2cDevice.Write(writeBuffer);
        }

        private byte ReadByte(Register register)
        {
            Span<byte> writeBuffer = stackalloc byte[] { (byte)register };
            Span<byte> readBuffer = stackalloc byte[1];

            _i2cDevice.WriteRead(writeBuffer, readBuffer);

            return readBuffer[0];
        }
    }
}
