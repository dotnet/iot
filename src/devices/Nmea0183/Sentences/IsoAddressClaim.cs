// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System;
using System.Collections.Generic;
using System.Globalization;

namespace Iot.Device.Nmea0183.Sentences
{
    /// <summary>
    /// ISO Address Claim (PGN 60928) - Claim a network address on NMEA2000 bus
    /// This message is used to claim or announce a device's presence on the NMEA2000 network.
    /// </summary>
    public class IsoAddressClaim : Nmea2000PackedMessage
    {
        /// <summary>
        /// Constant identifier for ISO Address Claim message (PGN 60928)
        /// </summary>
        public const int HexId = 0xEE00; // 60928 decimal

        /// <summary>
        /// The PGN identifier for ISO Address Claim
        /// </summary>
        public override uint Identifier => HexId;

        /// <summary>
        /// Unique identity number (21 bits, 0-2097151)
        /// </summary>
        public uint UniqueNumber { get; set; }

        /// <summary>
        /// Manufacturer code (11 bits)
        /// </summary>
        public ManufacturerCode ManufacturerCode { get; set; }

        /// <summary>
        /// Device instance (all 8 bits)
        /// </summary>
        public byte DeviceInstance { get; set; }

        /// <summary>
        /// Device function (8 bits)
        /// This is not an enum, because its value is worthless without the device class. Use DeviceDescription for a human-readable description.
        /// </summary>
        public byte DeviceFunction { get; set; }

        /// <summary>
        /// Device class (7 bits)
        /// </summary>
        public DeviceClass DeviceClass { get; set; }

        /// <summary>
        /// System instance (4 bits, 0-15)
        /// </summary>
        public byte SystemInstance { get; set; }

        /// <summary>
        /// Industry code (3 bits)
        /// </summary>
        public IndustryCode IndustryCode { get; set; }

        /// <summary>
        /// Arbitrary address capable (1 bit)
        /// True if the device can use any address, false if it requires a specific address
        /// </summary>
        public bool ArbitraryAddressCapable { get; set; }

        /// <summary>
        /// Gets the human-readable description of the device type based on function and class
        /// </summary>
        public string DeviceDescription => Nmea2000DeviceInformation.GetDeviceDescription(DeviceFunction, DeviceClass);

        /// <summary>
        /// Gets the human-readable description of the device class
        /// </summary>
        public string ClassDescription => Nmea2000DeviceInformation.GetClassDescription(DeviceClass);

        /// <inheritdoc/>
        public override bool ReplacesOlderInstance => true;

        /// <summary>
        /// Constructs a new ISO Address Claim message
        /// </summary>
        /// <param name="uniqueNumber">Unique identity number (21 bits, 0-2097151)</param>
        /// <param name="manufacturerCode">Manufacturer code</param>
        /// <param name="deviceInstance">Device instance (0-7)</param>
        /// <param name="deviceFunction">Device function</param>
        /// <param name="deviceClass">Device class (0-127)</param>
        /// <param name="systemInstance">System instance (0-15)</param>
        /// <param name="industryCode">Industry code</param>
        /// <param name="arbitraryAddressCapable">True if device can use any address</param>
        public IsoAddressClaim(
            uint uniqueNumber,
            ManufacturerCode manufacturerCode,
            byte deviceInstance,
            byte deviceFunction,
            DeviceClass deviceClass,
            byte systemInstance = 0,
            IndustryCode industryCode = IndustryCode.Marine,
            bool arbitraryAddressCapable = true)
            : base()
        {
            if (uniqueNumber > 0x1FFFFF)
            {
                throw new ArgumentOutOfRangeException(nameof(uniqueNumber), "Unique number must be 21 bits or less (0-2097151)");
            }

            if (deviceInstance > 7)
            {
                throw new ArgumentOutOfRangeException(nameof(deviceInstance), "Device instance must be 0-7");
            }

            if ((int)deviceClass > 127)
            {
                throw new ArgumentOutOfRangeException(nameof(deviceClass), "Device class must be 0-127");
            }

            if (systemInstance > 15)
            {
                throw new ArgumentOutOfRangeException(nameof(systemInstance), "System instance must be 0-15");
            }

            UniqueNumber = uniqueNumber;
            ManufacturerCode = manufacturerCode;
            DeviceInstance = deviceInstance;
            DeviceFunction = deviceFunction;
            DeviceClass = deviceClass;
            SystemInstance = systemInstance;
            IndustryCode = industryCode;
            ArbitraryAddressCapable = arbitraryAddressCapable;
            Valid = true;
        }

        /// <summary>
        /// Internal constructor for decoding
        /// </summary>
        public IsoAddressClaim(TalkerSentence sentence, DateTimeOffset time)
            : this(sentence.TalkerId, sentence.Fields, time)
        {
        }

