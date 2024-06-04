// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System;
using System.Buffers.Binary;
using Iot.Device.Common;
using Iot.Device.Ndef;
using Microsoft.Extensions.Logging;

namespace Iot.Device.Card.Ultralight
{
    /// <summary>
    /// A Ultralight card class
    /// </summary>
    public class UltralightCard
    {
        /// <summary>
        /// Default password used for write and read
        /// </summary>
        public static readonly byte[] DefaultPassword = new byte[4] { 0xFF, 0xFF, 0xFF, 0xFF };

        // This is the actual RFID reader
        private readonly CardTransceiver _rfid;
        private readonly ILogger _logger;
        private byte[]? _pack;

        private byte _endAddress;

        /// <summary>
        /// The tag number detected by the reader, only 1 or 2
        /// </summary>
        public byte Target { get; set; }

        /// <summary>
        /// The NDEF capacity in bytes
        /// </summary>
        public int NdefCapacity { get; internal set; }

        /// <summary>
        /// The type of card
        /// </summary>
        public UltralightCardType UltralightCardType { get; internal set; }

        /// <summary>
        /// The block number to authenticate or read or write
        /// </summary>
        public byte BlockNumber { get; set; }

        /// <summary>
        /// The Data which has been read or to write for the specific block
        /// </summary>
        public byte[] Data { get; set; } = new byte[0];

        /// <summary>
        /// The command to execute on the card
        /// </summary>
        public UltralightCommand Command { get; set; }

        /// <summary>
        /// The counter to read or increment
        /// </summary>
        public byte Counter { get; set; }

        /// <summary>
        /// Authentication key
        /// </summary>
        public byte[] AuthenticationKey { get; set; } = DefaultPassword;

        /// <summary>
        /// UUID is the Serial Number, called MAC sometimes
        /// </summary>
        public byte[]? SerialNumber { get; set; }

        /// <summary>
        /// Reselect the card after a card command fails
        /// After an error, the card will not respond to any further commands
        /// until it is reselected. If this property is false, the caller
        /// is responsible for calling ReselectCard when RunUltralightCommand
        /// returns an error (-1).
        /// </summary>
        public bool ReselectAfterError { get; set; } = false;

        /// <summary>
        /// Check if this is a Ultralight card type
        /// </summary>
        /// <param name="ATQA">The ATQA</param>
        /// <param name="SAK">The SAK</param>
        /// <returns>True if this is an Ultralight</returns>
        public static bool IsUltralightCard(ushort ATQA, byte SAK) => (ATQA == 0x0044) && (SAK == 0);

