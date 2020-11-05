// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System;
using System.Buffers.Binary;
using System.Device.I2c;
using System.Numerics;

namespace Iot.Device.Lsm9Ds1
{
    /// <summary>
    /// LSM9DS1 magnetometer
    /// </summary>
    public class Lsm9Ds1Magnetometer : IDisposable
    {
        private const byte ReadMask = 0x80;
        private I2cDevice _i2c;
        private MagneticInductionScale _magneticInductionScale;

        /// <summary>
        /// Lsm9Ds1 - Magnetometer bus
        /// </summary>
        public Lsm9Ds1Magnetometer(
            I2cDevice i2cDevice,
            MagneticInductionScale magneticInductionScale = MagneticInductionScale.Scale04G)
        {
            if (i2cDevice == null)
            {
                throw new ArgumentNullException(nameof(i2cDevice));
            }

            _i2c = i2cDevice;
            _magneticInductionScale = magneticInductionScale;

            byte temperatureCompensation = 1; // enable temperature compensation
            byte operativeMode = 0b11; // Ultra high performance mode
            byte outputDataRate = 0b111; // 80Hz
            byte fastOutputDataRate = 0; // disable fast ODR (> 80Hz)
            byte selfTestEnabled = 0; // disable self-test

            byte disableI2c = 0; // enable I2C
            byte lowPowerMode = 0; // higher power mode
            byte spiAllowReads = 0; // only writes allowed
            byte operatingMode = 0b00; // continous mode

            byte operativeModeForZAxis = 0b11; // ultra-high performance mode
            byte bigEndianEnabled = 0; // little endian

            Span<byte> buff = stackalloc byte[5]
            {
                (byte)((temperatureCompensation << 7)
                        | (operativeMode << 5)
                        | (outputDataRate << 2)
                        | (fastOutputDataRate << 1)
                        | selfTestEnabled),
                (byte)((byte)magneticInductionScale << 5),
                (byte)((disableI2c << 7)
                        | (lowPowerMode << 5)
                        | (spiAllowReads << 2)
                        | operatingMode),
                (byte)((operativeModeForZAxis << 2)
                        | (bigEndianEnabled << 1)),
                0x00,
            };

            Write(RegisterM.Control1, buff);
        }

        /// <summary>
        /// Magnetic Induction measured in Gauss (G)
        /// </summary>
        public Vector3 MagneticInduction => Vector3.Divide(ReadVector3(RegisterM.OutX), GetMagneticInductionDivisor());

        private Vector3 ReadVector3(RegisterM register)
        {
            Span<byte> vec = stackalloc byte[6];
            Read(register, vec);

            short x = BinaryPrimitives.ReadInt16LittleEndian(vec.Slice(0, 2));
            short y = BinaryPrimitives.ReadInt16LittleEndian(vec.Slice(2, 2));
            short z = BinaryPrimitives.ReadInt16LittleEndian(vec.Slice(4, 2));
            return new Vector3(x, y, z);
        }

        private void Write(RegisterM register, ReadOnlySpan<byte> data)
        {
            Span<byte> buff = stackalloc byte[data.Length + 1];
            buff[0] = (byte)register;
            data.CopyTo(buff.Slice(1));

            _i2c.Write(buff);
        }

        private void Read(RegisterM register, Span<byte> buffer)
        {
            _i2c.WriteByte((byte)((byte)register | ReadMask));
            _i2c.Read(buffer);
        }

        private float GetMagneticInductionDivisor()
        {
            const float max = (1 << 15);
            switch (_magneticInductionScale)
            {
                case MagneticInductionScale.Scale04G:
                    return max / 4;
                case MagneticInductionScale.Scale08G:
                    return max / 8;
                case MagneticInductionScale.Scale12G:
                    return max / 12;
                case MagneticInductionScale.Scale16G:
                    return max / 16;
                default:
                    throw new ArgumentException(nameof(_magneticInductionScale));
            }
        }

        /// <inheritdoc/>
        public void Dispose()
        {
            _i2c?.Dispose();
            _i2c = null!;
        }
    }
}
