// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System;
using System.Linq;
using System.Collections.Generic;
using Iot.Device.Common;
using Iot.Device.Ndef;
using Microsoft.Extensions.Logging;

namespace Iot.Device.Card.Mifare
{
    /// <summary>
    /// A Mifare card class
    /// Supports Mifare Classic 1K and 4K
    /// Also supports Mifare Plus 2K and 4K operating in SL1
    /// </summary>
    public class MifareCard
    {
        private const byte BytesPerBlock = 16;
        private const byte BlocksPerSmallSector = 4;
        private const byte BlocksPerLargeSector = 16;
        private const byte NumberOfSmallSectors = 32;
        private static readonly MifareApplicationIdentifier NfcNdefId = new(0xE103);
        private static readonly byte[] EmptyNdefBlock = new byte[] { 0x03, 0x00, 0xFE, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00 };
        private static readonly byte[] StaticDefaultKeyA = new byte[6] { 0x00, 0x00, 0x00, 0x00, 0x00, 0x00 };
        private static readonly byte[] StaticDefaultKeyB = new byte[6] { 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF };
        private static readonly byte[] StaticDefaultFirstBlockNdefKeyA = new byte[6] { 0xA0, 0xA1, 0xA2, 0xA3, 0xA4, 0xA5 };
        private static readonly byte[] StaticDefaultBlocksNdefKeyA = new byte[6] { 0xD3, 0xF7, 0xD3, 0xF7, 0xD3, 0xF7 };

        // This is the actual RFID reader
        private readonly CardTransceiver _rfid;

        private readonly ILogger _logger;

        /// <summary>
        /// Default Key A
        /// </summary>
        public static ReadOnlySpan<byte> DefaultKeyA => StaticDefaultKeyA;

        /// <summary>
        /// Default Key B
        /// </summary>
        public static ReadOnlySpan<byte> DefaultKeyB => StaticDefaultKeyB;

        /// <summary>
        /// Default Mifare Application Directory block Key A for NDEF card
        /// The MAD is in the first sector on all cards and also sector 16 on 2K and 4K cards
        /// </summary>
        /// <remarks>See https://www.nxp.com/docs/en/application-note/AN10787.pdf for more information</remarks>
        public static ReadOnlySpan<byte> DefaultFirstBlockNdefKeyA => StaticDefaultFirstBlockNdefKeyA;

        /// <summary>
        /// Default block Key A for NDEF card
        /// </summary>
        /// <remarks>See https://www.nxp.com/docs/en/application-note/AN10787.pdf for more information</remarks>
        public static ReadOnlySpan<byte> DefaultBlocksNdefKeyA => StaticDefaultBlocksNdefKeyA;

        /// <summary>
        /// The tag number detected by the reader, only 1 or 2
        /// </summary>
        public byte Target { get; set; }

        /// <summary>
        /// The command to execute on the card
        /// </summary>
        public MifareCardCommand Command { get; set; }

        /// <summary>
        /// Key A Used for encryption/decryption
        /// </summary>
        public byte[]? KeyA { get; set; }

        /// <summary>
        /// Key B Used for encryption/decryption
        /// </summary>
        public byte[]? KeyB { get; set; }

        /// <summary>
        /// UUID is the Serial Number, called MAC sometimes
        /// </summary>
        public byte[]? SerialNumber { get; set; }

        /// <summary>
        /// The storage capacity
        /// </summary>
        public MifareCardCapacity Capacity { get; set; }

        /// <summary>
        /// The block number to authenticate or read or write
        /// </summary>
        public byte BlockNumber { get; set; }

        /// <summary>
        /// The Data which has been read or to write for the specific block
        /// </summary>
        public byte[] Data { get; set; } = new byte[0];

        /// <summary>
        /// Reselect the card after a card command fails
        /// After an error, the card will not respond to any further commands
        /// until it is reselected. If this property is false, the caller
        /// is responsible for calling ReselectCard when RunMifareCardCommand
        /// returns an error (-1).
        /// </summary>
        public bool ReselectAfterError { get; set; } = false;

        /// <summary>
        /// Determine the block group corresponding to a block number
        /// </summary>
        /// <param name="blockNumber">block number</param>
        /// <returns>block group</returns>
        /// In a 1K card there are 16 sectors, each containing four blocks.
        /// In a 2K card there are 32 sectors, each containing four blocks.
        /// In a 4K card there are four blocks in the first 32 sectors and 16 blocks in the remaining sectors.
        /// There are three groups of data blocks (either 1 or 5 blocks per group).
        /// The last block in the sector is the sector trailer.
        public static byte BlockNumberToBlockGroup(byte blockNumber) =>
            (byte)((blockNumber < 128) ? (blockNumber % 4) : (blockNumber % 16) / 5);

        /// <summary>
        /// Determine the sector number corresponding to a particular block number
        /// </summary>
        /// <param name="blockNumber">block number</param>
        /// <returns>sector number</returns>
        public static byte BlockNumberToSector(byte blockNumber) =>
            (byte)((blockNumber < 128) ? blockNumber / 4 : 32 + (blockNumber - 128) / 16);

        /// <summary>
        /// Determine the first block number of a specified sector and block group
        /// </summary>
        /// <param name="sector">sector number</param>
        /// <param name="group">group (0 to 3, where 3 is the sector trailer)</param>
        /// <returns>block number of the first (or only) block in the group</returns>
        public static byte SectorToBlockNumber(byte sector, byte group = 0) =>
            (byte)((sector < 32) ? sector * 4 + group : 128 + (sector - 32) * 16 + group * 5);

