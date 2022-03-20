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
        }

        [ArduinoImplementation("MonitorPulseAll")]
        public static void PulseAll(Object o)
        {
        }

        public static void Pulse(Object o)
        {
            PulseAll(o);
        }

        [ArduinoImplementation("MonitorWait")]
        public static bool Wait(Object obj, Int32 millisecondsTimeout)
        {
            throw new NotImplementedException();
        }

        [ArduinoImplementation("MonitorTryEnter")]
        public static bool TryEnter(object obj, Int32 millisecondsTimeout)
        {
            throw new NotImplementedException();
        }

        public static bool TryEnter(object obj, Int32 millisecondsTimeout, ref bool lockTaken)
        {
            if (TryEnter(obj, millisecondsTimeout))
            {
                lockTaken = true;
                return true;
            }

            return false;
        }
    }
}
