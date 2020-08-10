// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

namespace Iot.Device.Ccs811
{
    /// <summary>
    /// Arguments of the Measurement Threshold event
    /// Contains the measurements done and potential error
    /// </summary>
    public class MeasurementThresholdArgs
    {
        /// <summary>
        /// True if measurement is successful
        /// </summary>
        public bool MeasurementSuccess { get; set; }

        /// <summary>
        /// Equivalent CO2 in ppm
        /// </summary>
        public int EquivalentCO2InPpm { get; set; }

        /// <summary>
        /// Equivalent Total Volatile Organic Compound in ppb
        /// </summary>
        public int EquivalentTotalVolatileOrganicCompoundInPpb { get; set; }

        /// <summary>
        /// Raw current selected
        /// </summary>
        public int RawCurrentSelected { get; set; }

        /// <summary>
        /// Raw ADC reading
        /// </summary>
        public int RawAdcReading { get; set; }
    }
}
