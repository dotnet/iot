// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

namespace Iot.Device.Pn532
{
    /// <summary>
    /// Radio Frequence configuration mode
    /// </summary>
    public enum  RfConfigurationMode
    {
        RfField = 0x01,
        VariousTimings = 0x02,
        MaxRtyCOM = 0x04,
        MaxRetries = 0x05,
        AnalogSettingsB106kbpsTypeA = 0x0A,
        AnalogSettingsB212_424kbps = 0x0B,
        AnalogSettingsTypeB = 0x0C,
        AnalogSettingsB212_424_848ISO_IEC14443_4 = 0x0D,
    }
}
