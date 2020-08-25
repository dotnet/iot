// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

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
        private const float PROBE_MV_TO_PH = 59.2F;
        private const float TEMP_CORRECTION_FACTOR = 0.03F;

        /// <summary>
        /// pH (Power of Hydrogen) units measurement
        /// </summary>
        public float Ph = 0;

        /// <summary>
        /// pOH units measurement, for the relationship between pH and pOH see https://www.chem.purdue.edu/gchelp/howtosolveit/Equilibrium/Calculating_pHandpOH.htm#pOH
        /// </summary>
        public float Poh = 0;

        /// <summary>
        /// Initializes a new instance of the <see cref="UFirePh "/> class.
        /// </summary>
        /// <param name="i2cDevice">The I2C device to be used</param>
        public UFirePh(I2cDevice i2cDevice)
            : base(i2cDevice)
        {
        }

        /// <summary>
        /// Tries to measure pH (Power of Hydrogen) .
        /// </summary>
        /// <param name="pH">The measure pH value</param>
        /// <param name="temp">Temperature compensation is available by passing the temperature.</param>
        /// <returns>True if it could measure pH (Power of Hydrogen) else false</returns>
        public bool TryMeasurepH(out float pH, Temperature? temp = null)
        {
            ElectricPotential mV = Measure();
            if (mV.Value == -1)
            {
                pH = -1;
                Poh = -1;

                return false;
            }

            pH = Convert.ToSingle(Math.Abs(7.0 - (mV.Millivolts / PROBE_MV_TO_PH)));

            if (temp != null)
            {
                double distance_from_7 = Math.Abs(7 - Math.Round(Ph));
                double distance_from_25 = Math.Floor(Math.Abs(25 - Math.Round(temp.Value.DegreesCelsius)) / 10);
                double temp_multiplier = (distance_from_25 * distance_from_7) * TEMP_CORRECTION_FACTOR;

                if ((Ph >= 8.0) && (temp.Value.DegreesCelsius >= 35))
                {
                    temp_multiplier *= -1;
                }

                if ((Ph <= 6.0) && (temp.Value.DegreesCelsius <= 15))
                {
                    temp_multiplier *= -1;
                }

                Ph += Convert.ToSingle(temp_multiplier);
            }

            Poh = Math.Abs(Ph - 14);

            if (Ph <= 0.0 || Ph >= 14.0)
            {
                pH = -1;
                pH = -1;

                return false;
            }

            if (float.IsNaN(Ph) || float.IsInfinity(Ph))
            {
                pH = -1;
                pH = -1;

                return false;
            }

            return true;
        }

        /// <summary>
        /// Calibrates the probe using a single point using a pH value.
        /// </summary>
        /// <param name="solutionpH">pH value</param>
        public void CalibrateSingle(float solutionpH)
        {
            CalibrateSingle(PHtomV(solutionpH));
        }

        /// <summary>
        /// Calibrates the dual-point values for the high reading and saves them in the devices's EEPROM.
        /// </summary>
        /// <param name="solutionpH">The pH of the calibration solution</param>
        public void CalibrateProbeHigh(float solutionpH)
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
        public void CalibrateProbeLow(float solutionpH)
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
