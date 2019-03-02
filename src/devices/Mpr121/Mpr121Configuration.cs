// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

namespace Iot.Device.Mpr121
{
    /// <summary>
    /// Configuration for registers listed on datasheet page 8.
    /// </summary>
    public class Mpr121Configuration
    {
        /// <summary>
        /// Determines the largest magnitude of variation to pass through the baseline filter (rising).
        /// </summary>
        public byte MaxHalfDeltaRising { get; set; }

        /// <summary>
        /// Determines the incremental change when non-noise drift is detected (rising).
        /// </summary>
        public byte NoiseHalfDeltaRising { get; set; }

        /// <summary>
        /// Determines the number of samples consecutively greater than the Max Half Delta value (rising).
        /// This is necessary to determine that it is not noise. 
        /// </summary>
        public byte NoiseCountLimitRising { get; set; }

        /// <summary>
        /// Determines the operation rate of the filter. A larger count limit means the filter delay is operating more slowly (rising).
        /// </summary>
        public byte FilterDelayCountLimitRising { get; set; }

        /// <summary>
        /// Determines the largest magnitude of variation to pass through the baseline filter (falling).
        /// </summary>
        public byte MaxHalfDeltaFalling { get; set; }

        /// <summary>
        /// Determines the incremental change when non-noise drift is detected (falling).
        /// </summary>
        public byte NoiseHalfDeltaFalling { get; set; }

        /// <summary>
        /// Determines the number of samples consecutively greater than the Max Half Delta value (falling).
        /// This is necessary to determine that it is not noise. 
        /// </summary>
        public byte NoiseCountLimitFalling { get; set; }

        /// <summary>
        /// Determines the operation rate of the filter. A larger count limit means the filter delay is operating more slowly (falling).
        /// </summary>
        public byte FilterDelayCountLimitFalling { get; set; }

        /// <summary>
        /// Electrode touch threshold.
        /// </summary>
        /// <remark>
        /// Threshold settings are dependant on the touch/release signal strength, system sensitivity and noise immunity requirements.
        /// In a typical touch detection application, threshold is typically in the range 0x04~0x10.
        /// The touch threshold is several counts larger than the release threshold. This is to provide hysteresis and to prevent noise and jitter.
        /// </remark>
        public byte ElectrodeTouchThreshold { get; set; }

        /// <summary>
        /// Electrode release threshold.
        /// </summary>
        /// <remark>
        /// Threshold settings are dependant on the touch/release signal strength, system sensitivity and noise immunity requirements.
        /// In a typical touch detection application, threshold is typically in the range 0x04~0x10.
        /// The touch threshold is several counts larger than the release threshold. This is to provide hysteresis and to prevent noise and jitter.
        /// </remark>
        public byte ElectrodeReleaseThreshold { get; set; }

        /// <summary>
        /// Filter/Global Charge Discharge Time Configuration (datasheet page 14).
        /// </summary>
        public byte ChargeDischargeTimeConfiguration { get; set; }

        /// <summary>
        /// Electrode Configuration (datasheet page 15).
        /// </summary>
        public byte ElectrodeConfiguration  { get; set; }
    }
}