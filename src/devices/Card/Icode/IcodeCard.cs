// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System;
using Iot.Device.Common;
using Microsoft.Extensions.Logging;

namespace Iot.Device.Card.Icode
{
    /// <summary>
    /// A Icode card class Supports ICODE SLIX,ICODE SLIX2,ICODE DNA,ICODE 3
    /// </summary>
    public class IcodeCard
    {
        private const byte BytesPerBlock = 4;

        // This is the actual RFID reader
        private readonly CardTransceiver _rfid;

        private readonly ILogger _logger;

        // the size of response
        private ushort _responseSize;

        // The command to execute on the card
        private IcodeCardCommand _command;

        /// <summary>
        /// The tag number detected by the reader
        /// </summary>
        public byte Target { get; set; }

        /// <summary>
        /// unique identifier of the card
        /// </summary>
        public byte[]? Uid { get; set; }

        /// <summary>
        /// The storage capacity
        /// </summary>
        public IcodeCardCapacity Capacity { get; set; }

        /// <summary>
        /// The block number to read or write
        /// </summary>
        public byte BlockNumber { get; set; }

        /// <summary>
        /// The block count when read multiple blocks
        /// </summary>
        public byte BlockCount { get; set; }

        /// <summary>
        /// The Data which has been read or to write for the specific block
        /// </summary>
        public byte[] Data { get; set; } = new byte[0];

        /// <summary>
        /// AFI (Application family identifier) represents the type of application targeted by the VCD
        /// </summary>
        public byte Afi { get; set; }

        /// <summary>
        /// The Data storage format identifier indicates how the data is structured in the VICC memory
        /// </summary>
        public byte Dsfid { get; set; }

        /// <summary>
        /// Constructor for IcodeCard
        /// </summary>
        /// <param name="rfid">A card transceiver class</param>
        /// <param name="target">The target number as some card readers attribute one</param>
        public IcodeCard(CardTransceiver rfid, byte target)
        {
            _rfid = rfid;
            Target = target;
            _logger = this.GetCurrentClassLogger();
        }

        /// <summary>
        /// Provide a calculation of CRC for ISO15693
        /// The PN5180 module seems to have implemented crc and does not need to calculate when coding
        /// </summary>
        /// <param name="buffer">The buffer to process</param>
        /// <param name="crc">The CRC, Must be a 2 bytes buffer</param>
        public void CalculateCrcIso15693(ReadOnlySpan<byte> buffer, Span<byte> crc)
        {
            if (crc.Length != 2)
            {
                throw new ArgumentException($"The length of crc must be 2 bytes.", nameof(crc));
            }

            ushort polynomial = 0x8408;
            ushort currentCrc = 0xFFFF;
            // ISO15693-3.pdf
            for (int i = 0; i < buffer.Length; i++)
            {
                currentCrc = (ushort)(currentCrc ^ buffer[i]);
                for (int j = 0; j < 8; j++)
                {
                    if ((currentCrc & 0x0001) != 0)
                    {
                        currentCrc = (ushort)((currentCrc >> 1) ^ polynomial);
                    }
                    else
                    {
                        currentCrc = (ushort)(currentCrc >> 1);
                    }
                }
            }

            currentCrc = (ushort)~currentCrc;
            crc[0] = (byte)(currentCrc & 0xFF);
            crc[1] = (byte)((currentCrc >> 8) & 0xFF);
        }

        /// <summary>
        /// Run the last setup command. In case of reading bytes, they are automatically pushed into the Data property
        /// </summary>
        /// <returns>-1 if the process fails otherwise the number of bytes read</returns>
        private int RunIcodeCardCommand()
        {
            byte[] requestData = Serialize();
            byte[] dataOut = new byte[_responseSize];

            var ret = _rfid.Transceive(Target, requestData, dataOut.AsSpan(), NfcProtocol.Iso15693);
            if (_logger.IsEnabled(LogLevel.Debug))
            {
                _logger.LogDebug($"{nameof(RunIcodeCardCommand)}: {_command}, Target: {Target}, Data: {BitConverter.ToString(requestData)}, Success: {ret}, Dataout: {BitConverter.ToString(dataOut)}");
            }

            if (ret > 0)
            {
                Data = dataOut;
            }

            return ret;
        }

