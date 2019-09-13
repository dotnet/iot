// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

namespace Iot.Device.Pn532
{
    /// <summary>
    /// Radio Frequency configuration mode
    /// </summary>
    public enum  RfConfigurationMode
    {
        /// <summary>
        /// Rf Field
        /// </summary>
        RfField = 0x01,
        /// <summary>
        /// Various Timings
        /// </summary>
        VariousTimings = 0x02,
        /// <summary>
        /// Max Retry COM
        /// </summary>
        MaxRetryCOM = 0x04,
        /// <summary>
        /// Max Retries
        /// </summary>
        MaxRetries = 0x05,
        /// <summary>
        /// Analog Settings B106 kbps Type A
        /// </summary>
        AnalogSettingsB106kbpsTypeA = 0x0A,
        /// <summary>
        /// Analog Settings B212_424 kbps
        /// </summary>
        AnalogSettingsB212_424kbps = 0x0B,
        /// <summary>
        /// Analog Settings Type B
        /// </summary>
        AnalogSettingsTypeB = 0x0C,
        /// <summary>
        /// Analog Settings B212_424_848 ISO_IEC14443_4
        /// </summary>
        AnalogSettingsB212_424_848ISO_IEC14443_4 = 0x0D,
    }
}
