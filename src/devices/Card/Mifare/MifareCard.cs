// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System;
using System.Linq;
using Iot.Device.Ndef;

namespace Iot.Device.Card.Mifare
{
    /// <summary>
    /// A Mifare card class
    /// So far only supports the classical 1K cards
    /// </summary>
    public class MifareCard
    {
        // This is the actual RFID reader
        private CardTransceiver _rfid;
        private static readonly byte[] Mifare1KBlock1 = new byte[] { 0x14, 0x01, 0x03, 0xE1, 0x03, 0xE1, 0x03, 0xE1, 0x03, 0xE1, 0x03, 0xE1, 0x03, 0xE1, 0x03, 0xE1 };
        private static readonly byte[] Mifare1KBlock2 = new byte[] { 0x03, 0xE1, 0x03, 0xE1, 0x03, 0xE1, 0x03, 0xE1, 0x03, 0xE1, 0x03, 0xE1, 0x03, 0xE1, 0x03, 0xE1 };
        private static readonly byte[] Mifare1KBlock4 = new byte[] { 0x03, 0x00, 0xFE, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00 };
        private static readonly byte[] Mifare2KBlock64 = new byte[] { 0x14, 0x01, 0x03, 0xE1, 0x03, 0xE1, 0x03, 0xE1, 0x03, 0xE1, 0x03, 0xE1, 0x03, 0xE1, 0x03, 0xE1 };
        private static readonly byte[] Mifare2KBlock66 = new byte[] { 0x00, 0x05, 0x00, 0x05, 0x00, 0x05, 0x00, 0x05, 0x00, 0x05, 0x00, 0x05, 0x00, 0x05, 0x00, 0x05 };
        private static readonly byte[] Mifare4KBlock64 = new byte[] { 0x14, 0x01, 0x03, 0xE1, 0x03, 0xE1, 0x03, 0xE1, 0x03, 0xE1, 0x03, 0xE1, 0x03, 0xE1, 0x03, 0xE1 };
        private static readonly byte[] StaticDefaultKeyA = new byte[6] { 0x00, 0x00, 0x00, 0x00, 0x00, 0x00 };
        private static readonly byte[] StaticDefaultKeyB = new byte[6] { 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF };
        private static readonly byte[] StaticDefaultFirstBlockNdefKeyA = new byte[6] { 0xA0, 0xA1, 0xA2, 0xA3, 0xA4, 0xA5 };
        private static readonly byte[] StaticDefaultBlocksNdefKeyA = new byte[6] { 0xD3, 0xF7, 0xD3, 0xF7, 0xD3, 0xF7 };

        /// <summary>
        /// Default Key A
        /// </summary>
        public static ReadOnlySpan<byte> DefaultKeyA => StaticDefaultKeyA;

        /// <summary>
        /// Default Key B
        /// </summary>
        public static ReadOnlySpan<byte> DefaultKeyB => StaticDefaultKeyB;

        /// <summary>
        /// Default first block Key A for NDEF card
        /// </summary>
        /// <remarks>See https://www.nxp.com/docs/en/application-note/AN1304.pdf for more information</remarks>
        public static ReadOnlySpan<byte> DefaultFirstBlockNdefKeyA => StaticDefaultFirstBlockNdefKeyA;

        /// <summary>
        /// Default block Key A for NDEF card
        /// </summary>
        /// <remarks>See https://www.nxp.com/docs/en/application-note/AN1304.pdf for more information</remarks>
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
        public byte[]? Data { get; set; }

        /// <summary>
        /// Constructor for Mifarecard
        /// </summary>
        /// <param name="rfid">A card transceiver class</param>
        /// <param name="target">The target number as some card readers attribute one</param>
        public MifareCard(CardTransceiver rfid, byte target)
        {
            _rfid = rfid;
            Target = target;
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

            var ret = _rfid.Transceive(Target, Serialize(), dataOut.AsSpan());
            LogInfo.Log($"{nameof(RunMifareCardCommand)}: {Command}, Target: {Target}, Data: {BitConverter.ToString(Serialize())}, Success: {ret}, Dataout: {BitConverter.ToString(dataOut)}", LogLevel.Debug);
            if ((ret > 0) && (Command == MifareCardCommand.Read16Bytes))
            {
                Data = dataOut;
            }

            return ret;
        }

        #region Sector Tailer and Access Type

