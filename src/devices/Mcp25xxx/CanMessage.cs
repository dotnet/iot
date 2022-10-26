// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System;
using System.Linq;

namespace Iot.Device.Mcp25xxx
{
    /// <summary>
    /// CAN bus message
    /// </summary>
    public class CanMessage
    {
        /// <summary>
        /// Four bytes CAN id
        /// </summary>
        public byte[] Id { get; }

        /// <summary>
        /// CAN data (max 8 bytes)
        /// </summary>
        public byte[] Data { get; }

        private CanMessage(byte[] id, byte[] data)
        {
            Id = id;
            Data = data;
        }

        /// <summary>
        /// Create new standard CAN message
        /// </summary>
        /// <param name="shortId">Two bytes id</param>
        /// <param name="data">message data max 8 bytes</param>
        public static CanMessage CreateStandard(byte[] shortId, byte[] data)
        {
            if (shortId.Length != 2)
            {
                throw new ArgumentException($"Id size {shortId.Length} must be 2 bytes.", nameof(shortId));
            }

            if (data.Length > 8)
            {
                throw new ArgumentException($"Data size {data.Length} more than 8 bytes.", nameof(data));
            }

            var id = new byte[] { shortId[0], shortId[1], 0, 0 };
            return new CanMessage(id, data);
        }

        /// <summary>
        /// Create new extended CAN message
        /// </summary>
        /// <param name="id">Four bytes id</param>
        /// <param name="data">message data max 8 bytes</param>
        public static CanMessage CreateExtended(byte[] id, byte[] data)
        {
            if (id.Length != 4)
            {
                throw new ArgumentException($"Id size {id.Length} must be 4 bytes.", nameof(id));
            }

            if (data.Length > 8)
            {
                throw new ArgumentException($"Data size {data.Length} more than 8 bytes.", nameof(data));
            }

            return new CanMessage(id, data);
        }

        /// <summary>
        /// Create message from bytes
        /// </summary>
        /// <param name="data">byte array from buffer</param>
        /// <exception cref="ArgumentException"></exception>
        public static CanMessage CreateFromBytes(byte[] data)
        {
            const int messageDataStartIndex = 5;
            if (data.Length < 5)
            {
                throw new ArgumentException($"Data size {data.Length} must be greater then {messageDataStartIndex} bytes.", nameof(data));
            }

            var id = new[] { data[0], data[1], data[2], data[3] };
            var messageLength = data[4] & 0x0F;
            var messageData = data.Skip(messageDataStartIndex).Take(messageLength).ToArray();

            return CreateExtended(id, messageData);
        }
    }
}
