// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System.Threading.Tasks;

namespace System.Device.Ports.SerialPort
{
    internal partial class LinuxSerialPort
    {
        public override int Read(byte[] buffer, int offset, int count)
        {
            Validate();
            return _file.Read(buffer, offset, count);
        }

        public override int Read(Span<byte> buffer)
        {
            Validate();
            return _file.Read(buffer);
        }

        public override ValueTask<int> ReadAsync(Memory<byte> buffer)
        {
            Validate();
            return _file.ReadAsync(buffer);
        }
    }
}
