// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using System;
using System.Device.I2c;

namespace Iot.Device.Ds1307
{
    /// <summary>
    /// Realtime Clock DS1307
    /// </summary>
    public class Ds1307 : IDisposable
    {
        /// <summary>
        /// DS1307 Default I2C Address
        /// </summary>
        public const byte DefaultI2cAddress = 0x68;

        private I2cDevice _sensor = null;

        /// <summary>
        /// DS1307 DateTime
        /// </summary>
        public DateTime DateTime { get => ReadTime(); set => SetTime(value); }

        /// <summary>
        /// Creates a new instance of the DS1307
        /// </summary>
        /// <param name="sensor">I2C Device, like UnixI2cDevice or Windows10I2cDevice</param>
        public Ds1307(I2cDevice sensor)
        {
            _sensor = sensor;
        }

        /// <summary>
        /// Read Time from DS1307
        /// </summary>
        /// <returns>DS1307 Time</returns>
        private DateTime ReadTime()
        {
            Span<byte> rawData = stackalloc byte[7];

            _sensor.WriteByte((byte)Register.RTC_SEC_REG_ADDR);
            _sensor.Read(rawData);

            // Details in the Datasheet P8
            return new DateTime(2000 + Bcd2Int(rawData[6]), 
                                Bcd2Int(rawData[5]), 
                                Bcd2Int(rawData[4]), 
                                Bcd2Int(rawData[2]),
                                Bcd2Int(rawData[1]), 
                                Bcd2Int((byte)(rawData[0] & 0b_0111_1111)));
        }

        /// <summary>
        /// Set DS1307 Time
        /// </summary>
        /// <param name="time">Time</param>
        private void SetTime(DateTime time)
        {
            Span<byte> setData = stackalloc byte[8];

            setData[0] = (byte)Register.RTC_SEC_REG_ADDR;

            // Details in the Datasheet P8
            // | bit 7: CH | bit 6-0: sec |
            setData[1] = (byte)(Int2Bcd(time.Second) & 0b_0111_1111);
            setData[2] = Int2Bcd(time.Minute);
            // | bit 7: 0 | bit 6: 12/24 hour | bit 5-0: hour |
            setData[3] = (byte)(Int2Bcd(time.Hour) & 0b_0011_1111);
            setData[4] = Int2Bcd((int)time.DayOfWeek + 1);
            setData[5] = Int2Bcd(time.Day);
            setData[6] = Int2Bcd(time.Month);
            setData[7] = Int2Bcd(time.Year - 2000);

            _sensor.Write(setData);
        }

        /// <summary>
        /// Cleanup
        /// </summary>
        public void Dispose()
        {
            _sensor?.Dispose();
            _sensor = null;
        }

        /// <summary>
        /// BCD To Int
        /// </summary>
        /// <param name="bcd">BCD Code</param>
        /// <returns>int</returns>
        private int Bcd2Int(byte bcd)
        {
            return ((bcd >> 4) * 10) + (bcd % 16);
        }

        /// <summary>
        /// Int To BCD
        /// </summary>
        /// <param name="dec">int</param>
        /// <returns>BCD Code</returns>
        private byte Int2Bcd(int dec)
        {
            return (byte)(((dec / 10) << 4) + (dec % 10));
        }
    }
}
