// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using System;

namespace Iot.Device.Pn532.AsTarget
{
    /// <summary>
    /// Parameters for a PICC card (like a Credit Card) when PN532 is
    /// setup as a target
    /// See document AN133910.pdf on nxp website for default values
    /// </summary>
    public class TargetPiccParameters
    {
        // Logical Link Control Protocol magic number, version parameter and MIU for TLV
        // MIU is the maximum length of bytes in the information field
        private byte[] _generalTarget = new byte[] { 0x46, 0x66, 0x6D, 0x01, 0x01, 0x10, 0x02, 0x02, 0x00, 0x80 };
        private byte[] _nfcId3 = new byte[10] { 0x01, 0xFE, 0x0F, 0xBB, 0xBA, 0xA6, 0xC9, 0x89, 0x00, 0x00 };
        private byte[] _historicalTarget = new byte[0];

        /// <summary>
        /// The NFC Id for a PICC card, legnth must be 10 bytes
        /// </summary>
        public byte[] NfcId3
        {
            get { return _nfcId3; }
            set
            {
                if (value.Length != _nfcId3.Length)
                    throw new ArgumentException($"{nameof(NfcId3)} can only be {_nfcId3.Length} byte long");
                value.CopyTo(_nfcId3, 0);
            }
        }

        /// <summary>
        /// General target initialization bytes, length can't be more than 47
        /// Default values are provided
        /// </summary>
        public byte[] GeneralTarget
        {
            get { return _generalTarget; }
            set
            {
                if (value.Length > 47)
                    throw new ArgumentException($"{nameof(GeneralTarget)} can only be less than 47 byte long");
                _generalTarget = new byte[value.Length];
                value.CopyTo(_generalTarget, 0);
            }
        }

        /// <summary>
        /// Historical data for target initialization bytes, length can't be more than 48
        /// </summary>
        public byte[] HistoricalTarget
        {
            get { return _historicalTarget; }
            set
            {
                if (value.Length > 48)
                    throw new ArgumentException($"{nameof(HistoricalTarget)} can only be less than 48 byte long");
                _historicalTarget = new byte[value.Length];
                value.CopyTo(_historicalTarget, 0);
            }
        }

        /// <summary>
        /// Serialize the data to be used for initialization
        /// </summary>
        /// <returns>Data serialized</returns>
        public byte[] Serialize()
        {
            // +2 because we have to encode the size of General and Target
            byte[] ser = new byte[_nfcId3.Length + _generalTarget.Length + _historicalTarget.Length + 2];
            _nfcId3.CopyTo(ser, 0);
            ser[_nfcId3.Length] = (byte)_generalTarget.Length;
            _generalTarget.CopyTo(ser, _nfcId3.Length + 1);
            ser[_nfcId3.Length + _generalTarget.Length + 1] = (byte)_historicalTarget.Length;
            _historicalTarget.CopyTo(ser, _nfcId3.Length + _generalTarget.Length + 2);
            return ser;
        }
    }
}