        /// <summary>
        /// Serialize request data according to the protocol
        /// Request format: SOF, Flags, Command code, Parameters (opt.), Data (opt.), CRC16, EOF
        /// </summary>
        /// <returns>The serialized bits</returns>
        private byte[] Serialize()
        {
            byte[]? ser = null;
            switch (_command)
            {
                case IcodeCardCommand.ReadSingleBlock:
                    // Flags(1 byte), Command code(1 byte), UID(8 byte), BlockNumber(1 byte)
                    ser = new byte[2 + 8 + 1];
                    ser[0] = 0x22;
                    ser[1] = (byte)_command;
                    ser[10] = BlockNumber;
                    Uid?.CopyTo(ser, 2);
                    _responseSize = 5;
                    return ser;
                case IcodeCardCommand.WriteSingleBlock:
                    // Flags(1 byte), Command code(1 byte), UID(8 byte), BlockNumber(1 byte),Data to write(4 byte)
                    ser = new byte[2 + 8 + 1 + 4];
                    ser[0] = 0x22;
                    ser[1] = (byte)_command;
                    ser[10] = BlockNumber;
                    Uid?.CopyTo(ser, 2);
                    Data.CopyTo(ser, 11);
                    _responseSize = 2;
                    return ser;
                case IcodeCardCommand.LockBlock:
                    // Flags(1 byte), Command code(1 byte), UID(8 byte), BlockNumber(1 byte)
                    ser = new byte[2 + 8 + 1];
                    ser[0] = 0x22;
                    ser[1] = (byte)_command;
                    ser[10] = BlockNumber;
                    Uid?.CopyTo(ser, 2);
                    _responseSize = 2;
                    return ser;
                case IcodeCardCommand.ReadMultipleBlocks:
                    // Flags(1 byte), Command code(1 byte), UID(8 byte), FirstBlockNumber(1 byte), NumBlocks
                    ser = new byte[2 + 8 + 2];
                    ser[0] = 0x22;
                    ser[1] = (byte)_command;
                    ser[10] = BlockNumber;
                    ser[11] = BlockCount;
                    Uid?.CopyTo(ser, 2);
                    _responseSize = (ushort)(1 + (BlockCount + 1) * 4);
                    return ser;
                case IcodeCardCommand.WriteMultipleBlocks:
                    // Flags(1 byte), Command code(1 byte), UID(8 byte), FirstBlockNumber(1 byte), numBlocks, Data to write
                    ser = new byte[2 + 8 + 2 + Data.Length];
                    ser[0] = 0x22;
                    ser[1] = (byte)_command;
                    ser[10] = BlockNumber;
                    ser[11] = (byte)(Data.Length / 4);
                    Uid?.CopyTo(ser, 2);
                    Data.CopyTo(ser, 12);
                    _responseSize = 2;
                    return ser;
                case IcodeCardCommand.StayQuiet:
                case IcodeCardCommand.Select:
                case IcodeCardCommand.ResettoRead:
                case IcodeCardCommand.LockAfi:
                case IcodeCardCommand.LockDsfid:
                    // Flags(1 byte), Command code(1 byte), UID(8 byte)
                    ser = new byte[2 + 8];
                    ser[0] = 0x22;
                    ser[1] = (byte)_command;
                    Uid?.CopyTo(ser, 2);
                    _responseSize = 2;
                    return ser;
                case IcodeCardCommand.GetSystemInformation:
                    // Flags(1 byte), Command code(1 byte), UID(8 byte)
                    ser = new byte[2 + 8];
                    ser[0] = 0x22;
                    ser[1] = (byte)_command;
                    Uid?.CopyTo(ser, 2);
                    _responseSize = 15;
                    return ser;
                case IcodeCardCommand.WriteAfi:
                    // Flags(1 byte), Command code(1 byte), UID(8 byte), AFI(1 byte)
                    ser = new byte[2 + 8 + 1];
                    ser[0] = 0x22;
                    ser[1] = (byte)_command;
                    ser[10] = Afi;
                    Uid?.CopyTo(ser, 2);
                    _responseSize = 2;
                    return ser;
                case IcodeCardCommand.WriteDsfid:
                    // Flags(1 byte), Command code(1 byte), UID(8 byte), DSFID(1 byte)
                    ser = new byte[2 + 8 + 1];
                    ser[0] = 0x22;
                    ser[1] = (byte)_command;
                    ser[10] = Dsfid;
                    Uid?.CopyTo(ser, 2);
                    _responseSize = 2;
                    return ser;
                default:
                    return new byte[0];
            }
        }

        /// <summary>
        /// Perform a read and place the result into the 4 bytes Data property on a specific block
        /// </summary>
        /// <param name="block">The block number to read</param>
        /// <returns>True if success. This only means whether the communication between VCD and VICC is successful or not </returns>
        public bool ReadSingleBlock(byte block)
        {
            BlockNumber = block;
            _command = IcodeCardCommand.ReadSingleBlock;
            var ret = RunIcodeCardCommand();
            return ret >= 0;
        }

        /// <summary>
        /// Perform a write using the 4 bytes present in Data on a specific block
        /// </summary>
        /// <param name="block">The block number to write</param>
        /// <returns>True if success. This only means whether the communication between VCD and VICC is successful or not</returns>
        public bool WriteSingleBlock(byte block)
        {
            if (Data.Length < 1 || Data.Length > 4)
            {
                _logger.LogDebug("Length of data must be larger than zero and less than or equal four.");
                return false;
            }

            BlockNumber = block;
            _command = IcodeCardCommand.WriteSingleBlock;
            var ret = RunIcodeCardCommand();
            return ret >= 0;
        }