        /// <summary>
        /// Constructor for Mifarecard
        /// </summary>
        /// <param name="rfid">A card transceiver class</param>
        /// <param name="target">The target number as some card readers attribute one</param>
        public MifareCard(CardTransceiver rfid, byte target)
        {
            _rfid = rfid;
            Target = target;
            _logger = this.GetCurrentClassLogger();
        }

        /// <summary>
        /// Run the last setup command. In case of reading bytes, they are automatically pushed into the Data property
        /// </summary>
        /// <returns>-1 if the process fails otherwise the number of bytes read</returns>
        public int RunMifareCardCommand()
        {
            byte[] dataOut = new byte[0];
            if (Command == MifareCardCommand.Read16Bytes)
            {
                dataOut = new byte[16];
            }

            var ret = _rfid.Transceive(Target, Serialize(), dataOut.AsSpan(), NfcProtocol.Mifare);
            _logger.LogDebug($"{nameof(RunMifareCardCommand)}: {Command}, Target: {Target}, Data: {BitConverter.ToString(Serialize())}, Success: {ret}, Dataout: {BitConverter.ToString(dataOut)}");
            if ((ret > 0) && (Command == MifareCardCommand.Read16Bytes))
            {
                Data = dataOut;
            }

            if (ret < 0 && ReselectAfterError)
            {
                ReselectCard();
            }

            return ret;
        }

        #region Sector Tailer and Access Type

        private (byte C1a, byte C1b, byte C2a, byte C2b, byte C3a, byte C3b) DecodeSectorTailer(byte blockGroup, byte[] sectorData)
        {
            // Bit      7    6    5    4    3    2    1    0
            // Byte 6 !C23 !C22 !C21 !C20 !C13 !C12 !C11 !C10
            // Byte 7  C13  C12  C11  C10 !C33 !C32 !C31 !C30
            // Byte 8  C33  C32  C31  C30  C23  C22  C21  C20
            // Cab a = access bit and b = block number
            // Extract the C1
            byte c1a = (byte)((~(sectorData[6]) >> blockGroup) & 0b0000_0001);
            byte c1b = (byte)((sectorData[7] >> (4 + blockGroup)) & 0b0000_0001);
            // Extract the C2
            byte c2a = (byte)((sectorData[8] >> blockGroup) & 0b0000_0001);
            byte c2b = (byte)((~(sectorData[6]) >> (4 + blockGroup)) & 0b0000_0001);
            // Extract the C3
            byte c3a = (byte)((~(sectorData[7]) >> blockGroup) & 0b0000_0001);
            byte c3b = (byte)((sectorData[8] >> (4 + blockGroup)) & 0b0000_0001);
            return (c1a, c1b, c2a, c2b, c3a, c3b);
        }

        /// <summary>
        /// Get the sector tailer bytes for a specific access sector configuration
        /// </summary>
        /// <param name="accessSector">the access sector</param>
        /// <returns>the 3 bytes for configuration</returns>
        public (byte B6, byte B7, byte B8) EncodeSectorTailer(AccessSector accessSector)
        {
            byte c1 = 0;
            byte c2 = 0;
            byte c3 = 0;

            // Ignore AccessSector.KeyBRead
            accessSector = accessSector & ~AccessSector.ReadKeyB;
            // Find the table of truth and the core Access Bits
            if (accessSector == (AccessSector.WriteKeyAWithKeyA | AccessSector.ReadAccessBitsWithKeyA | AccessSector.ReadKeyBWithKeyA |
                    AccessSector.WriteKeyBWithKeyA))
            {
                c1 = 0;
                c2 = 0;
                c3 = 0;
            }

            if (accessSector == (AccessSector.ReadAccessBitsWithKeyA | AccessSector.ReadKeyBWithKeyA))
            {
                c1 = 0;
                c2 = 1;
                c3 = 0;
            }

            if (accessSector == (AccessSector.WriteKeyAWithKeyB | AccessSector.ReadAccessBitsWithKeyA |
                    AccessSector.ReadAccessBitsWithKeyB | AccessSector.WriteKeyBWithKeyB))
            {
                c1 = 1;
                c2 = 0;
                c3 = 0;
            }

            if (accessSector == (AccessSector.ReadAccessBitsWithKeyA | AccessSector.ReadAccessBitsWithKeyB))
            {
                c1 = 1;
                c2 = 1;
                c3 = 0;
            }

            if (accessSector == (AccessSector.WriteKeyAWithKeyA | AccessSector.ReadAccessBitsWithKeyA |
                    AccessSector.WriteAccessBitsWithKeyA | AccessSector.ReadKeyBWithKeyA |
                    AccessSector.WriteKeyBWithKeyA))
            {
                c1 = 0;
                c2 = 0;
                c3 = 1;
            }

            if (accessSector == (AccessSector.WriteKeyAWithKeyB | AccessSector.ReadAccessBitsWithKeyA |
                    AccessSector.ReadAccessBitsWithKeyB | AccessSector.WriteAccessBitsWithKeyB |
                    AccessSector.WriteKeyBWithKeyB))
            {
                c1 = 0;
                c2 = 1;
                c3 = 1;
            }

            if (accessSector == (AccessSector.ReadAccessBitsWithKeyA |
                    AccessSector.ReadAccessBitsWithKeyB | AccessSector.WriteAccessBitsWithKeyB))
            {
                c1 = 1;
                c2 = 0;
                c3 = 1;
            }

            if (accessSector == (AccessSector.ReadAccessBitsWithKeyA |
                    AccessSector.ReadAccessBitsWithKeyB))
            {
                c1 = 1;
                c2 = 1;
                c3 = 1;
            }

            // Encode the into the 3 bytes
            byte b6 = (byte)((((~c2) & 0x01) << 7) | (((~c1) & 0x01) << 3));
            byte b7 = (byte)(((c1) << 7) | (((~c3) & 0x01) << 3));
            byte b8 = (byte)(((c3) << 7) | ((c2) << 3));
            return (b6, b7, b8);
        }

