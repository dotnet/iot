// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

namespace System.Device.Ports.SerialPort
{
    /// <summary>
    /// Specifies the type of change that occurred on the SerialPort object.
    /// </summary>
    public enum SerialPinChange
    {
        /// <summary>
        /// The Clear to Send (CTS) signal changed state.
        /// This signal is used to indicate whether data can be sent
        /// over the serial port.
        /// </summary>
        CtsChanged = 0x08,

        /// <summary>
        /// The Data Set Ready (DSR) signal changed state.
        /// This signal is used to indicate whether the device
        /// on the serial port is ready to operate.
        /// </summary>
        DsrChanged = 0x10,

        /// <summary>
        /// The Carrier Detect (CD) signal changed state.
        /// This signal is used to indicate whether a modem is connected
        /// to a working phone line and a data carrier signal is detected.
        /// </summary>
        CDChanged = 0x20,

        /// <summary>
        /// A ring indicator was detected.
        /// </summary>
        Ring = 0x100,

        /// <summary>
        /// A break was detected on input.
        /// </summary>
        Break = 0x40
    }
}
