// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using System;
using System.Device.Spi;
using System.Numerics;

namespace Iot.Device.Adxl345
{
    /// <summary>
    /// SPI Accelerometer ADX1345
    /// </summary>
    public class Adxl345 : IDisposable
    {
        private SpiDevice _sensor = null;
                              
        private readonly byte _gravityRangeByte;                             
        private readonly int _range;

        private const int Resolution = 1024;        // All g ranges resolution

        #region Addr
        private const byte ADDRESS_POWER_CTL = 0x2D;        // Address of the Power Control register              
        private const byte ADDRESS_DATA_FORMAT = 0x31;      // Address of the Data Format register               
        private const byte ADDRESS_X0 = 0x32;               // Address of the X Axis data register                
        private const byte ADDRESS_Y0 = 0x34;               // Address of the Y Axis data register              
        private const byte ADDRESS_Z0 = 0x36;               // Address of the Z Axis data register               

        private const byte ACCEL_SPI_RW_BIT = 0x80;         // Bit used in SPI transactions to indicate read/write  
        private const byte ACCEL_SPI_MB_BIT = 0x40;         // Bit used to indicate multi-byte SPI transactions    
        #endregion

        #region SpiSetting
        /// <summary>
        /// ADXL345 SPI ClockFrequency
        /// </summary>
        public const int ClockFrequency = 5000000;

        /// <summary>
        /// ADXL345 SpiMode
        /// </summary>
        public const SpiMode Mode = SpiMode.Mode3;
        #endregion

        /// <summary>
        /// Read Acceleration from ADXL345
        /// </summary>
        public Vector3 Acceleration => ReadAcceleration();

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="sensor">The communications channel to a device on a SPI bus</param>
        /// <param name="gravityRange">Gravity Measurement Range</param>
        public Adxl345(SpiDevice sensor, GravityRange gravityRange)
        {
            if (gravityRange == GravityRange.Range1)
            {
                _range = 4;
            }
            else if (gravityRange == GravityRange.Range2)
            {
                _range = 8;
            }
            else if (gravityRange == GravityRange.Range3)
            {
                _range = 16;
            }
            else if (gravityRange == GravityRange.Range4)
            {
                _range = 32;
            }
            _gravityRangeByte = (byte)gravityRange;

            _sensor = sensor;

            Initialize();
        }

        /// <summary>
        /// Initialize ADXL345
        /// </summary>
        private void Initialize()
        {
            Span<byte> dataFormat = stackalloc byte[] { ADDRESS_DATA_FORMAT, _gravityRangeByte };
            Span<byte> powerControl = stackalloc byte[] { ADDRESS_POWER_CTL, 0x08 };

            _sensor.Write(dataFormat);
            _sensor.Write(powerControl);
        }

        /// <summary>
        /// Read data from ADXL345
        /// </summary>
        /// <returns>Acceleration</returns>
        private Vector3 ReadAcceleration()
        {
            int units = Resolution / _range;

            byte[] readBuf = new byte[6 + 1];
            byte[] regAddrBuf = new byte[1 + 6];

            regAddrBuf[0] = ADDRESS_X0 | ACCEL_SPI_RW_BIT | ACCEL_SPI_MB_BIT;
            _sensor.TransferFullDuplex(regAddrBuf, readBuf);
            Array.Copy(readBuf, 1, readBuf, 0, 6);

            short AccelerationX = BitConverter.ToInt16(readBuf, 0);
            short AccelerationY = BitConverter.ToInt16(readBuf, 2);
            short AccelerationZ = BitConverter.ToInt16(readBuf, 4);

            Vector3 accel = new Vector3
            {
                X = (float)AccelerationX / units,
                Y = (float)AccelerationY / units,
                Z = (float)AccelerationZ / units
            };

            return accel;
        }

        /// <summary>
        /// Cleanup
        /// </summary>
        public void Dispose()
        {
            if (_sensor != null)
            {
                _sensor.Dispose();
                _sensor = null;
            }
        }
    }
}