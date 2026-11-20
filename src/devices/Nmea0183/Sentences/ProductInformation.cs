// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System;
using System.Collections.Generic;
using System.Globalization;
using System.Text;

namespace Iot.Device.Nmea0183.Sentences
{
    /// <summary>
    /// Product Information (PGN 126996) - Provides identification and version information about a device
    /// This message contains manufacturer and product identification strings, version numbers,
    /// and certification information for NMEA2000 devices.
    /// See https://canboat.github.io/canboat/canboat.html for the NMEA2000 protocol specification.
    /// </summary>
    public class ProductInformation : Nmea2000PackedMessage
    {
        /// <summary>
        /// The identifier of this message
        /// </summary>
        public const int HexId = 0x1F014;

        /// <summary>
        /// The PGN identifier for Product Information
        /// </summary>
        public override uint Identifier => HexId;

        /// <summary>
        /// NMEA 2000 Database Version (2 bytes)
        /// </summary>
        public ushort Nmea2000Version { get; set; }

        /// <summary>
        /// Product Code (2 bytes)
        /// </summary>
        public ushort ProductCode { get; set; }

        /// <summary>
        /// Model ID (up to 32 bytes, null-terminated string)
        /// </summary>
        public string ModelId { get; set; } = string.Empty;

        /// <summary>
        /// Software Version Code (up to 32 bytes, null-terminated string)
        /// </summary>
        public string SoftwareVersionCode { get; set; } = string.Empty;

        /// <summary>
        /// Model Version (up to 32 bytes, null-terminated string)
        /// </summary>
        public string ModelVersion { get; set; } = string.Empty;

        /// <summary>
        /// Serial Code (up to 32 bytes, null-terminated string)
        /// </summary>
        public string SerialCode { get; set; } = string.Empty;

        /// <summary>
        /// Certification Level (1 byte)
        /// </summary>
        public byte CertificationLevel { get; set; }

        /// <summary>
        /// Load Equivalency (1 byte)
        /// Number of 50mA units the device draws from the bus
        /// </summary>
        public byte LoadEquivalency { get; set; }

        /// <inheritdoc/>
        public override bool ReplacesOlderInstance => true;

        /// <summary>
        /// Constructs a new Product Information message
        /// </summary>
        /// <param name="nmeaDatabaseVersion">NMEA 2000 Database Version</param>
        /// <param name="productCode">Product Code</param>
        /// <param name="modelId">Model ID string</param>
        /// <param name="softwareVersionCode">Software Version Code string</param>
        /// <param name="modelVersion">Model Version string</param>
        /// <param name="serialCode">Serial Code string</param>
        /// <param name="certificationLevel">Certification Level</param>
        /// <param name="loadEquivalency">Load Equivalency (50mA units)</param>
        public ProductInformation(
            ushort nmeaDatabaseVersion,
            ushort productCode,
            string modelId,
            string softwareVersionCode,
            string modelVersion,
            string serialCode,
            byte certificationLevel = 0,
            byte loadEquivalency = 1)
            : base()
        {
            Nmea2000Version = nmeaDatabaseVersion;
            ProductCode = productCode;
            ModelId = modelId ?? string.Empty;
            SoftwareVersionCode = softwareVersionCode ?? string.Empty;
            ModelVersion = modelVersion ?? string.Empty;
            SerialCode = serialCode ?? string.Empty;
            CertificationLevel = certificationLevel;
            LoadEquivalency = loadEquivalency;
            Valid = true;
        }

        /// <summary>
        /// Internal constructor for decoding
        /// </summary>
        public ProductInformation(TalkerSentence sentence, DateTimeOffset time)
            : this(sentence.TalkerId, sentence.Fields, time)
        {
        }