        /// <summary>
        /// Decoding constructor
        /// </summary>
        public IsoAddressClaim(TalkerId talkerId, IEnumerable<string> fields, DateTimeOffset time)
            : base(talkerId, Id, time)
        {
            IEnumerator<string> field = fields.GetEnumerator();

            // Parse common header fields (PGN, timestamp, source)
            ParseCommonFields(field, isAddressedMessage: false);

            // Parse the data payload (8 bytes for NAME field)
            string data = ReadString(field);

            if (data.Length >= 16) // 8 bytes = 16 hex characters
            {
                // Byte 0-2 (bits 0-20): Unique Number (21 bits)
                // Byte 2-3 (bits 21-31): Manufacturer Code (11 bits)
                // Byte 3 (bits 32-34): Device Instance Lower (3 bits)
                // Byte 4 (bits 35-39): Device Instance Upper (5 bits) - actually part of Device Function
                // Byte 4 (bits 35-42): Device Function (8 bits)
                // Byte 5 (bits 43-49): Device Class (7 bits)
                // Byte 5 (bit 50): Device Class MSB / Reserved (1 bit)
                // Byte 6 (bits 51-54): System Instance (4 bits)
                // Byte 6-7 (bits 55-57): Industry Code (3 bits)
                // Byte 7 (bit 63): Arbitrary Address Capable (1 bit)
                if (ReadUnsignedFromHexString(data, 0, 8, true, out uint nameLow) &&
                    ReadUnsignedFromHexString(data, 8, 8, true, out uint nameHigh))
                {
                    // Extract fields from the 64-bit NAME
                    ulong bitsequence = ((ulong)nameHigh << 32) | nameLow;

                    UniqueNumber = (uint)(bitsequence & 0x1FFFFF); // Bits 0-20
                    ManufacturerCode = (ManufacturerCode)((bitsequence >> 21) & 0x7FF); // Bits 21-31
                    DeviceInstance = (byte)((bitsequence >> 32) & 0xFF); // Bits 32-40
                    DeviceFunction = (byte)((bitsequence >> 40) & 0xFF); // Bits 40-48
                    DeviceClass = (DeviceClass)((bitsequence >> 49) & 0x7F); // Bits 43-49
                    SystemInstance = (byte)((bitsequence >> 56) & 0xF);
                    IndustryCode = (IndustryCode)((bitsequence >> 60) & 0x7); // Bits 60-62
                    ArbitraryAddressCapable = ((bitsequence >> 63) & 0x1) == 1; // Bit 63

                    Valid = true;
                }
                else
                {
                    Valid = false;
                }
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

            // Build the 64-bit NAME field
            ulong name = 0;
            name |= (UniqueNumber & 0x1FFFFF); // Bits 0-20
            name |= ((ulong)((uint)ManufacturerCode & 0x7FF) << 21); // Bits 21-31
            name |= ((ulong)(DeviceInstance & 0xFF) << 32); // Bits 32-34
            name |= ((ulong)DeviceFunction << 40); // Bits 35-42
            name |= ((ulong)((int)DeviceClass & 0x7F) << 49); // Bits 43-49
            name |= ((ulong)(SystemInstance & 0xF) << 56); // Bits 51-54
            name |= ((ulong)((uint)IndustryCode & 0x7) << 60); // Bits 55-57
            name |= ((ulong)(ArbitraryAddressCapable ? 1 : 0) << 63); // Bit 63

            // Convert to little-endian hex string
            uint nameLow = (uint)(name & 0xFFFFFFFF);
            uint nameHigh = (uint)((name >> 32) & 0xFFFFFFFF);

            string nameLowHex = nameLow.ToString("X8", CultureInfo.InvariantCulture);
            string nameHighHex = nameHigh.ToString("X8", CultureInfo.InvariantCulture);

            // Reverse byte order for little-endian
            string nameData = ReverseBytesInHexString(nameLowHex) + ReverseBytesInHexString(nameHighHex);

            return $"{header}{nameData}";
        }

        /// <summary>
        /// Returns a human-readable representation of the message
        /// </summary>
        public override string ToReadableContent()
        {
            return $"ISO Address Claim: Source={MessageSource}, Manufacturer={ManufacturerCode}, " +
                   $"UniqueNumber={UniqueNumber}, DeviceFunction={DeviceFunction}, DeviceClass={DeviceClass}, " +
                   $"Industry={IndustryCode}, ArbitraryAddr={ArbitraryAddressCapable}";
        }

        /// <summary>
        /// Helper method to reverse bytes in a hex string (for little-endian conversion)
        /// </summary>
        private static string ReverseBytesInHexString(string hex)
        {
            if (hex.Length % 2 != 0)
            {
                throw new ArgumentException("Hex string length must be even", nameof(hex));
            }

            string result = string.Empty;
            for (int i = hex.Length - 2; i >= 0; i -= 2)
            {
                result += hex.Substring(i, 2);
            }

            return result;
        }
    }
}
