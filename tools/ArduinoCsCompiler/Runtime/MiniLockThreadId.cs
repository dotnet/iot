// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System.Threading;
using Iot.Device.Arduino;

namespace ArduinoCsCompiler.Runtime
{
    [ArduinoReplacement("System.Threading.Lock+ThreadId", "System.Private.CoreLib.dll", true, IncludingPrivates = true)]
    internal struct MiniLockThreadId
    {
        private uint _id;

        public MiniLockThreadId(uint id)
        {
            _id = id;
        }

        // The firmware initializes a worker's thread-static storage from the initial thread's current values.
        public static MiniLockThreadId Current_NoInitialize => new MiniLockThreadId((uint)Thread.CurrentThread.ManagedThreadId);

        public uint Id => _id;

        public bool IsInitialized => _id != 0;

        public void InitializeForCurrentThread()
        {
            _id = (uint)Thread.CurrentThread.ManagedThreadId;
        }
    }
}
