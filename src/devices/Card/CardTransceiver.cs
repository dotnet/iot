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
        /// <returns>-1 in case of error, otherwise the number of bytes read and copied into the <paramref name="dataFromCard"/> array</returns>
        public abstract int Transceive(byte targetNumber, ReadOnlySpan<byte> dataToSend, Span<byte> dataFromCard);
    }
}
