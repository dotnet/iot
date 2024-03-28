// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System;
using System.Threading;
using Iot.Device.Arduino;

namespace ArduinoCsCompiler.Runtime
{
    /// <summary>
    /// These locking primitives are no-ops as long as we're not supporting threads.
    /// </summary>
    [ArduinoReplacement(typeof(Monitor), true)]
    internal class MiniMonitor
    {
        [ArduinoImplementation]
        public static void Enter(Object o)
        {
            Monitor.TryEnter(o, -1);
        }

        [ArduinoImplementation]
        public static void Enter(Object o, ref bool lockTaken)
        {
            Enter(o);
            lockTaken = true;
        }

        [ArduinoImplementation("MonitorExit")]
        public static void Exit(Object o)
        {
            throw new NotImplementedException();
        }

        public static void PulseAll(Object o)
        {
            // Simplistic implementation: don't do anything.
            // Should work because at the moment we have a fair scheduler (note that this is called while the calling thread still owns the lock)
        }

        [ArduinoImplementation]
        public static void Pulse(Object o)
        {
            PulseAll(o);
        }

        [ArduinoImplementation("MonitorWait")]
        public static bool Wait(Object obj, Int32 millisecondsTimeout)
        {
            throw new NotImplementedException();
        }

        [ArduinoImplementation]
        public static bool Wait(Object obj, TimeSpan timeOut)
        {
            return Wait(obj, (int)timeOut.TotalMilliseconds);
        }

        [ArduinoImplementation("MonitorTryEnter")]
        public static bool TryEnter(object obj, Int32 millisecondsTimeout)
        {
            throw new NotImplementedException();
        }

        [ArduinoImplementation]
        public static bool TryEnter(object obj, TimeSpan timeOut)
        {
            return TryEnter(obj, (int)timeOut.TotalMilliseconds);
        }

        [ArduinoImplementation]
        public static bool TryEnter(object obj, Int32 millisecondsTimeout, ref bool lockTaken)
        {
            if (TryEnter(obj, millisecondsTimeout))
            {
                lockTaken = true;
                return true;
            }

            return false;
        }

        [ArduinoImplementation]
        public static bool TryEnter(object obj, TimeSpan timeout, ref bool lockTaken)
        {
            if (TryEnter(obj, (int)timeout.TotalMilliseconds))
            {
                lockTaken = true;
                return true;
            }

            return false;
        }
    }
}
