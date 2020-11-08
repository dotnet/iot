// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System;
using System.Device.I2c;
using UnitsNet;

namespace Iot.Device.UFire
{
    /// <summary>
    /// Get pH measuremens from μFire ISE (ion-selective electrode) Probe Interface
    /// </summary>
    public class UFirePh : UFireIse
    {
        private const float ProbeMvToPh = 59.2F;
        private const float TemperatureCorrectionFactor = 0.03F;

        /// <summary>
        /// pH (Power of Hydrogen) units measurement
        /// </summary>
        public float Ph = 0;

        /// <summary>
        /// Initializes a new instance of the <see cref="UFirePh "/> class.
        /// </summary>
        /// <param name="i2cDevice">The I2C device to be used</param>
        public UFirePh(I2cDevice i2cDevice)
            : base(i2cDevice)
        {
        }

        /// <summary>
        /// Tries to measure pH (Power of Hydrogen).
        /// </summary>
        /// <param name="pH">The measure pH value</param>
        /// <param name="temp">Temperature compensation is available by passing the temperature.</param>
        /// <returns>True if it could measure pH (Power of Hydrogen) else false</returns>
        public bool TryMeasurepH(out float pH, Temperature? temp = null)
        {
            // It return -1 on error
            ElectricPotential mV = ReadElectricPotential();
            if (mV.Value == -1)
            {
                Ph = pH = float.NaN;

                return false;
            }

            pH = Convert.ToSingle(Math.Abs(7.0 - (mV.Millivolts / ProbeMvToPh)));

            if (temp != null)
            {
                double distanceFrom7 = Math.Abs(7 - Math.Round(Ph));
                double distanceFrom25 = Math.Floor(Math.Abs(25 - Math.Round(temp.Value.DegreesCelsius)) / 10);
                double temperatureMultiplier = (distanceFrom25 * distanceFrom7) * TemperatureCorrectionFactor;

                if ((Ph >= 8.0) && (temp.Value.DegreesCelsius >= 35))
                {
                    temperatureMultiplier *= -1;
                }

                if ((Ph <= 6.0) && (temp.Value.DegreesCelsius <= 15))
                {
                    temperatureMultiplier *= -1;
                }

                Ph += Convert.ToSingle(temperatureMultiplier);
            }

            if (Ph <= 0.0 || Ph >= 14.0)
            {
                pH = float.NaN;
                Ph = pH;

                return false;
            }

            if (float.IsNaN(Ph) || float.IsInfinity(Ph))
            {
                pH = float.NaN;
                Ph = pH;

                return false;
            }

            Ph = pH;

            return true;
        }

        /// <summary>
        /// Calibrates the probe using a single point using a pH value.
        /// </summary>
        /// <param name="solutionpH">pH value</param>
        public void CalibrateSingle(float solutionpH) =>
            CalibrateSingle(PhToMillivolts(solutionpH));

        /// <summary>
        /// Calibrates the dual-point values for the high reading and saves them in the devices's EEPROM.
        /// </summary>
        /// <param name="solutionpH">The pH of the calibration solution</param>
        public void CalibrateProbeHigh(float solutionpH) =>
            CalibrateProbeHigh(PhToMillivolts(solutionpH));

        /// <summary>
        /// Returns the dual-point calibration high-reference value.
        /// </summary>
        /// <returns></returns>
        public new float GetCalibrateHighReference() =>
            MVtopH(GetCalibrateHighReference());

        /// <summary>
        /// Returns the dual-point calibration high-reading value.
        /// </summary>
        /// <returns></returns>
        public new float GetCalibrateHighReading() =>
            MVtopH(GetCalibrateHighReading());

        /// <summary>
        /// Calibrates the dual-point values for the low reading and saves them in the devices's EEPROM.
        /// </summary>
        /// <param name="solutionpH"> the pH of the calibration solution</param>
        public void CalibrateProbeLow(float solutionpH) =>
            CalibrateProbeLow(PhToMillivolts(solutionpH));

        /// <summary>
        /// Returns the dual-point calibration low-reference value.
        /// </summary>
        /// <returns></returns>
        public new float GetCalibrateLowReference() =>
            MVtopH(GetCalibrateLowReference());

        /// <summary>
        /// Returns the dual-point calibration low-reading value.
        /// </summary>
        /// <returns></returns>
        public new float GetCalibrateLowReading() =>
            MVtopH(GetCalibrateLowReading());

        private float PhToMillivolts(float pH) =>
            (7 - pH) * ProbeMvToPh;

        private float MVtopH(float mV) =>
            Convert.ToSingle(Math.Abs(7.0 - (mV / ProbeMvToPh)));
    }
}
