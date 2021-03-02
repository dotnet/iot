using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;

namespace Iot.Device.Arduino
{
    [ArduinoReplacement(typeof(System.Buffer), true, IncludingPrivates = true)]
    internal static class MiniBuffer
    {
        [ArduinoImplementation(NativeMethod.BufferMemmove)]
        public static unsafe void Memmove(byte* dest, byte* src, uint len)
        {
            throw new NotImplementedException();
        }

        public static unsafe void Memmove(ref byte dest, ref byte src, uint len)
        {
            fixed (byte* srcPointer = &src)
            {
                fixed (byte* destPointer = &dest)
                {
                    Memmove(destPointer, srcPointer, len);
                }
            }
        }

        internal static void Memmove<T>(ref T destination, ref T source, uint elementCount)
        {
            // Blittable memmove. The standard CLR uses a different implementation if the element
            // to be copied contains references, without actually specifying why, though. I guess because
            // we (currently) have no mark-and-copy GC, there's no big difference
            Memmove(
                ref MiniUnsafe.As<T, byte>(ref destination),
                ref MiniUnsafe.As<T, byte>(ref source),
                elementCount * (uint)MiniUnsafe.SizeOf<T>());
        }

        internal static void BulkMoveWithWriteBarrier(ref byte destination, ref byte source, uint byteCount)
        {
            Memmove(ref destination, ref source, byteCount);
        }

        internal static unsafe void Memcpy(byte* dest, byte* src, int len)
        {
            Memmove(dest, src, (uint)len);
        }

        internal static unsafe void Memcpy(byte* dest, byte* src, uint len)
        {
            Memmove(dest, src, len);
        }

        public static unsafe void ZeroMemory(ref byte b, uint byteLength)
        {
            fixed (byte* bytePointer = &b)
            {
                ZeroMemory(bytePointer, byteLength);
            }
        }

        [ArduinoImplementation(NativeMethod.BufferZeroMemory)]
        public static unsafe void ZeroMemory(void* b, uint byteLength)
        {
            throw new NotImplementedException();
        }

        [ArduinoImplementation(NativeMethod.None)]
        public static unsafe void ZeroMemory(void* b, UIntPtr length)
        {
            ZeroMemory(b, (uint)length);
        }

        [ArduinoImplementation(NativeMethod.None)]
        public static unsafe void ZeroMemory(byte* b, UIntPtr length)
        {
            ZeroMemory((void*)b, (uint)length);
        }
    }
}
