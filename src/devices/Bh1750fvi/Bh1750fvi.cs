// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using System;
using System.Buffers.Binary;
using System.Device.I2c;
using System.Threading;

namespace Iot.Device.Bh1750fvi
{
    /// <summary>
    /// Ambient Light Sensor BH1750FVI
    /// </summary>
    public class Bh1750fvi : IDisposable
    {
        private const byte DefaultLightTransmittance = 0b_0100_0101;

        private I2cDevice _sensor;

        private double _lightTransmittance;

        /// <summary>
        /// BH1750FVI Light Transmittance, from 27.20% to 222.50%
        /// </summary>
        public double LightTransmittance
        {
            get => _lightTransmittance;
            set
            {
                SetLightTransmittance(value);
                _lightTransmittance = value;
            }
        }

        /// <summary>
        /// BH1750FVI Measuring Mode
        /// </summary>
        public MeasuringMode MeasuringMode { get; set; }

        /// <summary>
        /// BH1750FVI Illuminance (Lux)
        /// </summary>
        public double Illuminance => Math.Round(GetIlluminance(), 1);

        /// <summary>
        /// Creates a new instance of the BH1750FVI
        /// </summary>
        /// <param name="sensor">I2C Device, like UnixI2cDevice or Windows10I2cDevice</param>
        /// <param name="measuringMode">The measuring mode of BH1750FVI</param>
        /// <param name="lightTransmittance">BH1750FVI Light Transmittance, from 27.20% to 222.50%</param>
        public Bh1750fvi(I2cDevice sensor, MeasuringMode measuringMode = MeasuringMode.ContinuouslyHighResolutionMode, double lightTransmittance = 1)
        {
            _sensor = sensor;

            _sensor.WriteByte((byte)Command.PowerOn);
            _sensor.WriteByte((byte)Command.Reset);

            LightTransmittance = lightTransmittance;
            MeasuringMode = measuringMode;
        }

        /// <summary>
        /// Cleanup
        /// </summary>
        public void Dispose()
        {
            _sensor?.Dispose();
            _sensor = null;
        }

        /// <summary>
        /// Set BH1750FVI Light Transmittance
        /// </summary>
        /// <param name="transmittance">Light Transmittance, from 27.20% to 222.50%</param>
        private void SetLightTransmittance(double transmittance)
        {
            if (transmittance > 2.225 || transmittance < 0.272)
            {
                throw new ArgumentOutOfRangeException(nameof(transmittance), $"{nameof(transmittance)} needs to be in the range of 27.20% to 222.50%.");
            }

            byte val = (byte)(DefaultLightTransmittance / transmittance);

            _sensor.WriteByte((byte)((byte)Command.MeasurementTimeHigh | (val >> 5)));
            _sensor.WriteByte((byte)((byte)Command.MeasurementTimeLow | (val & 0b_0001_1111)));
        }

        /// <summary>
        /// Get BH1750FVI Illuminance
        /// </summary>
        /// <returns>Illuminance (Lux)</returns>
        private double GetIlluminance()
        {
            if (MeasuringMode == MeasuringMode.OneTimeHighResolutionMode || MeasuringMode == MeasuringMode.OneTimeHighResolutionMode2 || MeasuringMode == MeasuringMode.OneTimeLowResolutionMode)
                _sensor.WriteByte((byte)Command.PowerOn);

            Span<byte> readBuff = stackalloc byte[2];

            _sensor.WriteByte((byte)MeasuringMode);
            _sensor.Read(readBuff);

            ushort raw = BinaryPrimitives.ReadUInt16BigEndian(readBuff);

            double result = raw / (1.2 * _lightTransmittance);

            if (MeasuringMode == MeasuringMode.ContinuouslyHighResolutionMode2 || MeasuringMode == MeasuringMode.OneTimeHighResolutionMode2)
                result *= 2;

            return result;
        }
    }
}