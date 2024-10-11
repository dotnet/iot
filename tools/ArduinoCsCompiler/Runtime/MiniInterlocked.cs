// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System;
using System.Threading;
using Iot.Device.Arduino;

namespace ArduinoCsCompiler.Runtime
{
    /// <summary>
    /// These functions are implemented in the backend, even though for a single-processor CPU, without interrupts, a simple implementation
    /// would do. But some microprocessors allow multithreading, so that we should be prepared for this.
    /// </summary>
    [ArduinoReplacement(typeof(System.Threading.Interlocked), IncludingPrivates = true)]
    internal class MiniInterlocked
    {
        [ArduinoImplementation("InterlockedCompareExchange_Object")]
        public static object? CompareExchange(ref object? location1, object? value, object? comparand)
        {
            throw new NotImplementedException();
        }

        [ArduinoImplementation("InterlockedExchangeAdd")]
        public static int ExchangeAdd(ref int location1, int value)
        {
            throw new NotImplementedException();
        }

        [ArduinoImplementation("InterlockedExchangeInt")]
        public static int Exchange(ref Int32 location, Int32 value)
        {
            throw new NotImplementedException();
        }

        [ArduinoImplementation("InterlockedCompareExchange_Int32")]
        public static System.Int32 CompareExchange(ref System.Int32 location1, System.Int32 value, System.Int32 comparand)
        {
            throw new NotImplementedException();
        }

        [ArduinoImplementation("InterlockedExchangeObject")]
        public static object? Exchange(ref object? location1, object? value)
        {
            throw new NotImplementedException();
        }

        [ArduinoImplementation("InterlockedExchangeInt64")]
        public static long Exchange(ref long location, long value)
        {
            throw new NotImplementedException();
        }

        [ArduinoImplementation("InterlockedCompareExchange_Int64")]
        public static long CompareExchange(ref long location, long value, long comparand)
        {
            throw new NotImplementedException();
        }

        [ArduinoImplementation]
        public static void MemoryBarrier()
        {
            // Nothing to do here
        }

        [ArduinoImplementation]
        public static void ReadMemoryBarrier()
        {
            // Nothing to do here
        }
    }
}
