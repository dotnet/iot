// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

namespace Iot.Device.Rfid
{
    /// <summary>
    /// 26 and 53 kbps card elements like ICODE
    /// </summary>
    public class Data26_53kbps
    {
        /// <summary>
        /// create 26 and 53 kbps card elements like ICODE
        /// </summary>
        /// <param name="targetNumber">The target number</param>
        /// <param name="afi">Application Family Idenfifie</param>
        /// <param name="eas">Electronic Article Surveillance</param>
        /// <param name="dsfid">Data Storage Format Identify</param>
        /// <param name="nfcId">The unique NFC ID</param>
        public Data26_53kbps(byte targetNumber, byte afi, byte eas, byte dsfid, byte[] nfcId)
        {
            TargetNumber = targetNumber;
            Afi = afi;
            Eas = eas;
            Dsfid = dsfid;
            NfcId = nfcId;
        }

        /// <summary>
        /// The target number
        /// </summary>
        public byte TargetNumber { get; set; }

        /// <summary>
        /// Application Family Idenfifie
        /// </summary>
        public byte Afi { get; set; }

        /// <summary>
        /// Electronic Article Surveillance
        /// </summary>
        public byte Eas { get; set; }

        /// <summary>
        /// Data Storage Format Identify
        /// </summary>
        public byte Dsfid { get; set; }

        /// <summary>
        /// The unique NFC ID
        /// </summary>
        public byte[] NfcId { get; set; }
    }
}