        /// <summary>
        /// Encode the sector tailer access type for a specific block
        /// </summary>
        /// <param name="blockNumber">The block sector to encode</param>
        /// <param name="accessType">The access type to encode</param>
        /// <returns>The encoded sector tailer for the specific block</returns>
        public (byte B6, byte B7, byte B8) EncodeSectorTailer(byte blockNumber, AccessType accessType)
        {
            byte blockGroup = BlockNumberToBlockGroup(blockNumber);

            byte c1 = 0;
            byte c2 = 0;
            byte c3 = 0;

            if (accessType == (AccessType.ReadKeyA | AccessType.ReadKeyB | AccessType.WriteKeyA | AccessType.WriteKeyB |
                    AccessType.IncrementKeyA | AccessType.IncrementKeyB |
                    AccessType.DecrementTransferRestoreKeyA | AccessType.DecrementTransferRestoreKeyB))
            {
                c1 = 0;
                c2 = 0;
                c3 = 0;
            }

            if (accessType == (AccessType.ReadKeyA | AccessType.ReadKeyB))
            {
                c1 = 0;
                c2 = 1;
                c3 = 0;
            }

            if (accessType == (AccessType.ReadKeyA | AccessType.ReadKeyB | AccessType.WriteKeyB))
            {
                c1 = 1;
                c2 = 0;
                c3 = 0;
            }

            if (accessType == (AccessType.ReadKeyA | AccessType.ReadKeyB | AccessType.WriteKeyB |
                    AccessType.IncrementKeyB |
                    AccessType.DecrementTransferRestoreKeyA | AccessType.DecrementTransferRestoreKeyB))
            {
                c1 = 1;
                c2 = 1;
                c3 = 0;
            }

            if (accessType == (AccessType.ReadKeyA | AccessType.ReadKeyB |
                    AccessType.DecrementTransferRestoreKeyA | AccessType.DecrementTransferRestoreKeyB))
            {
                c1 = 0;
                c2 = 0;
                c3 = 1;
            }

            if (accessType == (AccessType.ReadKeyB | AccessType.WriteKeyB))
            {
                c1 = 0;
                c2 = 1;
                c3 = 1;
            }

            if (accessType == AccessType.ReadKeyB)
            {
                c1 = 1;
                c2 = 0;
                c3 = 1;
            }

            if (accessType == AccessType.None)
            {
                c1 = 1;
                c2 = 1;
                c3 = 1;
            }

            // Encore the access bits
            byte b6 = (byte)((((~c2) & 0x01) << (4 + blockGroup)) | (((~c1) & 0x01) << blockGroup));
            byte b7 = (byte)(((c1) << (4 + blockGroup)) | (((~c3) & 0x01) << blockGroup));
            byte b8 = (byte)(((c3) << (4 + blockGroup)) | ((c2) << blockGroup));
            return (b6, b7, b8);
        }

        /// <summary>
        /// Get the sector tailer access information
        /// </summary>
        /// <param name="blockNumber">the block sector number</param>
        /// <param name="sectorData">The full sector data to decode</param>
        /// <returns>the access sector rights</returns>
        public AccessSector SectorTailerAccess(byte blockNumber, byte[] sectorData)
        {
            // Bit      7    6    5    4    3    2    1    0
            // Byte 6 !C23 !C22 !C21 !C20 !C13 !C12 !C11 !C10
            // Byte 7  C13  C12  C11  C10 !C33 !C32 !C31 !C30
            // Byte 8  C33  C32  C31  C30  C23  C22  C21  C20
            // Cab a = access bit and b = block number
            byte blockGroup = BlockNumberToBlockGroup(blockNumber);
            if (blockGroup != 3)
            {
                return AccessSector.None;
            }

            var (c1a, c1b, c2a, c2b, c3a, c3b) = DecodeSectorTailer(blockGroup, sectorData);
            if (c1a != c1b)
            {
                return AccessSector.None;
            }

            if (c2a != c2b)
            {
                return AccessSector.None;
            }

            if (c3a != c3b)
            {
                return AccessSector.None;
            }

            // Table of truth
            if ((c1a == 0) && (c2a == 0) && (c3a == 0))
            {
                return AccessSector.WriteKeyAWithKeyA | AccessSector.ReadAccessBitsWithKeyA | AccessSector.ReadKeyBWithKeyA |
                    AccessSector.WriteKeyBWithKeyA | AccessSector.ReadKeyB;
            }

            if ((c1a == 0) && (c2a == 1) && (c3a == 0))
            {
                return AccessSector.ReadAccessBitsWithKeyA | AccessSector.ReadKeyBWithKeyA | AccessSector.ReadKeyB;
            }

            if ((c1a == 1) && (c2a == 0) && (c3a == 0))
            {
                return AccessSector.WriteKeyAWithKeyB | AccessSector.ReadAccessBitsWithKeyA |
                    AccessSector.ReadAccessBitsWithKeyB | AccessSector.WriteKeyBWithKeyB;
            }

            if ((c1a == 1) && (c2a == 1) && (c3a == 0))
            {
                return AccessSector.ReadAccessBitsWithKeyA | AccessSector.ReadAccessBitsWithKeyB;
            }

            if ((c1a == 0) && (c2a == 0) && (c3a == 1))
            {
                return AccessSector.WriteKeyAWithKeyA | AccessSector.ReadAccessBitsWithKeyA |
                    AccessSector.WriteAccessBitsWithKeyA | AccessSector.ReadKeyBWithKeyA |
                    AccessSector.WriteKeyBWithKeyA | AccessSector.ReadKeyB;
            }

            if ((c1a == 0) && (c2a == 1) && (c3a == 1))
            {
                return AccessSector.WriteKeyAWithKeyB | AccessSector.ReadAccessBitsWithKeyA |
                    AccessSector.ReadAccessBitsWithKeyB | AccessSector.WriteAccessBitsWithKeyB |
                    AccessSector.WriteKeyBWithKeyB;
            }

            if ((c1a == 1) && (c2a == 0) && (c3a == 1))
            {
                return AccessSector.ReadAccessBitsWithKeyA |
                    AccessSector.ReadAccessBitsWithKeyB | AccessSector.WriteAccessBitsWithKeyB;
            }

            if ((c1a == 1) && (c2a == 0) && (c3a == 1))
            {
                return AccessSector.ReadAccessBitsWithKeyA |
                    AccessSector.ReadAccessBitsWithKeyB;
            }

            return AccessSector.None;
        }

