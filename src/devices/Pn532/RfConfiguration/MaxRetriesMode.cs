// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

namespace Iot.Device.Pn532.RfConfiguration
{
    /// <summary>
    /// The parameters MxRtyATR, MxRtyPSL and MxRtyPassiveActivation define the
    /// number of retries that the PN532 will use in case of the following processes
    /// </summary>
    public class MaxRetriesMode
    {
        /// <summary>
        /// MxRtyATR is a byte containing the number of times that the PN532 will retry to
        /// send the ATR_REQ in case of incorrect reception of the ATR_RES(or no
        /// reception at all - timeout).
        /// For active mode, value 0xFF means to try eternally, 0x00 means only once(no
        /// retry, only one try). The default value of this parameter is 0xFF (infinitely).
        /// For passive mode, the value is always overruled with 0x02 (two retries).
        /// </summary>
        public byte MaxRetryAnswerToReset { get; set; } = 0x02;

        /// <summary>
        /// MxRtyPSL is a byte containing the number of times that:
        /// • The PN532 will retry to send the PSL_REQ in case of incorrect reception of
        /// the PSL_RES(or no reception at all) for the NFC IP1 protocol,
        /// • The PN532 will retry to send the PPS request in case of incorrect reception
        /// of the PPS response(or no reception at all) for the ISO/IEC14443-4
        /// protocol.
        /// Value 0xFF means to try eternally, 0x00 means only once(no retry, only one
        /// try).The default value of this parameter is 0x01 (the PSL_REQ/PPS request is
        /// sent twice in case of need).
        /// </summary>
        public byte MaxRetryPSL { get; set; } = 0x01;

        /// <summary>
        /// MxRtyPassiveActivation is a byte containing the number of times that the
        /// PN532 will retry to activate a target in InListPassiveTarget command
        /// (§7.3.5, p: 115).
        /// Value 0xFF means to try eternally, 0x00 means only once(no retry, only one
        /// try).
        /// The default value of this parameter is 0xFF (infinitely).
        /// </summary>
        public byte MaxRetryPassiveActivation { get; set; }

        /// <summary>
        /// Get the byte array to send
        /// </summary>
        /// <returns>Serialized value</returns>
        public byte[] Serialize()
        {
            return new byte[3] { MaxRetryAnswerToReset, MaxRetryPSL, MaxRetryPassiveActivation };
        }
    }
}
