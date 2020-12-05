// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System;

namespace Iot.Device.Pn532.AsTarget
{
    /// <summary>
    /// Parameters to emulate a Mifare card when PN532 is
    /// setup as a target
    /// See document AN133910.pdf on nxp website for default values
    /// </summary>
    public class TargetMifareParameters
    {
        private byte[] _atqa = new byte[2] { 0x00, 0x00 };
        private byte[] _nfcId3 = new byte[3] { 0x00, 0x00, 0x00 };

        /// <summary>
        /// SENS_RES (2 bytes LSB first, as defined in ISO/IEC14443-3)
        /// </summary>
        public byte[] Atqa
        {
            get => _atqa;
            set
            {
                if (value.Length != _atqa.Length)
                {
                    throw new ArgumentException(nameof(Atqa), $"Value must be {_atqa.Length} bytes.");
                }

                value.CopyTo(_atqa, 0);
            }
        }

        /// <summary>
        /// NFCID for emulation is only 3 lenght
        /// PN532 has hardware coded first NFCID byte to avoid
        /// having full copy of cards
        /// </summary>
        public byte[] NfcId3
        {
            get => _nfcId3;
            set
            {
                if (value.Length != _nfcId3.Length)
                {
                    throw new ArgumentException(nameof(NfcId3), $"Value must be {_nfcId3.Length} bytes.");
                }

                value.CopyTo(_nfcId3, 0);
            }
        }

        /// <summary>
        /// SEL_RES (1 byte), typical value:
        /// = 0x40 (for DEP)
        /// = 0x20 (for ISO/IEC14443-4 PICC emulation)
        /// = 0x60 (for both DEP and emulation of ISO/IEC14443-4 PICC)
        /// </summary>
        public byte Sak { get; set; } = 0x40;

        /// <summary>
        /// Serialize the data to be used for initialization
        /// </summary>
        /// <returns>Data serialized</returns>
        public byte[] Serialize()
        {
            byte[] ser = new byte[6];
            _atqa.CopyTo(ser, 0);
            _nfcId3.CopyTo(ser, 2);
            ser[5] = Sak;
            return ser;
        }
    }
}
