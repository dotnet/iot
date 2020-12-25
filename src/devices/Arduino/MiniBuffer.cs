using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;

namespace Iot.Device.Arduino
{
    internal class MiniBuffer
    {
        [ArduinoImplementation(ArduinoImplementation.BufferMemmove)]
        internal static unsafe void Memmove(byte* dest, byte* src, uint len)
        {
            throw new NotImplementedException();
        }

        [ArduinoImplementation(ArduinoImplementation.BufferMemmoveRefArgs)]
        private static void Memmove(ref byte dest, ref byte src, uint len)
        {
            throw new NotImplementedException();
        }

        internal static void Memmove<T>(ref T destination, ref T source, uint elementCount)
        {
            if (!RuntimeHelpers.IsReferenceOrContainsReferences<T>())
            {
                // Blittable memmove
                Memmove(
                    ref MiniUnsafe.As<T, byte>(ref destination),
                    ref MiniUnsafe.As<T, byte>(ref source),
                    elementCount * (uint)MiniUnsafe.SizeOf<T>());
            }
            else
            {
                // Non-blittable memmove
                BulkMoveWithWriteBarrier(
                    ref MiniUnsafe.As<T, byte>(ref destination),
                    ref MiniUnsafe.As<T, byte>(ref source),
                    elementCount * (uint)MiniUnsafe.SizeOf<T>());
            }
        }

        // The maximum block size to for __BulkMoveWithWriteBarrier FCall. This is required to avoid GC starvation.
#if DEBUG // Stress the mechanism in debug builds
        private const uint BulkMoveWithWriteBarrierChunk = 0x400;
#else
        private const uint BulkMoveWithWriteBarrierChunk = 0x4000;
#endif

        internal static void BulkMoveWithWriteBarrier(ref byte destination, ref byte source, uint byteCount)
        {
            if (byteCount <= BulkMoveWithWriteBarrierChunk)
            {
                __BulkMoveWithWriteBarrier(ref destination, ref source, byteCount);
            }
            else
            {
                _BulkMoveWithWriteBarrier(ref destination, ref source, byteCount);
            }
        }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("StyleCop.CSharp.NamingRules", "SA1300:Element should begin with upper-case letter", Justification = "CLR internal method name")]
        private static void _BulkMoveWithWriteBarrier(ref byte destination, ref byte source, uint byteCount)
        {
            if (MiniUnsafe.AreSame(ref source, ref destination))
            {
                return;
            }

            // This is equivalent to: (destination - source) >= byteCount || (destination - source) < 0
            if ((uint)(int)MiniUnsafe.ByteOffset(ref source, ref destination) >= byteCount)
            {
                // Copy forwards
                do
                {
                    byteCount -= BulkMoveWithWriteBarrierChunk;
                    __BulkMoveWithWriteBarrier(ref destination, ref source, BulkMoveWithWriteBarrierChunk);
                    destination = ref MiniUnsafe.AddByteOffset(ref destination, BulkMoveWithWriteBarrierChunk);
                    source = ref MiniUnsafe.AddByteOffset(ref source, BulkMoveWithWriteBarrierChunk);
                }
                while (byteCount > BulkMoveWithWriteBarrierChunk);
            }
            else
            {
                // Copy backwards
                do
                {
                    byteCount -= BulkMoveWithWriteBarrierChunk;
                    __BulkMoveWithWriteBarrier(ref MiniUnsafe.AddByteOffset(ref destination, byteCount), ref MiniUnsafe.AddByteOffset(ref source, byteCount), BulkMoveWithWriteBarrierChunk);
                }
                while (byteCount > BulkMoveWithWriteBarrierChunk);
            }

            __BulkMoveWithWriteBarrier(ref destination, ref source, byteCount);
        }

        [ArduinoImplementation(ArduinoImplementation.MiniBuffer_BulkMoveWithWriteBarrier)]
        [System.Diagnostics.CodeAnalysis.SuppressMessage("StyleCop.CSharp.NamingRules", "SA1300:Element should begin with upper-case letter", Justification = "CLR internal method name")]
        private static void __BulkMoveWithWriteBarrier(ref byte destination, ref byte source, uint byteCount)
        {
            throw new NotImplementedException();
        }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("StyleCop.CSharp.NamingRules", "SA1300:Element should begin with upper-case letter", Justification = "CLR internal method name")]
        internal static unsafe void _ZeroMemory(ref byte b, uint byteLength)
        {
            fixed (byte* bytePointer = &b)
            {
                __ZeroMemory(bytePointer, byteLength);
            }
        }

        [ArduinoImplementation(ArduinoImplementation.MiniBuffer_ZeroMemory)]
        [System.Diagnostics.CodeAnalysis.SuppressMessage("StyleCop.CSharp.NamingRules", "SA1300:Element should begin with upper-case letter", Justification = "CLR internal method name")]
        private static unsafe void __ZeroMemory(void* b, uint byteLength)
        {
            throw new NotImplementedException();
        }

    }
}