        /// <summary>
        /// Constructor for Ultralight
        /// </summary>
        /// <param name="rfid">A card transceiver class</param>
        /// <param name="target">The target number as some card readers attribute one</param>
        public UltralightCard(CardTransceiver rfid, byte target)
        {
            _logger = this.GetCurrentClassLogger();
            _rfid = rfid;
            Target = target;
            // Try to get the version, if not sucessful than it's one of the early model or C
            // See https://stackmirror.com/questions/37002498
            GetVersion();
            if ((Data != null) && (Data.Length == 8))
            {
                if ((Data[2] == 0x04) && (Data[3] == 0x01) && (Data[4] == 0x01) && (Data[5] == 0x00) && (Data[6] == 0x0B))
                {
                    NdefCapacity = 48;
                    UltralightCardType = UltralightCardType.UltralightNtag210;
                }
                else if ((Data[2] == 0x04) && (Data[3] == 0x01) && (Data[4] == 0x01) && (Data[5] == 0x00) && (Data[6] == 0x0E))
                {
                    NdefCapacity = 128;
                    UltralightCardType = UltralightCardType.UltralightNtag212;
                }
                else if ((Data[2] == 0x04) && (Data[3] == 0x02) && (Data[4] == 0x01) && (Data[5] == 0x00) && (Data[6] == 0x0F))
                {
                    NdefCapacity = 144;
                    UltralightCardType = UltralightCardType.UltralightNtag213;
                }
                else if ((Data[2] == 0x04) && (Data[3] == 0x04) && (Data[4] == 0x01) && (Data[5] == 0x00) && (Data[6] == 0x0F))
                {
                    NdefCapacity = 144;
                    UltralightCardType = UltralightCardType.UltralightNtag213F;
                }
                else if ((Data[2] == 0x04) && (Data[3] == 0x02) && (Data[4] == 0x01) && (Data[5] == 0x00) && (Data[6] == 0x11))
                {
                    NdefCapacity = 504;
                    UltralightCardType = UltralightCardType.UltralightNtag215;
                }
                else if ((Data[2] == 0x04) && (Data[3] == 0x02) && (Data[4] == 0x01) && (Data[5] == 0x00) && (Data[6] == 0x13))
                {
                    NdefCapacity = 888;
                    UltralightCardType = UltralightCardType.UltralightNtag216;
                }
                else if ((Data[2] == 0x04) && (Data[3] == 0x04) && (Data[4] == 0x01) && (Data[5] == 0x00) && (Data[6] == 0x13))
                {
                    NdefCapacity = 888;
                    UltralightCardType = UltralightCardType.UltralightNtag216F;
                }
                else if ((Data[2] == 0x04) && (Data[3] == 0x02) && (Data[4] == 0x01) && (Data[5] == 0x01) && (Data[6] == 0x13))
                {
                    NdefCapacity = 888;
                    UltralightCardType = UltralightCardType.UltralightNtagI2cNT3H1101;
                }
                else if ((Data[2] == 0x04) && (Data[3] == 0x05) && (Data[4] == 0x02) && (Data[5] == 0x01) && (Data[6] == 0x13))
                {
                    NdefCapacity = 888;
                    UltralightCardType = UltralightCardType.UltralightNtagI2cNT3H1101W0;
                }
                else if ((Data[2] == 0x04) && (Data[3] == 0x05) && (Data[4] == 0x02) && (Data[5] == 0x02) && (Data[6] == 0x13))
                {
                    NdefCapacity = 888;
                    UltralightCardType = UltralightCardType.UltralightNtagI2cNT3H2111W0;
                }
                else if ((Data[2] == 0x04) && (Data[3] == 0x02) && (Data[4] == 0x01) && (Data[5] == 0x01) && (Data[6] == 0x15))
                {
                    NdefCapacity = 1912;
                    UltralightCardType = UltralightCardType.UltralightNtagI2cNT3H2101;
                }
                else if ((Data[2] == 0x04) && (Data[3] == 0x05) && (Data[4] == 0x02) && (Data[5] == 0x01) && (Data[6] == 0x15))
                {
                    NdefCapacity = 1912;
                    UltralightCardType = UltralightCardType.UltralightNtagI2cNT3H1201W0;
                }
                else if ((Data[2] == 0x04) && (Data[3] == 0x05) && (Data[4] == 0x02) && (Data[5] == 0x02) && (Data[6] == 0x15))
                {
                    NdefCapacity = 1912;
                    UltralightCardType = UltralightCardType.UltralightNtagI2cNT3H2211W0;
                }
                else if ((Data[2] == 0x03) && (Data[3] == 0x01) && (Data[4] == 0x01) && (Data[5] == 0x00) && (Data[6] == 0x0B))
                {
                    NdefCapacity = 48;
                    UltralightCardType = UltralightCardType.UltralightEV1MF0UL1101;
                }
                else if ((Data[2] == 0x03) && (Data[3] == 0x02) && (Data[4] == 0x01) && (Data[5] == 0x00) && (Data[6] == 0x0B))
                {
                    NdefCapacity = 48;
                    UltralightCardType = UltralightCardType.UltralightEV1MF0ULH1101;
                }
                else if ((Data[2] == 0x03) && (Data[3] == 0x01) && (Data[4] == 0x01) && (Data[5] == 0x00) && (Data[6] == 0x0E))
                {
                    NdefCapacity = 128;
                    UltralightCardType = UltralightCardType.UltralightEV1MF0UL2101;
                }
                else if ((Data[2] == 0x03) && (Data[3] == 0x02) && (Data[4] == 0x01) && (Data[5] == 0x00) && (Data[6] == 0x0E))
                {
                    NdefCapacity = 128;
                    UltralightCardType = UltralightCardType.UltralightEV1MF0ULH2101;
                }
                else if ((Data[2] == 0x21) && (Data[3] == 0x01) && (Data[4] == 0x01) && (Data[5] == 0x00) && (Data[6] == 0x0E))
                {
                    NdefCapacity = 128;
                    UltralightCardType = UltralightCardType.UltralightNtag203;
                }
                else
                {
                    NdefCapacity = 1 << (Data[6] >> 1);
                }
            }
            else
            {
                // Check if AuthenticationPart1 returns something
                Command = UltralightCommand.ThreeDsAuthenticationPart1;
                var res = RunUltralightCommand();
                if (res > 0)
                {
                    NdefCapacity = 144;
                    UltralightCardType = UltralightCardType.UltralightC;
                }
                else
                {
                    // Then it's either a Ultralight or 203. Read block 41
                    Command = UltralightCommand.Read16Bytes;
                    res = RunUltralightCommand();
                    if (res > 0)
                    {
                        NdefCapacity = 137;
                        UltralightCardType = UltralightCardType.UltralightNtag203;
                    }
                    else
                    {
                        NdefCapacity = 144;
                        UltralightCardType = UltralightCardType.MifareUltralight;
                    }
                }
            }
        }

