// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using System;
using System.Device.I2c.Devices;

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

        private I2cDevice _i2cDevice = null;

        /// <summary>
        /// DS1307 DateTime
        /// </summary>
        public DateTime DateTime { get => ReadTime(); set => SetTime(value); }

        /// <summary>
        /// Creates a new instance of the DS1307
        /// </summary>
        /// <param name="i2cDevice">>The I2C device used for communication.</param>
        public Ds1307(I2cDevice i2cDevice)
        {
            _i2cDevice = i2cDevice;
        }

        /// <summary>
        /// Read Time from DS1307
        /// </summary>
        /// <returns>DS1307 Time</returns>
        private DateTime ReadTime()
        {
            Span<byte> readBuffer = stackalloc byte[7];

            // Read all registers at the same time
            _i2cDevice.WriteByte((byte)Register.RTC_SEC_REG_ADDR);
            _i2cDevice.Read(readBuffer);

            // Details in the Datasheet P8
            return new DateTime(2000 + Bcd2Int(readBuffer[6]), 
                                Bcd2Int(readBuffer[5]), 
                                Bcd2Int(readBuffer[4]), 
                                Bcd2Int(readBuffer[2]),
                                Bcd2Int(readBuffer[1]), 
                                Bcd2Int((byte)(readBuffer[0] & 0b_0111_1111)));
        }

        /// <summary>
        /// Set DS1307 Time
        /// </summary>
        /// <param name="time">Time</param>
        private void SetTime(DateTime time)
        {
            Span<byte> writeBuffer = stackalloc byte[8];

            writeBuffer[0] = (byte)Register.RTC_SEC_REG_ADDR;

            // Details in the Datasheet P8
            // | bit 7: CH | bit 6-0: sec |
            writeBuffer[1] = (byte)(Int2Bcd(time.Second) & 0b_0111_1111);
            writeBuffer[2] = Int2Bcd(time.Minute);
            // | bit 7: 0 | bit 6: 12/24 hour | bit 5-0: hour |
            writeBuffer[3] = (byte)(Int2Bcd(time.Hour) & 0b_0011_1111);
            writeBuffer[4] = Int2Bcd((int)time.DayOfWeek + 1);
            writeBuffer[5] = Int2Bcd(time.Day);
            writeBuffer[6] = Int2Bcd(time.Month);
            writeBuffer[7] = Int2Bcd(time.Year - 2000);

            _i2cDevice.Write(writeBuffer);
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
