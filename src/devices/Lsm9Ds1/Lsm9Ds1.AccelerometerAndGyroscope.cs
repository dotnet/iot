// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System;
using System.Buffers.Binary;
using System.Device.I2c;
using System.Numerics;

namespace Iot.Device.Lsm9Ds1
{
    /// <summary>
    /// LSM9DS1 accelerometer and gyroscope
    /// </summary>
    public class Lsm9Ds1AccelerometerAndGyroscope : IDisposable
    {
        private const byte ReadMask = 0x80;
        private const int Max = (1 << 15);
        private I2cDevice _i2c;
        private AccelerationScale _accelerometerScale;
        private AngularRateScale _angularRateScale;

        /// <summary>
        /// Lsm9Ds1 - Accelerometer and Gyroscope bus
        /// </summary>
        public Lsm9Ds1AccelerometerAndGyroscope(
            I2cDevice i2cDevice,
            AccelerationScale accelerationScale = AccelerationScale.Scale02G,
            AngularRateScale angularRateScale = AngularRateScale.Scale0245Dps)
        {
            _i2c = i2cDevice ?? throw new ArgumentNullException($"{nameof(i2cDevice)} cannot be null");
            _accelerometerScale = accelerationScale;
            _angularRateScale = angularRateScale;

            byte accelerometerOutputDataRate = 0b011; // 119Hz, we cannot measure time accurate enough to use higher frequency
            WriteByte(RegisterAG.AccelerometerControl6, (byte)((accelerometerOutputDataRate << 5) | ((byte)accelerationScale << 3)));

            // enable all 3 axis of gyroscope
            WriteByte(RegisterAG.Control4, 0b0011_1000);

            byte angularRateOutputDataRate = 0b011; // 119Hz
            WriteByte(RegisterAG.AngularRateControl1, (byte)((angularRateOutputDataRate << 5) | ((byte)angularRateScale << 3)));
        }

        /// <summary>
        /// Acceleration measured in degrees per second (DPS)
        /// </summary>
        public Vector3 AngularRate => Vector3.Divide(ReadVector3(RegisterAG.AngularRateX), GetAngularRateDivisor());

        /// <summary>
        /// Acceleration measured in gravitational force
        /// </summary>
        public Vector3 Acceleration => Vector3.Divide(ReadVector3(RegisterAG.AccelerometerX), GetAccelerationDivisor());

        private Vector3 ReadVector3(RegisterAG register)
        {
            Span<byte> vec = stackalloc byte[6];
            Read(register, vec);

            short x = BinaryPrimitives.ReadInt16LittleEndian(vec.Slice(0, 2));
            short y = BinaryPrimitives.ReadInt16LittleEndian(vec.Slice(2, 2));
            short z = BinaryPrimitives.ReadInt16LittleEndian(vec.Slice(4, 2));
            return new Vector3(x, y, z);
        }

        private void WriteByte(RegisterAG register, byte data)
        {
            Span<byte> buff = stackalloc byte[2]
            {
                (byte)register,
                data
            };

            _i2c.Write(buff);
        }

        private short ReadInt16(RegisterAG register)
        {
            Span<byte> val = stackalloc byte[2];
            Read(register, val);
            return BinaryPrimitives.ReadInt16LittleEndian(val);
        }

        private void Read(RegisterAG register, Span<byte> buffer)
        {
            _i2c.WriteByte((byte)((byte)register | ReadMask));
            _i2c.Read(buffer);
        }

        // intentionally return float
        // we have 16-bit signed number
        // we can use int since our divisors are powers of 2
        private float GetAccelerationDivisor() => _accelerometerScale switch
        {
            AccelerationScale.Scale02G => Max / 2,
            AccelerationScale.Scale04G => Max / 4,
            AccelerationScale.Scale08G => Max / 8,
            AccelerationScale.Scale16G => Max / 16,
            _ => throw new ArgumentException($"{nameof(_accelerometerScale)} is unknown value"),
        };

        // we have 16-bit signed number
        private float GetAngularRateDivisor() => _angularRateScale switch
        {
            AngularRateScale.Scale0245Dps => Max / 245,
            AngularRateScale.Scale0500Dps => Max / 500,
            AngularRateScale.Scale2000Dps => Max / 2000,
            _ => throw new ArgumentException($"{nameof(_angularRateScale)} is unknown value"),
        };

        /// <inheritdoc/>
        public void Dispose()
        {
            _i2c?.Dispose();
            _i2c = null!;
        }
    }
}