        /// <summary>
        /// Get the version data
        /// </summary>
        /// <returns>Empty byte array if error, otherwise a 8 bytes array</returns>
        public byte[] GetVersion()
        {
            Command = UltralightCommand.GetVersion;
            var res = RunUltralightCommand();
            if ((res == 8) && (Data is object))
            {
                return Data;
            }

            return new byte[0];
        }

        /// <summary>
        /// Read at once multiple pages blocks of 4 bytes
        /// </summary>
        /// <param name="startPage">The start block</param>
        /// <param name="endPage">The end block</param>
        /// <returns>A buffer with the read bytes</returns>
        public byte[] ReadFast(byte startPage, byte endPage)
        {
            BlockNumber = startPage < endPage ? startPage : endPage;
            _endAddress = startPage > endPage ? startPage : endPage;
            Command = UltralightCommand.ReadFast;
            var ret = RunUltralightCommand();
            return ret > 0 ? Data! : new byte[0];
        }

        /// <summary>
        /// Run the last setup command. In case of reading bytes, they are automatically pushed into the Data property
        /// </summary>
        /// <returns>-1 if the process fails otherwise the number of bytes read</returns>
        public int RunUltralightCommand()
        {
            byte[] dataOut = new byte[0];
            bool awaitingData = false;
            if (Command == UltralightCommand.Read16Bytes)
            {
                awaitingData = true;
                dataOut = new byte[16];
            }
            else if (Command == UltralightCommand.ReadSignature)
            {
                awaitingData = true;
                dataOut = new byte[32];
            }
            else if (Command == UltralightCommand.PasswordAuthentication)
            {
                awaitingData = true;
                dataOut = new byte[2];
            }
            else if (Command == UltralightCommand.ReadFast)
            {
                awaitingData = true;
                if ((_endAddress - BlockNumber) < 0)
                {
                    throw new ArgumentException("For fast read, last block has to be more than first block");
                }

                var dataOutLength = (_endAddress - BlockNumber + 1) * 4;
                if (dataOutLength > _rfid.MaximumReadSize)
                {
                    throw new ArgumentException($"Fast read is too large for transceiver - maximum is {_rfid.MaximumReadSize / 4} blocks");
                }

                dataOut = new byte[dataOutLength];
            }
            else if (Command == UltralightCommand.GetVersion)
            {
                awaitingData = true;
                dataOut = new byte[8];
            }
            else if ((Command == UltralightCommand.ThreeDsAuthenticationPart1) || (Command == UltralightCommand.ThreeDsAuthenticationPart2))
            {
                awaitingData = true;
                dataOut = new byte[9];
            }
            else if (Command == UltralightCommand.ReadCounter)
            {
                awaitingData = true;
                dataOut = new byte[3];
            }

            var protocol = (Command == UltralightCommand.WriteCompatible) ? NfcProtocol.Mifare : NfcProtocol.Iso14443_3;
            var ret = _rfid.Transceive(Target, Serialize(), dataOut.AsSpan(), protocol);
            _logger.LogDebug($"{nameof(UltralightCommand)}: {Command}, Target: {Target}, Data: {BitConverter.ToString(Serialize())}, Success: {ret}, Dataout: {BitConverter.ToString(dataOut)}");
            if ((ret > 0) && awaitingData)
            {
                Data = dataOut;
            }

            if (ret < 0 && ReselectAfterError)
            {
                ReselectCard();
            }

            return ret;
        }

