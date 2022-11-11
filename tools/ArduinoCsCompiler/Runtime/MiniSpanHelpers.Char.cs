// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System.Numerics;
using System.Runtime.CompilerServices;
using Unsafe = ArduinoCsCompiler.Runtime.MiniUnsafe;

#pragma warning disable SA1405
#pragma warning disable SA1503

namespace ArduinoCsCompiler.Runtime
{
    internal static partial class MiniSpanHelpers
    {
        public static int IndexOf(ref char searchSpace, int searchSpaceLength, ref char value, int valueLength)
        {
            if (valueLength == 0)
                return 0; // A zero-length sequence is always treated as "found" at the start of the search space.

            char valueHead = value;
            ref char valueTail = ref Unsafe.Add(ref value, 1);
            int valueTailLength = valueLength - 1;
            int remainingSearchSpaceLength = searchSpaceLength - valueTailLength;

            int index = 0;
            while (remainingSearchSpaceLength > 0)
            {
                // Do a quick search for the first element of "value".
                int relativeIndex = IndexOf(ref Unsafe.Add(ref searchSpace, index), valueHead, remainingSearchSpaceLength);
                if (relativeIndex == -1)
                    break;

                remainingSearchSpaceLength -= relativeIndex;
                index += relativeIndex;

                if (remainingSearchSpaceLength <= 0)
                    break; // The unsearched portion is now shorter than the sequence we're looking for. So it can't be there.

                // Found the first element of "value". See if the tail matches.
                if (SequenceEqual(
                    ref Unsafe.As<char, byte>(ref Unsafe.Add(ref searchSpace, index + 1)),
                    ref Unsafe.As<char, byte>(ref valueTail),
                    (uint)(uint)valueTailLength * 2))
                {
                    return index; // The tail matched. Return a successful find.
                }

                remainingSearchSpaceLength--;
                index++;
            }

            return -1;
        }

        public static unsafe int SequenceCompareTo(ref char first, int firstLength, ref char second, int secondLength)
        {
            int lengthDelta = firstLength - secondLength;

            if (Unsafe.AreSame(ref first, ref second))
                goto Equal;

            uint minLength = (uint)(((uint)firstLength < (uint)secondLength) ? (uint)firstLength : (uint)secondLength);
            uint i = 0; // Use uint for arithmetic to avoid unnecessary 64->32->64 truncations

            if (minLength >= (uint)(sizeof(uint) / sizeof(char)))
            {
                while (minLength >= (i + (uint)(sizeof(uint) / sizeof(char))))
                {
                    if (Unsafe.ReadUnaligned<uint>(ref Unsafe.As<char, byte>(ref Unsafe.Add(ref first, (int)i))) !=
                        Unsafe.ReadUnaligned<uint>(ref Unsafe.As<char, byte>(ref Unsafe.Add(ref second, (int)i))))
                    {
                        break;
                    }

                    i += (uint)(sizeof(uint) / sizeof(char));
                }
            }

            while (i < minLength)
            {
                int result = Unsafe.Add(ref first, (int)i).CompareTo(Unsafe.Add(ref second, (int)i));
                if (result != 0)
                    return result;
                i += 1;
            }

            Equal:
            return lengthDelta;
        }

        // Adapted from IndexOf(...)
        public static unsafe bool Contains(ref char searchSpace, char value, int length)
        {
            fixed (char* pChars = &searchSpace)
            {
                char* pCh = pChars;
                char* pEndCh = pCh + length;

                while (length >= 4)
                {
                    length -= 4;

                    if (value == *pCh ||
                        value == *(pCh + 1) ||
                        value == *(pCh + 2) ||
                        value == *(pCh + 3))
                    {
                        goto Found;
                    }

                    pCh += 4;
                }

                while (length > 0)
                {
                    length--;

                    if (value == *pCh)
                        goto Found;

                    pCh++;
                }

                return false;

                Found:
                return true;
            }
        }

