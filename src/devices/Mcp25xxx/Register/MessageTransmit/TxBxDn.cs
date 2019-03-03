// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

namespace Iot.Device.Mcp25xxx.Register.MessageTransmit
{
    /// <summary>
    /// Transmit Buffer Data Byte Register.
    /// </summary>
    public class TxBxDn
    {
        /// <summary>
        /// Initializes a new instance of the TxBxDn class.
        /// </summary>
        /// <param name="txBufferNumber">Transmit Buffer Number.</param>
        /// <param name="index">Index of data.  Must be a value of 0 - 7.</param>
        /// <param name="data">Transmit Buffer Data Field Bytes.</param>
        public TxBxDn(TxBufferNumber txBufferNumber, int index, byte data)
        {
            TxBufferNumber = txBufferNumber;
            // TODO: Add range check.

            Index = index;
            Data = data;
        }

        /// <summary>
        /// Transmit Buffer Number.
        /// </summary>
        public TxBufferNumber TxBufferNumber { get; set; }

        /// <summary>
        /// Index of data.  Must be a value of 0 - 7.
        /// </summary>
        public int Index { get; }

        /// <summary>
        /// Transmit Buffer Data Field Bytes.
        /// </summary>
        public byte Data { get; }
    }
}