        /// <summary>
        /// Check if a page is read only
        /// </summary>
        /// <param name="page">The page number</param>
        /// <returns>True is read only</returns>
        public bool IsPageReadOnly(byte page)
        {
            // Note: to improve this method, a caching mechanism can be put in place
            // In general, this is used to check if something is password protected and not in
            // an intense way
            if (page <= 0x02)
            {
                return true;
            }

            // Read block 2
            // byte 2
            // bit 0 = page 03
            // bit 1 = page 04-09
            // bit 2 = page 0A-0F
            // bit 3 = 03 as well
            // bit 4 = 4
            // bit 8 = 7
            // Byte 3
            // bit 0 = 8
            // bit 7 = 15
            // warning byte2 bit0, 1 and 2 are write once, once status changed, can't be changes again
            if ((page >= 0x3) && page <= 0x7)
            {
                Span<byte> toSend = stackalloc byte[2] { (byte)UltralightCommand.Read16Bytes, 2 };
                Span<byte> dataOut = stackalloc byte[16];
                _rfid.Transceive(Target, toSend, dataOut, NfcProtocol.Iso14443_3);
                return (dataOut[2] & (0b0000_0001 << page)) == (0b0000_0001 << page);
            }
            else if ((page >= 0x08) && (page <= 0x0F))
            {
                Span<byte> toSend = stackalloc byte[2] { (byte)UltralightCommand.Read16Bytes, 2 };
                Span<byte> dataOut = stackalloc byte[16];
                _rfid.Transceive(Target, toSend, dataOut, NfcProtocol.Iso14443_3);
                return (dataOut[3] & (0b0000_0001 << (page - 8))) == (0b0000_0001 << (page - 8));
            }
            else
            {
                Span<byte> toSend = stackalloc byte[2] { (byte)UltralightCommand.Read16Bytes, 0x28 };
                Span<byte> dataOut = stackalloc byte[16];
                switch (UltralightCardType)
                {
                    case UltralightCardType.UltralightNtag203:
                        // Read 0x24
                        toSend[1] = 0x24;
                        _rfid.Transceive(Target, toSend, dataOut, NfcProtocol.Iso14443_3);
                        // byte 0, 1 and 2 are used , 2 pages lock with specific cases
                        if ((page >= 16) && (page <= 31))
                        {
                            var inc = (page - 16) / 2;
                            return (dataOut[0] & (0b0000_0001 << inc)) == (0b0000_0001 << inc);
                        }
                        else if ((page >= 32) && (page <= 35))
                        {
                            var inc = (page - 32) / 2;
                            return (dataOut[1] & (0b0000_0001 << inc)) == (0b0000_0001 << inc);
                        }

                        return true;

                    case UltralightCardType.UltralightNtag213:
                    case UltralightCardType.UltralightNtag213F:
                    case UltralightCardType.UltralightNtag212:
                        // Read 0x28
                        _rfid.Transceive(Target, toSend, dataOut, NfcProtocol.Iso14443_3);
                        // byte 0, 1 and 2 are used , 2 pages lock with specific cases
                        if ((page >= 16) && (page <= 31))
                        {
                            var inc = (page - 16) / 2;
                            return (dataOut[0] & (0b0000_0001 << inc)) == (0b0000_0001 << inc);
                        }
                        else if ((page >= 32) && (page <= 39))
                        {
                            var inc = (page - 32) / 2;
                            return (dataOut[1] & (0b0000_0001 << inc)) == (0b0000_0001 << inc);
                        }

                        return true;
                    case UltralightCardType.UltralightNtag215:
                        // Read 0x82
                        toSend[1] = 0x82;
                        _rfid.Transceive(Target, toSend, dataOut, NfcProtocol.Iso14443_3);
                        // byte 0 and 16 pages blocks
                        if ((page >= 16) && (page <= 129))
                        {
                            var inc = (page - 16) / 16;
                            return (dataOut[0] & (0b0000_0001 << inc)) == (0b0000_0001 << inc);
                        }

                        return true;
                    case UltralightCardType.UltralightNtag216:
                    case UltralightCardType.UltralightNtag216F:
                    case UltralightCardType.UltralightNtagI2cNT3H1101:
                    case UltralightCardType.UltralightNtagI2cNT3H1101W0:
                        // Read 0xE2
                        toSend[1] = 0xE2;
                        _rfid.Transceive(Target, toSend, dataOut, NfcProtocol.Iso14443_3);
                        // byte 0 and 1 for 16 pages blocks
                        if ((page >= 16) && (page <= 143))
                        {
                            var inc = (page - 16) / 16;
                            return (dataOut[0] & (0b0000_0001 << inc)) == (0b0000_0001 << inc);
                        }
                        else if ((page >= 144) && (page <= 225))
                        {
                            var inc = (page - 144) / 16;
                            return (dataOut[1] & (0b0000_0001 << inc)) == (0b0000_0001 << inc);
                        }

                        return true;
                    case UltralightCardType.Unknown:
                    default:
                        // safe side...
                        return true;
                }
            }
        }

