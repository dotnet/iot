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
        // User-accessible async read method.  Returns WindowsSerialPortAsyncResult : IAsyncResult
        public IAsyncResult BeginRead(byte[] array, int offset, int numBytes, AsyncCallback userCallback, object stateObject)
        {
            CheckReadWriteArguments(array, offset, numBytes);

            int oldtimeout = ReadTimeout;
            ReadTimeout = SerialPort.InfiniteTimeout;
            IAsyncResult result;
            try
            {
                result = BeginReadCore(array, offset, numBytes, userCallback, stateObject);
            }
            finally
            {
                ReadTimeout = oldtimeout;
            }

            return result;
        }

        // Async companion to BeginRead.
        // Note, assumed IAsyncResult argument is of derived type WindowsSerialPortAsyncResult,
        // and throws an exception if untrue.
        public unsafe int EndRead(IAsyncResult asyncResult)
        {
            if (asyncResult == null)
            {
                throw new ArgumentNullException(nameof(asyncResult));
            }

            var afsar = asyncResult as WindowsSerialPortAsyncResult;
            if (afsar == null || afsar.IsWrite)
            {
                throw new ArgumentException(Strings.Arg_WrongAsyncResult);
            }

            // This sidesteps race conditions, avoids memory corruption after freeing the
            // NativeOverlapped class or GCHandle twice.
            if (1 == Interlocked.CompareExchange(ref afsar.EndXxxCalled, 1, 0))
            {
                throw new ArgumentException(Strings.InvalidOperation_EndReadCalledMultiple);
            }

            bool failed = false;

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
                    Debug.Assert(afsar.IsCompleted == true, "SerialStream::EndRead - AsyncFSCallback didn't set _isComplete to true!");

                    /*
                    // InfiniteTimeout is not something native to the underlying serial device,
                    // we specify the timeout to be a very large value (MAXWORD-1) to achieve
                    // an infinite timeout illusion.

                    // I'm not sure what we can do here after an asyn operation with infinite
                    // timeout returns with no data. From a purist point of view we should
                    // somehow restart the read operation but we are not in a position to do so
                    // (and frankly that may not necessarily be the right thing to do here)
                    // I think the best option in this (almost impossible to run into) situation
                    // is to throw some sort of IOException.
                    */
                    if ((afsar._numBytes == 0) && (ReadTimeout == SerialPort.InfiniteTimeout) && (afsar.ErrorCode == 0))
                    {
                        failed = true;
                    }
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
                // Legacy behavior as indicated by tests (e.g.: System.IO.Ports.Tests.SerialStream_EndRead.EndReadAfterClose)
                // expects to be able to call EndRead after Close/Dispose - even if disposed _threadPoolBinding can free the
                // native overlapped.
                _threadPoolBound?.FreeNativeOverlapped(overlappedPtr);
            }

            // Check for non-timeout errors during the read.
            if (afsar.ErrorCode != 0)
            {
                throw WindowsHelpers.GetExceptionForWin32Error(afsar.ErrorCode, PortName);
            }

            if (failed)
            {
                throw new IOException(Strings.IO_OperationAborted);
            }

            return afsar._numBytes;
        }

        public override int Read(byte[] array, int offset, int count)
        {
            return Read(array, offset, count, ReadTimeout);
        }

        internal unsafe int Read(byte[] array, int offset, int count, int timeout)
        {
            CheckReadWriteArguments(array, offset, count);

            if (count == 0)
            {
                return 0; // return immediately if no bytes requested; no need for overhead.
            }

            Debug.Assert(timeout == SerialPort.InfiniteTimeout || timeout >= 0, $"Serial Stream Read - called with timeout {timeout}");

            int numBytes = 0;
            IAsyncResult result = BeginReadCore(array, offset, count, null, null);
            numBytes = EndRead(result);

            if (numBytes == 0)
            {
                throw new TimeoutException();
            }

            return numBytes;
        }

        /// <summary>
        /// This method is currently not exposed because the
        /// same strategy is exposed in the SerialStream class that
        /// is valid for any platform-specific implementation
        /// Should be this the final implementation, this method
        /// can be removed
        /// </summary>
        internal unsafe int ReadByte(int timeout)
        {
            if (_portHandle == null)
            {
                throw new InvalidOperationException(Strings.Port_not_open);
            }

            int numBytes = 0;
            IAsyncResult result = BeginReadCore(_singleByteBuf, 0, 1, null, null);
            numBytes = EndRead(result);

            if (numBytes == 0)
            {
                throw new TimeoutException();
            }
            else
            {
                return _singleByteBuf[0];
            }
        }

        private unsafe WindowsSerialPortAsyncResult BeginReadCore(byte[] array, int offset, int numBytes,
            AsyncCallback? userCallback, object? stateObject)
        {
            if (_threadPoolBound == null)
            {
                var privateVar = nameof(_threadPoolBound);
                throw new ArgumentNullException($"The internal parameter {privateVar} is null");
            }

            // Create and store async stream class library specific data in the async result
            var asyncResult = new WindowsSerialPortAsyncResult(new ManualResetEvent(false), userCallback, stateObject, false);

            NativeOverlapped* intOverlapped = _threadPoolBound.AllocateNativeOverlapped(AsyncFSCallback, asyncResult, array);

            asyncResult.Overlapped = intOverlapped;

            // queue an async ReadFile operation and pass in a packed overlapped
            // int r = ReadFile(_handle, array, numBytes, null, intOverlapped);
            int r = ReadFileNative(array, offset, numBytes, intOverlapped, out WIN32_ERROR hr);

            // ReadFile, the OS version, will return 0 on failure.  But
            // my ReadFileNative wrapper returns -1.  My wrapper will return
            // the following:
            // On error, r==-1.
            // On async requests that are still pending, r==-1 w/ hr==ERROR_IO_PENDING
            // on async requests that completed sequentially, r==0
            // Note that you will NEVER RELIABLY be able to get the number of bytes
            // read back from this call when using overlapped structures!  You must
            // not pass in a non-null lpNumBytesRead to ReadFile when using
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

        // Internal method, wrapping the PInvoke to ReadFile().
        private unsafe int ReadFileNative(byte[] bytes, int offset, int count, NativeOverlapped* overlapped, out WIN32_ERROR hr)
        {
            if (_portHandle == null)
            {
                throw new InvalidOperationException(Strings.Port_not_open);
            }

            // Don't corrupt memory when multiple threads are erroneously writing
            // to this stream simultaneously.
            if (bytes.Length - offset < count)
            {
                throw new IndexOutOfRangeException(Strings.IndexOutOfRange_IORaceCondition);
            }

            if (bytes.Length == 0)
            {
                hr = 0;
                return 0;
            }

            int r = 0;
            int numBytesRead = 0;

            fixed (byte* p = bytes)
            {
                r = WindowsHelpers.ReadFile(_portHandle.DangerousGetHandle(), p + offset, (uint)count, null, overlapped);
            }

            if (r == 0)
            {
                hr = (WIN32_ERROR)Marshal.GetLastWin32Error();

                // Note: we should never silently ignore an error here without some
                // extra work.  We must make sure that BeginReadCore won't return an
                // IAsyncResult that will cause EndRead to block, since the OS won't
                // call AsyncFSCallback for us.

                // For invalid handles, detect the error and mark our handle
                // as closed to give slightly better error messages.  Also
                // help ensure we avoid handle recycling bugs.
                if (hr == WIN32_ERROR.ERROR_INVALID_HANDLE)
                {
                    _portHandle.SetHandleAsInvalid();
                    _portHandle.Dispose();
                    _portHandle = null;
                }

                return -1;
            }
            else
            {
                hr = 0;
            }

            return numBytesRead;
        }

    }
}

#pragma warning restore CA1416 // Validate platform compatibility
