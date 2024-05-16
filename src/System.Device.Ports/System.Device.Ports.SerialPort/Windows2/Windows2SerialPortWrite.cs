// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System.Globalization;
using System.Runtime.InteropServices;
using System.Threading;
using System.Threading.Tasks;
using System.IO;
using System.Diagnostics;
using System.Device.Ports.SerialPort.Windows;

using Microsoft.Win32.SafeHandles;

using Windows.Win32;
using Windows.Win32.Foundation;
using Windows.Win32.Devices;
using Windows.Win32.Devices.Communication;
using Windows.Win32.Storage.FileSystem;

#pragma warning disable CA1416 // Validate platform compatibility

namespace System.Device.Ports.SerialPort.Windows2
{
    internal partial class Windows2SerialPort : SerialPort
    {
        public override void Write(byte[] array, int offset, int count)
        {
            Validate();
            throw new NotImplementedException();
        }

        public override void Write(ReadOnlySpan<byte> buffer)
        {
            Validate();
            throw new NotImplementedException();
        }

        public override ValueTask WriteAsync(ReadOnlyMemory<byte> buffer)
        {
            Validate();
            throw new NotImplementedException();
        }
    }

}

#pragma warning restore CA1416 // Validate platform compatibility
