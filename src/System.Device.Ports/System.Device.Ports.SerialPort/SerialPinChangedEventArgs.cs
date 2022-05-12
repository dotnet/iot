// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System;

namespace System.Device.Ports.SerialPort
{
    /// <summary>
    /// Provides data for the PinChanged event.
    /// </summary>
    public class SerialPinChangedEventArgs : EventArgs
    {
        internal SerialPinChangedEventArgs(SerialPinChange eventCode)
        {
            EventType = eventCode;
        }

        /// <summary>
        /// Gets or sets the event type.
        /// </summary>
        public SerialPinChange EventType { get; private set; }
    }
}
