// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

namespace Iot.Device.Nmea0183.Ais
{
    /// <summary>
    /// The type of AIS transceiver used by a vessel.
    /// </summary>
    public enum AisTransceiverClass
    {
        /// <summary>
        /// The type is unknown (neither A nor B, e.g. an AtoN target)
        /// </summary>
        Unknown,

        /// <summary>
        /// Transceiver class A, large ships and professional traffic
        /// </summary>
        A,

        /// <summary>
        /// Transceiver class B, used mostly by pleasure craft and yachts.
        /// Class B transceivers do not send voyage related information, but are cheaper to buy.
        /// </summary>
        B
    }
}
