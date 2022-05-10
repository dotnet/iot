// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

namespace System.Device.Ports.SerialPort
{
    /// <summary>
    /// The arguments for the SerialDataReceived event
    /// </summary>
    public class SerialDataReceivedEventArgs : EventArgs
    {
        internal SerialDataReceivedEventArgs(SerialData eventCode)
        {
            EventType = eventCode;
        }

        /// <summary>
        /// The nature of data recevied from the serial port
        /// </summary>
        public SerialData EventType { get; private set; }
    }
}
