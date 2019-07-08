// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using System;
using System.Device.I2c;

namespace Iot.Device.Pcf8563
{
    /// <summary>
    /// Realtime Clock PCF8563
    /// </summary>
    public class Pcf8563 : IDisposable
    {
        private I2cDevice _sensor = null;

        /// <summary>
        /// PCF8563 Default I2C Address
        /// </summary>
        public const byte DefaultI2cAddress = 0x51;

        /// <summary>
        /// DS3231 DateTime
        /// </summary>
        public DateTime DateTime { get => ReadTime(); set => SetTime(value); }

        /// <summary>
        /// Creates a new instance of the PCF8563
        /// </summary>
        /// <param name="sensor">I2C Device, like UnixI2cDevice or Windows10I2cDevice</param>
        public Pcf8563(I2cDevice sensor)
        {
            _sensor = sensor;

            // Set "Normal Mode"
            Span<byte> ctrl1Config = stackalloc byte[] { (byte)Register.PCF_CTRL1_ADDR, 0x00 };
            _sensor.Write(ctrl1Config);
        }

        /// <summary>
        /// Set PCF8563 Time
        /// </summary>
        /// <param name="time">Time</param>
        private void SetTime(DateTime time)
        {
            Span<byte> writeBuffer = stackalloc byte[8];

            writeBuffer[0] = (byte)Register.PCF_SEC_ADDR;
            // Set bit8 as 0 to guarantee clock integrity
            writeBuffer[1] = (byte)(NumberTool.Dec2Bcd(time.Second) & 0b_0111_1111);
            writeBuffer[2] = NumberTool.Dec2Bcd(time.Minute);
            writeBuffer[3] = NumberTool.Dec2Bcd(time.Hour);
            writeBuffer[4] = NumberTool.Dec2Bcd(time.Day);
            writeBuffer[5] = NumberTool.Dec2Bcd((int)time.DayOfWeek);
            if (time.Year >= 2000)
            {
                writeBuffer[6] = (byte)(NumberTool.Dec2Bcd(time.Month) | 0b_1000_0000);
                writeBuffer[7] = NumberTool.Dec2Bcd(time.Year - 2000);
            }
            else
            {
                writeBuffer[6] = NumberTool.Dec2Bcd(time.Month);
                writeBuffer[7] = NumberTool.Dec2Bcd(time.Year - 1900);
            }

            _sensor.Write(writeBuffer);
        }

        /// <summary>
        /// Read Time from PCF8563
        /// </summary>
        /// <returns>PCF8563 Time</returns>
        private DateTime ReadTime()
        {
            // Sec, Min, Hour, Date, Day, Month & Century, Year
            Span<byte> readBuffer = stackalloc byte[7];

            _sensor.WriteByte((byte)Register.PCF_SEC_ADDR);
            _sensor.Read(readBuffer);

            return new DateTime(readBuffer[5] >> 7 == 1 ? 2000 + NumberTool.Bcd2Dec(readBuffer[6]) : 1900 + NumberTool.Bcd2Dec(readBuffer[6]),
                                NumberTool.Bcd2Dec((byte)(readBuffer[5] & 0b_0001_1111)),
                                NumberTool.Bcd2Dec((byte)(readBuffer[3] & 0b_0011_1111)),
                                NumberTool.Bcd2Dec((byte)(readBuffer[2] & 0b_0011_1111)),
                                NumberTool.Bcd2Dec((byte)(readBuffer[1] & 0b_0111_1111)),
                                NumberTool.Bcd2Dec((byte)(readBuffer[0] & 0b_0111_1111)));
        }

        /// <summary>
        /// Cleanup
        /// </summary>
        public void Dispose()
        {
            _sensor?.Dispose();
            _sensor = null;
        }
    }
}
