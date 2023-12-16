// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ArduinoCsCompiler.Runtime
{
    /// <summary>
    /// A lightweight non-recursive mutex. Waits on this lock are uninterruptible (from Thread.Interrupt(), which is supported
    /// in some runtimes). That is the main reason this lock type would be used over interruptible locks, such as in a
    /// low-level-infrastructure component that was historically not susceptible to a pending interrupt, and for compatibility
    /// reasons, to ensure that it still would not be susceptible after porting that component to managed code.
    /// </summary>
    [ArduinoReplacement("System.Threading.LowLevelLock", "System.Private.Corelib.dll", replaceEntireType: true, typeInSameAssembly: typeof(System.String))]
    internal sealed class MiniLowLevelLock : IDisposable
    {
        private object _lock;
        public MiniLowLevelLock()
        {
            _lock = new object();
        }

        public void Acquire()
        {
            MiniMonitor.Enter(_lock);
        }

        public void Release()
        {
            MiniMonitor.Exit(_lock);
        }

        public bool TryAcquire()
        {
            return MiniMonitor.TryEnter(_lock, 0);
        }

        public void Dispose()
        {
            MiniMonitor.Exit(_lock);
        }
    }
}
