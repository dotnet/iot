// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System;
using Iot.Device.Arduino;

#pragma warning disable SA1300 // Element should begin with an uppercase letter
namespace ArduinoCsCompiler.Runtime
{
    [ArduinoReplacement(typeof(System.Buffer), true, IncludingPrivates = true)]
    internal static class MiniBuffer
    {
        [ArduinoImplementation("BufferMemmove")]
        public static unsafe void Memmove(byte* dest, byte* src, uint len)
        {
            throw new NotImplementedException();
        }

        [ArduinoImplementation]
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

        [ArduinoImplementation]
        public static void Memmove<T>(ref T destination, ref T source, uint elementCount)
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

        public static unsafe void _ZeroMemory(ref byte b, uint byteLength)
        {
            fixed (byte* bytePointer = &b)
            {
                __ZeroMemory((void*)bytePointer, byteLength);
            }
        }

        [ArduinoImplementation("BufferZeroMemory")]
        public static unsafe void __ZeroMemory(void* b, uint byteLength)
        {
            throw new NotImplementedException();
        }

        [ArduinoImplementation]
        public static unsafe void __ZeroMemory(void* b, UIntPtr length)
        {
            __ZeroMemory(b, (uint)length);
        }

        [ArduinoImplementation]
        public static unsafe void _ZeroMemory(byte* b, UIntPtr length)
        {
            __ZeroMemory((void*)b, (uint)length);
        }

        // Copies from one primitive array to another primitive array without
        // respecting types.  This calls memmove internally.  The count and
        // offset parameters here are in bytes.  If you want to use traditional
        // array element indices and counts, use Array.Copy.
        // Note: Part of validation removed!
        public static unsafe void BlockCopy(Array src, int srcOffset, Array dst, int dstOffset, int count)
        {
            if (src == null)
            {
                throw new ArgumentNullException(nameof(src));
            }

            if (dst == null)
            {
                throw new ArgumentNullException(nameof(dst));
            }

            if (srcOffset < 0)
            {
                throw new ArgumentOutOfRangeException(nameof(srcOffset));
            }

            if (dstOffset < 0)
            {
                throw new ArgumentOutOfRangeException(nameof(dstOffset));
            }

            if (count < 0)
            {
                throw new ArgumentOutOfRangeException(nameof(count));
            }

            uint uCount = (uint)count;
            uint uSrcOffset = (uint)srcOffset;
            uint uDstOffset = (uint)dstOffset;

            Memmove(ref MiniUnsafe.AddByteOffset(ref dst.GetRawArrayData(), uDstOffset), ref MiniUnsafe.AddByteOffset(ref src.GetRawArrayData(), uSrcOffset), uCount);
        }

        public static unsafe void MemoryCopy(void* source, void* destination, System.Int64 destinationSizeInBytes, System.Int64 sourceBytesToCopy)
        {
            if (destinationSizeInBytes < sourceBytesToCopy)
            {
                throw new ArgumentOutOfRangeException(nameof(sourceBytesToCopy));
            }

            Memmove((byte*)destination, (byte*)source, (uint)sourceBytesToCopy);
        }
    }
}
