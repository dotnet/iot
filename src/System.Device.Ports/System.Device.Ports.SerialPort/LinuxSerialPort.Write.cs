// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

namespace System.Device.Ports.SerialPort
{
    internal partial class LinuxSerialPort
    {
        public override void Write(byte[] array, int offset, int count)
        {
            throw new NotImplementedException();
        }
    }
}