        /// <summary>
        /// Decoding constructor
        /// </summary>
        public ProductInformation(TalkerId talkerId, IEnumerable<string> fields, DateTimeOffset time)
            : base(talkerId, Id, time)
        {
            IEnumerator<string> field = fields.GetEnumerator();

            // Parse common header fields (PGN, timestamp, source)
            ParseCommonFields(field, isAddressedMessage: false);

            // Parse the data payload
            string data = ReadString(field);

            if (data.Length >= 8) // Minimum: 2 bytes DB version + 2 bytes product code + at least some string data
            {
                int offset = 0;

                // NMEA Database Version (2 bytes, little-endian)
                if (ReadUshortFromHexString(data, offset, out ushort dbVersion))
                {
                    Nmea2000Version = dbVersion;
                    offset += 4; // 2 bytes = 4 hex chars
                }
                else
                {
                    Valid = false;
                    return;
                }

                // Product Code (2 bytes, little-endian)
                if (ReadUshortFromHexString(data, offset, out ushort prodCode))
                {
                    ProductCode = prodCode;
                }
                else
                {
                    Valid = false;
                    return;
                }

                // Model ID
                ModelId = ReadFixedString(data, 64, 8);

                // Software Version Code
                SoftwareVersionCode = ReadFixedString(data, 64, 72);

                // Model Version
                ModelVersion = ReadFixedString(data, 64, 136);

                // Serial Code
                SerialCode = ReadFixedString(data, 64, 200);

                // Certification Level (1 byte)
                if (ReadByteFromHexString(data, 264, out byte certLevel))
                {
                    CertificationLevel = certLevel;
                }

                // Load Equivalency (1 byte)
                if (ReadByteFromHexString(data, 266, out byte loadEquiv))
                {
                    LoadEquivalency = loadEquiv;
                }

                Valid = true;
            }
            else
            {
                Valid = false;
            }
        }

        /// <summary>
        /// Converts the message to NMEA parameter list format
        /// </summary>
        public override string ToNmeaParameterList()
        {
            // Start with common header fields
            string header = base.ToNmeaParameterList();

            // Build the data payload
            StringBuilder dataBuilder = new StringBuilder();

            // NMEA Database Version (2 bytes, little-endian)
            ushort versionCombined = Nmea2000Version;
            dataBuilder.Append(WriteUshortToHex(versionCombined));

            // Product Code (2 bytes, little-endian)
            dataBuilder.Append(WriteUshortToHex(ProductCode));

            // Model ID (null-terminated string)
            dataBuilder.Append(WriteFixedLengthString(ModelId, 64));

            // Software Version Code (null-terminated string)
            dataBuilder.Append(WriteFixedLengthString(SoftwareVersionCode, 64));

            // Model Version (null-terminated string)
            dataBuilder.Append(WriteFixedLengthString(ModelVersion, 64));

            // Serial Code (null-terminated string)
            dataBuilder.Append(WriteFixedLengthString(SerialCode, 64));

            // Certification Level (1 byte)
            dataBuilder.Append(WriteByteToHex(CertificationLevel));

            // Load Equivalency (1 byte)
            dataBuilder.Append(WriteByteToHex(LoadEquivalency));

            string data = dataBuilder.ToString();

            return $"{header}{data}";
        }

        /// <summary>
        /// Returns a human-readable representation of the message
        /// </summary>
        public override string ToReadableContent()
        {
            return $"Product Information: Model={ModelId}, Version={SoftwareVersionCode}, " +
                   $"Serial={SerialCode}, DB Version={Nmea2000Version}, " +
                   $"Product Code={ProductCode}, Cert Level={CertificationLevel}";
        }

        /// <summary>
        /// Reads a fixed length string from hex data
        /// </summary>
        private string ReadFixedString(string hexData, int length, int offset)
        {
            StringBuilder sb = new StringBuilder();

            while (offset < hexData.Length && sb.Length < length / 2)
            {
                if (ReadByteFromHexString(hexData, offset, out byte b))
                {
                    if (b == 0 || b == 0xFF) // Null terminator or invalid
                    {
                        break;
                    }

                    // ASCII printable character
                    if (b >= 0x20 && b <= 0x7E)
                    {
                        sb.Append((char)b);
                    }

                    offset += 2; // Move to next byte (2 hex chars)
                }
                else
                {
                    break;
                }
            }

            return sb.ToString().TrimEnd(' ', '\u0255', '@');
        }

        /// <summary>
        /// Writes a null-terminated string to hex format
        /// </summary>
        private string WriteFixedLengthString(string? text, int outputChars)
        {
            if (string.IsNullOrEmpty(text))
            {
                text = string.Empty;
            }

            StringBuilder sb = new StringBuilder();

            // Exactly "outputChars" nibbles expected in the output.
            string limitedText = text.Length > outputChars / 2 ? text.Substring(0, outputChars / 2) : text;

            foreach (char c in limitedText)
            {
                // Only write ASCII printable characters
                if (c >= 0x20 && c <= 0x7E)
                {
                    sb.Append(((byte)c).ToString("X2", CultureInfo.InvariantCulture));
                }
            }

            // Fill up with spaces
            while (sb.Length < outputChars)
            {
                sb.Append("20");
            }

            return sb.ToString();
        }
    }
}