        /// <summary>
        /// Get the block access information
        /// </summary>
        /// <param name="blockNumber">the block number</param>
        /// <param name="sectorData">the sector tailer data</param>
        /// <returns>The access type rights</returns>
        public AccessType BlockAccess(byte blockNumber, byte[] sectorData)
        {
            // Bit      7    6    5    4    3    2    1    0
            // Byte 6 !C23 !C22 !C21 !C20 !C13 !C12 !C11 !C10
            // Byte 7  C13  C12  C11  C10 !C33 !C32 !C31 !C30
            // Byte 8  C33  C32  C31  C30  C23  C22  C21  C20
            // Cab a = access bit and b = block number
            byte blockGroup = BlockNumberToBlockGroup(blockNumber);
            if (blockGroup == 3)
            {
                return AccessType.None;
            }

            var (c1a, c1b, c2a, c2b, c3a, c3b) = DecodeSectorTailer(blockGroup, sectorData);
            if (c1a != c1b)
            {
                return AccessType.None;
            }

            if (c2a != c2b)
            {
                return AccessType.None;
            }

            if (c3a != c3b)
            {
                return AccessType.None;
            }

            // Table of truth
            if ((c1a == 0) && (c2a == 0) && (c3a == 0))
            {
                return AccessType.ReadKeyA | AccessType.ReadKeyB | AccessType.WriteKeyA | AccessType.WriteKeyB |
                    AccessType.IncrementKeyA | AccessType.IncrementKeyB |
                    AccessType.DecrementTransferRestoreKeyA | AccessType.DecrementTransferRestoreKeyB;
            }

            if ((c1a == 0) && (c2a == 1) && (c3a == 0))
            {
                return AccessType.ReadKeyA | AccessType.ReadKeyB;
            }

            if ((c1a == 1) && (c2a == 0) && (c3a == 0))
            {
                return AccessType.ReadKeyA | AccessType.ReadKeyB | AccessType.WriteKeyB;
            }

            if ((c1a == 1) && (c2a == 1) && (c3a == 0))
            {
                return AccessType.ReadKeyA | AccessType.ReadKeyB | AccessType.WriteKeyB |
                    AccessType.IncrementKeyB |
                    AccessType.DecrementTransferRestoreKeyA | AccessType.DecrementTransferRestoreKeyB;
            }

            if ((c1a == 0) && (c2a == 0) && (c3a == 1))
            {
                return AccessType.ReadKeyA | AccessType.ReadKeyB |
                    AccessType.DecrementTransferRestoreKeyA | AccessType.DecrementTransferRestoreKeyB;
            }

            if ((c1a == 0) && (c2a == 1) && (c3a == 1))
            {
                return AccessType.ReadKeyB | AccessType.WriteKeyB;
            }

            if ((c1a == 1) && (c2a == 0) && (c3a == 1))
            {
                return AccessType.ReadKeyB;
            }

            return AccessType.None;
        }

        /// <summary>
        /// Encode the desired access for the full sector including the block tailer
        /// </summary>
        /// <param name="accessSector">The access desired</param>
        /// <param name="accessTypes">An array of 3 AccessType determining access of each block</param>
        /// <returns>The 3 bytes encoding the rights</returns>
        /// This is a synonym of EncodeSectorAndBlockTailer (for backward compatibility)
        [Obsolete("deprecated, use EncodeSectorAndBlockTailer instead")]
        public (byte B6, byte B7, byte B8) EncodeSectorAndClockTailer(AccessSector accessSector, AccessType[] accessTypes) =>
            EncodeSectorAndBlockTailer(accessSector, accessTypes);

