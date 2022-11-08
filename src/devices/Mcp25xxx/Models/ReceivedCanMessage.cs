// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System;
using System.IO;
using System.Linq;

namespace Iot.Device.Mcp25xxx.Models
{
    /// <summary>
    /// CAN bus message
    /// </summary>
    public class ReceivedCanMessage
    {
        /// <summary>
        /// Buffer received this message
        /// </summary>
        public ReceiveBuffer Buffer { get; }

        /// <summary>
        /// Received CAN message
        /// </summary>
        public byte[] RawData { get; }

        /// <summary>
        /// Received CAN message constructor
        /// </summary>
        /// <param name="buffer">buffer received this message</param>
        /// <param name="rawData">data from buffer</param>
        public ReceivedCanMessage(ReceiveBuffer buffer, byte[] rawData)
        {
            Buffer = buffer;
            RawData = rawData;
        }

        /// <summary>
        /// CAN message id
        /// </summary>
        /// <returns>message id</returns>
        /// <exception cref="ArgumentException">Raw data can contain not valid id</exception>
        public byte[] GetId()
        {
            if (RawData.Length < 4)
            {
                throw new InvalidDataException($"Raw data size {RawData.Length} must be greater then 4 bytes.");
            }

            return new[] { RawData[0], RawData[1], RawData[2], RawData[3] };
        }

        /// <summary>
        /// CAN message data
        /// </summary>
        /// <returns>message data (max 8 bytes)</returns>
        /// <exception cref="ArgumentException">Raw data can contain not valid message</exception>
        public byte[] GetData()
        {
            const int messageDataStartIndex = 5;
            if (RawData.Length < messageDataStartIndex)
            {
                throw new InvalidDataException($"Raw data size {RawData.Length} must be greater then {messageDataStartIndex} bytes.");
            }

            var messageLength = RawData[4] & 0x0F;
            return RawData.Skip(messageDataStartIndex).Take(messageLength).ToArray();
        }
    }
}
