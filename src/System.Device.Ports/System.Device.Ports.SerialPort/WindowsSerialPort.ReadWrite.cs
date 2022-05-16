// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System.Globalization;
using System.Runtime.InteropServices;
using System.Threading;
using System.Threading.Tasks;
using System.IO;
using System.Diagnostics;

using Microsoft.Win32.SafeHandles;

using Windows.Win32;
using Windows.Win32.Foundation;
using Windows.Win32.Devices;
using Windows.Win32.Devices.Communication;
using Windows.Win32.Storage.FileSystem;

#pragma warning disable CA1416 // Validate platform compatibility

namespace System.Device.Ports.SerialPort
{
    internal partial class WindowsSerialPort
    {
        private readonly byte[] _singleByteBuf = new byte[1];

        // This is a the callback prompted when a thread completes any async I/O operation.
        private static unsafe void AsyncFSCallback(uint errorCode, uint numBytes, NativeOverlapped* pOverlapped)
        {
            // Extract async the result from overlapped structure
            var asyncResult = (WindowsSerialPortAsyncResult?)ThreadPoolBoundHandle.GetNativeOverlappedState(pOverlapped);
            if (asyncResult == null)
            {
                return;
            }

            asyncResult._numBytes = (int)numBytes;
            asyncResult.ErrorCode = (WIN32_ERROR)errorCode;

            // Call the user-provided callback.  Note that it can and often should
            // call EndRead or EndWrite.  There's no reason to use an async
            // delegate here - we're already on a threadpool thread.
            // Note the IAsyncResult's completedSynchronously property must return
            // false here, saying the user callback was called on another thread.
            asyncResult.CompletedSynchronously = false;
            asyncResult.IsCompleted = true;

            // The OS does not signal this event.  We must do it ourselves.
            // But don't close it if the user callback called EndXxx,
            // which then closed the manual reset event already.
            ManualResetEvent wh = asyncResult._waitHandle;
            if (wh != null)
            {
                bool r = wh.Set();
                if (!r)
                {
                    throw WindowsHelpers.GetExceptionForLastWin32Error();
                }
            }

            asyncResult.UserCallback?.Invoke(asyncResult);
        }

        private void CheckReadWriteArguments(byte[] array, int offset, int count)
        {
            if (array == null)
            {
                throw new ArgumentNullException(nameof(array));
            }

            if (offset < 0)
            {
                throw new ArgumentOutOfRangeException(nameof(offset), Strings.ArgumentOutOfRange_NeedNonNegNumRequired);
            }

            if (count < 0)
            {
                throw new ArgumentOutOfRangeException(nameof(count), Strings.ArgumentOutOfRange_NeedNonNegNumRequired);
            }

            if (array.Length - offset < count)
            {
                throw new ArgumentException(Strings.Argument_InvalidOffLen);
            }

            if (_portHandle == null)
            {
                throw new InvalidOperationException(Strings.Port_not_open);
            }
        }

    }
}

#pragma warning restore CA1416 // Validate platform compatibility
