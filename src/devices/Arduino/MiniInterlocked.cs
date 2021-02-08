using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Iot.Device.Arduino
{
    /// <summary>
    /// These functions are implemented in the backend, even though for a single-processor CPU, without interrupts, a simple implementation
    /// would do. But some microprocessors allow multithreading, so that we should be prepared for this.
    /// </summary>
    [ArduinoReplacement(typeof(System.Threading.Interlocked), IncludingPrivates = true)]
    internal class MiniInterlocked
    {
        [ArduinoImplementation(NativeMethod.InterlockedCompareExchange_Object)]
        public static object? CompareExchange(ref object? location1, object? value, object? comparand)
        {
            throw new NotImplementedException();
        }

        [ArduinoImplementation(NativeMethod.ExchangeAdd)]
        public static int ExchangeAdd(ref int location1, int value)
        {
            throw new NotImplementedException();
        }
    }
}
