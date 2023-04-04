// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System.Globalization;
using System.Runtime.InteropServices;
using System.Threading;
using System.Threading.Tasks;
using System.IO;
using System.Diagnostics;
using System.Device.Ports.SerialPort.Windows;
using System.Diagnostics.CodeAnalysis;
using System.Text;

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
        private unsafe void Prepare()
        {
            // PInvoke.GetQueuedCompletionStatus
            // PInvoke.PostQueuedCompletionStatus(
            // ...
            var buffer = new byte[1024];
            var overlapped = new Overlapped
            {
                AsyncResult = new AsyncReadResult()
                {
                    Callback = (bytesCount, buffer) =>
                    {
                        var contentRead = Encoding.UTF8.GetString(buffer, 0, (int)bytesCount);
                        // ...
                    },
                    Buffer = buffer,
                }
            };

            NativeOverlapped* nativeOverlapped = overlapped.UnsafePack(null, buffer);
        }

        public override int Read(byte[] buffer, int offset, int count)
        {
            Validate();
            throw new NotImplementedException();
        }

        public override int Read(Span<byte> buffer)
        {
            Validate();
            throw new NotImplementedException();
        }

        public override ValueTask<int> ReadAsync(Memory<byte> buffer)
        {
            Validate();
            throw new NotImplementedException();
        }

        private class AsyncReadResult : IAsyncResult
        {
            public object? AsyncState { get; set; }

            [AllowNull]
            public WaitHandle AsyncWaitHandle { get; set; }
            public bool CompletedSynchronously { get; set; }
            public bool IsCompleted { get; set; }

            public Action<uint, byte[]>? Callback { get; set; }
            public byte[]? Buffer { get; set; }
        }

    }

}

#pragma warning restore CA1416 // Validate platform compatibility
