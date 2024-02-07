// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System;
using System.Collections;
using System.Collections.Generic;
using Iot.Device.Arduino;

#pragma warning disable CA2208 // ArgumentException should be used with proper parameters (No: Saves memory)
namespace ArduinoCsCompiler.Runtime
{
    [ArduinoReplacement(typeof(System.Array), false, IncludingPrivates = true)]
    internal class MiniArray
    {
        public int Length
        {
            [ArduinoImplementation("ArrayGetLength", 50)]
            get
            {
                throw new NotImplementedException();
            }
        }

        public long LongLength => Length;

        public nuint NativeLength
        {
            [ArduinoImplementation]
            get
            {
                return (nuint)Length;
            }
        }

        [ArduinoImplementation]
        public static void Copy(Array sourceArray, Array destinationArray, long length)
        {
            int ilength = (int)length;
            if (length != ilength)
            {
                throw new ArgumentOutOfRangeException();
            }

            Copy(sourceArray, destinationArray, ilength);
        }

        [ArduinoImplementation]
        public static void Copy(Array sourceArray, Array destinationArray, int length)
        {
            if (sourceArray == null)
            {
                throw new ArgumentNullException();
            }

            if (destinationArray == null)
            {
                throw new ArgumentNullException();
            }

            Copy(sourceArray, sourceArray.GetLowerBound(0), destinationArray, destinationArray.GetLowerBound(0), length, reliable: false);
        }

        // Copies length elements from sourceArray, starting at sourceIndex, to
        // destinationArray, starting at destinationIndex.
        [ArduinoImplementation]
        public static void Copy(Array sourceArray, int sourceIndex, Array destinationArray, int destinationIndex, int length)
        {
            // Less common
            Copy(sourceArray!, sourceIndex, destinationArray!, destinationIndex, length, reliable: false);
        }

        [ArduinoImplementation]
        private static void Copy(Array sourceArray, int sourceIndex, Array destinationArray, int destinationIndex, int length, bool reliable)
        {
            if (sourceArray == null || destinationArray == null)
            {
                throw new ArgumentNullException();
            }

            if (sourceArray.GetType() != destinationArray.GetType() && sourceArray.Rank != destinationArray.Rank)
            {
                throw new RankException();
            }

            if (length < 0)
            {
                throw new ArgumentOutOfRangeException(nameof(length), "Argument out of Range");
            }

            int srcLB = sourceArray.GetLowerBound(0);
            if (sourceIndex < srcLB || sourceIndex - srcLB < 0)
            {
                throw new ArgumentOutOfRangeException(nameof(sourceIndex), "Argument out of Range");
            }

            sourceIndex -= srcLB;

            int dstLB = destinationArray.GetLowerBound(0);
            if (destinationIndex < dstLB || destinationIndex - dstLB < 0)
            {
                throw new ArgumentOutOfRangeException(nameof(destinationIndex), "Argument out of Range");
            }

            destinationIndex -= dstLB;

            if ((uint)(sourceIndex + length) > (nuint)sourceArray.LongLength)
            {
                throw new ArgumentException();
            }

            if ((uint)(destinationIndex + length) > (nuint)destinationArray.LongLength)
            {
                throw new ArgumentException();
            }

            // The standard runtime does allow this under certain conditions. For simplicity, we don't.
            if (sourceArray.GetType() != destinationArray.GetType())
            {
                throw new ArrayTypeMismatchException();
            }

            CopyCore(sourceArray, sourceIndex, destinationArray, destinationIndex, length);
        }

        [ArduinoImplementation]
        public static void Copy(Array sourceArray, long sourceIndex, Array destinationArray, long destinationIndex, long length)
        {
            int isourceIndex = (int)sourceIndex;
            int idestinationIndex = (int)destinationIndex;
            int ilength = (int)length;

            Copy(sourceArray, isourceIndex, destinationArray, idestinationIndex, ilength);
        }

        [ArduinoImplementation("ArrayCopyCore", 51)]
        private static void CopyCore(Array sourceArray, int sourceIndex, Array destinationArray, int destinationIndex, int length)
        {
            throw new NotImplementedException();
        }

        [ArduinoImplementation("ArrayClear", 52)]
        public static void Clear(Array array, int index, int length)
        {
            throw new NotImplementedException();
        }

        [ArduinoImplementation]
        public static unsafe Array CreateInstance(Type elementType, int length)
        {
            if (elementType is null)
            {
                throw new ArgumentNullException(nameof(elementType));
            }

            if (length < 0)
            {
                throw new ArgumentOutOfRangeException();
            }

            return InternalCreate(elementType, 1, &length, null);
        }

        [ArduinoImplementation]
        public static unsafe Array CreateInstance(Type elementType, int length1, int length2)
        {
            if (elementType is null)
            {
                throw new ArgumentNullException(nameof(elementType));
            }

            if (length1 < 0)
            {
                throw new ArgumentOutOfRangeException();
            }

            if (length2 < 0)
            {
                throw new ArgumentOutOfRangeException();
            }

            int* pLengths = stackalloc int[2];
            pLengths[0] = length1;
            pLengths[1] = length2;
            return InternalCreate(elementType, 2, pLengths, null);
        }