        /// <summary>
        /// Encode the desired access for the full sector including the block tailer
        /// </summary>
        /// <param name="accessSector">The access desired</param>
        /// <param name="accessTypes">An array of 3 AccessType determining access of each block</param>
        /// <returns>The 3 bytes encoding the rights</returns>
        public (byte B6, byte B7, byte B8) EncodeSectorAndBlockTailer(AccessSector accessSector, AccessType[] accessTypes)
        {
            if (accessTypes.Length != 3)
            {
                throw new ArgumentException("Array must have 3 elements.", nameof(accessTypes));
            }

            var tupleRes = EncodeSectorTailer(accessSector);
            byte b6 = tupleRes.B6;
            byte b7 = tupleRes.B7;
            byte b8 = tupleRes.B8;
            for (byte i = 0; i < 3; i++)
            {
                tupleRes = EncodeSectorTailer(i, accessTypes[i]);
                b6 |= tupleRes.B6;
                b7 |= tupleRes.B7;
                b8 |= tupleRes.B8;
            }

            return (b6, b7, b8);
        }

        /// <summary>
        /// Encode with default value the access sector and tailer blocks
        /// </summary>
        /// <returns></returns>
        public (byte B6, byte B7, byte B8) EncodeDefaultSectorAndBlockTailer() => (0xFF, 0x07, 0x80);

        /// <summary>
        /// From the ATQA and SAK data find common card capacity
        /// </summary>
        /// <param name="ATQA">The ATQA response</param>
        /// <param name="SAK">The SAK response</param>
        /// <remarks>Does not recognize Mifare Plus cards, capacity must be set manually</remarks>
        public void SetCapacity(ushort ATQA, byte SAK)
        {
            // Type of Mifare can be partially determined by ATQA and SAK
            // https://www.nxp.com/docs/en/application-note/AN10833.pdf
            // Not complete
            if (ATQA == 0x0004 || ATQA == 0x0044)
            {
                if (SAK == 0x08)
                {
                    Capacity = MifareCardCapacity.Mifare1K;
                }
                else if (SAK == 0x09)
                {
                    Capacity = MifareCardCapacity.Mifare300;
                }
            }
            else if ((ATQA == 0x0002) && (SAK == 0x18))
            {
                Capacity = MifareCardCapacity.Mifare4K;
            }
        }

        /// <summary>
        /// Is it a block sector?
        /// </summary>
        /// <param name="blockNumber">Input block number</param>
        /// <returns>True if it is a sector block</returns>
        public bool IsSectorBlock(byte blockNumber) => BlockNumberToBlockGroup(blockNumber) == 3;

        /// <summary>
        /// Get the number of blocks for a specific sector
        /// </summary>
        /// <param name="sectorNumber">Input sector number</param>
        /// <returns>The number of blocks for this specific sector</returns>
        public byte GetNumberBlocks(byte sectorNumber) => sectorNumber < 32 ? BlocksPerSmallSector : BlocksPerLargeSector;

        /// <summary>
        /// Get the number of blocks for a specific sector
        /// </summary>
        /// <returns>The number of blocks for this specific sector</returns>
        public int GetNumberBlocks() => Capacity switch
        {
            MifareCardCapacity.Mifare1K => 1024 / 16,
            MifareCardCapacity.Mifare2K => 2048 / 16,
            MifareCardCapacity.Mifare4K => 4096 / 16,
            _ or MifareCardCapacity.Mifare300 or MifareCardCapacity.Unknown => 0,
        };

        /// <summary>
        /// Get the number of sectors
        /// </summary>
        /// <returns></returns>
        public int GetNumberSectors() => Capacity switch
        {
            MifareCardCapacity.Mifare1K => 16,
            MifareCardCapacity.Mifare2K => 32,
            MifareCardCapacity.Mifare4K => 40,
            _ or MifareCardCapacity.Mifare300 or MifareCardCapacity.Unknown => 0,
        };

        #endregion

        /// <summary>
        /// Depending on the command, serialize the needed data
        /// Authentication will serialize the command, the concerned key and
        /// the serial number
        /// Reading data will just serialize the command
        /// Writing data will serialize the data as well
        /// </summary>
        /// <returns>The serialized bits</returns>
        private byte[] Serialize()
        {
            byte[]? ser = null;
            switch (Command)
            {
                case MifareCardCommand.AuthenticationA:
                case MifareCardCommand.AuthenticationB:
                    byte[]? key = (Command == MifareCardCommand.AuthenticationA) ? KeyA : KeyB;
                    if (key is null || key.Length != 6 || SerialNumber is null ||
                        (SerialNumber.Length != 4 && SerialNumber.Length != 7))
                    {
                        throw new ArgumentException($"Card is not configured for {Command}.");
                    }

                    ser = new byte[2 + 6 + 4];
                    ser[0] = (byte)Command;
                    ser[1] = BlockNumber;
                    key.CopyTo(ser, 2);
                    // SerialNumber[^4..].CopyTo(ser, 2 + 6);
                    SerialNumber.AsSpan().Slice(SerialNumber.Length - 4).CopyTo(ser.AsSpan().Slice(2 + 6));
                    return ser;
                case MifareCardCommand.Write16Bytes:
                case MifareCardCommand.Write4Bytes:
                    if (Data is null)
                    {
                        throw new ArgumentException($"Card is not configured for {nameof(MifareCardCommand.Write4Bytes)}.");
                    }

                    ser = new byte[2 + Data.Length];
                    ser[0] = (byte)Command;
                    ser[1] = BlockNumber;
                    if (Data.Length > 0)
                    {
                        Data.CopyTo(ser, 2);
                    }

                    return ser;
                case MifareCardCommand.Incrementation:
                case MifareCardCommand.Decrementation:
                case MifareCardCommand.Transfer:
                case MifareCardCommand.Restore:
                case MifareCardCommand.Read16Bytes:
                    ser = new byte[2];
                    ser[0] = (byte)Command;
                    ser[1] = BlockNumber;
                    return ser;
                default:
                    return new byte[0];
            }
        }

