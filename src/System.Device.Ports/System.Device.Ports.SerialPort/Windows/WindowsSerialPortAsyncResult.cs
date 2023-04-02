// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

using Windows.Win32.Foundation;

namespace System.Device.Ports.SerialPort.Windows
{
    internal unsafe sealed class WindowsSerialPortAsyncResult : IAsyncResult
    {
        // User code callback
        internal ManualResetEvent _waitHandle;

        internal int _numBytes;     // number of bytes read OR written

        internal int EndXxxCalled;
        /*
         * Needed for the methods that are not implemented yet
        //internal bool _isComplete;
        //internal bool _completedSynchronously;  // Which thread called callback
        internal int _EndXxxCalled;   // Whether we've called EndXxx already.
        internal int _errorCode;
        internal NativeOverlapped* _overlapped;
        */

        public WindowsSerialPortAsyncResult(ManualResetEvent waitHandle, AsyncCallback? userCallback,
            object? userStateObject, bool isWrite)
        {
            _waitHandle = waitHandle;
            UserCallback = userCallback;
            AsyncState = userStateObject;
            IsWrite = isWrite;
        }

        public AsyncCallback? UserCallback { get; }
        public object? AsyncState { get; }

        // number of bytes read OR written
        // internal int NumBytes { get; set; }
        internal WIN32_ERROR ErrorCode { get; set; }
        internal NativeOverlapped* Overlapped { get; set; }

        /// <summary>
        /// Whether this is a read or a write
        /// </summary>
        internal bool IsWrite { get; }

        public bool IsCompleted { get; internal set; }

        public WaitHandle AsyncWaitHandle
        {
            get
            {
                /*
                  // Consider uncommenting this someday soon - the EventHandle
                  // in the Overlapped struct is really useless half of the
                  // time today since the OS doesn't signal it.  If users call
                  // EndXxx after the OS call happened to complete, there's no
                  // reason to create a synchronization primitive here.  Fixing
                  // this will save us some perf, assuming we can correctly
                  // initialize the ManualResetEvent.
                if (_waitHandle == null) {
                    ManualResetEvent mre = new ManualResetEvent(false);
                    if (_overlapped != null && _overlapped->EventHandle != IntPtr.Zero)
                        mre.Handle = _overlapped->EventHandle;
                    if (_isComplete)
                        mre.Set();
                    _waitHandle = mre;
                }
                */
                return _waitHandle;
            }
        }

        // Returns true if the user callback was called by the thread that
        // called BeginRead or BeginWrite.  If we use an async delegate or
        // threadpool thread internally, this will be false.  This is used
        // by code to determine whether a successive call to BeginRead needs
        // to be done on their main thread or in their callback to avoid a
        // stack overflow on many reads or writes.
        public bool CompletedSynchronously { get; internal set; }
    }
}
