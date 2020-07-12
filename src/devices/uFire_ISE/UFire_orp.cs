// Licensed to the.NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using System.Device.I2c;

namespace Iot.Device.UFire
{
    /// <summary>
    /// Get ORP measuremens from μFire ISE Probe Interface
    /// </summary>
    public class UFire_orp : UFire_ISE
    {
        private const float POTENTIAL_REGISTER_ADDRESS = 0x64;

        /// <summary>
        /// ORP measuremens
        /// </summary>
        public float ORP = 0;

        /// <summary>
        /// Eh measuremens
        /// </summary>
        public float Eh = 0;

        /// <summary>
        /// Initializes a new instance of the <see cref="UFire_orp"/> class.
        /// </summary>
        /// <param name="i2cDevice">The I2C device to be used</param>
        public UFire_orp(I2cDevice i2cDevice)
            : base(i2cDevice)
        {
        }

        /// <summary>
        /// Starts an ORP measurement.
        /// </summary>
        /// <returns>The measured result in mV, or -1 on error</returns>
        public float MeasureORP()
        {
            float mV = MeasuremV();
            ORP = mV;
            Eh = mV + GetProbePotential();

            if (float.IsNaN(ORP) || float.IsInfinity(mV))
            {
                ORP = -1;
                Eh = -1;
            }

            return mV;
        }

        private float GetProbePotential()
        {
            return ReadEEPROM(POTENTIAL_REGISTER_ADDRESS);
        }

        private void SetProbePotential(float potential)
        {
            WriteEEPROM(POTENTIAL_REGISTER_ADDRESS, potential);
        }

    }
}
