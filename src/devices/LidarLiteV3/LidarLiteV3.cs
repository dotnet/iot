// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using System;
using System.Buffers.Binary;
using System.Collections.Generic;
using System.Device;
using System.Device.I2c;
using System.IO;
using System.Numerics;
using System.Text;
using System.Threading;

namespace Iot.Device.LidarLiteV3
{
    /// <summary>
    /// Lidar Lite v3 is a long-range fixed position distance sensor by Garmin.
    /// </summary>
    public class LidarLiteV3 : IDisposable
    {
        /// <summary>
        /// Default address for LidarLiteV3
        /// </summary>
        public const byte DefaultI2cAddress = 0x62;

        internal I2cDevice _i2cDevice;

        /// <summary>
        /// Initialize the LidarLiteV3
        /// </summary>
        /// <param name="i2cDevice">The I2C device</param>
        public LidarLiteV3(I2cDevice i2cDevice)
        {
            _i2cDevice = i2cDevice;
            Reset();            
        }

        /// <summary>
        /// Reset FPGA, all registers return to default values
        /// </summary>
        public void Reset() 
        {
            try {
                WriteRegister(Register.ACQ_COMMAND, 0x00);
            } catch (System.IO.IOException)
            {
                // This exception is expected due to the reset.                
            }
        }

        /// <summary>
        /// Measure distance
        /// </summary>
        public ushort MeasureDistance(bool withReceiverBiasCorrection = true) 
        {
            if(withReceiverBiasCorrection) {
                WriteRegister(Register.ACQ_COMMAND, 0x04);
            } else {
                WriteRegister(Register.ACQ_COMMAND, 0x03);
            }

            SystemStatus status = GetStatus();                
            while(status.HasFlag(SystemStatus.BusyFlag))
            {
                status = GetStatus();
                Thread.Sleep(1);
            }

            return GetDistanceMeasurement();
        }

        /// <summary>
        /// Get the distance measurement.
        /// </summary>
        public ushort GetDistanceMeasurement() {
            Span<byte> rawData = stackalloc byte[2] { 0, 0 };
            ReadBytes(Register.FULL_DELAY, rawData);
            return BinaryPrimitives.ReadUInt16BigEndian(rawData);
        }

        /// <summary>
        /// Get the system status
        /// </summary>
        public SystemStatus GetStatus() {
            Span<byte> rawData = stackalloc byte[1] { 0 };
            ReadBytes(Register.STATUS, rawData);
            return (SystemStatus)rawData[0];
        }

        #region I2C

        internal void WriteRegister(Register reg, byte data)
        {
            Span<byte> dataout = stackalloc byte[] { (byte)reg, data };
            _i2cDevice.Write(dataout);
        }

        internal byte ReadByte(Register reg)
        {
            _i2cDevice.WriteByte((byte)reg);
            return _i2cDevice.ReadByte();
        }

        internal void ReadBytes(Register reg, Span<byte> readBytes)
        {
            _i2cDevice.WriteByte((byte)reg);
            _i2cDevice.Read(readBytes);
        }

        /// <summary>
        /// Cleanup everything
        /// </summary>
        public void Dispose()
        {
            _i2cDevice?.Dispose();
            _i2cDevice = null;
        }

        #endregion
    }
}