        [ArduinoImplementation]
        public static unsafe Array CreateInstance(Type elementType, int length1, int length2, int length3)
        {
            if (elementType is null)
            {
                throw new ArgumentNullException(nameof(elementType));
            }

            if (length1 < 0)
            {
                throw new ArgumentOutOfRangeException();
            }

            if (length2 < 0)
            {
                throw new ArgumentOutOfRangeException();
            }

            if (length3 < 0)
            {
                throw new ArgumentOutOfRangeException();
            }

            int* pLengths = stackalloc int[3];
            pLengths[0] = length1;
            pLengths[1] = length2;
            pLengths[2] = length3;
            return InternalCreate(elementType, 3, pLengths, null);
        }

        [ArduinoImplementation]
        public static unsafe Array CreateInstance(Type elementType, params int[] lengths)
        {
            if (elementType is null)
            {
                throw new ArgumentNullException(nameof(elementType));
            }

            if (lengths == null)
            {
                throw new ArgumentNullException(nameof(lengths));
            }

            if (lengths.Length == 0)
            {
                throw new ArgumentOutOfRangeException(nameof(lengths));
            }

            // Check to make sure the lengths are all positive. Note that we check this here to give
            // a good exception message if they are not; however we check this again inside the execution
            // engine's low level allocation function after having made a copy of the array to prevent a
            // malicious caller from mutating the array after this check.
            for (int i = 0; i < lengths.Length; i++)
            {
                if (lengths[i] < 0)
                {
                    throw new ArgumentOutOfRangeException(nameof(lengths));
                }
            }

            fixed (int* pLengths = &lengths[0])
            {
                return InternalCreate(elementType, lengths.Length, pLengths, null);
            }
        }

        [ArduinoImplementation]
        public static unsafe Array CreateInstance(Type elementType, int[] lengths, int[] lowerBounds)
        {
            if (elementType is null)
            {
                throw new ArgumentNullException(nameof(elementType));
            }

            if (lengths == null)
            {
                throw new ArgumentNullException(nameof(lengths));
            }

            if (lengths.Length == 0)
            {
                throw new ArgumentOutOfRangeException(nameof(lengths));
            }

            if (lowerBounds == null)
            {
                throw new ArgumentNullException(nameof(lowerBounds));
            }

            if (lowerBounds.Length != lengths.Length)
            {
                throw new ArgumentOutOfRangeException(nameof(lowerBounds));
            }

            // Check to make sure the lengths are all positive. Note that we check this here to give
            // a good exception message if they are not; however we check this again inside the execution
            // engine's low level allocation function after having made a copy of the array to prevent a
            // malicious caller from mutating the array after this check.
            for (int i = 0; i < lengths.Length; i++)
            {
                if (lengths[i] < 0)
                {
                    throw new ArgumentOutOfRangeException(nameof(lengths));
                }
            }

            throw new NotImplementedException();
            // fixed (int* pLengths = &lengths[0])
            // fixed (int* pLowerBounds = &lowerBounds[0])
            //    return InternalCreate((void*)t.TypeHandle.Value, lengths.Length, pLengths, pLowerBounds);
        }

        [ArduinoImplementation("ArrayInternalCreate", 53)]
        private static unsafe Array InternalCreate(Type elementType, int rank, int* pLengths, int* pLowerBounds)
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// Used by the backend to construct an instance of <see cref="ArrayIterator{T}"/>
        /// </summary>
        public static IEnumerator<T> GetEnumerator<T>(T[] array)
        {
            return new ArrayIterator<T>(array);
        }

        [ArduinoImplementation]
        public static void Clear(Array array)
        {
            Clear(array, 0, array.Length);
        }

        [ArduinoImplementation("ArraySetValue1", 54)]
        public void SetValue(object? value, int index)
        {
            throw new NotImplementedException();
        }

        [ArduinoImplementation("ArrayGetValue1", 55)]
        public object GetValue(int index)
        {
            throw new NotImplementedException();
        }

        [ArduinoImplementation("ArrayGetValue1", 55)]
        public object InternalGetValue(int index)
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// This class is used to implement the iterator on T[] that is returned on the implicitly generated GetEnumerator()
        /// </summary>
        internal class ArrayIterator<T> : IEnumerator<T>
        {
            private T[] _array;
            private int _current;
            public ArrayIterator(T[] array)
            {
                _array = array;
                _current = -1;
            }

            public bool MoveNext()
            {
                _current++;
                return _current < _array.Length;
            }

            public void Reset()
            {
                _current = -1;
            }

            public T Current
            {
                get
                {
                    return _array[_current];
                }
            }

            object IEnumerator.Current => Current!;

            public void Dispose()
            {
            }
        }
    }
}
