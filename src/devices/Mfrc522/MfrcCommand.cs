// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

namespace Iot.Device.Mfrc522
{
    /// <summary>
    /// Command of MFRC522 reader
    /// </summary>
    /// <remarks>Most of the time, the only one you need to use is the Transceive command.
    /// The SendAndReceiveData function is already using some of those commands to allow send and receive scenarios</remarks>
    public enum MfrcCommand
    {
        /// <summary>
        /// Idle : no action, cancels current command execution
        /// </summary>
        Idle = 0x00,

        /// <summary>
        /// Memory: stores 25 bytes into the internal buffer
        /// </summary>
        Memory = 0x01,

        /// <summary>
        /// Generate Random Id: generates a 10-byte random ID number
        /// </summary>
        GenerateRandomId = 0x02,

        /// <summary>
        /// Calculate CRC: activates the CRC coprocessor or performs a self test
        /// </summary>
        CalculateCrc = 0x03,

        /// <summary>
        /// Transmit: transmits data from the FIFO buffer
        /// </summary>
        Transmit = 0x04,

        /// <summary>
        /// No Command Change: no command change, can be used to modify the
        /// CommandReg register bits without affecting the command,
        /// for example, the PowerDown bit
        /// </summary>
        NoCommandChange = 0x07,

        /// <summary>
        /// Receive: activates the receiver circuits
        /// </summary>
        Receive = 0x08,

        /// <summary>
        /// Transceive: transmits data from FIFO buffer to antenna and automatically
        /// activates the receiver after transmission
        /// </summary>
        Transceive = 0x0C,

        /// <summary>
        /// Mifare Authenticate: performs the MIFARE standard authentication as a reader
        /// </summary>
        MifareAuthenticate = 0x0E,

        /// <summary>
        /// Reset Phase: soft resets the MFRC522
        /// </summary>
        ResetPhase = 0x0F,
    }
}