        /// <summary>
        /// Erase one sector
        /// </summary>
        /// <param name="newKeyA">The new key A, empty to use current one</param>
        /// <param name="newKeyB">The new key B, empty to use current one</param>
        /// <param name="sector">The sector number. Refer to Mifare documentation to understand how blocks work especially for Mifare 2K and 4K</param>
        /// <param name="authenticateWithKeyA">True to authenticate with current Key A, false to authenticate with Key B</param>
        /// <param name="resetAccessBytes">True to reset all the access bytes</param>
        /// <returns>True if success</returns>
        /// <remarks>Sector 0 can't be fully erase, only the blocks 1 and 2 will be erased</remarks>
        public bool EraseSector(ReadOnlySpan<byte> newKeyA, ReadOnlySpan<byte> newKeyB, byte sector, bool authenticateWithKeyA, bool resetAccessBytes)
        {
            int nbSectors = GetNumberSectors();
            if (sector >= nbSectors)
            {
                throw new ArgumentException($"{nameof(sector)} has to be less than the total number of sector for this card {nbSectors}");
            }

            int nbBlocks = GetNumberBlocks(sector);

            if ((KeyB is not object or { Length: not 6 }) || (KeyA is not object or { Length: not 6 }))
            {
                throw new ArgumentException($"You must have a key A and key B of 6 bytes long");
            }

            if (Data is not object)
            {
                Data = new byte[16];
            }

            byte sectorTailer = SectorToBlockNumber(sector, 3);
            BlockNumber = sectorTailer;
            Command = authenticateWithKeyA ? MifareCardCommand.AuthenticationA : MifareCardCommand.AuthenticationB;
            var ret = RunMifareCardCommand();
            if (ret < 0)
            {
                return false;
            }

            if (resetAccessBytes)
            {
                (Data[6], Data[7], Data[8]) = EncodeDefaultSectorAndBlockTailer();
                Data[9] = 0x00;
            }

            if (newKeyA.Length is 6)
            {
                KeyA = newKeyA.ToArray();
                newKeyA.CopyTo(Data.AsSpan().Slice(0, 6));
            }

            if (newKeyB.Length is 6)
            {
                KeyB = newKeyB.ToArray();
                newKeyB.CopyTo(Data.AsSpan().Slice(10, 6));
            }

            // Find the sector tailer based on the sector number and it's a safe byte cast
            var success = WriteDataBlock(sectorTailer);
            if (!success)
            {
                return false;
            }

            byte firstBlock = SectorToBlockNumber(sector);
            // Authenticate to the rest of the blocks to format and format them
            for (byte block = firstBlock == 0 ? firstBlock++ : firstBlock; block < (firstBlock + nbBlocks - 1); block++)
            {
                BlockNumber = block;
                Command = authenticateWithKeyA ? MifareCardCommand.AuthenticationA : MifareCardCommand.AuthenticationB;
                ret = RunMifareCardCommand();
                if (ret < 0)
                {
                    return false;
                }

                Data = new byte[16];
                success = WriteDataBlock(block);
                if (!success)
                {
                    return false;
                }
            }

            return true;
        }

        /// <summary>
        /// Select the card. Needed if authentication or read/write failed
        /// </summary>
        /// <returns>True if success</returns>
        public bool ReselectCard()
        {
            return _rfid.ReselectTarget(Target);
        }

        /// <summary>
        /// Format the entire card to NDEF
        /// </summary>
        /// <param name="keyB">The key B to be used for formatting, if empty, will use the default key B</param>
        /// <returns>True if success</returns>
        /// <exception cref="ArgumentException">The card size is unknown or the specified KeyB is invalid</exception>
        public bool FormatNdef(ReadOnlySpan<byte> keyB = default) => FormatNdef(0, keyB);