        /// <summary>
        /// Get the number of blocks for a specific sector
        /// </summary>
        public int NumberBlocks => UltralightCardType switch
        {
            UltralightCardType.UltralightNtag210 => 16,
            UltralightCardType.UltralightNtag212 => 45,
            UltralightCardType.UltralightNtag213 => 45,
            UltralightCardType.UltralightNtag213F => 45,
            UltralightCardType.UltralightNtag215 => 135,
            UltralightCardType.UltralightNtag216 => 231,
            UltralightCardType.UltralightNtag216F => 231,
            UltralightCardType.UltralightEV1MF0UL1101 => 20,
            UltralightCardType.UltralightEV1MF0ULH1101 => 20,
            UltralightCardType.UltralightEV1MF0UL2101 => 41,
            UltralightCardType.UltralightEV1MF0ULH2101 => 41,
            UltralightCardType.UltralightNtagI2cNT3H1101 => 231,
            UltralightCardType.UltralightNtagI2cNT3H1101W0 => 231,
            UltralightCardType.UltralightNtagI2cNT3H2111W0 => 476 + 9,
            UltralightCardType.UltralightNtagI2cNT3H2101 => 476 + 9,
            UltralightCardType.UltralightNtagI2cNT3H1201W0 => 476 + 9,
            UltralightCardType.UltralightNtagI2cNT3H2211W0 => 255 * 4,
            UltralightCardType.UltralightC => 36,
            UltralightCardType.UltralightNtag203 => 168,
            UltralightCardType.MifareUltralight => 48,
            _ or UltralightCardType.Unknown => 0,
        };

        /// <summary>
        /// Select the card. Needed if authentication or read/write failed
        /// </summary>
        /// <returns>True if success</returns>
        public bool ReselectCard()
        {
            return _rfid.ReselectTarget(Target);
        }

