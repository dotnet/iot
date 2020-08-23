// Licensed to the.NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using System;
using System.Device.I2c;
using UnitsNet;

namespace Iot.Device.UFire
{
    /// <summary>
    /// Get ORP measuremens from μFire ISE Probe Interface
    /// </summary>
    public class UFireOrp : UFireIse
    {
        private const float POTENTIAL_REGISTER_ADDRESS = 0x64;

        /// <summary>
        /// ORP measuremens
        /// </summary>
        public ElectricPotential ORP = new ElectricPotential();

        /// <summary>
        /// Eh measuremens
        /// </summary>
        public ElectricPotential Eh = new ElectricPotential();

        /// <summary>
        /// Initializes a new instance of the <see cref="UFireOrp"/> class.
        /// </summary>
        /// <param name="i2cDevice">The I2C device to be used</param>
        public UFireOrp(I2cDevice i2cDevice)
            : base(i2cDevice)
        {
        }

        /// <summary>
        /// The probe potential
        /// </summary>
        public ElectricPotential ProbePotential
        {
            get
            {
                return ElectricPotential.FromMillivolts(ReadEEPROM(POTENTIAL_REGISTER_ADDRESS));
            }

        }

        /// <summary>
        /// Tries to measurer ORP (Oxidation-Reduction Potential).
        /// </summary>
        /// <param name="orp">ORP (Oxidation-Reduction Potential) measurement</param>
        /// <returns>True if it could measure ORP (Oxidation-Reduction Potential) else false</returns>
        public bool TryMeasureORP(out ElectricPotential orp)
        {
            ElectricPotential mV = MeasuremV();
            ORP = mV;
            Eh = new ElectricPotential(mV.Millivolts + GetProbePotential(), UnitsNet.Units.ElectricPotentialUnit.Millivolt);

            if (double.IsNaN(ORP.Value) || double.IsInfinity(mV.Value))
            {
                ORP = new ElectricPotential();
                Eh = new ElectricPotential();
            }

            orp = mV;
            return double.IsNaN(mV.Value) || double.IsInfinity(mV.Value);
        }

        private float GetProbePotential()
        {
            return ReadEEPROM(POTENTIAL_REGISTER_ADDRESS);
        }
    }
}
