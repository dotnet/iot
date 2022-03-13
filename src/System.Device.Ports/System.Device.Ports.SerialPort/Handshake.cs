// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

namespace System.Device.Ports.SerialPort
{
    /// <summary>
    /// Defines how the transfer is done between
    /// the sender (DTE) and the receiver (DCE).
    /// </summary>
    public enum Handshake
    {
        /// <summary>
        /// This is the most common option.
        /// All the trasmitted and received data may flow at the same time.
        /// </summary>
        None,

        /// <summary>
        /// The data flow is controlled using the two ascii characters
        /// XOn (17) and XOff (19).<par />
        /// The receiver can stop receiving data by sending XOff and
        /// restarting the flow by sending XOn to the transmitter.<para/>
        /// This flow precludes the use of those two characters in
        /// the message data.
        /// </summary>
        XOnXOff,

        /// <summary>
        /// The data flow is controlled using the RTS and CTS wires
        /// and is used when a half-duplex communication is desired.
        /// The RTS line is set by the transmitter. The receiver
        /// should assert the CTS to let the trasmitter begin.
        /// </summary>
        RequestToSend,

        /// <summary>
        /// Both the Request-to-Send (RTS) hardware control and
        /// the XON/XOFF software controls are used
        /// </summary>
        RequestToSendXOnXOff
    }
}
