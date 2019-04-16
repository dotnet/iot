// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using System;

namespace Iot.Device.Nrf24l01
{
    /// <summary>
    /// nRF24L01 Receive Pipe
    /// </summary>
    public class Nrf24l01Pipe
    {
        private readonly byte _pipeID;
        private readonly Nrf24l01 _nrf;

        /// <summary>
        /// Creates a new instance of the Nrf24l01Pipe
        /// </summary>
        /// <param name="nrf">nRF24L01</param>
        /// <param name="pipeID">Pipe ID</param>
        public Nrf24l01Pipe(Nrf24l01 nrf, byte pipeID)
        {
            _nrf = nrf;
            _pipeID = pipeID;
        }

        /// <summary>
        /// Receive Pipe Address
        /// </summary>
        public byte[] Address { get => _nrf.ReadRxAddress(_pipeID); set => _nrf.SetRxAddress(_pipeID, value); }

        /// <summary>
        /// Auto Acknowledgment
        /// </summary>
        public bool AutoAck { get => _nrf.ReadAutoAck(_pipeID); set => _nrf.SetAutoAck(_pipeID, value); }

        /// <summary>
        /// Receive Pipe Payload
        /// </summary>
        public byte Payload { get => _nrf.ReadRxPayload(_pipeID); set => _nrf.SetRxPayload(_pipeID, value); }

        /// <summary>
        /// Enable Pipe
        /// </summary>
        public bool Enable { get => _nrf.ReadRxPipe(_pipeID); set => _nrf.SetRxPipe(_pipeID, value); }
    }
}