        /// <summary>
        /// Write an NDEF Message
        /// </summary>
        /// <param name="message">The NDEF Message to write</param>
        /// <returns>True if success</returns>
        public bool WriteNdefMessage(NdefMessage message)
        {
            const int BlockSize = 4;

            // We need to add 0x03 then the length on 1 or 2 bytes then the trailer 0xFE
            int messageLengthBytes = message.Length > 254 ? 3 : 1;
            Span<byte> serializedMessage = stackalloc byte[message.Length + 2 + messageLengthBytes];
            if (serializedMessage.Length > NdefCapacity)
            {
                throw new ArgumentOutOfRangeException(nameof(message), $"NDEF message too large, maximum {NdefCapacity} bytes, current size is {serializedMessage.Length} bytes");
            }

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
            // Blocks are 4 bytes
            int nbBlocks = serializedMessage.Length / BlockSize + (serializedMessage.Length % BlockSize > 0 ? 1 : 0);

            int inc = 4;
            for (int block = 0; block < nbBlocks; block++)
            {
                BlockNumber = (byte)(inc + block); // Safe cast, never higher than 255
                Command = UltralightCommand.Write4Bytes;
                if (block * BlockSize + BlockSize <= serializedMessage.Length)
                {
                    Data = serializedMessage.Slice(block * BlockSize, BlockSize).ToArray();
                }
                else
                {
                    Data = new byte[BlockSize];
                    serializedMessage.Slice(block * BlockSize).CopyTo(Data);
                }

                var res = RunUltralightCommand();
                if (res < 0)
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
            BlockNumber = 3;
            Command = UltralightCommand.Read16Bytes;
            var res = RunUltralightCommand();
            if (res < 0)
            {
                return false;
            }

            if ((Data![0] != 0xE1) || (Data[1] != 0x10))
            {
                return false;
            }

            if ((Data[4] == 0x01) && (Data[5] == 0x03) && (Data[6] == 0xA0) && (Data[7] == 0x0C))
            {
                return true;
            }
            else if ((Data[4] == 0x03))
            {
                return true;
            }

            return false;
        }

        /// <summary>
        /// Format the Card to NDEF
        /// </summary>
        /// <param name="authenticationKey">An authentication key if authentication is required.</param>
        /// <returns>True if success</returns>
        public bool FormatNdef(ReadOnlySpan<byte> authenticationKey = default)
        {
            // NDEF formatting is starting on page 3:
            // E1 10 CP 00
            // 03 00 FE 00
            // Early NTAG should be formated a bit differently but this is enough
            // CP = NdefCapacity / 8
            if (authenticationKey != default)
            {
                var authOK = ProcessAuthentication(authenticationKey);
                if (!authOK)
                {
                    return false;
                }
            }

            // Block 3 can only be bitwise modified
            // Once the logic is set to 1, it cannot be set back to 0
            // So read it first and check if the E1 and 10 are correct and skip the write
            // If it is.
            BlockNumber = 3;
            Command = UltralightCommand.Read16Bytes;
            var res = RunUltralightCommand();
            if (res > 0)
            {
                if ((Data![0] != 0xE1) || (Data[1] != 0x10))
                {
                    Command = UltralightCommand.Write4Bytes;
                    Data = new byte[4] { 0xE1, 0x10, (byte)(NdefCapacity / 8), 0x00 };
                    res = RunUltralightCommand();
                    if (res < 0)
                    {
                        return false;
                    }
                }
            }
            else
            {
                return false;
            }

            BlockNumber++;
            Data = new byte[4] { 0x03, 0x00, 0xFE, 0x00 }; // NDEF start marker, length 0, NDEF end marker, empty
            Command = UltralightCommand.Write4Bytes;
            res = RunUltralightCommand();
            if (res < 0)
            {
                return false;
            }

            return true;
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
            // Read page 4
            BlockNumber = 4;
            Command = UltralightCommand.Read16Bytes;
            var res = RunUltralightCommand();
            if (res <= 0)
            {
                return false;
            }

            // Check if it's the old formating and skip it
            // 01:03:A0:0C
            int slice = 0;
            if ((Data![0] == 0x01) && (Data[1] == 0x03) && (Data[2] == 0xA0) && (Data[3] == 0x0C))
            {
                slice = 4;
            }

            var (start, size) = NdefMessage.GetStartSizeNdef(Data.AsSpan(slice));

            if ((start < 0) || (size < 0))
            {
                return false;
            }

            // calculate the size to read
            int blocksToRead = (size + start) / BlockSize + ((size + start) % BlockSize != 0 ? 1 : 0);

            // We would have to read more available data than the capacity
            if ((NdefCapacity + 12) <= blocksToRead * BlockSize)
            {
                return false;
            }

            Span<byte> card = new byte[(blocksToRead + slice / 4) * BlockSize];
            Data.AsSpan(slice).CopyTo(card);

            byte idxCard = 1;
            // Decrease by 1 block if we skip first page
            int block = 8;
            while (idxCard < (blocksToRead + slice / 4))
            {
                BlockNumber = (byte)block; // Safe cast as never more than 255
                Command = UltralightCommand.Read16Bytes;
                res = RunUltralightCommand();
                if (res <= 0)
                {
                    return false;
                }

                Data.CopyTo(card.Slice(idxCard * BlockSize - slice));
                idxCard++;
                // Here, we read 4 blocks of 4, so 16 blocks
                block += 4;
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

        /// <summary>
        /// Process authentication
        /// </summary>
        /// <param name="authenticationKkey">An authentication key</param>
        /// <returns>True if success</returns>
        /// <remarks>Depending on the type of authentication, the process will be done transparently</remarks>
        public bool ProcessAuthentication(ReadOnlySpan<byte> authenticationKkey)
        {
            if (UltralightCardType == UltralightCardType.UltralightC)
            {
                // Not yet implemented, need to implement with 3DS
                throw new NotImplementedException();
            }

            if (authenticationKkey.Length != 4)
            {
                throw new ArgumentException($"Password can only be 4 bytes long");
            }

            AuthenticationKey = authenticationKkey.ToArray();
            Command = UltralightCommand.PasswordAuthentication;
            var res = RunUltralightCommand();
            if (res < 0)
            {
                return false;
            }

            _pack = new byte[2];
            Data!.CopyTo(_pack, 0);
            return true;
        }

        /// <summary>
        /// Get the counter value
        /// </summary>
        /// <param name="counter">A valid counter value, can vary depending on the card. 0xFF will ignore the value and use the one set in Counter</param>
        /// <returns>The counter value or -1 if any error</returns>
        public int GetCounter(byte counter = 0xFF)
        {
            Command = UltralightCommand.ReadCounter;
            Counter = counter != 0xFF ? counter : Counter;
            var res = RunUltralightCommand();
            if (res < 0)
            {
                return -1;
            }

            return Data![0] + (Data[1] << 8) + (Data[2] << 16);
        }

        /// <summary>
        /// Increase a counter by a specified amount
        /// </summary>
        /// <param name="counter">A valid counter value, can vary depending on the card. 0xFF will ignore the value and use the one set in Counter</param>
        /// <param name="increment">The amount to increment the counter. If negative, it will use the value of Data</param>
        /// <returns>True if success</returns>
        public bool IncreaseCounter(byte counter = 0xFF, int increment = -1)
        {
            Command = UltralightCommand.IncreaseCounter;
            Counter = counter != 0xFF ? counter : Counter;
            if (increment >= 0)
            {
                Data = new byte[4];
                BinaryPrimitives.WriteInt32LittleEndian(Data, increment);
            }

            var ret = RunUltralightCommand();
            return ret >= 0;
        }

        /// <summary>
        /// Perform a write using the 16 bytes present in Data on a specific block
        /// </summary>
        /// <param name="block">The block number to write</param>
        /// <returns>True if success</returns>
        /// <remarks>You will need to be authenticated properly before</remarks>
        public bool WriteDataBlock(byte block)
        {
            if (_pack == null)
            {
                var authOK = ProcessAuthentication(AuthenticationKey);
                if (!authOK)
                {
                    return false;
                }
            }

            BlockNumber = block;
            Command = UltralightCommand.Write4Bytes;
            var ret = RunUltralightCommand();
            return ret >= 0;
        }

        /// <summary>
        /// Get the chip signature
        /// </summary>
        /// <returns>The signature or an empty array</returns>
        public byte[] GetSignature()
        {
            Command = UltralightCommand.ReadSignature;
            var res = RunUltralightCommand();
            if (res > 0)
            {
                return Data!;
            }

            return new byte[0];
        }

        /// <summary>
        /// Try to get the configuration
        /// </summary>
        /// <param name="configuration">The detailed configuration</param>
        /// <returns></returns>
        public bool TryGetConfiguration(out Configuration configuration)
        {
            configuration = new();

            // Read the last 4 blocks
            BlockNumber = (byte)(NumberBlocks - 4); // Safe cast as never more than 255
            if (UltralightCardType is UltralightCardType.UltralightNtagI2cNT3H1101 or UltralightCardType.UltralightNtagI2cNT3H1101W0 or UltralightCardType.UltralightNtagI2cNT3H2111W0
                or UltralightCardType.UltralightNtagI2cNT3H2101 or UltralightCardType.UltralightNtagI2cNT3H1201W0 or UltralightCardType.UltralightNtagI2cNT3H2211W0)
            {
                BlockNumber--;
            }
            else if (UltralightCardType is UltralightCardType.UltralightC)
            {
                // There is no configuration for this card
                return false;
            }

            Command = UltralightCommand.Read16Bytes;
            var res = RunUltralightCommand();
            if (res < 0)
            {
                return false;
            }

            configuration.Mirror.MirrorType = (MirrorType)(Data![0] >> 6);
            configuration.Mirror.Position = (byte)((Data[0] >> 4) & 0b0000_0011);
            configuration.IsStrongModulation = ((Data[0] >> 2) & 1) == 1;
            configuration.Mirror.Page = Data[2];
            configuration.Authentication.AuthenticationPageRequirement = Data[3];
            configuration.Authentication.IsReadWriteAuthenticationRequired = ((Data[4] >> 7) & 1) == 1;
            configuration.Authentication.IsWritingLocked = ((Data[4] >> 6) & 1) == 1;
            configuration.IsSleepEnabled = ((Data[0] >> 3) & 1) == 1;
            configuration.FieldDetectPin = (FieldDetectPin)(Data[0] & 0b0000_0011);
            configuration.NfcCounter.IsEnabled = ((Data[4] >> 4) & 1) == 1;
            configuration.NfcCounter.IsPasswordProtected = ((Data[4] >> 3) & 1) == 1;
            configuration.Authentication.MaximumNumberOfPossibleTries = (byte)(Data[4] & 0b0000_0111);
            return true;
        }

        /// <summary>
        /// Write the configuration
        /// </summary>
        /// <param name="configuration">The configuration to write</param>
        /// <returns>True if success</returns>
        /// <remarks>An authentication has to happen and will use the credentials stored</remarks>
        public bool WriteConfiguration(Configuration configuration)
        {
            if (UltralightCardType is UltralightCardType.UltralightC)
            {
                // There is no configuration for this card
                return false;
            }

            var res = ProcessAuthentication(AuthenticationKey);
            if (!res)
            {
                return false;
            }

            var serialized = configuration.Serialize();
            BlockNumber = (byte)(NumberBlocks - 4);
            if (UltralightCardType is UltralightCardType.UltralightNtagI2cNT3H1101 or UltralightCardType.UltralightNtagI2cNT3H1101W0 or UltralightCardType.UltralightNtagI2cNT3H2111W0
                or UltralightCardType.UltralightNtagI2cNT3H2101 or UltralightCardType.UltralightNtagI2cNT3H1201W0 or UltralightCardType.UltralightNtagI2cNT3H2211W0)
            {
                // Those are using different configurations, Configuration registers and session registers
                BlockNumber--;
            }

            Data = new byte[4];
            serialized.AsSpan(0, 4).CopyTo(Data);
            Command = UltralightCommand.Write4Bytes;
            var ret = RunUltralightCommand();
            if (ret < 0)
            {
                return false;
            }

            BlockNumber++;
            serialized.AsSpan(4, 4).CopyTo(Data);
            Command = UltralightCommand.Write4Bytes;
            ret = RunUltralightCommand();
            if (ret < 0)
            {
                return false;
            }

            return true;
        }

        /// <summary>
        /// Set the password, the AuthenticationKey is used as the old password
        /// </summary>
        /// <param name="newAuthenticationKkey">The new authentication key</param>
        /// <returns>True if success</returns>
        public bool SetPassword(ReadOnlySpan<byte> newAuthenticationKkey)
        {
            if (_pack == null)
            {
                var res = ProcessAuthentication(AuthenticationKey);
                if (!res)
                {
                    return false;
                }
            }

            BlockNumber = (byte)(NumberBlocks - 4); // Safe cast as never more than 255
            if (UltralightCardType is UltralightCardType.UltralightNtagI2cNT3H1101 or UltralightCardType.UltralightNtagI2cNT3H1101W0 or UltralightCardType.UltralightNtagI2cNT3H2111W0
                or UltralightCardType.UltralightNtagI2cNT3H2101 or UltralightCardType.UltralightNtagI2cNT3H1201W0 or UltralightCardType.UltralightNtagI2cNT3H2211W0)
            {
                BlockNumber--;
            }
            else if (UltralightCardType is UltralightCardType.UltralightC)
            {
                // Not implemented, this require special operations
                throw new NotImplementedException();
            }

            if (newAuthenticationKkey.Length != 4)
            {
                throw new ArgumentException($"Password can only be 4 bytes long");
            }

            Data = newAuthenticationKkey.ToArray();
            Command = UltralightCommand.Write4Bytes;
            var ret = RunUltralightCommand();
            if (ret < 0)
            {
                return false;
            }

            return ProcessAuthentication(newAuthenticationKkey);
        }

        private byte[] Serialize()
        {
            byte[]? ser = null;
            switch (Command)
            {
                case UltralightCommand.GetVersion:
                    ser = new byte[1] { (byte)Command };
                    break;
                case UltralightCommand.Read16Bytes:
                    ser = new byte[2] { (byte)Command, BlockNumber };
                    break;
                case UltralightCommand.ReadFast:
                    // We're using the current block as start block and _endAddress as end block
                    ser = new byte[3] { (byte)Command, BlockNumber, _endAddress };
                    break;
                case UltralightCommand.WriteCompatible:
                case UltralightCommand.Write4Bytes:
                    if (Data is null)
                    {
                        throw new ArgumentException($"Card is not configured for writing.");
                    }

                    ser = new byte[2 + Data.Length];
                    ser[0] = (byte)Command;
                    ser[1] = BlockNumber;
                    if (Data.Length > 0)
                    {
                        Data.CopyTo(ser, 2);
                    }

                    break;
                case UltralightCommand.ReadCounter:
                    ser = new byte[2] { (byte)Command, Counter };
                    break;
                case UltralightCommand.IncreaseCounter:
                    if (Data is null)
                    {
                        throw new ArgumentException($"Card is not configured for counter increase.");
                    }

                    ser = new byte[6];
                    ser[0] = (byte)Command;
                    ser[1] = Counter;
                    Data.AsSpan(0, 3).CopyTo(ser.AsSpan(2));    // 24-bit increment, fourth byte ignored
                    break;
                case UltralightCommand.PasswordAuthentication:
                    if (AuthenticationKey is null or { Length: not 4 })
                    {
                        throw new ArgumentException($"Authentication key can't be null and has to be 4 bytes.");
                    }

                    ser = new byte[1 + AuthenticationKey.Length];
                    ser[0] = (byte)Command;
                    AuthenticationKey.CopyTo(ser, 1);
                    break;
                case UltralightCommand.ThreeDsAuthenticationPart1:
                case UltralightCommand.ReadSignature:
                    ser = new byte[2] { (byte)Command, 0x00 };
                    break;
                case UltralightCommand.ThreeDsAuthenticationPart2:
                    if (Data is null or { Length: not 16 })
                    {
                        throw new ArgumentException($"Card is not configured for 3DS authentication.");
                    }

                    ser = new byte[17];
                    ser[0] = (byte)Command;
                    Data.CopyTo(ser, 1);
                    break;
                default:
                    throw new ArgumentException("Not a supported command.");
            }

            return ser;
        }
    }
}
