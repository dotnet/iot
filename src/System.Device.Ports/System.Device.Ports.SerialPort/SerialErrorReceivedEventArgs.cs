// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

namespace System.Device.Ports.SerialPort
{
    /// <summary>
    /// Prepares data for the ErrorReceived event.
    /// </summary>
    public class SerialErrorReceivedEventArgs : EventArgs
    {
        internal SerialErrorReceivedEventArgs(SerialError eventCode)
        {
            EventType = eventCode;
        }

        /// <summary>
        /// The nature of data recevied from the serial port
        /// </summary>
        public SerialError EventType { get; private set; }
    }
}
