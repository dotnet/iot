using System;
using System.Collections.Generic;
using System.Text;

using UnitsNet;

namespace Iot.Device.MettlerToledo.Readings
{
    /// <summary>
    /// Weight reading returned by Mettler Toledo scales
    /// </summary>
    public class MettlerToledoWeightReading
    {
        /// <summary>
        /// Scale returned this weight value is stable
        /// </summary>
        public bool Stable { get; }

        /// <summary>
        /// Weight returned by the scale
        /// </summary>
        public Mass Weight { get; }

        /// <summary>
        /// Initializes a new instance of the <see cref="MettlerToledoWeightReading"/> class.
        /// </summary>
        /// <param name="stable">Whether this value is stable</param>
        /// <param name="weight">Weight returned by scale</param>
        public MettlerToledoWeightReading(bool stable, Mass weight) => (Stable, Weight) = (stable, weight);
    }
}
