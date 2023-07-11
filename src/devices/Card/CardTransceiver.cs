// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System;
using System.Collections.Generic;
using System.Text;

namespace Iot.Device.Card
{
    /// <summary>
    /// Abstract class implementing a specific Write and Read function
    /// This class allow to transceive information with the card
    /// This class has to be implemented in all RFID/NFC/Card readers
    /// So Mifare cards can be used the same way independent of any reader
    /// </summary>
    public abstract class CardTransceiver
    {
        /// <summary>
        /// This function has to be implemented by all NFC/RFID/Card readers. This function is used in exchange of data with
        /// the reader and the cards.
        /// </summary>
        /// <param name="targetNumber">Some readers have a notion of target number for the cards as they can read multiple ones</param>
        /// <param name="dataToSend">A standardized raw buffer with the command at the position 0 in the array</param>
        /// <param name="dataFromCard">If any data are read from the card, they will be put into this array</param>
        /// <param name="protocol">NFC protocol for this data exchange (e.g., Mifare)</param>
        /// <returns>-1 in case of error, otherwise the number of bytes read and copied into the <paramref name="dataFromCard"/> array</returns>
        public abstract int Transceive(byte targetNumber, ReadOnlySpan<byte> dataToSend, Span<byte> dataFromCard, NfcProtocol protocol);

        /// <summary>
        /// Once you have an authentication operation failing with Mifare cards or a read/write, the card stop.
        /// TYhe only way to have it back is to send the unselect and anti collision.
        /// This function provides this feature
        /// </summary>
        /// <param name="targetNumber">The target number to reselect</param>
        /// <returns>True if success</returns>
        public abstract bool ReselectTarget(byte targetNumber);

        /// <summary>
        /// The maximum number of bytes that can be read from the card in a single transaction,
        /// (excluding CRC). This is constrained by the operating mode as well as transceiver limitations (such as
        /// the size of a FIFO buffer in the transceiver).
        /// </summary>
        public abstract uint MaximumReadSize { get; }

        /// <summary>
        /// The maximum number of bytes that can be written to the card in a single transaction,
        /// (excluding CRC). This is constrained by the operating mode as well as transceiver limitations (such as
        /// the size of a FIFO buffer in the transceiver).
        /// </summary>
        public abstract uint MaximumWriteSize { get; }
    }
}