        /// <summary>
        /// Format a portion of the card to NDEF
        /// </summary>
        /// <param name="numberOfSectors">The number of sectors for NDEF, if zero, use the entire card</param>
        /// <param name="keyB">The key B to be used for formatting, if empty, will use the default key B</param>
        /// <returns>True if success</returns>
        /// <exception cref="ArgumentException">The card size is unknown or the specified KeyB is invalid</exception>
        /// <remarks>The requested number of sectors are configured as NFC Forum sectors. To reserve some
        /// space on the card for other purposes, specify a nonzero value for <paramref name="numberOfSectors" />
        /// and then allocate additional applications using <see cref="MifareDirectory"/> </remarks>
        public bool FormatNdef(uint numberOfSectors, ReadOnlySpan<byte> keyB = default)
        {
            if (Capacity is not (MifareCardCapacity.Mifare1K or MifareCardCapacity.Mifare2K or MifareCardCapacity.Mifare4K))
            {
                throw new ArgumentException($"Only Mifare card classic are supported with capacity of 1K, 2K and 4K");
            }

            int nbBlocks = GetNumberBlocks();

            byte[] keyFormat = keyB.Length == 0 ? StaticDefaultKeyB : keyB.ToArray();
            if (keyFormat.Length != 6)
            {
                throw new ArgumentException($"{nameof(keyB)} can only be empty or 6 bytes length");
            }

            // Create and write the Mifare Application Directory
            MifareDirectory directory = MifareDirectory.CreateEmpty(this);
            MifareDirectoryEntry? ndefEntry = directory.Allocate(NfcNdefId, numberOfSectors);
            var formatOk = (ndefEntry != null) && directory.StoreToCard(keyFormat);

            // Write the empty NDEF TLV in the first NFC sector
            byte ndefBlockNumber = ndefEntry?.GetAllDataBlocks().FirstOrDefault() ?? 0;
            formatOk &= (ndefBlockNumber != 0) && AuthenticateBlockKeyB(keyFormat, ndefBlockNumber);
            Data = EmptyNdefBlock;
            formatOk &= WriteDataBlock(ndefBlockNumber);

            // We wrote the MAD sector trailers already, so we write the NFC sector trailers here
            Data = new byte[] { 0, 0, 0, 0, 0, 0, 0x7F, 0x07, 0x88, 0x40, 0, 0, 0, 0, 0, 0 };
            DefaultBlocksNdefKeyA.CopyTo(Data);
            keyFormat.CopyTo(Data, 10);
            for (byte sector = 1; sector < GetNumberSectors(); sector++)
            {
                if (sector != 16)
                {
                    byte block = SectorToBlockNumber(sector, 3);
                    formatOk &= AuthenticateBlockKeyB(keyFormat, block);
                    formatOk &= WriteDataBlock(block);
                }
            }

            return formatOk;
        }

        /// <summary>
        /// Write an NDEF Message
        /// </summary>
        /// <param name="message">The NDEF Message to write</param>
        /// <param name="writeKeyA">True to write with Key A, false to write with Key B</param>
        /// <returns>True if success</returns>
        /// <exception cref="ArgumentException">If using KeyB, it must be 6 bytes long</exception>
        /// <exception cref="InvalidOperationException">The card is not formatted for NDEF</exception>
        /// <exception cref="ArgumentOutOfRangeException">The message to be written is larger than the available space on the card</exception>
        /// <remarks>The Mifare application directory indicates range of sectors for NDEF. Normally,
        /// these begin at sector 1. The message must fit within the allocated space on the card.</remarks>
        public bool WriteNdefMessage(NdefMessage message, bool writeKeyA = true)
        {
            if ((KeyB is not object or { Length: not 6 }) && (!writeKeyA))
            {
                throw new ArgumentException("The Key B must be 6 bytes long");
            }

            // We need to add 0x03 then the length on 1 or 2 bytes then the trailer 0xFE
            int messageLengthBytes = message.Length > 254 ? 3 : 1;
            Span<byte> serializedMessage = stackalloc byte[message.Length + 2 + messageLengthBytes];
            message.Serialize(serializedMessage.Slice(1 + messageLengthBytes));
            serializedMessage[0] = 0x03;
            if (messageLengthBytes == 1)
            {
                serializedMessage[1] = (byte)message.Length;
            }
            else
            {
                serializedMessage[1] = 0xFF;
                serializedMessage[2] = (byte)((message.Length >> 8) & 0xFF);
                serializedMessage[3] = (byte)(message.Length & 0xFF);
            }

            serializedMessage[serializedMessage.Length - 1] = 0xFE;

            // Get NDEF application entry from Mifare application directory
            MifareDirectory? directory = MifareDirectory.LoadFromCard(this);
            MifareDirectoryEntry ndefEntry =
                directory?.TryGetApplication(NfcNdefId) ?? throw new InvalidOperationException("card is not formatted for NDEF");

            // Number of blocks to write
            int nbBlocks = (serializedMessage.Length + BytesPerBlock - 1) / BytesPerBlock;
            if (nbBlocks > ndefEntry.NumberOfBlocks)
            {
                uint maximumNumberOfBytes = ndefEntry.NumberOfBlocks * BytesPerBlock;
                throw new ArgumentOutOfRangeException(nameof(message), $"NDEF message too large, maximum {maximumNumberOfBytes} bytes, current size is {serializedMessage.Length} bytes");
            }

            bool Authenticate(byte block) => writeKeyA ? AuthenticateBlockKeyA(StaticDefaultBlocksNdefKeyA, block) : AuthenticateBlockKeyB(KeyB!, block);
            Data = new byte[BytesPerBlock];
            bool ret = true;
            IEnumerator<byte> sectorBlocks = ndefEntry.GetAllDataBlocks().GetEnumerator();
            for (int byteOffset = 0; byteOffset < serializedMessage.Length; byteOffset += BytesPerBlock)
            {
                if (!sectorBlocks.MoveNext())
                {
                    // this "can't happen" because we checked the size above
                    ret = false;
                    break;
                }

                // copy the next block, filling with zeros at the end of the last block
                int dataLen = Math.Min(BytesPerBlock, serializedMessage.Length - byteOffset);
                serializedMessage.Slice(byteOffset, dataLen).CopyTo(Data);
                Array.Clear(Data, dataLen, BytesPerBlock - dataLen);
                ret &= Authenticate(sectorBlocks.Current) && WriteDataBlock(sectorBlocks.Current);
            }

            return ret;
        }

