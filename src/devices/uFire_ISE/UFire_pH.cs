// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using System;
using System.Device.I2c;

namespace Iot.Device.UFire
{
    /// <summary>
    /// Get pH measuremens from μFire ISE Probe Interface
    /// </summary>
    public class UFire_pH : UFire_ISE
    {
        private const float PROBE_MV_TO_PH = 59.2F;
        private const float TEMP_CORRECTION_FACTOR = 0.03F;

        /// <summary>
        /// pH measuremen
        /// </summary>
        public float PH = 0;

        /// <summary>
        /// poH measuremen
        /// </summary>
        public float POH = 0;

        /// <summary>
        /// Initializes a new instance of the <see cref="UFire_pH"/> class.
        /// </summary>
        /// <param name="i2cDevice">The I2C device to be used</param>
        public UFire_pH(I2cDevice i2cDevice)
            : base(i2cDevice)
        {
        }

        /// <summary>
        /// Starts a pH measurement.
        /// </summary>
        /// <param name="temp">Temperature compensation is available by passing the temperature.</param>
        /// <returns>The measured result in pH, or -1 on error. If the probe is unconnected, the value will float</returns>
        public float MeasurepH(float? temp = null)
        {
            float mV = MeasuremV();
            if (mV == -1)
            {
                PH = -1;
                POH = -1;

                return -1;
            }

            PH = Convert.ToSingle(Math.Abs(7.0 - (mV / PROBE_MV_TO_PH)));

            if (temp != null)
            {
                double distance_from_7 = Math.Abs(7 - Math.Round(PH));
                double distance_from_25 = Math.Floor(Math.Abs(25 - Math.Round(temp.Value)) / 10);
                double temp_multiplier = (distance_from_25 * distance_from_7) * TEMP_CORRECTION_FACTOR;

                if ((PH >= 8.0) && (temp >= 35))
                {
                    temp_multiplier *= -1;
                }

                if ((PH <= 6.0) && (temp <= 15))
                {
                    temp_multiplier *= -1;
                }

                PH += Convert.ToSingle(temp_multiplier);
            }

            POH = Math.Abs(PH - 14);

            if (PH <= 0.0 || PH >= 14.0)
            {
                PH = -1;
                POH = -1;
            }

            if (float.IsNaN(PH) || float.IsInfinity(PH))
            {
                PH = -1;
                POH = -1;
            }

            return PH;
        }

        /// <summary>
        /// Calibrates the probe using a single point using a pH value.
        /// </summary>
        /// <param name="solutionpH">pH value</param>
        public new void CalibrateSingle(float solutionpH)
        {
            CalibrateSingle(PHtomV(solutionpH));
        }

        /// <summary>
        /// Calibrates the dual-point values for the high reading and saves them in the devices's EEPROM.
        /// </summary>
        /// <param name="solutionpH">The pH of the calibration solution</param>
        public new void CalibrateProbeHigh(float solutionpH)
        {
            CalibrateProbeHigh(PHtomV(solutionpH));
        }

        /// <summary>
        /// Returns the dual-point calibration high-reference value.
        /// </summary>
        /// <returns></returns>
        public new float GetCalibrateHighReference()
        {
            return MVtopH(GetCalibrateHighReference());
        }

        /// <summary>
        /// Returns the dual-point calibration high-reading value.
        /// </summary>
        /// <returns></returns>
        public new float GetCalibrateHighReading()
        {
            return MVtopH(GetCalibrateHighReading());
        }

        /// <summary>
        /// Calibrates the dual-point values for the low reading and saves them in the devices's EEPROM.
        /// </summary>
        /// <param name="solutionpH"> the pH of the calibration solution</param>
        public new void CalibrateProbeLow(float solutionpH)
        {
            CalibrateProbeLow(PHtomV(solutionpH));
        }

        /// <summary>
        /// Returns the dual-point calibration low-reference value.
        /// </summary>
        /// <returns></returns>
        public new float GetCalibrateLowReference()
        {
            return MVtopH(GetCalibrateLowReference());
        }

        /// <summary>
        /// Returns the dual-point calibration low-reading value.
        /// </summary>
        /// <returns></returns>
        public new float GetCalibrateLowReading()
        {
            return MVtopH(GetCalibrateLowReading());
        }

        private float PHtomV(float pH)
        {
            return (7 - pH) * PROBE_MV_TO_PH;
        }

        private float MVtopH(float mV)
        {
            return Convert.ToSingle(Math.Abs(7.0 - (mV / PROBE_MV_TO_PH)));
        }

    }
}
