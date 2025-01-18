// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System;

namespace Iot.Device.Bmp180
{
    /// <summary>
    ///  Represents the trim registers of the sensor (trim values in the "trim_data" of device structure).
    /// </summary>
    public class Bmm150TrimRegisterData
    {
        /// <summary>
        /// trim DigX1 data
        /// </summary>
        public sbyte DigX1 { get; set; }

        /// <summary>
        /// trim DigY1 data
        /// </summary>
        public sbyte DigY1 { get; set; }

        /// <summary>
        /// trim DigX2 data
        /// </summary>
        public sbyte DigX2 { get; set; }

        /// <summary>
        /// trim DigY2 data
        /// </summary>
        public sbyte DigY2 { get; set; }

        /// <summary>
        /// trim DigZ1 data
        /// </summary>
        public ushort DigZ1 { get; set; }

        /// <summary>
        /// trim DigZ2 data
        /// </summary>
        public short DigZ2 { get; set; }

        /// <summary>
        /// trim DigZ3 data
        /// </summary>
        public short DigZ3 { get; set; }

        /// <summary>
        /// trim DigZ4 data
        /// </summary>
        public short DigZ4 { get; set; }

        /// <summary>
        /// trim DigXy1 data
        /// </summary>
        public byte DigXy1 { get; set; }

        /// <summary>
        /// trim DigXy2 data
        /// </summary>
        public sbyte DigXy2 { get; set; }

        /// <summary>
        /// trim DigXyz1 data
        /// </summary>
        public ushort DigXyz1 { get; set; }

        /// <summary>
        /// Creates a new instace
        /// </summary>
        public Bmm150TrimRegisterData()
        {
        }

        /// <summary>
        /// Creates a new instace based on the trim registers
        /// </summary>
        /// <param name="trimX1y1Data">trimX1y1Data bytes</param>
        /// <param name="trimXyzData">trimXyzData bytes</param>
        /// <param name="trimXy1Xy2Data">trimXy1Xy2Data bytes</param>
        public Bmm150TrimRegisterData(Span<Byte> trimX1y1Data, Span<Byte> trimXyzData, Span<Byte> trimXy1Xy2Data)
        {
            unchecked
            {
                DigX1 = (sbyte)trimX1y1Data[0];
                DigY1 = (sbyte)trimX1y1Data[1];
                DigX2 = (sbyte)trimXyzData[2];
                DigY2 = (sbyte)trimXyzData[3];
                DigZ1 = (ushort)(trimXy1Xy2Data[3] << 8 | trimXy1Xy2Data[2]);
                DigZ2 = (short)(trimXy1Xy2Data[1] << 8 | trimXy1Xy2Data[0]);
                DigZ3 = (short)(trimXy1Xy2Data[7] << 8 | trimXy1Xy2Data[6]);
                DigZ4 = (short)(trimXyzData[1] << 8 | trimXyzData[0]);
                DigXy1 = trimXy1Xy2Data[9];
                DigXy2 = (sbyte)trimXy1Xy2Data[8];
                DigXyz1 = (ushort)(((trimXy1Xy2Data[5] & 0x7F) << 8) | trimXy1Xy2Data[4]);
            }
        }
    }
}
