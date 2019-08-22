// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using System;
using System.Device.I2c;

namespace Iot.Device.Kt0803
{
    /// <summary>
    /// FM Radio Modulator Module KT0803
    /// </summary>
    public class Kt0803 : IDisposable
    {
        private I2cDevice _i2cDevice;

        /// <summary>
        /// Kt0803 I2C Address
        /// </summary>
        public const byte I2cAddress = 0x3E;

        /// <summary>
        /// Kt0803 Standby
        /// </summary>
        public bool Standby { get => GetStandby(); set => SetStandby(value); }

        /// <summary>
        /// Kt0803 Mute
        /// </summary>
        public bool Mute { get => GetMute(); set => SetMute(value); }

        /// <summary>
        /// Kt0803 FM Channel (Range from 70Mhz to 108Mhz)
        /// </summary>
        public double Channel { get => GetChannel(); set => SetChannel(value); }

        /// <summary>
        /// Kt0803 PGA ( Programmable Gain Amplifier ) Gain
        /// </summary>
        public PgaGain PgaGain { get => GetPga(); set => SetPga(value); }

        /// <summary>
        /// Kt0803 Transmission Power
        /// </summary>
        public TransmissionPower TransmissionPower { get => GetTransmissionPower(); set => SetTransmissionPower(value); }

        private Country _country;
        /// <summary>
        /// Kt0803 Country
        /// </summary>
        public Country Country { get => _country; set { SetCountry(value); _country = value; } }

        /// <summary>
        /// Kt0803 Bass Boost
        /// </summary>
        public BassBoost BassBoost { get => GetBassBoost(); set => SetBassBoost(value); }

        /// <summary>
        /// Creates a new instance of the Kt0803
        /// </summary>
        /// <param name="mHz">FM Channel (Range from 70Mhz to 108Mhz)</param>
        /// <param name="country">Country</param>
        /// <param name="power">Transmission Power</param>
        /// <param name="pga">PGA (Programmable Gain Amplifier) Gain</param>
        public Kt0803(I2cDevice i2cDevice, double mHz, Country country, TransmissionPower power = TransmissionPower.Power_108dBuV, PgaGain pga = PgaGain.PGA_00dB)
        {
            _i2cDevice = i2cDevice;
            Channel = mHz;
            TransmissionPower = power;
            PgaGain = pga;
            Country = country;
            Mute = false;
            Standby = false;
        }

        /// <summary>
        /// Cleanup
        /// </summary>
        public void Dispose()
        {
            _i2cDevice?.Dispose();
            _i2cDevice = null;
        }

        /// <summary>
        /// Set Kt0803 FM Channel
        /// </summary>
        /// <param name="mhz">MHz ( Range from 70Mhz to 108Mhz )</param>
        private void SetChannel(double mhz)
        {
            // Details in Datasheet P7
            if (mhz < 70 || mhz > 108)
            {
                throw new ArgumentOutOfRangeException("Range from 70MHz to 108MHz.");
            }

            int freq, reg0, reg1, reg2;

            reg1 = ReadByte(Register.KT_CONFIG1);
            reg2 = ReadByte(Register.KT_CONFIG2);

            // 3 bytes
            freq = (int)(mhz * 20);
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

            WriteByte(Register.KT_CONFIG2, (byte)reg2);
            WriteByte(Register.KT_CHSEL, (byte)reg0);
            WriteByte(Register.KT_CONFIG1, (byte)reg1);
        }

        /// <summary>
        /// Get Kt0803 FM Channel
        /// </summary>
        /// <returns>FM Channel</returns>
        private double GetChannel()
        {
            int reg0 = ReadByte(Register.KT_CHSEL);
            int reg1 = ReadByte(Register.KT_CONFIG1);
            int reg2 = ReadByte(Register.KT_CONFIG2);

            int freq = ((reg1 & 0b_0111) << 9) | (reg0 << 1) | (reg2 & 0b_1000_0000 >> 7);

            return Math.Round(freq / 20.0, 1);
        }

        /// <summary>
        /// Set Kt0803 PGA ( Programmable Gain Amplifier ) Gain
        /// </summary>
        /// <param name="pgaGain">PGA Gain</param>
        private void SetPga(PgaGain pgaGain)
        {
            // Details in Datasheet P9
            int reg1 = ReadByte(Register.KT_CONFIG1);
            int reg3 = ReadByte(Register.KT_CONFIG3);

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

            WriteByte(Register.KT_CONFIG1, (byte)reg1);
            WriteByte(Register.KT_CONFIG3, (byte)reg3);
        }

