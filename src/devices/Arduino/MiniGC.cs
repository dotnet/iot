using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Iot.Device.Arduino
{
    [ArduinoReplacement(typeof(System.GC), true, IncludingPrivates = true)]
    internal class MiniGC
    {
        public static T[] AllocateUninitializedArray<T>(int length, bool pinned = false) // T[] rather than T?[] to match `new T[length]` behavior
        {
            return new T[length]; // Initializing is so much cheaper than emulating the implementation, so that we don't care.
        }

        public static void KeepAlive(object? obj)
        {
            // This in fact is a no-op
        }
    }
}
