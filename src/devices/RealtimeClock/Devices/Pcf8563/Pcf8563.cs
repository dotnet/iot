// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using System;
using System.Device.I2c;

namespace Iot.Device.Rtc
{
    /// <summary>
    /// Realtime Clock PCF8563
    /// </summary>
    public class Pcf8563 : RtcBase
    {
        /// <summary>
        /// PCF8563 Default I2C Address
        /// </summary>
        public const byte DefaultI2cAddress = 0x51;

        private I2cDevice _i2cDevice;
        private bool _disposedValue = false;

        /// <summary>
        /// Creates a new instance of the PCF8563
        /// </summary>
        /// <param name="i2cDevice">The I2C device used for communication.</param>
        public Pcf8563(I2cDevice i2cDevice)
        {
            _i2cDevice = i2cDevice;

            // Set "Normal Mode"
            Span<byte> ctrl1Config = stackalloc byte[] { (byte)Pcf8563Register.PCF_CTRL1_ADDR, 0x00 };
            _i2cDevice.Write(ctrl1Config);
        }

        /// <summary>
        /// Read Time from PCF8563
        /// </summary>
        /// <returns>PCF8563 Time</returns>
        protected override DateTime ReadTime()
        {
            // Sec, Min, Hour, Date, Day, Month & Century, Year
            Span<byte> readBuffer = stackalloc byte[7];

            _i2cDevice.WriteByte((byte)Pcf8563Register.PCF_SEC_ADDR);
            _i2cDevice.Read(readBuffer);

            return new DateTime(readBuffer[5] >> 7 == 1 ? 2000 + NumberTool.Bcd2Dec(readBuffer[6]) : 1900 + NumberTool.Bcd2Dec(readBuffer[6]),
                                NumberTool.Bcd2Dec((byte)(readBuffer[5] & 0b_0001_1111)),
                                NumberTool.Bcd2Dec((byte)(readBuffer[3] & 0b_0011_1111)),
                                NumberTool.Bcd2Dec((byte)(readBuffer[2] & 0b_0011_1111)),
                                NumberTool.Bcd2Dec((byte)(readBuffer[1] & 0b_0111_1111)),
                                NumberTool.Bcd2Dec((byte)(readBuffer[0] & 0b_0111_1111)));
        }

        /// <summary>
        /// Set PCF8563 Time
        /// </summary>
        /// <param name="time">Time</param>
        protected override void SetTime(DateTime time)
        {
            Span<byte> writeBuffer = stackalloc byte[8];

            writeBuffer[0] = (byte)Pcf8563Register.PCF_SEC_ADDR;
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

            _i2cDevice.Write(writeBuffer);
        }

        /// <summary>
        /// Cleanup
        /// </summary>
        protected override void Dispose(bool disposing)
        {
            if (!_disposedValue)
            {
                if (disposing)
                {
                    _i2cDevice?.Dispose();
                }

                _i2cDevice = null;
                _disposedValue = true;
            }
        }
    }
}
