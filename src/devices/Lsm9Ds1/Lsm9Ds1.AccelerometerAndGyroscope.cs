// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using System;
using System.IO;
using System.Device.Gpio;
using System.Device.I2c;
using System.Device.I2c.Devices;
using System.Diagnostics;
using System.Runtime.CompilerServices;
using System.Buffers.Binary;
using System.Numerics;

namespace Iot.Device.Lsm9Ds1
{
    public class Lsm9Ds1AccelerometerAndGyroscope : IDisposable
    {
        private const byte ReadMask = 0x80;
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
            if (i2cDevice == null)
                throw new ArgumentNullException(nameof(i2cDevice));

            _i2c = i2cDevice;
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

        private float GetAccelerationDivisor()
        {
            // intentionally return float

            // we have 16-bit signed number
            // we can use int since our divisors are powers of 2
            const int max = (1 << 15);
            switch (_accelerometerScale)
            {
                case AccelerationScale.Scale02G:
                    return max / 2;
                case AccelerationScale.Scale04G:
                    return max / 4;
                case AccelerationScale.Scale08G:
                    return max / 8;
                case AccelerationScale.Scale16G:
                    return max / 16;
                default:
                    throw new ArgumentException(nameof(_accelerometerScale));
            }
        }

        private float GetAngularRateDivisor()
        {
            // we have 16-bit signed number
            const float max = (float)(1 << 15);
            switch (_angularRateScale)
            {
                case AngularRateScale.Scale0245Dps:
                    return max / 245;
                case AngularRateScale.Scale0500Dps:
                    return max / 500;
                case AngularRateScale.Scale2000Dps:
                    return max / 2000;
                default:
                    throw new ArgumentException(nameof(_angularRateScale));
            }
        }

        public void Dispose()
        {
            _i2c?.Dispose();
            _i2c = null;
        }
    }
}
