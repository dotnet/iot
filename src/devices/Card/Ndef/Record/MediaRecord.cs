// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using System;
using System.Text;

namespace Iot.Device.Ndef
{
    /// <summary>
    /// Create a Media NDEF Record class
    /// </summary>
    public class MediaRecord : NdefRecord
    {
        /// <summary>
        /// Returns the Media Payload type
        /// </summary>
        public string PayloadType
        {
            get
            {
                var payloadType = Encoding.ASCII.GetString(Header.PayloadType!);
                payloadType = payloadType ?? string.Empty;
                return payloadType;
            }

            set
            {
                if (value.Length > 255)
                {
                    throw new ArgumentException($"Payload type can't be larger than 255");
                }

                var ptype = Encoding.ASCII.GetBytes(value);
                Header.PayloadType = ptype;
                Header.PayloadLength = (byte)ptype.Length;
            }
        }

        /// <summary>
        /// True if the payload is text based
        /// </summary>
        public bool IsTextType => PayloadType.ToLower().StartsWith("text/");

        /// <summary>
        /// Try to get our the encoded text
        /// </summary>
        /// <param name="payloadAsText">The payload as a text</param>
        /// <returns>True if success</returns>
        public bool TryGetPayloadAsText(out string payloadAsText)
        {
            payloadAsText = string.Empty;
            try
            {
                payloadAsText = Encoding.UTF8.GetString(Payload!);
            }
            catch (Exception ex) when (ex is ArgumentException || ex is ArgumentNullException || ex is DecoderFallbackException)
            {
                return false;
            }

            return true;
        }

        /// <summary>
        /// Create a Media Record from a NDEF Record
        /// </summary>
        /// <param name="ndefRecord">A valid NDEF Record</param>
        public MediaRecord(NdefRecord ndefRecord)
        {
            Header = ndefRecord.Header;
            Payload = ndefRecord.Payload;
        }

        /// <summary>
        /// Create a Media Record from the type and the payload
        /// </summary>
        /// <param name="payloadType">The payload type</param>
        /// <param name="payload">The byte payload</param>
        public MediaRecord(string payloadType, ReadOnlySpan<byte> payload)
            : base()
        {
            Header.TypeNameFormat = TypeNameFormat.MediaType;
            Header.PayloadType = Encoding.ASCII.GetBytes(payloadType);
            Header.PayloadLength = (uint)payload.Length;
            Payload = payload.ToArray();
        }

        /// <summary>
        /// Check if it's a valid Media Record
        /// </summary>
        /// <param name="ndefRecord">A valid NDEF Record</param>
        /// <returns></returns>
        public static bool IsMediaRecord(NdefRecord ndefRecord) => ndefRecord.Header.TypeNameFormat == TypeNameFormat.MediaType;
    }
}
