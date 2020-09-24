// Licensed to the.NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using System.Device.I2c;
using UnitsNet;

namespace Iot.Device.UFire
{
    /// <summary>
    /// Get ORP (oxidation-reduction potential) measuremens from μFire ISE (ion-selective electrode) Probe Interface
    /// </summary>
    public class UFireOrp : UFireIse
    {
        /// <summary>
        /// Oxidation-reduction potential (ORP) measuremens
        /// It makes it possible to the Oxidation-Reduction Potential (ORP) measurement without doing a measuremen (the retunres the old measurement)
        /// </summary>
        public ElectricPotential OxidationReductionPotential = new ElectricPotential();

        /// <summary>
        /// Reduction potential (Eh) measuremens (see https://www.eosremediation.com/converting-field-orp-measurements-into-eh/)
        /// It makes it possible to the Reduction potential (Eh) measurement without doing a measuremen (the retunres the old measurement)
        /// </summary>
        public ElectricPotential ReductionPotential = new ElectricPotential();

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
        public ElectricPotential ProbePotential => ElectricPotential.FromMillivolts(ReadEeprom(Register.POTENTIAL_REGISTER));

        /// <summary>
        /// Tries to measure ORP (Oxidation-Reduction Potential).
        /// </summary>
        /// <param name="orp">ORP (Oxidation-Reduction Potential) measurement</param>
        /// <returns>True if it could measure ORP (Oxidation-Reduction Potential) else false</returns>
        public bool TryMeasureOxidationReductionPotential(out ElectricPotential orp)
        {
            ElectricPotential mV = ReadElectricPotential();
            OxidationReductionPotential = mV;
            ReductionPotential = new ElectricPotential(mV.Millivolts + GetProbePotential(), UnitsNet.Units.ElectricPotentialUnit.Millivolt);

            if (double.IsNaN(mV.Value) || double.IsInfinity(mV.Value))
            {
                OxidationReductionPotential = new ElectricPotential();
                ReductionPotential = new ElectricPotential();
            }

            orp = mV;
            return double.IsNaN(mV.Value) || double.IsInfinity(mV.Value);
        }

        private float GetProbePotential()
        {
            return ReadEeprom(Register.POTENTIAL_REGISTER);
        }
    }
}
