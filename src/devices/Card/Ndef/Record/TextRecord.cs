// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using System;
using System.Text;

namespace Iot.Device.Ndef
{
    /// <summary>
    /// Create a Text Record class
    /// </summary>
    public class TextRecord : NdefRecord
    {
        private Encoding _encoding = Encoding.UTF8;
        private string _languageCode = string.Empty;
        private string _text = string.Empty;

        /// <summary>
        /// The Encoding type used for the text, only UTF8 and Unicode are valid
        /// </summary>
        public Encoding Encoding
        {
            get
            {
                return _encoding;
            }

            set
            {
                if ((value != Encoding.UTF8) || (value != Encoding.Unicode))
                {
                    throw new ArgumentException($"Encoding can only be {Encoding.UTF8} or {Encoding.Unicode}");
                }

                _encoding = value;
                SetPayload();
            }
        }

        /// <summary>
        /// A valid language code, should be less than 63 characters
        /// </summary>
        public string LanguageCode
        {
            get
            {
                return _languageCode;
            }

            set
            {
                if (value.Length > 63)
                {
                    throw new ArgumentException($"Language code maximum length is 63 characters and should be ASCII encoded");
                }

                _languageCode = value;
                SetPayload();
            }
        }

        /// <summary>
        /// The text payload
        /// </summary>
        public string Text
        {
            get
            {
                return _text;
            }

            set
            {
                _text = value;
                SetPayload();
            }
        }

        /// <summary>
        /// Create a Text Record based on its characteristics
        /// </summary>
        /// <param name="text">The text payload</param>
        /// <param name="language">The language of the text</param>
        /// <param name="encoding">The Encoding type. <see cref="Encoding"/></param>
        public TextRecord(string text, string language, Encoding encoding)
            : base()
        {
            _text = text;
            _languageCode = language;
            _encoding = encoding;
            // setup type of Text
            Header.PayloadType = new byte[1] { 0x054 };
            Header.TypeNameFormat = TypeNameFormat.NfcWellKnowType;
            SetPayload();
        }

        /// <summary>
        /// Create a Text Record from a span of bytes
        /// </summary>
        /// <param name="record">The record as a span of bytes</param>
        public TextRecord(ReadOnlySpan<byte> record)
            : base(record)
        {
            ExtractAll();
        }

        /// <summary>
        /// Create a Text Record from a NDEF Record
        /// </summary>
        /// <param name="ndefRecord">A valid NDEF Record</param>
        public TextRecord(NdefRecord ndefRecord)
        {
            Header = ndefRecord.Header;
            Payload = ndefRecord.Payload;
            ExtractAll();
        }

        private void ExtractAll()
        {
            if (!IsTextRecord(this) || Payload is not object)
            {
                throw new ArgumentException($"Record type must be {TypeNameFormat.NfcWellKnowType} and payload type 'T' (0x54)");
            }

            _encoding = (Payload[0] & 0b1000_0000) == 0 ? Encoding.UTF8 : Encoding.Unicode;

            var langSize = Payload[0] & 0b0011_1111;

            _languageCode = Encoding.ASCII.GetString(Payload, 1, langSize);

            _text = _encoding.GetString(Payload, 1 + langSize, Payload.Length - 1 - langSize);
        }

        private void SetPayload()
        {
            var textBytes = _encoding.GetBytes(_text);
            Payload = new byte[1 + _languageCode.Length + textBytes.Length];
            Payload[0] = (byte)((_encoding == Encoding.UTF8 ? 0 : 0b1000_0000) + _languageCode.Length);
            if (_languageCode.Length > 0)
            {
                Encoding.ASCII.GetBytes(_languageCode).CopyTo(Payload, 1);

            }

            if (_text.Length > 0)
            {
                textBytes.CopyTo(Payload, 1 + _languageCode.Length);
            }

            Header.PayloadLength = (uint)Payload.Length;
        }

        /// <summary>
        /// Check if it's a valid Text Record
        /// </summary>
        /// <param name="ndefRecord">The NDEF Record to check</param>
        /// <returns>True if it's a valid Text Record</returns>
        public static bool IsTextRecord(NdefRecord ndefRecord)
        {
            if ((ndefRecord.Header.TypeNameFormat != TypeNameFormat.NfcWellKnowType) || (ndefRecord.Header.PayloadTypeLength != 1))
            {
                return false;
            }

            if (ndefRecord.Header.PayloadType?[0] != 0x54)
            {
                return false;
            }

            return true;
        }
    }
}