        /// <summary>
        /// Lock a specific block
        /// </summary>
        /// <param name="block">The block number to Lock</param>
        /// <returns>True if success. This only means whether the communication between VCD and VICC is successful or not</returns>
        public bool LockBlock(byte block)
        {
            BlockNumber = block;
            _command = IcodeCardCommand.LockBlock;
            var ret = RunIcodeCardCommand();
            return ret >= 0;
        }

        /// <summary>
        ///  Perform a read and place the result into the Data property on continuous block
        /// </summary>
        /// <param name="block">The start block number to read</param>
        /// <param name="count">Total bolck count to read</param>
        /// <returns>True if success. This only means whether the communication between VCD and VICC is successful or not</returns>
        public bool ReadMultipleBlocks(byte block, byte count)
        {
            BlockNumber = block;
            BlockCount = count;
            _command = IcodeCardCommand.ReadMultipleBlocks;
            var ret = RunIcodeCardCommand();
            return ret >= 0;
        }

        /// <summary>
        /// Perform a write using the Data property on a specific blocks
        /// </summary>
        /// <param name="block">The start block number to write</param>
        /// <returns>True if success. This only means whether the communication between VCD and VICC is successful or not</returns>
        public bool WriteMultipleBlocks(byte block)
        {
            if (Data.Length < 1)
            {
                _logger.LogDebug("Length of data must be larger than zero.");
                return false;
            }

            BlockNumber = block;
            _command = IcodeCardCommand.WriteMultipleBlocks;
            var ret = RunIcodeCardCommand();
            return ret >= 0;
        }

        /// <summary>
        /// Let the VICC stay quiet stauts
        /// </summary>
        /// <returns>True if success. This only means whether the communication between VCD and VICC is successful or not</returns>
        public bool StayQuiet()
        {
            if (Uid == null || Uid.Length != 8)
            {
                _logger.LogDebug("Uid is null or is invalid!");
                return false;
            }

            _command = IcodeCardCommand.StayQuiet;
            var ret = RunIcodeCardCommand();
            return ret >= 0;
        }

        /// <summary>
        /// Select a card
        /// </summary>
        /// <param name="uid">The uid of card</param>
        /// <returns>True if success. This only means whether the communication between VCD and VICC is successful or not</returns>
        public bool Select(byte[] uid)
        {
            if (uid == null || uid.Length != 8)
            {
                _logger.LogDebug("Uid is null or is invalid!");
                return false;
            }

            Uid = uid;
            _command = IcodeCardCommand.Select;
            var ret = RunIcodeCardCommand();
            return ret >= 0;
        }

        /// <summary>
        /// Let the VICC stay ready status
        /// </summary>
        /// <returns>True if success. This only means whether the communication between VCD and VICC is successful or not</returns>
        public bool ResettoRead()
        {
            _command = IcodeCardCommand.ResettoRead;
            var ret = RunIcodeCardCommand();
            return ret >= 0;
        }

        /// <summary>
        /// Write AFI
        /// </summary>
        /// <returns>True if success. This only means whether the communication between VCD and VICC is successful or not</returns>
        public bool WriteAfi()
        {
            _command = IcodeCardCommand.WriteAfi;
            var ret = RunIcodeCardCommand();
            return ret >= 0;
        }

        /// <summary>
        /// Lock the AFI
        /// </summary>
        /// <returns>True if success. This only means whether the communication between VCD and VICC is successful or not</returns>
        public bool LockAfi()
        {
            _command = IcodeCardCommand.LockAfi;
            var ret = RunIcodeCardCommand();
            return ret >= 0;
        }

        /// <summary>
        /// Write DSFID
        /// </summary>
        /// <returns>True if success. This only means whether the communication between VCD and VICC is successful or not</returns>
        public bool WriteDsfid()
        {
            _command = IcodeCardCommand.WriteDsfid;
            var ret = RunIcodeCardCommand();
            return ret >= 0;
        }

        /// <summary>
        /// Lock the DSFID
        /// </summary>
        /// <returns>True if success. This only means whether the communication between VCD and VICC is successful or not</returns>
        public bool LockDsfid()
        {
            _command = IcodeCardCommand.LockDsfid;
            var ret = RunIcodeCardCommand();
            return ret >= 0;
        }

        /// <summary>
        /// Get the SystemInformation such as DSFID,AFI,Capacity of VICC
        /// </summary>
        /// <returns>True if success. This only means whether the communication between VCD and VICC is successful or not</returns>
        public bool GetSystemInformation()
        {
            _command = IcodeCardCommand.GetSystemInformation;
            var ret = RunIcodeCardCommand();
            return ret >= 0;
        }
    }
}
