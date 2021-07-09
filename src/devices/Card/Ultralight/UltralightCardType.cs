// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

namespace Iot.Device.Card.Ultralight
{
    /// <summary>
    /// Capacity of the Ultralight card
    /// </summary>
    public enum UltralightCardType
    {
        /// <summary>
        /// Unknown
        /// </summary>
        Unknown,

        /// <summary>
        /// Ultralight NTAG210
        /// </summary>
        UltralightNtag210,

        /// <summary>
        /// Ultralight NTAG212
        /// </summary>
        UltralightNtag212,

        /// <summary>
        /// Ultralight NTAG213 144 bytes NDEF
        /// </summary>
        UltralightNtag213,

        /// <summary>
        /// Ultralight NTAG213F
        /// </summary>
        UltralightNtag213F,

        /// <summary>
        /// Ultralight NTAG215 1504 bytes NDEF
        /// </summary>
        UltralightNtag215,

        /// <summary>
        /// Ultralight NTAG216 888 bytes NDEF
        /// </summary>
        UltralightNtag216,

        /// <summary>
        /// Ultralight NTAG216
        /// </summary>
        UltralightNtag216F,

        /// <summary>
        /// Ultralight EV1 MF0UL1101
        /// </summary>
        UltralightEV1MF0UL1101,

        /// <summary>
        /// Ultralight EV1 MF0ULH1101
        /// </summary>
        UltralightEV1MF0ULH1101,

        /// <summary>
        /// Ultralight EV1 MF0UL2101
        /// </summary>
        UltralightEV1MF0UL2101,

        /// <summary>
        /// Ultralight EV1 MF0ULH2101
        /// </summary>
        UltralightEV1MF0ULH2101,

        /// <summary>
        /// Ultralight NTAG I2C NT3H1101
        /// </summary>
        UltralightNtagI2cNT3H1101,

        /// <summary>
        /// Ultralight NTAG I2C NT3H1101W0
        /// </summary>
        UltralightNtagI2cNT3H1101W0,

        /// <summary>
        /// Ultralight NTAG I2C NT3H2111W0
        /// </summary>
        UltralightNtagI2cNT3H2111W0,

        /// <summary>
        /// Ultralight NTAG I2C NT3H2101
        /// </summary>
        UltralightNtagI2cNT3H2101,

        /// <summary>
        /// Ultralight NTAG I2C NT3H1201W0
        /// </summary>
        UltralightNtagI2cNT3H1201W0,

        /// <summary>
        /// Ultralight NTAG I2C NT3H2211W0
        /// </summary>
        UltralightNtagI2cNT3H2211W0,

        /// <summary>
        /// Ultralight C contactless ticket
        /// </summary>
        UltralightC,

        /// <summary>
        /// Ultralight NAG203 144 bytes NDEF
        /// </summary>
        UltralightNtag203,

        /// <summary>
        /// The older Mifare Ultralight model
        /// </summary>
        MifareUltralight,
    }
}
