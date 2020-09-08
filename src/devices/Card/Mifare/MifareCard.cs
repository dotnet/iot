// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using System;
using System.ComponentModel.Design;
using Iot.Device.Ndef;

namespace Iot.Device.Card.Mifare
{
    /// <summary>
    /// A Mifare card class
    /// So far only supports the classical 1K cards
    /// </summary>
    public class MifareCard
    {
        // This is the default key
        private byte[] _keyAStandard = new byte[6] { 0x00, 0x00, 0x00, 0x00, 0x00, 0x00 };
        // this is from https://www.nxp.com/docs/en/application-note/AN1304.pdf
        private byte[] _keyANdefKnown = new byte[6] { 0xA0, 0xA1, 0xA2, 0xA3, 0xA4, 0xA5 };
        private byte[] _keyANdefDataKnown = new byte[6] { 0xD3, 0xF7, 0xD3, 0xF7, 0xD3, 0xF7 };

        private CardTransceiver _rfid;

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
        public byte[] KeyA { get; set; }

        /// <summary>
        /// Key B Used for encryption/decryption
        /// </summary>
        public byte[] KeyB { get; set; }

        /// <summary>
        /// UUID is the Serial Number, called MAC sometimes
        /// </summary>
        public byte[] SerialNumber { get; set; }

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
        public byte[] Data { get; set; }

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
        public int RunMifiCardCommand()
        {
            byte[] dataOut = new byte[0];
            if (Command == MifareCardCommand.Read16Bytes)
            {
                dataOut = new byte[16];
            }

            var ret = _rfid.Transceive(Target, Serialize(), dataOut.AsSpan());
            LogInfo.Log($"{nameof(RunMifiCardCommand)}: {Command}, Target: {Target}, Data: {BitConverter.ToString(Serialize())}, Success: {ret}, Dataout: {BitConverter.ToString(dataOut)}", LogLevel.Debug);
            if ((ret > 0) && (Command == MifareCardCommand.Read16Bytes))
            {
                Data = dataOut;
            }

            return ret;
        }

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
        public (byte b6, byte b7, byte b8) EncodeSectorTailer(AccessSector accessSector)
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
        public (byte b6, byte b7, byte b8) EncodeSectorTailer(byte blockNumber, AccessType accessType)
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
        public (byte b6, byte b7, byte b8) EncodeSectorAndClockTailer(AccessSector accessSector, AccessType[] accessTypes)
        {
            if (accessTypes.Length != 3)
            {
                throw new ArgumentException($"{nameof(accessTypes)} can only be array of 3");
            }

            var tupleRes = EncodeSectorTailer(accessSector);
            byte b6 = tupleRes.Item1;
            byte b7 = tupleRes.Item2;
            byte b8 = tupleRes.Item3;
            for (byte i = 0; i < 3; i++)
            {
                tupleRes = EncodeSectorTailer(i, accessTypes[i]);
                b6 |= tupleRes.Item1;
                b7 |= tupleRes.Item2;
                b8 |= tupleRes.Item3;
            }

            return (b6, b7, b8);
        }