        /// <summary>
        /// Get Kt0803 PGA ( Programmable Gain Amplifier ) Gain
        /// </summary>
        /// <returns>PGA Gain</returns>
        private PgaGain GetPga()
        {
            int reg1 = ReadByte(Register.KT_CONFIG1);

            return (PgaGain)((reg1 & 0b_0011_1000) >> 3);
        }

        /// <summary>
        /// Set Kt0803 Transmission Power
        /// </summary>
        /// <param name="power">Transmission Power</param>
        private void SetTransmissionPower(TransmissionPower power)
        {
            // Details in Datasheet P8 Table4
            int reg1 = ReadByte(Register.KT_CONFIG1);
            int reg2 = ReadByte(Register.KT_CONFIG2);
            int reg10 = ReadByte(Register.KT_CONFIG10);

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
                WriteByte(Register.KT_CONFIG6, 0b_0010);
            }
            else
            {
                WriteByte(Register.KT_CONFIG6, 0b_0000);
            }

            WriteByte(Register.KT_CONFIG1, (byte)reg1);
            WriteByte(Register.KT_CONFIG2, (byte)reg2);
            WriteByte(Register.KT_CONFIG10, (byte)reg10);
        }

        /// <summary>
        /// Get Kt0803 Transmission Power
        /// </summary>
        /// <returns>Transmission Power</returns>
        private TransmissionPower GetTransmissionPower()
        {
            int reg1 = ReadByte(Register.KT_CONFIG1);
            int reg2 = ReadByte(Register.KT_CONFIG2);
            int reg10 = ReadByte(Register.KT_CONFIG10);

            return (TransmissionPower)(((reg2 & 0b_0100_0000) >> 3) | (reg10 >> 5) | ((reg1 & 0b_1100_0000) >> 6));
        }

        /// <summary>
        /// Set Kt0803 Country
        /// </summary>
        /// <param name="country">Country</param>
        private void SetCountry(Country country)
        {
            // Details in Datasheet P8
            int reg2 = ReadByte(Register.KT_CONFIG2);

            switch (country)
            {
                case Country.America:
                case Country.Japan:
                    reg2 &= ~0b_0001;
                    break;
                case Country.Europe:
                case Country.Australia:
                case Country.China:
                    reg2 |= 0b_0001;
                    break;
                default:
                    break;
            }

            WriteByte(Register.KT_CONFIG2, (byte)reg2);
        }

        /// <summary>
        /// Set Kt0803 Mute
        /// </summary>
        /// <param name="isMute">Mute when value is true</param>
        private void SetMute(bool isMute)
        {
            // Details in Datasheet P8
            int reg2 = ReadByte(Register.KT_CONFIG2);

            if (isMute)
            {
                reg2 |= 0b_1000;
            }
            else
            {
                reg2 &= ~0b_1000;
            }

            WriteByte(Register.KT_CONFIG2, (byte)reg2);
        }

        /// <summary>
        /// Get Kt0803 Mute
        /// </summary>
        /// <returns>Mute when value is true</returns>
        private bool GetMute()
        {
            int reg2 = ReadByte(Register.KT_CONFIG2);

            return (reg2 & 0b_1000) >> 3 == 1 ? true : false;
        }

        /// <summary>
        /// Set Kt0803 Standby
        /// </summary>
        /// <param name="isStandby">Standby when value is true</param>
        private void SetStandby(bool isStandby)
        {
            // Details in Datasheet P10
            int reg4 = ReadByte(Register.KT_CONFIG4);

            if (isStandby)
            {
                reg4 |= 0b_1000_0000;
            }
            else
            {
                reg4 &= ~0b_1000_0000;
            }

            WriteByte(Register.KT_CONFIG4, (byte)reg4);
        }

        /// <summary>
        /// Get Kt0803 Standby
        /// </summary>
        /// <returns>Standby when value is true</returns>
        private bool GetStandby()
        {
            int reg4 = ReadByte(Register.KT_CONFIG4);

            return reg4  >> 7 == 1 ? true : false;
        }

        /// <summary>
        /// Set Kt0803 Bass Boost
        /// </summary>
        /// <param name="bassBoost">Boost Mode</param>
        private void SetBassBoost(BassBoost bassBoost)
        {
            // Details in Datasheet P9
            int reg3 = ReadByte(Register.KT_CONFIG3);

            reg3 &= 0b_1111_1100;
            reg3 |= (byte)bassBoost;

            WriteByte(Register.KT_CONFIG3, (byte)reg3);
        }

        /// <summary>
        /// Get Kt0803 Bass Boost
        /// </summary>
        /// <returns>Bass Boost</returns>
        private BassBoost GetBassBoost()
        {
            byte reg3 = ReadByte(Register.KT_CONFIG3);

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
