// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using System;

namespace Iot.Device.Pn532.AsTarget
{
    /// <summary>
    /// Parameters for the FeliCa card when PN532 is
    /// setup as a target
    /// See document AN133910.pdf on nxp website for default values
    /// </summary>
    public class TargetFeliCaParameters
    {
        // First 2 bytes must be 0x01 0xFE
        private byte[] _nfcId2 = new byte[8] { 0x01, 0xFE, 0xA1, 0xB2, 0xC3, 0xD4, 0xE5, 0xF6 };
        private byte[] _pad = new byte[8];
        // those are typical values
        private byte[] _systemCode = new byte[2] { 0xFF, 0xFF };

        /// <summary>
        /// The NFC Id for a FeliCa card, legnth must be 8 bytes
        /// </summary>
        public byte[] NfcId2
        {
            get
            {
                return _nfcId2;
            }
            set
            {
                if (value.Length != _nfcId2.Length)
                {
                    throw new ArgumentException($"{nameof(NfcId2)} can only be {_nfcId2.Length} byte long");
                }

                value.CopyTo(_nfcId2, 0);
            }
        }

        /// <summary>
        /// The PAD for a FeliCa card, length must be 8 bytes
        /// </summary>
        public byte[] Pad
        {
            get
            {
                return _pad;
            }
            set
            {
                if (value.Length != _pad.Length)
                {
                    throw new ArgumentException($"{nameof(Pad)} can only be {_pad.Length} byte long");
                }

                value.CopyTo(_pad, 0);
            }
        }

        /// <summary>
        /// System Code (2 bytes), these two bytes are returned in the POL_RES frame if
        /// the 4th byte of the incoming POL_REQ command frame is 0x01
        /// </summary>
        public byte[] SystemCode
        {
            get
            {
                return _systemCode;
            }
            set
            {
                if (value.Length != _systemCode.Length)
                {
                    throw new ArgumentException($"{nameof(SystemCode)} can only be {_systemCode.Length} byte long");
                }

                value.CopyTo(_systemCode, 0);
            }
        }

        /// <summary>
        /// Serialize the data to be used for initialization
        /// </summary>
        /// <returns>Data serialized</returns>
        public byte[] Serialize()
        {
            byte[] ser = new byte[18];
            _nfcId2.CopyTo(ser, 0);
            _pad.CopyTo(ser, 8);
            _systemCode.CopyTo(ser, 16);
            return ser;
        }
    }
}