        /// <summary>
        /// Check if the card is formatted to NDEF
        /// </summary>
        /// <returns>True if NDEF formatted</returns>
        /// <remarks>This checks for a Mifare application directory in sector 0 and
        /// (for 2K and 4K cards) sector 16, that there is an NDEF application, that
        /// the sector trailer is readable in all sectors in that application, and
        /// that the GPB byte is set correctly in the trailers.</remarks>
        public bool IsFormattedNdef()
        {
            MifareDirectory? directory = MifareDirectory.LoadFromCard(this);
            MifareDirectoryEntry? ndefEntry = directory?.TryGetApplication(NfcNdefId);
            bool ret = (ndefEntry != null);
            foreach (byte sector in ndefEntry?.GetAllSectors() ?? Enumerable.Empty<byte>())
            {
                byte sectorBlock = SectorToBlockNumber(sector, 3);
                ret &= AuthenticateBlockKeyA(StaticDefaultBlocksNdefKeyA, sectorBlock);
                ret &= ReadDataBlock(sectorBlock);
                ret &= (Data[9] == 0x40);
            }

            return ret;
        }

        /// <summary>
        /// Perform a write using the 16 bytes present in Data on a specific block
        /// </summary>
        /// <param name="block">The block number to write</param>
        /// <returns>True if success</returns>
        /// <remarks>You will need to be authenticated properly before</remarks>
        public bool WriteDataBlock(byte block)
        {
            BlockNumber = block;
            Command = MifareCardCommand.Write16Bytes;
            var ret = RunMifareCardCommand();
            return ret >= 0;
        }

        /// <summary>
        /// Perform a read and place the result into the 16 bytes Data property on a specific block
        /// </summary>
        /// <param name="block">The block number to write</param>
        /// <returns>True if success</returns>
        /// /// <remarks>You will need to be authenticated properly before</remarks>
        private bool ReadDataBlock(byte block)
        {
            BlockNumber = block;
            Command = MifareCardCommand.Read16Bytes;
            var ret = RunMifareCardCommand();
            return ret >= 0;
        }

        /// <summary>
        /// Try to read a NDEF Message from a Mifare card
        /// </summary>
        /// <param name="message">The NDEF message</param>
        /// <returns>True if success</returns>
        /// <remarks>The Mifare application directory indicates the range of sectors used for NDEF. Normally
        /// these begin at sector 1.</remarks>
        public bool TryReadNdefMessage(out NdefMessage message)
        {
            message = new NdefMessage();

            // Get the NDEF application from the directory
            MifareDirectory? directory = MifareDirectory.LoadFromCard(this);
            MifareDirectoryEntry? ndefEntry = directory?.TryGetApplication(NfcNdefId);
            if (ndefEntry == null)
            {
                return false;
            }

            // Read the first NDEF data block
            IEnumerator<byte> dataBlocks = ndefEntry.GetAllDataBlocks().GetEnumerator();
            dataBlocks.MoveNext();
            var authOk = AuthenticateBlockKeyA(StaticDefaultBlocksNdefKeyA, dataBlocks.Current);
            authOk &= ReadDataBlock(dataBlocks.Current);
            if (!authOk)
            {
                return false;
            }

            var (start, size) = NdefMessage.GetStartSizeNdef(Data.AsSpan());
            if ((start < 0) || (size < 0))
            {
                return false;
            }

            // calculate the number of blocks to read, which must fit within the application
            int blocksToRead = (size + start + BytesPerBlock - 1) / BytesPerBlock;
            if (ndefEntry.NumberOfBlocks < blocksToRead)
            {
                return false;
            }

            Span<byte> card = new byte[blocksToRead * BytesPerBlock];
            Data.CopyTo(card);

            for (int byteIndex = BytesPerBlock; byteIndex < size + start; byteIndex += BytesPerBlock)
            {
                if (!dataBlocks.MoveNext())
                {
                    // this "can't happen" because we checked the size above
                    return false;
                }

                if (!AuthenticateBlockKeyA(StaticDefaultBlocksNdefKeyA, dataBlocks.Current) ||
                    !ReadDataBlock(dataBlocks.Current))
                {
                    return false;
                }

                Data.CopyTo(card.Slice(byteIndex));
            }

            var ndef = NdefMessage.ExtractMessage(card);

            try
            {
                message = new NdefMessage(ndef);
            }
            catch (Exception)
            {
                // Catching all exceptions as quite a lot can happen
                // This is checking if a message is valid or not
                return false;
            }

            return true;
        }

        private bool AuthenticateBlockKeyA(byte[] keyA, byte block)
        {
            byte[] oldKeyA = new byte[6];
            if (KeyA is not object)
            {
                KeyA = new byte[6];
            }

            if (KeyA.Length != 6)
            {
                throw new ArgumentException($"Key A can only be 6 bytes");
            }

            KeyA.CopyTo(oldKeyA, 0);
            keyA.CopyTo(KeyA, 0);
            BlockNumber = block;
            Command = MifareCardCommand.AuthenticationA;
            var ret = RunMifareCardCommand();
            oldKeyA.CopyTo(KeyA, 0);

            return ret >= 0;
        }

        private bool AuthenticateBlockKeyB(byte[] keyB, byte block)
        {
            byte[] oldKeyB = new byte[6];
            if (KeyB is not object)
            {
                KeyB = new byte[6];
            }

            if (KeyB.Length != 6)
            {
                throw new ArgumentException($"Key B can only be 6 bytes");
            }

            KeyB.CopyTo(oldKeyB, 0);
            keyB.CopyTo(KeyB, 0);
            BlockNumber = block;
            Command = MifareCardCommand.AuthenticationB;
            var ret = RunMifareCardCommand();
            oldKeyB.CopyTo(KeyB, 0);

            return ret >= 0;
        }
    }
}