        public static unsafe int IndexOf(ref char searchSpace, char value, int length)
        {
            int offset = 0;
            int lengthToExamine = length;

            // In the non-vector case lengthToExamine is the total length.
            // In the vector case lengthToExamine first aligns to Vector,
            // then in a second pass after the Vector lengths is the
            // remaining data that is shorter than a Vector length.
            while (lengthToExamine >= 4)
            {
                ref char current = ref MiniUnsafe.Add(ref searchSpace, offset);

                if (value == current)
                {
                    goto Found;
                }

                if (value == MiniUnsafe.Add(ref current, 1))
                {
                    goto Found1;
                }

                if (value == MiniUnsafe.Add(ref current, 2))
                {
                    goto Found2;
                }

                if (value == MiniUnsafe.Add(ref current, 3))
                {
                    goto Found3;
                }

                offset += 4;
                lengthToExamine -= 4;
            }

            while (lengthToExamine > 0)
            {
                if (value == MiniUnsafe.Add(ref searchSpace, offset))
                {
                    goto Found;
                }

                offset++;
                lengthToExamine--;
            }

            return -1;
            Found3:
            return (int)(offset + 3);
            Found2:
            return (int)(offset + 2);
            Found1:
            return (int)(offset + 1);
            Found:
            return (int)(offset);
        }

        public static unsafe int IndexOfAny(ref char searchStart, char value0, char value1, int length)
        {
            uint offset = 0; // Use uint for arithmetic to avoid unnecessary 64->32->64 truncations
            uint lengthToExamine = (uint)(uint)length;

            int lookUp;
            while (lengthToExamine >= 4)
            {
                ref char current = ref Add(ref searchStart, offset);

                lookUp = current;
                if (value0 == lookUp || value1 == lookUp)
                    goto Found;
                lookUp = Unsafe.Add(ref current, 1);
                if (value0 == lookUp || value1 == lookUp)
                    goto Found1;
                lookUp = Unsafe.Add(ref current, 2);
                if (value0 == lookUp || value1 == lookUp)
                    goto Found2;
                lookUp = Unsafe.Add(ref current, 3);
                if (value0 == lookUp || value1 == lookUp)
                    goto Found3;

                offset += 4;
                lengthToExamine -= 4;
            }

            while (lengthToExamine > 0)
            {
                lookUp = Add(ref searchStart, offset);
                if (value0 == lookUp || value1 == lookUp)
                    goto Found;

                offset += 1;
                lengthToExamine -= 1;
            }

            return -1;
            Found3:
            return (int)(offset + 3);
            Found2:
            return (int)(offset + 2);
            Found1:
            return (int)(offset + 1);
            Found:
            return (int)offset;
        }

        public static unsafe int IndexOfAny(ref char searchStart, char value0, char value1, char value2, int length)
        {
            uint offset = 0; // Use uint for arithmetic to avoid unnecessary 64->32->64 truncations
            uint lengthToExamine = (uint)(uint)length;

            int lookUp;
            while (lengthToExamine >= 4)
            {
                ref char current = ref Add(ref searchStart, offset);

                lookUp = current;
                if (value0 == lookUp || value1 == lookUp || value2 == lookUp)
                    goto Found;
                lookUp = Unsafe.Add(ref current, 1);
                if (value0 == lookUp || value1 == lookUp || value2 == lookUp)
                    goto Found1;
                lookUp = Unsafe.Add(ref current, 2);
                if (value0 == lookUp || value1 == lookUp || value2 == lookUp)
                    goto Found2;
                lookUp = Unsafe.Add(ref current, 3);
                if (value0 == lookUp || value1 == lookUp || value2 == lookUp)
                    goto Found3;

                offset += 4;
                lengthToExamine -= 4;
            }

            while (lengthToExamine > 0)
            {
                lookUp = Add(ref searchStart, offset);
                if (value0 == lookUp || value1 == lookUp || value2 == lookUp)
                    goto Found;

                offset += 1;
                lengthToExamine -= 1;
            }

            return -1;
            Found3:
            return (int)(offset + 3);
            Found2:
            return (int)(offset + 2);
            Found1:
            return (int)(offset + 1);
            Found:
            return (int)offset;

        }