        private (byte C1a, byte C1b, byte C2a, byte C2b, byte C3a, byte C3b) DecodeSectorTailer(byte blockNumber, byte[] sectorData)
        {
            // Bit      7    6    5    4    3    2    1    0
            // Byte 6 !C23 !C22 !C21 !C20 !C13 !C12 !C11 !C10
            // Byte 7  C13  C12  C11  C10 !C33 !C32 !C31 !C30
            // Byte 8  C33  C32  C31  C30  C23  C22  C21  C20
            // Cab a = access bit and b = block number
            // Extract the C1
            byte c1a = (byte)((~(sectorData[6]) >> blockNumber) & 0b0000_0001);
            byte c1b = (byte)((sectorData[7] >> (4 + blockNumber)) & 0b0000_0001);
            // Extract the C2
            byte c2a = (byte)((sectorData[8] >> blockNumber) & 0b0000_0001);
            byte c2b = (byte)((~(sectorData[6]) >> (4 + blockNumber)) & 0b0000_0001);
            // Extract the C3
            byte c3a = (byte)((~(sectorData[7]) >> blockNumber) & 0b0000_0001);
            byte c3b = (byte)((sectorData[8] >> (4 + blockNumber)) & 0b0000_0001);
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
            blockNumber = (byte)(blockNumber % 4);

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
            byte b6 = (byte)((((~c2) & 0x01) << (4 + blockNumber)) | (((~c1) & 0x01) << blockNumber));
            byte b7 = (byte)(((c1) << (4 + blockNumber)) | (((~c3) & 0x01) << blockNumber));
            byte b8 = (byte)(((c3) << (4 + blockNumber)) | ((c2) << blockNumber));
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
            blockNumber = (byte)(blockNumber % 4);
            if (blockNumber != 3)
            {
                return AccessSector.None;
            }

            var (c1a, c1b, c2a, c2b, c3a, c3b) = DecodeSectorTailer(blockNumber, sectorData);
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
            blockNumber = (byte)(blockNumber % 4);
            if (blockNumber == 3)
            {
                return AccessType.None;
            }

            var (c1a, c1b, c2a, c2b, c3a, c3b) = DecodeSectorTailer(blockNumber, sectorData);
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
        public (byte B6, byte B7, byte B8) EncodeSectorAndClockTailer(AccessSector accessSector, AccessType[] accessTypes)
        {
            if (accessTypes.Length != 3)
            {
                throw new ArgumentException(nameof(accessTypes), "Array must have 3 elements.");
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
        /// From the ATAQ ans SAK data find common card capacity
        /// </summary>
        /// <param name="ATAQ">The ATQA response</param>
        /// <param name="SAK">The SAK response</param>
        public void SetCapacity(ushort ATAQ, byte SAK)
        {
            // Type of Mifare can be partially determined by ATQA and SAK
            // https://www.nxp.com/docs/en/application-note/AN10833.pdf
            // Not complete
            if (ATAQ == 0x0004)
            {
                if (SAK == 0x08)
                {
                    Capacity = MifareCardCapacity.Mifare1K;
                }
                else if (SAK == 0x0009)
                {
                    Capacity = MifareCardCapacity.Mifare300;
                }
            }
            else if (ATAQ == 0x0002)
            {
                if (SAK == 0x18)
                {
                    Capacity = MifareCardCapacity.Mifare4K;
                }
            }
        }

        /// <summary>
        /// Is it a block sector?
        /// </summary>
        /// <param name="blockNumber">Input block number</param>
        /// <returns>True if it is a sector block</returns>
        public bool IsSectorBlock(byte blockNumber)
        {
            switch (Capacity)
            {
                default:
                case MifareCardCapacity.Mifare300:
                case MifareCardCapacity.Mifare1K:
                case MifareCardCapacity.Mifare2K:
                    return blockNumber % 4 == 3;
                case MifareCardCapacity.Mifare4K:
                    if (blockNumber < 128)
                    {
                        return blockNumber % 4 == 3;
                    }
                    else
                    {
                        return blockNumber % 16 == 15;
                    }
            }
        }

        /// <summary>
        /// Get the number of blocks for a specific sector
        /// </summary>
        /// <param name="sectorNumber">Input sector number</param>
        /// <returns>The number of blocks for this specific sector</returns>
        public byte GetNumberBlocks(byte sectorNumber) => sectorNumber < 32 ? 4 : 16;

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
                    if (KeyA is null || SerialNumber is null)
                    {
                        throw new Exception($"Card is not configured for {nameof(MifareCardCommand.AuthenticationA)}.");
                    }

                    ser = new byte[2 + KeyA.Length + SerialNumber.Length];
                    ser[0] = (byte)Command;
                    ser[1] = BlockNumber;
                    if (KeyA.Length > 0)
                    {
                        KeyA.CopyTo(ser, 2);
                    }

                    if (SerialNumber.Length > 0)
                    {
                        SerialNumber.CopyTo(ser, 2 + KeyA.Length);
                    }

                    return ser;
                case MifareCardCommand.AuthenticationB:
                    if (KeyB is null || SerialNumber is null)
                    {
                        throw new Exception($"Card is not configured for {nameof(MifareCardCommand.AuthenticationB)}.");
                    }

                    ser = new byte[2 + KeyB.Length + SerialNumber.Length];
                    ser[0] = (byte)Command;
                    ser[1] = BlockNumber;
                    if (KeyB.Length > 0)
                    {
                        KeyB.CopyTo(ser, 2);
                    }

                    if (SerialNumber.Length > 0)
                    {
                        SerialNumber.CopyTo(ser, 2 + KeyB.Length);
                    }

                    return ser;
                case MifareCardCommand.Write16Bytes:
                case MifareCardCommand.Write4Bytes:
                    if (Data is null)
                    {
                        throw new Exception($"Card is not configured for {nameof(MifareCardCommand.Write4Bytes)}.");
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
                default:
                    ser = new byte[2];
                    ser[0] = (byte)Command;
                    ser[1] = BlockNumber;
                    return ser;
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

            byte sectorTailer = (byte)(sector >= 32 ? 32 * 4 + (sector - 32) * 16 + 15 : sector * 4 + 3);
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

            byte firstBlock = (byte)(sector >= 32 ? 32 * 4 + (sector - 32) * 16 : sector * 4);
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
        /// Format the Card to NDEF
        /// </summary>
        /// <param name="keyB">The key B to be used for formatting, if empty, will use the default key B</param>
        /// <returns>True if success</returns>
        public bool FormatNdef(ReadOnlySpan<byte> keyB = default)
        {
            if (Capacity is not MifareCardCapacity.Mifare1K or MifareCardCapacity.Mifare2K or MifareCardCapacity.Mifare4K)
            {
                throw new ArgumentException($"Only Mifare card classic are supported with capacity of 1K, 2K and 4K");
            }

            int nbBlocks = GetNumberBlocks();

            byte[] keyFormat = keyB.Length == 0 ? StaticDefaultKeyB : keyB.ToArray();
            if (keyFormat.Length != 6)
            {
                throw new ArgumentException($"{nameof(keyB)} can only be empty or 6 bytes length");
            }

            // First write the data for the format
            // All block data coming from https://www.nxp.com/docs/en/application-note/AN1304.pdf page 30+
            var authOk = AuthenticateBlockKeyB(keyFormat, 1);
            Data = Mifare1KBlock1;
            authOk &= WriteDataBlock(1);
            authOk &= AuthenticateBlockKeyB(keyFormat, 2);
            Data = Mifare1KBlock2;
            authOk &= WriteDataBlock(2);
            authOk &= AuthenticateBlockKeyB(keyFormat, 3);
            Data = new byte[] { 0xA0, 0xA1, 0xA2, 0xA3, 0xA4, 0xA5, 0x78, 0x77, 088, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00 };
            Data[9] = Capacity == MifareCardCapacity.Mifare1K ? 0xC1 : 0xC2;
            keyFormat.CopyTo(Data, 10);
            authOk &= WriteDataBlock(3);
            authOk &= AuthenticateBlockKeyB(keyFormat, 4);
            Data = Mifare1KBlock4;
            authOk &= WriteDataBlock(4);

            if (Capacity == MifareCardCapacity.Mifare2K)
            {
                byte block = 16 * 4;
                authOk &= AuthenticateBlockKeyB(keyFormat, block);
                Data = Mifare2KBlock64;
                authOk &= WriteDataBlock(block);
                block++;
                authOk &= AuthenticateBlockKeyB(keyFormat, block);
                Data = Mifare1KBlock2; // 65 is same as 2
                authOk &= WriteDataBlock(block);
                block++;
                authOk &= AuthenticateBlockKeyB(keyFormat, block);
                Data = Mifare2KBlock66;
                authOk &= WriteDataBlock(block);
                authOk &= AuthenticateBlockKeyB(keyFormat, block);
                Data = new byte[] { 0xA0, 0xA1, 0xA2, 0xA3, 0xA4, 0xA5, 0x78, 0x77, 088, 0xC2, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00 };
                keyFormat.CopyTo(Data, 10);
                authOk &= WriteDataBlock(3);
            }
            else if (Capacity == MifareCardCapacity.Mifare4K)
            {
                byte block = 16 * 4;
                authOk &= AuthenticateBlockKeyB(keyFormat, block);
                Data = Mifare4KBlock64;
                authOk &= WriteDataBlock(block);
                block++;
                authOk &= AuthenticateBlockKeyB(keyFormat, block);
                Data = Mifare1KBlock2; // 65 is same as 2
                authOk &= WriteDataBlock(block);
                block++;
                authOk &= AuthenticateBlockKeyB(keyFormat, block);
                Data = Mifare1KBlock2; // 66 is same as 2
                authOk &= WriteDataBlock(block);
                authOk &= AuthenticateBlockKeyB(keyFormat, block);
                Data = new byte[] { 0xA0, 0xA1, 0xA2, 0xA3, 0xA4, 0xA5, 0x78, 0x77, 088, 0xC2, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00 };
                keyFormat.CopyTo(Data, 10);
                authOk &= WriteDataBlock(3);
            }

            // GBP should be 0xC1 for the MAD sectors and 0x40 for the others for a full read/write access
            for (int block = 4; block < nbBlocks; block++)
            {
                // Safe cast, max is 255
                if ((IsSectorBlock((byte)block)) && (block != 67))
                {
                    var ret = AuthenticateBlockKeyB(keyFormat, (byte)(block));
                    authOk &= ret;
                    if (ret)
                    {
                        BlockNumber = (byte)block;
                        Command = MifareCardCommand.Write16Bytes;
                        Data[6] = 0x7F;
                        Data[7] = 0x07;
                        Data[8] = 0x88;
                        Data[9] = 0x40;
                        StaticDefaultBlocksNdefKeyA.CopyTo(Data, 6);
                        keyFormat.CopyTo(Data, 10);
                        authOk &= WriteDataBlock((byte)block);
                    }
                }
            }

            return authOk;
        }

        /// <summary>
        /// Write an NDEF Message
        /// </summary>
        /// <param name="message">The NDEF Message to write</param>
        /// <param name="writeKeyA">True to write with Key A</param>
        /// <returns>True if success</returns>
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

            // Number of blocks to write
            int nbBlocks = serializedMessage.Length / 16 + (serializedMessage.Length % 16 > 0 ? 1 : 0);
            switch (Capacity)
            {
                default:
                case MifareCardCapacity.Unknown:
                case MifareCardCapacity.Mifare300:
                    throw new ArgumentException($"Mifare card unknown and 300 are not supported for writing");
                case MifareCardCapacity.Mifare2K:
                    // Total blocks where we can write = 30 * 3 = 90
                    if (nbBlocks > 90)
                    {
                        throw new ArgumentOutOfRangeException($"NNDEF message too large, maximum {90 * 16} bytes, current size is {nbBlocks * 16}");
                    }

                    break;
                case MifareCardCapacity.Mifare4K:
                    // Total blocks where we can write = 30 * 3 + 8 * 15 = 210
                    if (nbBlocks > 213)
                    {
                        throw new ArgumentOutOfRangeException($"NNDEF message too large, maximum {210 * 16} bytes, current size is {nbBlocks * 16}");
                    }

                    break;
                case MifareCardCapacity.Mifare1K:
                    // Total blocks where we can write = 15 * 3 = 45
                    if (nbBlocks > 45)
                    {
                        throw new ArgumentOutOfRangeException($"NNDEF message too large, maximum {45 * 16} bytes, current size is {nbBlocks * 16}");
                    }

                    break;
            }

            int inc = 4;
            bool ret;
            for (int block = 0; block < nbBlocks; block++)
            {
                if (IsSectorBlock((byte)(block + inc)))
                {
                    inc++;
                }

                // Skip as well sector 16 for 2K and 4K cards
                if (block == 16 * 4)
                {
                    inc += 4;
                }

                // Safe cast, max is 255 in all cases
                if (!writeKeyA)
                {
                    ret = AuthenticateBlockKeyB(KeyB!, (byte)(block + inc));
                }
                else
                {
                    ret = AuthenticateBlockKeyA(StaticDefaultBlocksNdefKeyA, (byte)(block + inc));
                }

                if (!ret)
                {
                    return false;
                }

                if (block * 16 + 16 <= serializedMessage.Length)
                {
                    Data = serializedMessage.Slice(block * 16, 16).ToArray();
                }
                else
                {
                    Data = new byte[16];
                    serializedMessage.Slice(block * 16).CopyTo(Data);
                }

                if (!WriteDataBlock((byte)(block + inc)))
                {
                    return false;
                }
            }

            return true;
        }

        /// <summary>
        /// Check if the card formated to NDEF
        /// </summary>
        /// <returns>True if NDEF formated</returns>
        /// <remarks>It will only check the first 2 block of the first sector and that the GPB is set properly</remarks>
        public bool IsFormattedNdef()
        {
            int nbBlocks = GetNumberBlocks();

            if (KeyA is not object or { Length: not 6 })
            {
                throw new ArgumentException($"{nameof(KeyA)} can only be empty or 6 bytes length");
            }

            if (Data is not object or { Length: not 6 })
            {
                Data = new byte[6];
            }

            // First write the data for the format
            // All block data coming from https://www.nxp.com/docs/en/application-note/AN1304.pdf page 30+
            var authOk = AuthenticateBlockKeyA(StaticDefaultFirstBlockNdefKeyA, 1);
            authOk &= ReadDataBlock(1);
            authOk &= Data.SequenceEqual(Mifare1KBlock1);
            authOk &= AuthenticateBlockKeyA(StaticDefaultFirstBlockNdefKeyA, 2);
            authOk &= ReadDataBlock(2);
            authOk &= Data.SequenceEqual(Mifare1KBlock2);

            if (Capacity == MifareCardCapacity.Mifare2K)
            {
                byte block = 16 * 4;
                authOk &= AuthenticateBlockKeyA(StaticDefaultFirstBlockNdefKeyA, block);
                authOk &= ReadDataBlock(block);
                authOk &= Data.SequenceEqual(Mifare2KBlock64);
                block++;
                authOk &= AuthenticateBlockKeyA(StaticDefaultFirstBlockNdefKeyA, block);
                authOk &= ReadDataBlock(block);
                authOk &= Data.SequenceEqual(Mifare1KBlock2); // 65 is same as 2
                block++;
                authOk &= AuthenticateBlockKeyA(StaticDefaultFirstBlockNdefKeyA, block);
                authOk &= ReadDataBlock(block);
                authOk &= Data.SequenceEqual(Mifare2KBlock66);
            }
            else if (Capacity == MifareCardCapacity.Mifare4K)
            {
                byte block = 16 * 4;
                authOk &= AuthenticateBlockKeyA(StaticDefaultFirstBlockNdefKeyA, block);
                authOk &= ReadDataBlock(block);
                authOk &= Data.SequenceEqual(Mifare4KBlock64);
                block++;
                authOk &= AuthenticateBlockKeyA(StaticDefaultFirstBlockNdefKeyA, block);
                authOk &= ReadDataBlock(block);
                authOk &= Data.SequenceEqual(Mifare1KBlock2); // 65 is same as 2
                block++;
                authOk &= AuthenticateBlockKeyA(StaticDefaultFirstBlockNdefKeyA, block);
                authOk &= ReadDataBlock(block);
                authOk &= Data.SequenceEqual(Mifare1KBlock2); // 66 is same as 2
            }

            // GBP should be 0xC1 for the MAD sectors and 0x40 for the others for a full read/write access
            for (int block = 0; block < nbBlocks; block++)
            {
                // Safe cast, max is 255
                if (IsSectorBlock((byte)block))
                {
                    if ((block == 3) || (block == 67))
                    {
                        authOk &= AuthenticateBlockKeyA(StaticDefaultFirstBlockNdefKeyA, (byte)block);
                    }
                    else
                    {
                        authOk &= AuthenticateBlockKeyA(StaticDefaultBlocksNdefKeyA, (byte)block);
                    }

                    if (authOk)
                    {
                        authOk &= ReadDataBlock((byte)block);
                        if (authOk)
                        {
                            byte gpb = 0x40;
                            // Change only the GPB byte
                            if ((block == 3) || (block == 67))
                            {
                                switch (Capacity)
                                {
                                    case MifareCardCapacity.Mifare4K:
                                    case MifareCardCapacity.Mifare2K:
                                        gpb = 0xC2;
                                        break;
                                    case MifareCardCapacity.Unknown:
                                    case MifareCardCapacity.Mifare300:
                                    case MifareCardCapacity.Mifare1K:
                                    default:
                                        gpb = 0xC1;
                                        break;
                                }
                            }

                            authOk &= Data[9] == gpb;
                        }
                    }
                }
            }

            return authOk;
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
        public bool TryReadNdefMessage(out NdefMessage message)
        {
            const int BlockSize = 16;
            message = new NdefMessage();

            int cardSize;
            switch (Capacity)
            {
                case MifareCardCapacity.Mifare1K:
                    // 15 sectors, 3 blocks, 16 bytes
                    cardSize = 720;
                    break;
                case MifareCardCapacity.Mifare2K:
                    // 30 sectors, 3 blocks, 16 bytes
                    cardSize = 1440;
                    break;
                case MifareCardCapacity.Mifare4K:
                    // 62 sectors, 3 blocks, 16 bytes
                    cardSize = 2976;
                    break;
                case MifareCardCapacity.Mifare300:
                case MifareCardCapacity.Unknown:
                default:
                    message = new NdefMessage();
                    return false;
            }

            if (KeyB is not object or { Length: not 6 })
            {
                KeyB = StaticDefaultKeyB;
            }

            // Read block 4 and check for the data, it should be present in this one
            var authOk = AuthenticateBlockKeyA(StaticDefaultBlocksNdefKeyA, 4);
            authOk &= ReadDataBlock(4);

            if (!authOk)
            {
                return false;
            }

            var (start, size) = GetStartSizeNdef(Data.AsSpan());

            if ((start < 0) || (size < 0))
            {
                return false;
            }

            // calculate the size to read
            int blocksToRead = (size + start) / BlockSize + ((size + start) % BlockSize != 0 ? 1 : 0);

            // We would have to read more available data than the capacity
            if (cardSize < blocksToRead * BlockSize)
            {
                return false;
            }

            Span<byte> card = new byte[blocksToRead * 16];
            Data.CopyTo(card);

            byte idxCard = 1;
            int block = 5;
            while (idxCard < blocksToRead)
            {
                // Skip sector blocks, safe cast, max is 255
                if (IsSectorBlock((byte)block))
                {
                    block++;
                    continue;
                }

                // Skip as well sector 16 for 2K and 4K cards
                if ((block == 16 * 4) || (block == 16 * 4 + 1) || (block == 16 * 4 + 2) || (block == 16 * 4 + 3))
                {
                    block++;
                    continue;
                }

                authOk = AuthenticateBlockKeyA(StaticDefaultBlocksNdefKeyA, (byte)block);
                if (!authOk)
                {
                    return false;
                }

                authOk = ReadDataBlock((byte)block);
                if (!authOk)
                {
                    return false;
                }

                Data.CopyTo(card.Slice(idxCard * BlockSize));
                idxCard++;
                block++;
            }

            var ndef = ExtractMessage(card);

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
            if (KeyA is not object or { Length: not 6 })
            {
                KeyA = new byte[6];
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

        private (int Start, int Size) GetStartSizeNdef(Span<byte> toExtract)
        {
            int idx = 0;
            // Check if we have 0x03 so it's a possible, NDEF Entry
            while (idx < toExtract.Length)
            {
                if (toExtract[idx++] == 0x03)
                {
                    break;
                }
            }

            if (idx == toExtract.Length)
            {
                return (-1, -1);
            }

            // Now check the size. If 0xFF then encoding is on 3 bytes otherwise just one
            int size = toExtract[idx++];
            if (idx == toExtract.Length)
            {
                return (idx, -1);
            }

            if (size == 0xFF)
            {
                if (idx + 2 >= toExtract.Length)
                {
                    return (idx, -1);
                }

                size = (toExtract[idx++] << 8) + toExtract[idx++];
            }

            return (idx, size);
        }

        private byte[]? ExtractMessage(Span<byte> toExtract)
        {
            var (idx, size) = GetStartSizeNdef(toExtract);
            // Finally check that the end terminator TLV is 0xFE
            var isRealEnd = toExtract[idx + size] == 0xFE;
            if (!isRealEnd)
            {
                return null;
            }

            // Now we have the real size and we can extract the real buffer
            byte[] toReturn = new byte[size];
            toExtract.Slice(idx, size).CopyTo(toReturn);

            return toReturn;
        }
    }
}
