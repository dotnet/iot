// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using System;
using System.Device;
using System.Device.I2c;
using System.Threading;
using Iot.Units;

namespace Iot.Device.Shtc3
{
    /// <summary>
    /// Temperature and humidity sensor Shtc3
    /// </summary>
    public class Shtc3 : IDisposable
    {
        // CRC const
        private const byte CRC_POLYNOMIAL = 0x31;
        private const byte CRC_INIT = 0xFF;

        /// <summary>
        /// Default I2C address
        /// </summary>
        public const int I2cAddress = 0x70;

        private I2cDevice _i2cDevice;

        /// <summary>
        /// Initializes a new instance of the <see cref="Shtc3"/> class.
        /// </summary>
        /// <param name="i2cDevice">The I2C device used for communication.</param>
        public Shtc3(I2cDevice i2cDevice)
        {
            _i2cDevice = i2cDevice;

            Wakeup();
            _status = Status.Idle;

            Reset();
        }

        private Status _status;

        /// <summary>
        /// Set Shtc3 state
        /// </summary>
        public Status Status
        {
            get
            {
                return _status;
            }
            set
            {
                if (value == _status)
                {
                    return;
                }

                if (value == Status.Idle)
                {
                    Wakeup();
                }
                else
                {
                    Sleep();
                }

                _status = value;
            }
        }

        /// <summary>
        /// Try read Temperature and Humidity
        /// </summary>
        /// <param name="sensorMeasure">measure return by sensor</param>
        /// <param name="LowPower">"true" measure in low power mode, "false"(default) measure in normal power mode</param>
        /// <param name="ClockStretching">"true" allow clock stretching, "false"(default) without clock stretching</param>
        /// <returns></returns>
        public bool TryGetTempAndHumi(out Measure sensorMeasure, bool LowPower = false, bool ClockStretching = false)
        {
            Register cmd = GetMeasurementCmd(LowPower, ClockStretching);

            if (!TryReadSensorData(cmd, out var st, out var srh))
            {
                sensorMeasure = default(Measure);
                return false;
            }

            // Details in the Datasheet P9
            sensorMeasure = new Measure(Temperature.FromCelsius(Math.Round(st * 175 / 65536.0 - 45, 1)),
                                                 Math.Round(srh * 100 / 65536.0, 1));
            return true;
        }

        private bool TryReadSensorData(Register cmd, out double temperature, out double humidity)
        {
            Write(cmd);

            Span<byte> readBuff = stackalloc byte[6];

            _i2cDevice.Read(readBuff);

            // Details in the Datasheet P7
            int st = (readBuff[0] << 8) | readBuff[1];      // Temp
            int srh = (readBuff[3] << 8) | readBuff[4];     // Humi

            // check 8-bit crc
            bool tCrc = CheckCrc8(readBuff.Slice(0, 2), readBuff[2]);
            bool rhCrc = CheckCrc8(readBuff.Slice(3, 2), readBuff[5]);
            if (tCrc == false || rhCrc == false)
            {
                temperature = double.NaN;
                humidity = double.NaN;
                return false;
            }

            temperature = st;
            humidity = srh;
            return true;
        }

        private static Register GetMeasurementCmd(bool LowPower, bool ClockStretching)
        {
            Register mea = Register.SHTC3_MEAS_T_RH_POLLING_NPM;
            if (LowPower)
            {
                if (ClockStretching)
                {
                    mea = Register.SHTC3_MEAS_T_RH_CLOCKSTR_LPM;
                }
                else
                {
                    mea = Register.SHTC3_MEAS_T_RH_POLLING_LPM;
                }
            }
            else
            {
                if (ClockStretching)
                {
                    mea = Register.SHTC3_MEAS_T_RH_CLOCKSTR_NPM;
                }
            }

            return mea;
        }

        /// <summary>
        /// SHTC3 Sleep
        /// </summary>
        private void Sleep()
        {
            Write(Register.SHTC3_SLEEP);
        }

        /// <summary>
        /// SHTC3 Wakeup
        /// </summary>
        private void Wakeup()
        {
            Write(Register.SHTC3_WAKEUP);
        }

        /// <summary>
        /// SHTC3 Soft Reset
        /// </summary>
        public void Reset()
        {
            Write(Register.SHTC3_RESET);
        }

        private int _id = 0;

        /// <summary>
        /// Sensor Id
        /// </summary>
        public int Id
        {
            get
            {
                if (_id == 0)
                {
                    Id = ReadId();
                }

                return _id;
            }
            set
            {
                _id = value;
            }
        }

        /// <summary>
        /// Read Id
        /// </summary>
        private int ReadId()
        {
            Write(Register.SHTC3_ID);

            Span<byte> readBuff = stackalloc byte[3];

            _i2cDevice.Read(readBuff);

            // check 8-bit crc
            if (!CheckCrc8(readBuff.Slice(0, 2), readBuff[2]))
            {
                return 0;
            }

            var id = (readBuff[0] << 8) | readBuff[1];

            // check the result match to the SHTC3 product code
            if (!ValidShtc3Id(id))
            {
                return 0;
            }

            return id;
        }

        /// <summary>
        /// Check the match to the SHTC3 product code
        /// Table 15  while bits 11 and 5:0 contain the SHTC3 specific product code 0b_0000_1000_0000_0111
        /// </summary>
        /// <param name="id">Id to test</param>
        /// <returns></returns>
        private static bool ValidShtc3Id(int id)
        {
            return (id & 0b_0000_1000_0011_1111) == 0b_0000_1000_0000_0111;
        }

        /// <summary>
        /// 8-bit CRC Checksum Calculation
        /// </summary>
        /// <param name="data">Raw Data</param>
        /// <param name="crc8">Raw CRC8</param>
        /// <returns>Checksum is true or false</returns>
        private static bool CheckCrc8(ReadOnlySpan<byte> data, byte crc8)
        {
            // Details in the Datasheet table 16 P9
            byte crc = CRC_INIT;
            for (int i = 0; i < 2; i++)
            {
                crc ^= data[i];

                for (int j = 8; j > 0; j--)
                {
                    if ((crc & 0x80) != 0)
                    {
                        crc = (byte)((crc << 1) ^ CRC_POLYNOMIAL);
                    }
                    else
                    {
                        crc = (byte)(crc << 1);
                    }
                }
            }

            return crc == crc8;
        }

        private void Write(Register register)
        {
            byte msb = (byte)((short)register >> 8);
            byte lsb = (byte)((short)register & 0xFF);

            Span<byte> writeBuff = stackalloc byte[]
            {
                msb,
                lsb
            };

            _i2cDevice.Write(writeBuff);

            // wait SCL free
            DelayHelper.DelayMilliseconds(20, false);
        }

        /// <summary>
        /// Cleanup
        /// </summary>
        public void Dispose()
        {
            _i2cDevice?.Dispose();
            _i2cDevice = null;
        }
    }
}
