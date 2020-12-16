// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System;
using System.Text;

namespace Iot.Device.Ndef
{
    /// <summary>
    /// Create a Uri Record class
    /// </summary>
    public class UriRecord : NdefRecord
    {
        private UriType _uriType;
        private string _uri = string.Empty;

        /// <summary>
        /// Uri Type
        /// </summary>
        public UriType UriType
        {
            get
            {
                return _uriType;
            }

            set
            {
                _uriType = value;
                SetPayload();
            }
        }

        /// <summary>
        /// Uri to encode
        /// </summary>
        /// <remarks>The Uri should be URL encoded to be valid in most cases. Consider encoding it.</remarks>
        public string Uri
        {
            get
            {
                return _uri;
            }

            set
            {
                _uri = value;
                SetPayload();
            }
        }

        /// <summary>
        /// The full Uri
        /// </summary>
        public Uri FullUri => new Uri($"{UriType.GetDescription()}{Uri}");

        /// <summary>
        /// Create a Uri Record from a Uri Type and a Uri
        /// </summary>
        /// <param name="uriType">The Uri type</param>
        /// <param name="uri">A Uri</param>
        public UriRecord(UriType uriType, string uri)
        {
            Header = new RecordHeader()
            {
                TypeNameFormat = TypeNameFormat.NfcWellKnowType,
                PayloadTypeLength = 1,
                PayloadType = new byte[1] { 0x55 },
                IdLength = 0,
            };

            _uriType = uriType;
            _uri = uri;
            SetPayload();
        }

        /// <summary>
        /// Create a URI Record from a valid NDEF Record
        /// </summary>
        /// <param name="ndefRecord">A valid NDEF Record</param>
        public UriRecord(NdefRecord ndefRecord)
        {
            Header = ndefRecord.Header;
            Payload = ndefRecord.Payload;
            ExtractAll();
        }

        /// <summary>
        /// Create a Uri Record from a span of bytes
        /// </summary>
        /// <param name="record">Record as a span of byte</param>
        public UriRecord(ReadOnlySpan<byte> record)
            : base(record)
        {
            ExtractAll();
        }

        private void ExtractAll()
        {
            if (!IsUriRecord(this) || Payload is not object)
            {
                throw new ArgumentException($"Record type must be {TypeNameFormat.NfcWellKnowType} and payload type 'U' (0x55)");
            }

            _uriType = (UriType)Payload[0];
            _uri = Encoding.UTF8.GetString(Payload, 1, Payload.Length - 1);
        }

        private void SetPayload()
        {
            Payload = new byte[_uri.Length + 1];
            Payload[0] = (byte)_uriType;
            Encoding.UTF8.GetBytes(_uri).CopyTo(Payload, 1);

            Header.PayloadLength = (uint)Payload.Length;
        }

        /// <summary>
        /// True if this is a valid URI Record
        /// </summary>
        /// <param name="ndefRecord">A valid NDEF Record</param>
        /// <returns></returns>
        public static bool IsUriRecord(NdefRecord ndefRecord)
        {
            if ((ndefRecord.Header.TypeNameFormat != TypeNameFormat.NfcWellKnowType) || (ndefRecord.Header.PayloadTypeLength != 1))
            {
                return false;
            }

            if (ndefRecord.Header.PayloadType?[0] != 0x55)
            {
                return false;
            }

            return true;
        }
    }
}
