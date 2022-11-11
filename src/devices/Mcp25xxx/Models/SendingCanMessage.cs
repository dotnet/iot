// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System;

namespace Iot.Device.Mcp25xxx.Models
{
    /// <summary>
    /// CAN bus message
    /// </summary>
    public class SendingCanMessage
    {
        /// <summary>
        /// Four bytes CAN id
        /// </summary>
        public byte[] Id { get; }

        /// <summary>
        /// CAN data (max 8 bytes)
        /// </summary>
        public byte[] Data { get; }

        private SendingCanMessage(byte[] id, byte[] data)
        {
            Id = id;
            Data = data;
        }

        /// <summary>
        /// Create new standard CAN message
        /// </summary>
        /// <param name="shortId">Two bytes id</param>
        /// <param name="data">message data max 8 bytes</param>
        public static SendingCanMessage CreateStandard(byte[] shortId, byte[] data)
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
            return new SendingCanMessage(id, data);
        }

        /// <summary>
        /// Create new extended CAN message
        /// </summary>
        /// <param name="id">Four bytes id</param>
        /// <param name="data">message data max 8 bytes</param>
        public static SendingCanMessage CreateExtended(byte[] id, byte[] data)
        {
            if (id.Length != 4)
            {
                throw new ArgumentException($"Id size {id.Length} must be 4 bytes.", nameof(id));
            }

            if (data.Length > 8)
            {
                throw new ArgumentException($"Data size {data.Length} more than 8 bytes.", nameof(data));
            }

            return new SendingCanMessage(id, data);
        }
    }
}
