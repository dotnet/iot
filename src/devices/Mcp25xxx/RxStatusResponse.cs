// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

namespace Iot.Device.Mcp25xxx
{
    /// <summary>
    /// Response from RX STATUS instruction.
    /// </summary>
    public class RxStatusResponse
    {
        /// <summary>
        /// Initializes a new instance of the RxStatusResponse class.
        /// </summary>
        /// <param name="receivedMessage">
        /// RXxIF (CANINTF) bits are mapped to bits 7 and 6.</param>
        /// <param name="messageTypeReceived">
        /// The extended ID bit is mapped to bit 4. The RTR bit is mapped to bit 3.</param>
        /// <param name="filterMatch">Filter match type</param>
        public RxStatusResponse(
            FilterMatchType filterMatch,
            MessageReceivedType messageTypeReceived,
            ReceivedMessageType receivedMessage)
        {
            FilterMatch = filterMatch;
            MessageTypeReceived = messageTypeReceived;
            ReceivedMessage = receivedMessage;
        }

        /// <summary>
        /// Initializes a new instance of the RxStatusResponse class.
        /// </summary>
        /// <param name="value">The value that represents the ReceivedMessage, MessageTypeReceived and FilterMatch.</param>
        public RxStatusResponse(byte value)
        {
            ReceivedMessage = (ReceivedMessageType)((value & 0b1100_0000) >> 6);
            MessageTypeReceived = (MessageReceivedType)((value & 0b0001_1000) >> 3);
            FilterMatch = (FilterMatchType)(value & 0b0000_0111);
        }

        /// <summary>
        /// Filter match type
        /// </summary>
        public FilterMatchType FilterMatch { get; }

        /// <summary>
        /// The extended ID bit is mapped to bit 4. The RTR bit is mapped to bit 3.
        /// </summary>
        public MessageReceivedType MessageTypeReceived { get; }

        /// <summary>
        /// RXxIF (CANINTF) bits are mapped to bits 7 and 6.
        /// </summary>
        public ReceivedMessageType ReceivedMessage { get; }

        /// <summary>
        /// RXxIF (CANINTF) bits are mapped to bits 7 and 6.
        /// </summary>
        public enum ReceivedMessageType
        {
            /// <summary>
            /// No RX Message.
            /// </summary>
            NoRxMessage = 0,

            /// <summary>
            /// Message in RXB0.
            /// </summary>
            MessageInRxB0 = 1,

            /// <summary>
            /// Message in RXB1.
            /// </summary>
            MessageInRxB1 = 2,

            /// <summary>
            /// Messages in Both Buffers.
            /// </summary>
            MessagesInBothBuffers = 3
        }

        /// <summary>
        /// The extended ID bit is mapped to bit 4. The RTR bit is mapped to bit 3.
        /// </summary>
        public enum MessageReceivedType
        {
            /// <summary>
            /// Standard Data Frame.
            /// </summary>
            StandardDataFrame = 0,

            /// <summary>
            /// Standard Remote Frame.
            /// </summary>
            StandardRemoteFrame = 1,

            /// <summary>
            /// Extended Data Frame.
            /// </summary>
            ExtendedDataFrame = 2,

            /// <summary>
            /// Extended Remote Frame.
            /// </summary>
            ExtendedRemoteFrame = 3
        }

        /// <summary>
        /// Filter match type
        /// </summary>
        public enum FilterMatchType
        {
            /// <summary>RxF0</summary>
            RxF0 = 0,

            /// <summary>RxF1</summary>
            RxF1 = 1,

            /// <summary>RxF2</summary>
            RxF2 = 2,

            /// <summary>RxF3</summary>
            RxF3 = 3,

            /// <summary>RxF4</summary>
            RxF4 = 4,

            /// <summary>RxF5</summary>
            RxF5 = 5,

            /// <summary>RxF0RolloverToRxB1</summary>
            RxF0RolloverToRxB1 = 6,

            /// <summary>RxF1RolloverToRxB1</summary>
            RxF1RolloverToRxB1 = 7,
        }

        /// <summary>
        /// Converts contents to a byte.
        /// </summary>
        /// <returns>The byte that represent the response contents.</returns>
        public byte ToByte()
        {
            byte value = (byte)((byte)ReceivedMessage << 6);
            value |= (byte)((byte)MessageTypeReceived << 3);
            value |= (byte)FilterMatch;
            return value;
        }
    }
}
