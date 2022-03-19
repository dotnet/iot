// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System;
using System.Device.Ports.SerialPort.Resources;
using System.Diagnostics;
using System.IO;
using System.Net.Sockets;
using System.Runtime.InteropServices;

using Microsoft.Win32.SafeHandles;

namespace System.Device.Ports.SerialPort
{
    internal sealed class SafeSerialDeviceHandle : SafeHandleMinusOneIsInvalid
    {
        public SafeSerialDeviceHandle()
            : base(ownsHandle: true)
        {
        }

        internal static SafeSerialDeviceHandle Open(string portName)
        {
            Debug.Assert(portName != null, $"{nameof(portName)} must not be null");
            if (portName == null)
            {
                throw new ArgumentNullException(nameof(portName));
            }

            SafeSerialDeviceHandle handle = Interop.Serial.SerialPortOpen(portName);

            if (handle.IsInvalid)
            {
                // exception type is matching Windows
                throw new UnauthorizedAccessException(
                    ISR.Format(Strings.UnauthorizedAccess_IODenied_Port, portName),
                    Interop.GetIOException(Interop.Sys.GetLastErrorInfo()));
            }

            return handle;
        }

        protected override bool ReleaseHandle()
        {
            Interop.Serial.Shutdown(handle, SocketShutdown.Both);
            int result = Interop.Serial.SerialPortClose(handle);

            Debug.Assert(result == 0, $"Close failed with result {result} and error {Interop.Sys.GetLastErrorInfo()}");

            return result == 0;
        }
    }
}
