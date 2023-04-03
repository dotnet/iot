// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System.Threading.Tasks;

namespace System.Device.Ports.SerialPort
{
    internal partial class LinuxSerialPort
    {
        public override void Write(byte[] array, int offset, int count)
        {
            Validate();
            _file.Write(array, offset, count);
        }

        public override void Write(ReadOnlySpan<byte> buffer)
        {
            Validate();
            _file.Write(buffer);
        }

        public override ValueTask WriteAsync(ReadOnlyMemory<byte> buffer)
        {
            Validate();
            return _file.WriteAsync(buffer);
        }
    }
}
