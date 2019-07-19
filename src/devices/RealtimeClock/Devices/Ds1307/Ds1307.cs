// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using System;
using System.Device.I2c;

namespace Iot.Device.Rtc
{
    /// <summary>
    /// Realtime Clock DS1307
    /// </summary>
    public class Ds1307 : RtcBase
    {
        /// <summary>
        /// DS1307 Default I2C Address
        /// </summary>
        public const byte DefaultI2cAddress = 0x68;

        private I2cDevice _i2cDevice;
        private bool _disposedValue = false;

        /// <summary>
        /// Creates a new instance of the DS1307
        /// </summary>
        /// <param name="i2cDevice">The I2C device used for communication.</param>
        public Ds1307(I2cDevice i2cDevice)
        {
            _i2cDevice = i2cDevice;
        }

        /// <summary>
        /// Read Time from DS1307
        /// </summary>
        /// <returns>DS1307 Time</returns>
        protected override DateTime ReadTime()
        {
            Span<byte> readBuffer = stackalloc byte[7];

            // Read all registers at the same time
            _i2cDevice.WriteByte((byte)Ds1307Register.RTC_SEC_REG_ADDR);
            _i2cDevice.Read(readBuffer);

            // Details in the Datasheet P8
            return new DateTime(2000 + NumberTool.Bcd2Dec(readBuffer[6]), 
                                NumberTool.Bcd2Dec(readBuffer[5]), 
                                NumberTool.Bcd2Dec(readBuffer[4]), 
                                NumberTool.Bcd2Dec(readBuffer[2]),
                                NumberTool.Bcd2Dec(readBuffer[1]), 
                                NumberTool.Bcd2Dec((byte)(readBuffer[0] & 0b_0111_1111)));
        }

        /// <summary>
        /// Set DS1307 Time
        /// </summary>
        /// <param name="time">Time</param>
        protected override void SetTime(DateTime time)
        {
            Span<byte> writeBuffer = stackalloc byte[8];

            writeBuffer[0] = (byte)Ds1307Register.RTC_SEC_REG_ADDR;

            // Details in the Datasheet P8
            // | bit 7: CH | bit 6-0: sec |
            writeBuffer[1] = (byte)(NumberTool.Dec2Bcd(time.Second) & 0b_0111_1111);
            writeBuffer[2] = NumberTool.Dec2Bcd(time.Minute);
            // | bit 7: 0 | bit 6: 12/24 hour | bit 5-0: hour |
            writeBuffer[3] = (byte)(NumberTool.Dec2Bcd(time.Hour) & 0b_0011_1111);
            writeBuffer[4] = NumberTool.Dec2Bcd((int)time.DayOfWeek + 1);
            writeBuffer[5] = NumberTool.Dec2Bcd(time.Day);
            writeBuffer[6] = NumberTool.Dec2Bcd(time.Month);
            writeBuffer[7] = NumberTool.Dec2Bcd(time.Year - 2000);

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