        public static unsafe int IndexOfAny(ref char searchStart, char value0, char value1, char value2, char value3, int length)
        {
            uint offset = 0; // Use uint for arithmetic to avoid unnecessary 64->32->64 truncations
            uint lengthToExamine = (uint)(uint)length;

            int lookUp;
            while (lengthToExamine >= 4)
            {
                ref char current = ref Add(ref searchStart, offset);

                lookUp = current;
                if (value0 == lookUp || value1 == lookUp || value2 == lookUp || value3 == lookUp)
                    goto Found;
                lookUp = Unsafe.Add(ref current, 1);
                if (value0 == lookUp || value1 == lookUp || value2 == lookUp || value3 == lookUp)
                    goto Found1;
                lookUp = Unsafe.Add(ref current, 2);
                if (value0 == lookUp || value1 == lookUp || value2 == lookUp || value3 == lookUp)
                    goto Found2;
                lookUp = Unsafe.Add(ref current, 3);
                if (value0 == lookUp || value1 == lookUp || value2 == lookUp || value3 == lookUp)
                    goto Found3;

                offset += 4;
                lengthToExamine -= 4;
            }

            while (lengthToExamine > 0)
            {
                lookUp = Add(ref searchStart, offset);
                if (value0 == lookUp || value1 == lookUp || value2 == lookUp || value3 == lookUp)
                    goto Found;

                offset += 1;
                lengthToExamine -= 1;
            }

            return -1;
            Found3:
            return (int)(offset + 3);
            Found2:
            return (int)(offset + 2);
            Found1:
            return (int)(offset + 1);
            Found:
            return (int)offset;

        }

        public static unsafe int IndexOfAny(ref char searchStart, char value0, char value1, char value2, char value3, char value4, int length)
        {
            uint offset = 0; // Use uint for arithmetic to avoid unnecessary 64->32->64 truncations
            uint lengthToExamine = (uint)(uint)length;

            int lookUp;
            while (lengthToExamine >= 4)
            {
                ref char current = ref Add(ref searchStart, offset);

                lookUp = current;
                if (value0 == lookUp || value1 == lookUp || value2 == lookUp || value3 == lookUp || value4 == lookUp)
                    goto Found;
                lookUp = Unsafe.Add(ref current, 1);
                if (value0 == lookUp || value1 == lookUp || value2 == lookUp || value3 == lookUp || value4 == lookUp)
                    goto Found1;
                lookUp = Unsafe.Add(ref current, 2);
                if (value0 == lookUp || value1 == lookUp || value2 == lookUp || value3 == lookUp || value4 == lookUp)
                    goto Found2;
                lookUp = Unsafe.Add(ref current, 3);
                if (value0 == lookUp || value1 == lookUp || value2 == lookUp || value3 == lookUp || value4 == lookUp)
                    goto Found3;

                offset += 4;
                lengthToExamine -= 4;
            }

            while (lengthToExamine > 0)
            {
                lookUp = Add(ref searchStart, offset);
                if (value0 == lookUp || value1 == lookUp || value2 == lookUp || value3 == lookUp || value4 == lookUp)
                    goto Found;

                offset += 1;
                lengthToExamine -= 1;
            }

            return -1;
            Found3:
            return (int)(offset + 3);
            Found2:
            return (int)(offset + 2);
            Found1:
            return (int)(offset + 1);
            Found:
            return (int)offset;
        }

