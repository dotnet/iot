// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using System;
using System.Collections.Generic;
using System.Text;

namespace Iot.Device.Pn532.RfConfiguration
{
    /// <summary>
    /// The radio frequency timing modes
    /// </summary>
    public class VariousTimingsMode
    {
        /// <summary>
        /// Reserved for Further Usage
        /// </summary>
        public const byte RFU = 0x00;

        /// <summary>
        /// The second byte in this item defines the timeout between ATR_REQ and
        /// ATR_RES when the PN532 is in initiator mode.A target is considered as mute if no
        /// valid ATR_RES is received within this timeout value. In this way, the PN532 can
        /// easily detect non TPE target in passive 212-424 kbps mode.
        /// The default value for this parameter is 0x0B (102.4 ms). 
        /// </summary>
        public RfTimeout AnsweToRequestResponseTimeout { get; set; } = RfTimeout.T102400MicroSeconds;

        /// <summary>
        /// The third byte defines the timeout value that the PN532 uses in the
        /// InCommunicateThru(§7.3.9, p: 136) command to give up reception from the
        /// target in case of no answer.
        /// The default value for this parameter is 0x0A (51.2 ms).
        /// This timeout definition is also used with InDataExchange(§7.3.8, p: 127) when
        /// the target is a FeliCa or a Mifare card(Ultralight, Standard…). 
        /// </summary>
        public RfTimeout RetryTimeout { get; set; } = RfTimeout.T51200MicroSeconds;

        /// <summary>
        /// Get the byte array to send
        /// </summary>
        /// <returns>Serialized value</returns>
        public byte[] Serialize()
        {
            return new byte[3] { RFU, (byte)AnsweToRequestResponseTimeout, (byte)RetryTimeout };
        }
    }
}
