using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Iot.Device.Arduino.Tests
{
    internal static partial class Number
    {
        // Helper functions from Math, because they're not public.
        internal static uint DivRem(uint a, uint b, out uint result)
        {
            uint div = a / b;
            result = a - (div * b);
            return div;
        }

        internal static ulong DivRem(ulong a, ulong b, out ulong result)
        {
            ulong div = a / b;
            result = a - (div * b);
            return div;
        }

        private static unsafe void ZeroMemory(byte* getBlocksPointer, uint maxResultLength)
        {
            throw new NotImplementedException();
        }

        private static unsafe void Memcpy(byte* dest, byte* src, int length)
        {
            throw new NotImplementedException();
        }
    }
}