        public static unsafe int LastIndexOf(ref char searchSpace, char value, int length)
        {
            fixed (char* pChars = &searchSpace)
            {
                char* pCh = pChars + length;
                char* pEndCh = pChars;

                while (length >= 4)
                {
                    length -= 4;
                    pCh -= 4;

                    if (*(pCh + 3) == value)
                        goto Found3;
                    if (*(pCh + 2) == value)
                        goto Found2;
                    if (*(pCh + 1) == value)
                        goto Found1;
                    if (*pCh == value)
                        goto Found;
                }

                while (length > 0)
                {
                    length--;
                    pCh--;

                    if (*pCh == value)
                        goto Found;
                }

                return -1;
                Found:
                return (int)(pCh - pEndCh);
                Found1:
                return (int)(pCh - pEndCh) + 1;
                Found2:
                return (int)(pCh - pEndCh) + 2;
                Found3:
                return (int)(pCh - pEndCh) + 3;
            }
        }

        // Vector sub-search adapted from https://github.com/aspnet/KestrelHttpServer/pull/1138
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static int LocateFirstFoundChar(Vector<ushort> match)
        {
            var vector64 = Vector.AsVectorUInt64(match);
            ulong candidate = 0;
            int i = 0;
            // Pattern unrolled by jit https://github.com/dotnet/coreclr/pull/8001
            for (; i < Vector<ulong>.Count; i++)
            {
                candidate = vector64[i];
                if (candidate != 0)
                {
                    break;
                }
            }

            // Single LEA instruction with jitted const (using function result)
            return i * 4 + LocateFirstFoundChar(candidate);
        }

        private static int LocateFirstFoundChar(ulong match)
            => MiniBitOperations.TrailingZeroCount(match) >> 4;

        // Vector sub-search adapted from https://github.com/aspnet/KestrelHttpServer/pull/1138
        private static int LocateLastFoundChar(Vector<ushort> match)
        {
            var vector64 = Vector.AsVectorUInt64(match);
            ulong candidate = 0;
            int i = Vector<ulong>.Count - 1;
            // Pattern unrolled by jit https://github.com/dotnet/coreclr/pull/8001
            for (; i >= 0; i--)
            {
                candidate = vector64[i];
                if (candidate != 0)
                {
                    break;
                }
            }

            // Single LEA instruction with jitted const (using function result)
            return i * 4 + LocateLastFoundChar(candidate);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static int LocateLastFoundChar(ulong match)
            => MiniBitOperations.Log2(match) >> 4;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static Vector<ushort> LoadVector(ref char start, int offset)
            => Unsafe.ReadUnaligned<Vector<ushort>>(ref Unsafe.As<char, byte>(ref Unsafe.Add(ref start, offset)));

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static Vector<ushort> LoadVector(ref char start, uint offset)
            => Unsafe.ReadUnaligned<Vector<ushort>>(ref Unsafe.As<char, byte>(ref Unsafe.Add(ref start, (int)offset)));

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static ref char Add(ref char start, uint offset) => ref Unsafe.Add(ref start, (int)offset);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static int GetCharVectorSpanLength(int offset, int length)
            => (length - offset) & ~(Vector<ushort>.Count - 1);

        private static unsafe int UnalignedCountVector(ref char searchSpace)
        {
            const int ElementsPerByte = sizeof(ushort) / sizeof(byte);
            // Figure out how many characters to read sequentially until we are vector aligned
            // This is equivalent to:
            //         unaligned = ((int)pCh % Unsafe.SizeOf<Vector<ushort>>()) / ElementsPerByte
            //         length = (Vector<ushort>.Count - unaligned) % Vector<ushort>.Count

            // This alignment is only valid if the GC does not relocate; so we use ReadUnaligned to get the data.
            // If a GC does occur and alignment is lost, the GC cost will outweigh any gains from alignment so it
            // isn't too important to pin to maintain the alignment.
            return (int)(uint)(-(int)Unsafe.AsPointer(ref searchSpace) / ElementsPerByte) & (Vector<ushort>.Count - 1);
        }

    }
}
