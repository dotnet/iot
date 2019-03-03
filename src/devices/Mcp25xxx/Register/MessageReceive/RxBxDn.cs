// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

namespace Iot.Device.Mcp25xxx.Register.MessageReceive
{
    /// <summary>
    /// Receive Buffer Data Byte Register.
    /// </summary>
    public class RxBxDn
    {
        /// <summary>
        /// Initializes a new instance of the RxBxDn class.
        /// </summary>
        /// <param name="rxBufferNumber">Receive Buffer Number.</param>
        /// <param name="index">Index of data.  Must be a value of 0 - 7.</param>
        /// <param name="data">Receive Buffer Data Field Bytes.</param>
        public RxBxDn(RxBufferNumber rxBufferNumber, int index, byte data)
        {
            RxBufferNumber = rxBufferNumber;

            // TODO: Add range check.

            Index = index;
            Data = data;
        }

        /// <summary>
        /// Receive Buffer Number.
        /// </summary>
        public RxBufferNumber RxBufferNumber { get; set; }

        /// <summary>
        /// Index of data.  Must be a value of 0 - 7.
        /// </summary>
        public int Index { get; }

        /// <summary>
        /// Receive Buffer Data Field Bytes.
        /// </summary>
        public byte Data { get; }
    }
}
