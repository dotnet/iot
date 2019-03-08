// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using System;

namespace Iot.Device.Nrf24l01
{
    /// <summary>
    /// nRF24L01 Data Received Event Args
    /// </summary>
    public class DataReceivedEventArgs : EventArgs
    {
        /// <summary>
        /// nRF24L01 Received Data
        /// </summary>
        public byte[] Data { get; }
        public DataReceivedEventArgs(byte[] data)
        {
            Data = data;
        }
    }
}
