using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Unsafe = Iot.Device.Arduino.MiniUnsafe;

#pragma warning disable SA1405
#pragma warning disable SA1503
#pragma warning disable SA1513

namespace Iot.Device.Arduino
{
    [ArduinoReplacement("System.SpanHelpers", null, true, false, IncludingPrivates = true)]
    internal static partial class MiniSpanHelpers
    {
        public static unsafe void ClearWithoutReferences(ref byte b, uint byteLength)
        {
            if (byteLength == 0)
                return;
            MiniBuffer._ZeroMemory(ref b, byteLength);
        }

        public static unsafe void ClearWithReferences(ref IntPtr ip, uint pointerSizeLength)
        {
            // First write backward 8 natural words at a time.
            // Writing backward allows us to get away with only simple modifications to the
            // mov instruction's base and index registers between loop iterations.
            for (; pointerSizeLength >= 8; pointerSizeLength -= 8)
            {
                Unsafe.Add(ref Unsafe.Add(ref ip, (int)pointerSizeLength), -1) = default;
                Unsafe.Add(ref Unsafe.Add(ref ip, (int)pointerSizeLength), -2) = default;
                Unsafe.Add(ref Unsafe.Add(ref ip, (int)pointerSizeLength), -3) = default;
                Unsafe.Add(ref Unsafe.Add(ref ip, (int)pointerSizeLength), -4) = default;
                Unsafe.Add(ref Unsafe.Add(ref ip, (int)pointerSizeLength), -5) = default;
                Unsafe.Add(ref Unsafe.Add(ref ip, (int)pointerSizeLength), -6) = default;
                Unsafe.Add(ref Unsafe.Add(ref ip, (int)pointerSizeLength), -7) = default;
                Unsafe.Add(ref Unsafe.Add(ref ip, (int)pointerSizeLength), -8) = default;
            }

            // The logic below works by trying to minimize the number of branches taken for any
            // given range of lengths. For example, the lengths [ 4 .. 7 ] are handled by a single
            // branch, [ 2 .. 3 ] are handled by a single branch, and [ 1 ] is handled by a single
            // branch.
            //
            // We can write both forward and backward as a perf improvement. For example,
            // the lengths [ 4 .. 7 ] can be handled by zeroing out the first four natural
            // words and the last 3 natural words. In the best case (length = 7), there are
            // no overlapping writes. In the worst case (length = 4), there are three
            // overlapping writes near the middle of the buffer. In perf testing, the
            // penalty for performing duplicate writes is less expensive than the penalty
            // for complex branching.
            if (pointerSizeLength >= 4)
            {
                goto Write4To7;
            }
            else if (pointerSizeLength >= 2)
            {
                goto Write2To3;
            }
            else if (pointerSizeLength > 0)
            {
                goto Write1;
            }
            else
            {
                return; // nothing to write
            }

        Write4To7:
            // Write first four and last three.
            Unsafe.Add(ref ip, 2) = default;
            Unsafe.Add(ref ip, 3) = default;
            Unsafe.Add(ref Unsafe.Add(ref ip, (int)pointerSizeLength), -3) = default;
            Unsafe.Add(ref Unsafe.Add(ref ip, (int)pointerSizeLength), -2) = default;

        Write2To3:
            // Write first two and last one.
            Unsafe.Add(ref ip, 1) = default;
            Unsafe.Add(ref Unsafe.Add(ref ip, (int)pointerSizeLength), -1) = default;

        Write1:
            // Write only element.
            ip = default;
        }
    }
}
