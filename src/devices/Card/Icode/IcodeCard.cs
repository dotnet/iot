// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System;
using System.Linq;
using System.Collections.Generic;
using Iot.Device.Common;
using Microsoft.Extensions.Logging;
using Iot.Device.Card.Mifare;

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

        /// <summary>
        /// The tag number detected by the reader
        /// </summary>
        public byte Target { get; set; }

        /// <summary>
        /// The command to execute on the card
        /// </summary>
        public IcodeCardCommand Command { get; set; }

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
        /// </summary>
        /// <param name="buffer">The buffer to process</param>
        /// <param name="crc">The CRC, Must be a 2 bytes buffer</param>
        public void CalculateCrc15693(ReadOnlySpan<byte> buffer, Span<byte> crc)
        {
            if (crc.Length != 2)
            {
                throw new ArgumentException($"Value must be 2 bytes.", nameof(crc));
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
        public int RunIcodeCardCommand()
        {
            byte[] requestData = Serialize();
            byte[] dataOut = new byte[_responseSize];

            var ret = _rfid.Transceive(Target, requestData, dataOut.AsSpan(), NfcProtocol.Iso15693);
            _logger.LogDebug($"{nameof(RunIcodeCardCommand)}: {Command}, Target: {Target}, Data: {BitConverter.ToString(Serialize())}, Success: {ret}, Dataout: {BitConverter.ToString(dataOut)}");
            if (ret > 0)
            {
                Data = dataOut;
            }

            return ret;
        }

        /// <summary>
        /// Depending on the command, serialize the needed data
        /// Reading data will just serialize the command
        /// Writing data will serialize the data as well
        /// </summary>
        /// <returns>The serialized bits</returns>
        private byte[] Serialize()
        {
            byte[]? ser = null;
            switch (Command)
            {
                case IcodeCardCommand.ReadSingleBlock:
                    ser = new byte[2 + 8 + 1];
                    ser[0] = 0x22;
                    ser[1] = (byte)Command;
                    ser[10] = BlockNumber;
                    Uid?.CopyTo(ser, 2);
                    _responseSize = 5;
                    return ser;
                case IcodeCardCommand.WriteSingleBlock:
                    ser = new byte[2 + 8 + 1 + 4];
                    ser[0] = 0x22;
                    ser[1] = (byte)Command;
                    ser[10] = BlockNumber;
                    Uid?.CopyTo(ser, 2);
                    Data.CopyTo(ser, 11);
                    _responseSize = 2;
                    return ser;
                case IcodeCardCommand.LockBlock:
                    ser = new byte[2 + 8 + 1];
                    ser[0] = 0x22;
                    ser[1] = (byte)Command;
                    ser[10] = BlockNumber;
                    Uid?.CopyTo(ser, 2);
                    _responseSize = 2;
                    return ser;
                case IcodeCardCommand.ReadMultipleBlocks:
                    ser = new byte[2 + 8 + 2];
                    ser[0] = 0x22;
                    ser[1] = (byte)Command;
                    ser[10] = BlockNumber;
                    ser[11] = BlockCount;
                    Uid?.CopyTo(ser, 2);
                    _responseSize = (ushort)(1 + (BlockCount + 1) * 4);
                    return ser;
                case IcodeCardCommand.WriteMultipleBlocks:
                    ser = new byte[2 + 8 + 2 + Data.Length];
                    ser[0] = 0x22;
                    ser[1] = (byte)Command;
                    ser[10] = BlockNumber;
                    ser[11] = (byte)(Data.Length / 4);
                    Uid?.CopyTo(ser, 2);
                    Data.CopyTo(ser, 12);
                    _responseSize = 2;
                    return ser;
                case IcodeCardCommand.StayQuiet:
                case IcodeCardCommand.Select:
                case IcodeCardCommand.ResettoRead:
                case IcodeCardCommand.LockAFI:
                case IcodeCardCommand.LockDSFID:
                    ser = new byte[2 + 8];
                    ser[0] = 0x22;
                    ser[1] = (byte)Command;
                    Uid?.CopyTo(ser, 2);
                    _responseSize = 2;
                    return ser;
                case IcodeCardCommand.GetSystemInformation:
                    ser = new byte[2 + 8];
                    ser[0] = 0x22;
                    ser[1] = (byte)Command;
                    Uid?.CopyTo(ser, 2);
                    _responseSize = 15;
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
            Command = IcodeCardCommand.ReadSingleBlock;
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
            BlockNumber = block;
            Command = IcodeCardCommand.WriteSingleBlock;
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
            Command = IcodeCardCommand.LockBlock;
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
            Command = IcodeCardCommand.ReadMultipleBlocks;
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
            BlockNumber = block;
            Command = IcodeCardCommand.WriteMultipleBlocks;
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

            Command = IcodeCardCommand.StayQuiet;
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
            Command = IcodeCardCommand.Select;
            var ret = RunIcodeCardCommand();
            return ret >= 0;
        }

        /// <summary>
        /// Let the VICC stay ready status
        /// </summary>
        /// <returns>True if success. This only means whether the communication between VCD and VICC is successful or not</returns>
        public bool ResettoRead()
        {
            Command = IcodeCardCommand.ResettoRead;
            var ret = RunIcodeCardCommand();
            return ret >= 0;
        }

        /// <summary>
        /// Lock the AFI
        /// </summary>
        /// <returns>True if success. This only means whether the communication between VCD and VICC is successful or not</returns>
        public bool LockAFI()
        {
            Command = IcodeCardCommand.LockAFI;
            var ret = RunIcodeCardCommand();
            return ret >= 0;
        }

        /// <summary>
        /// Lock the DSFID
        /// </summary>
        /// <returns>True if success. This only means whether the communication between VCD and VICC is successful or not</returns>
        public bool LockDSFID()
        {
            Command = IcodeCardCommand.LockDSFID;
            var ret = RunIcodeCardCommand();
            return ret >= 0;
        }

        /// <summary>
        /// Get the SystemInformation such as DSFID,AFI,Capacity of VICC
        /// </summary>
        /// <returns>True if success. This only means whether the communication between VCD and VICC is successful or not</returns>
        public bool GetSystemInformation()
        {
            Command = IcodeCardCommand.GetSystemInformation;
            var ret = RunIcodeCardCommand();
            return ret >= 0;
        }
    }
}