        /// <summary>
        /// Encode with default value the access sector and tailer blocks
        /// </summary>
        /// <returns></returns>
        public (byte b6, byte b7, byte b8) EncodeDefaultSectorAndBlockTailer()
        {
            return (0xFF, 0x07, 0x80);
        }

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
        /// Depending on the command, serialize the needed data
        /// Authentication will serialize the command, the concerned key and
        /// the serial number
        /// Reading data will just serialize the command
        /// Writing data will serialize the data as well
        /// </summary>
        /// <returns>The serialized bits</returns>
        private byte[] Serialize()
        {
            byte[] ser = null;
            switch (Command)
            {
                case MifareCardCommand.AuthenticationA:
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
        /// Format the Card to NDEF
        /// </summary>
        /// <returns></returns>
        public bool FormatNdef()
        {
            int nbBlocks;
            switch (Capacity)
            {
                case MifareCardCapacity.Mifare300:
                case MifareCardCapacity.Unknown:
                    return false;
                case MifareCardCapacity.Mifare1K:
                    nbBlocks = 1024 / 16;
                    break;
                case MifareCardCapacity.Mifare2K:
                    nbBlocks = 2048 / 16;
                    break;
                case MifareCardCapacity.Mifare4K:
                default:
                    nbBlocks = 4096 / 16;
                    break;
            }

            // First write the data for the format
            // All block data coming from https://www.nxp.com/docs/en/application-note/AN1304.pdf page 30+
            var authOk = AuthenticateWithAnyKnownKey(1, true);
            Data = new byte[] { 0x14, 0x01, 0x03, 0xE1, 0x03, 0xE1, 0x03, 0xE1, 0x03, 0xE1, 0x03, 0xE1, 0x03, 0xE1, 0x03, 0xE1 };
            authOk &= WriteDataBlock(1);
            authOk &= AuthenticateWithAnyKnownKey(2, true);
            Data = new byte[] { 0x03, 0xE1, 0x03, 0xE1, 0x03, 0xE1, 0x03, 0xE1, 0x03, 0xE1, 0x03, 0xE1, 0x03, 0xE1, 0x03, 0xE1 };
            authOk &= WriteDataBlock(2);
            authOk &= AuthenticateWithAnyKnownKey(4, true);
            Data = new byte[] { 0x03, 0x00, 0xFE, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00 };
            authOk &= WriteDataBlock(4);

            if (Capacity == MifareCardCapacity.Mifare2K)
            {
                byte block = 16 * 4;
                authOk &= AuthenticateWithAnyKnownKey(block, true);
                Data = new byte[] { 0x14, 0x01, 0x03, 0xE1, 0x03, 0xE1, 0x03, 0xE1, 0x03, 0xE1, 0x03, 0xE1, 0x03, 0xE1, 0x03, 0xE1 };
                authOk &= WriteDataBlock(block);
                block++;
                authOk &= AuthenticateWithAnyKnownKey(block, true);
                Data = new byte[] { 0x03, 0xE1, 0x03, 0xE1, 0x03, 0xE1, 0x03, 0xE1, 0x03, 0xE1, 0x03, 0xE1, 0x03, 0xE1, 0x03, 0xE1 };
                authOk &= WriteDataBlock(block);
                block++;
                authOk &= AuthenticateWithAnyKnownKey(block, true);
                Data = new byte[] { 0x00, 0x05, 0x00, 0x05, 0x00, 0x05, 0x00, 0x05, 0x00, 0x05, 0x00, 0x05, 0x00, 0x05, 0x00, 0x05 };
                authOk &= WriteDataBlock(block);
            }
            else if (Capacity == MifareCardCapacity.Mifare4K)
            {
                byte block = 16 * 4;
                authOk &= AuthenticateWithAnyKnownKey(block, true);
                Data = new byte[] { 0x14, 0x01, 0x03, 0xE1, 0x03, 0xE1, 0x03, 0xE1, 0x03, 0xE1, 0x03, 0xE1, 0x03, 0xE1, 0x03, 0xE1 };
                authOk &= WriteDataBlock(block);
                block++;
                authOk &= AuthenticateWithAnyKnownKey(block, true);
                Data = new byte[] { 0x03, 0xE1, 0x03, 0xE1, 0x03, 0xE1, 0x03, 0xE1, 0x03, 0xE1, 0x03, 0xE1, 0x03, 0xE1, 0x03, 0xE1 };
                authOk &= WriteDataBlock(block);
                block++;
                authOk &= AuthenticateWithAnyKnownKey(block, true);
                Data = new byte[] { 0x03, 0xE1, 0x03, 0xE1, 0x03, 0xE1, 0x03, 0xE1, 0x03, 0xE1, 0x03, 0xE1, 0x03, 0xE1, 0x03, 0xE1 };
                authOk &= WriteDataBlock(block);
            }

            // GBP should be 0xC1 for the MAD sectors and 0x40 for the others for a full read/write access
            for (int block = 0; block < nbBlocks; block++)
            {
                // Safe cast, max is 255
                if (IsSectorBlock((byte)block))
                {
                    var ret = AuthenticateWithAnyKnownKey((byte)(block), true);
                    if (ret)
                    {
                        BlockNumber = (byte)block;
                        Command = MifareCardCommand.Read16Bytes;
                        var readBytes = RunMifiCardCommand();
                        if (readBytes == 16)
                        {
                            byte gpb = 0x40;
                            // Change only the GPB byte
                            if ((block == 0) || (block == 16))
                            {
                                gpb = 0xC1;
                            }

                            Data[9] = gpb;
                            WriteDataBlock((byte)block);
                        }
                    }
                }
            }

            return authOk;
        }

        private bool WriteDataBlock(byte block)
        {
            BlockNumber = block;
            Command = MifareCardCommand.Write16Bytes;
            var ret = RunMifiCardCommand();
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
            message = null;

            int cardSize;
            int cardFullSize;
            switch (Capacity)
            {
                case MifareCardCapacity.Mifare1K:
                    // 15 sectors, 3 blocks, 16 bytes
                    cardSize = 720;
                    cardFullSize = 1024;
                    break;
                case MifareCardCapacity.Mifare2K:
                    // 30 sectors, 3 blocks, 16 bytes
                    cardSize = 1440;
                    cardFullSize = 2048;
                    break;
                case MifareCardCapacity.Mifare4K:
                    // 62 sectors, 3 blocks, 16 bytes
                    cardSize = 2976;
                    cardFullSize = 4096;
                    break;
                case MifareCardCapacity.Mifare300:
                case MifareCardCapacity.Unknown:
                default:
                    return false;
            }

            Span<byte> card = new byte[cardSize];

            KeyB = new byte[6] { 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF };
            // In theory, we're not obliged to read all the card, that can be improved in further implementations
            byte idxCard = 0;
            for (int block = 4; block < cardFullSize / BlockSize; block++)
            {
                // Skip sector blocks, safe cast, max is 255
                if (IsSectorBlock((byte)block))
                {
                    continue;
                }

                // Skip as well sector 16 for 2K and 4K cards
                if ((block == 16 * 4) || (block == 16 * 4 + 1) || (block == 16 * 4 + 2) || (block == 16 * 4 + 3))
                {
                    continue;
                }

                var authOk = AuthenticateWithAnyKnownKey((byte)block);

                // If the authentication is not OK, it doesn't mean that there is an issue
                // You can protect a block and have data in the next one
                if (authOk)
                {
                    BlockNumber = (byte)block;
                    Command = MifareCardCommand.Read16Bytes;
                    var ret = RunMifiCardCommand();
                    // Similar as for the authentication, we can consider it's OK to have reading issues
                    // At this stage, it should not happen but taking the risk
                    if (ret >= 0)
                    {
                        Data.CopyTo(card.Slice(idxCard * BlockSize));
                        idxCard++;
                    }
                }
            }

            var ndef = ExtractMessage(card);

            try
            {
                message = new NdefMessage(ndef);
            }
            catch (Exception)
            {
                return false;
            }

            return true;
        }

        private bool AuthenticateWithAnyKnownKey(byte block, bool keyAOnly = false)
        {
            var authOk = AuthenticateBlock(_keyAStandard, block);
            if (!authOk)
            {
                authOk = AuthenticateBlock(_keyANdefKnown, block);
                if (!authOk)
                {
                    authOk = AuthenticateBlock(_keyANdefDataKnown, block);
                }
            }

            return authOk;
        }

        private bool AuthenticateBlock(byte[] keyA, byte block, bool keyAOnly = false)
        {
            KeyA = keyA;
            BlockNumber = block;
            Command = keyAOnly ? MifareCardCommand.AuthenticationA : MifareCardCommand.AuthenticationB;
            var ret = RunMifiCardCommand();
            if ((ret < 0) && (!keyAOnly))
            {
                // Try another one
                Command = MifareCardCommand.AuthenticationA;
                ret = RunMifiCardCommand();
            }

            return ret >= 0;
        }

        private byte[] ExtractMessage(Span<byte> toExtract)
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
                return null;
            }

            // Now check the size. If 0xFF then encoding is on 3 bytes otherwise just one
            int size = toExtract[idx++];
            if (size == 0xFF)
            {
                size = (toExtract[idx++] << 8) + toExtract[idx++];
            }

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
