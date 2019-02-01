// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using System;
using System.Device.Spi;
using System.Runtime.InteropServices;
using System.Device.Spi.Drivers;

namespace Iot.Device.Adxl345
{
    /// <summary>
    /// SPI Accelerometer ADX1345
    /// </summary>
    public class Adxl345 : IDisposable
    {
        private SpiDevice _sensor = null;

        private readonly int _busId;
        private readonly OSPlatform _os;
        private readonly int _cs;                                
        private readonly byte _gravityRangeByte;             
        private readonly int _resolution = 1024;                      
        private readonly int _range;

        #region Addr
        private const byte ADDRESS_POWER_CTL = 0x2D;        // Address of the Power Control register              
        private const byte ADDRESS_DATA_FORMAT = 0x31;      // Address of the Data Format register               
        private const byte ADDRESS_X0 = 0x32;               // Address of the X Axis data register                
        private const byte ADDRESS_Y0 = 0x34;               // Address of the Y Axis data register              
        private const byte ADDRESS_Z0 = 0x36;               // Address of the Z Axis data register               

        private const byte ACCEL_SPI_RW_BIT = 0x80;         // Bit used in SPI transactions to indicate read/write  
        private const byte ACCEL_SPI_MB_BIT = 0x40;         // Bit used to indicate multi-byte SPI transactions    
        #endregion

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="os">The program runing platform (Linux or Windows10)</param>
        /// <param name="busId">SPI Bus ID</param>
        /// <param name="chipSelect">CS Port</param>
        /// <param name="gravityRange">Gravity Range</param>
        public Adxl345(OSPlatform os, int busId, int chipSelect, GravityRange gravityRange)
        {
            _busId = busId;
            _os = os;
            _cs = chipSelect;

            if (gravityRange == GravityRange.Two)
            {
                _range = 4;
            }
            else if (gravityRange == GravityRange.Four)
            {
                _range = 8;
            }
            else if (gravityRange == GravityRange.Eight)
            {
                _range = 16;
            }
            else if (gravityRange == GravityRange.Sixteen)
            {
                _range = 32;
            }
            _gravityRangeByte = (byte)gravityRange;
        }

        /// <summary>
        /// Initialize ADXL345
        /// </summary>
        public void Initialize()
        {
            var settings = new SpiConnectionSettings(_busId, _cs)
            {
                ClockFrequency = 5000000,
                Mode = SpiMode.Mode3
            };

            if (_os == OSPlatform.Linux)
            {
                _sensor = new UnixSpiDevice(settings);
            }
            else if (_os == OSPlatform.Windows)
            {
                _sensor = new Windows10SpiDevice(settings);
            }

            byte[] WriteBuf_DataFormat = new byte[] { ADDRESS_DATA_FORMAT, _gravityRangeByte };
            byte[] WriteBuf_PowerControl = new byte[] { ADDRESS_POWER_CTL, 0x08 };

            _sensor.Write(WriteBuf_DataFormat);
            _sensor.Write(WriteBuf_PowerControl);
        }

        /// <summary>
        /// Read data from ADXL345
        /// </summary>
        /// <returns>Acceleration contains double type of X, Y, Z</returns>
        public Acceleration ReadAcceleration()
        {
            int units = _resolution / _range;

            byte[] ReadBuf = new byte[6 + 1];
            byte[] RegAddrBuf = new byte[1 + 6];

            RegAddrBuf[0] = ADDRESS_X0 | ACCEL_SPI_RW_BIT | ACCEL_SPI_MB_BIT;
            _sensor.TransferFullDuplex(RegAddrBuf, ReadBuf);
            Array.Copy(ReadBuf, 1, ReadBuf, 0, 6);

            short AccelerationX = BitConverter.ToInt16(ReadBuf, 0);
            short AccelerationY = BitConverter.ToInt16(ReadBuf, 2);
            short AccelerationZ = BitConverter.ToInt16(ReadBuf, 4);

            Acceleration accel = new Acceleration
            {
                X = (double)AccelerationX / units,
                Y = (double)AccelerationY / units,
                Z = (double)AccelerationZ / units
            };

            return accel;
        }

        /// <summary>
        /// Get ADX1345 Device
        /// </summary>
        /// <returns></returns>
        public SpiDevice GetDevice()
        {
            return _sensor;
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
