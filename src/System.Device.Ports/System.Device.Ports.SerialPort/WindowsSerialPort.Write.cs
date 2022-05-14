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
        // User-accessible async write method.  Returns WindowsSerialPortAsyncResult : IAsyncResult
        // Throws an exception if port is in break state.
        public IAsyncResult BeginWrite(byte[] array, int offset, int numBytes,
            AsyncCallback userCallback, object stateObject)
        {
            if (BreakState)
            {
                throw new InvalidOperationException(Strings.In_Break_State);
            }

            CheckReadWriteArguments(array, offset, numBytes);

            int oldtimeout = WriteTimeout;
            WriteTimeout = SerialPort.InfiniteTimeout;
            IAsyncResult result;
            try
            {
                result = BeginWriteCore(array, offset, numBytes, userCallback, stateObject);
            }
            finally
            {
                WriteTimeout = oldtimeout;
            }

            return result;
        }

        // Async companion to BeginWrite.
        // Note, assumed IAsyncResult argument is of derived type WindowsSerialPortAsyncResult,
        // and throws an exception if untrue.
        // Also fails if called in port's break state.
        public unsafe void EndWrite(IAsyncResult asyncResult)
        {
            if (BreakState)
            {
                throw new InvalidOperationException(Strings.In_Break_State);
            }

            if (asyncResult == null || !(asyncResult is WindowsSerialPortAsyncResult afsar))
            {
                throw new ArgumentNullException(nameof(asyncResult));
            }

            if (!afsar.IsWrite)
            {
                throw new ArgumentException(Strings.Arg_WrongAsyncResult);
            }

            // This sidesteps race conditions, avoids memory corruption after freeing the
            // NativeOverlapped class or GCHandle twice.
            if (1 == Interlocked.CompareExchange(ref afsar.EndXxxCalled, 1, 0))
            {
                throw new ArgumentException(Strings.InvalidOperation_EndWriteCalledMultiple);
            }

            // Obtain the WaitHandle, but don't use public property in case we
            // delay initialize the manual reset event in the future.
            WaitHandle wh = afsar._waitHandle;
            if (wh != null)
            {
                // We must block to ensure that AsyncFSCallback has completed,
                // and we should close the WaitHandle in here.
                try
                {
                    wh.WaitOne();
                    Debug.Assert(afsar.IsCompleted == true, "SerialStream::EndWrite - AsyncFSCallback didn't set _isComplete to true!");
                }
                finally
                {
                    wh.Close();
                }
            }

            // Free memory, GC handles.
            NativeOverlapped* overlappedPtr = afsar.Overlapped;
            if (overlappedPtr != null)
            {
                // Legacy behavior as indicated by tests (e.g.: System.IO.Ports.Tests.SerialStream_EndWrite.EndWriteAfterSerialStreamClose)
                // expects to be able to call EndWrite after Close/Dispose - even if disposed _threadPoolBinding can free the
                // native overlapped.
                _threadPoolBound?.FreeNativeOverlapped(overlappedPtr);
            }

            // Now check for any error during the write.
            if (afsar.ErrorCode != 0)
            {
                throw WindowsHelpers.GetExceptionForWin32Error(afsar.ErrorCode, _portName);
            }

            // Number of bytes written is afsar._numBytes.
        }

        internal unsafe void Write(byte[] array, int offset, int count, int timeout)
        {
            if (BreakState)
            {
                throw new InvalidOperationException(Strings.In_Break_State);
            }

            CheckReadWriteArguments(array, offset, count);

            if (count == 0)
            {
                return; // no need to expend overhead in creating asyncResult, etc.
            }

            Debug.Assert(timeout == SerialPort.InfiniteTimeout || timeout >= 0, $"Serial Stream Write - write timeout is {timeout}");

            int numBytes;
            IAsyncResult result = BeginWriteCore(array, offset, count, null, null);
            EndWrite(result);

            var afsar = result as WindowsSerialPortAsyncResult;
            Debug.Assert(afsar != null, "afsar should be a WindowsSerialPortAsyncResult and should not be null");
            numBytes = afsar._numBytes;

            if (numBytes == 0)
            {
                throw new TimeoutException(Strings.Write_timed_out);
            }
        }

        // use default timeout as argument to WriteByte with timeout arg
        public void WriteByte(byte value)
        {
            WriteByte(value, WriteTimeout);
        }

        internal unsafe void WriteByte(byte value, int timeout)
        {
            if (BreakState)
            {
                throw new InvalidOperationException(Strings.In_Break_State);
            }

            if (_portHandle == null)
            {
                throw new InvalidOperationException(Strings.Port_not_open);
            }

            _singleByteBuf[0] = value;

            int numBytes;
            IAsyncResult result = BeginWriteCore(_singleByteBuf, 0, 1, null, null);
            EndWrite(result);

            if (result == null || !(result is WindowsSerialPortAsyncResult afsar))
            {
                throw new ArgumentException(Strings.Arg_WrongAsyncResult);
            }

            numBytes = afsar._numBytes;

            if (numBytes == 0)
            {
                throw new TimeoutException(Strings.Write_timed_out);
            }

            return;
        }

        private unsafe WindowsSerialPortAsyncResult BeginWriteCore(byte[] array, int offset, int numBytes,
            AsyncCallback? userCallback, object? stateObject)
        {
            if (_threadPoolBound == null)
            {
                throw new ArgumentNullException(nameof(_threadPoolBound));
            }

            // Create and store async stream class library specific data in the async result
            var asyncResult = new WindowsSerialPortAsyncResult(new ManualResetEvent(false), userCallback, stateObject, true);

            NativeOverlapped* intOverlapped = _threadPoolBound.AllocateNativeOverlapped(AsyncFSCallback, asyncResult, array);

            asyncResult.Overlapped = intOverlapped;

            // queue an async WriteFile operation and pass in a packed overlapped
            int r = WriteFileNative(array, offset, numBytes, intOverlapped, out WIN32_ERROR hr);

            // WriteFile, the OS version, will return 0 on failure.  But
            // my WriteFileNative wrapper returns -1.  My wrapper will return
            // the following:
            // On error, r==-1.
            // On async requests that are still pending, r==-1 w/ hr==ERROR_IO_PENDING
            // On async requests that completed sequentially, r==0
            // Note that you will NEVER RELIABLY be able to get the number of bytes
            // written back from this call when using overlapped IO!  You must
            // not pass in a non-null lpNumBytesWritten to WriteFile when using
            // overlapped structures!
            if (r == -1)
            {
                if (hr != WIN32_ERROR.ERROR_IO_PENDING)
                {
                    if (hr == WIN32_ERROR.ERROR_HANDLE_EOF)
                    {
                        throw new EndOfStreamException(Strings.IO_EOF_ReadBeyondEOF);
                    }
                    else
                    {
                        throw WindowsHelpers.GetExceptionForWin32Error(hr, string.Empty);
                    }
                }
            }

            return asyncResult;
        }

        private unsafe int WriteFileNative(byte[] bytes, int offset, int count, NativeOverlapped* overlapped, out WIN32_ERROR hr)
        {
            if (_portHandle == null)
            {
                throw new InvalidOperationException(Strings.Port_not_open);
            }

            // Don't corrupt memory when multiple threads are erroneously writing
            // to this stream simultaneously.  (Note that the OS is reading from
            // the array we pass to WriteFile, but if we read beyond the end and
            // that memory isn't allocated, we could get an AV.)
            if (bytes.Length - offset < count)
            {
                throw new IndexOutOfRangeException(Strings.IndexOutOfRange_IORaceCondition);
            }

            if (bytes.Length == 0)
            {
                hr = 0;
                return 0;
            }

            int numBytesWritten = 0;
            int r = 0;

            fixed (byte* p = bytes)
            {
                r = WindowsHelpers.WriteFile(_portHandle.DangerousGetHandle(), p + offset, (uint)count, null, overlapped);
            }

            if (r == 0)
            {
                hr = (WIN32_ERROR)Marshal.GetLastWin32Error();
                // Note: we should never silently ignore an error here without some
                // extra work.  We must make sure that BeginWriteCore won't return an
                // IAsyncResult that will cause EndWrite to block, since the OS won't
                // call AsyncFSCallback for us.

                // For invalid handles, detect the error and mark our handle
                // as closed to give slightly better error messages.  Also
                // help ensure we avoid handle recycling bugs.
                if (hr == WIN32_ERROR.ERROR_INVALID_HANDLE)
                {
                    _portHandle?.SetHandleAsInvalid();
                    _portHandle = null;
                }

                return -1;
            }
            else
            {
                hr = 0;
            }

            return numBytesWritten;
        }

    }
}

#pragma warning restore CA1416 // Validate platform compatibility
