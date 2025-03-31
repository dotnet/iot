// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System;
using System.Runtime.CompilerServices;
using Iot.Device.Arduino;

namespace ArduinoCsCompiler.Runtime
{
    [ArduinoReplacement("System.Runtime.CompilerServices.Unsafe", "System.Private.CoreLib.dll", true, IncludingPrivates = true)]
    internal unsafe class MiniUnsafe
    {
        /// <summary>
        /// This method just unsafely casts object to T. The underlying implementation just does a "return this" without any type test.
        /// The implementation of the following two methods is identical, therefore it doesn't really matter which one we match.
        /// </summary>
        [ArduinoImplementation("UnsafeAs2", 0x20, IgnoreGenericTypes = true, MergeGenericImplementations = true)]
        public static T As<T>(object? value)
            where T : class?
        {
            throw new PlatformNotSupportedException();

            // ldarg.0
            // ret
        }

        [ArduinoImplementation("UnsafeAs2", 0x20, MergeGenericImplementations = true)]
        public static ref TTo As<TFrom, TTo>(ref TFrom source)
        {
            throw new PlatformNotSupportedException();

            // ldarg.0
            // ret
        }

        [ArduinoImplementation("UnsafeAsPointer", 0x21, MergeGenericImplementations = true)]
        public static void* AsPointer<T>(ref T value)
        {
            throw new PlatformNotSupportedException();

            // ldarg.0
            // conv.u
            // ret
        }

        /// <summary>
        /// Determines the byte offset from origin to target from the given references.
        /// </summary>
        [ArduinoImplementation("UnsafeByteOffset", 0x22, CompareByParameterNames = true, MergeGenericImplementations = true)]
        public static IntPtr ByteOffset<T>(ref T origin, ref T target)
        {
            throw new PlatformNotSupportedException();
        }

        [ArduinoImplementation("UnsafeAreSame", 0x23, CompareByParameterNames = true, MergeGenericImplementations = true)]
        public static bool AreSame<T>(ref T left, ref T right)
        {
            throw new PlatformNotSupportedException();

            // ldarg.0
            // ldarg.1
            // ceq
            // ret
        }

        /// <summary>
        /// Reinterprets the given value of type <typeparamref name="TFrom" /> as a value of type <typeparamref name="TTo" />.
        /// </summary>
        /// <exception cref="NotSupportedException">The size of <typeparamref name="TFrom" /> and <typeparamref name="TTo" /> are not the same.</exception>
        [ArduinoImplementation]
        public static TTo BitCast<TFrom, TTo>(TFrom source)
            where TFrom : struct
            where TTo : struct
        {
#pragma warning disable CS8500 // Erfasst die Adresse, ermittelt die Größe oder deklariert einen Zeiger auf einen verwalteten Typ.
            if (sizeof(TFrom) != sizeof(TTo))
            {
                throw new NotSupportedException();
            }
#pragma warning restore CS8500 // Erfasst die Adresse, ermittelt die Größe oder deklariert einen Zeiger auf einen verwalteten Typ.

            return ReadUnaligned<TTo>(ref As<TFrom, byte>(ref source));
        }

        [ArduinoImplementation(CompareByParameterNames = true, MergeGenericImplementations = true)]
        public static bool IsNullRef<T>(ref T source)
        {
            return AsPointer(ref source) == null;

            // ldarg.0
            // ldc.i4.0
            // conv.u
            // ceq
            // ret
        }

        [ArduinoImplementation(CompareByParameterNames = true)]
        public static ref T Add<T>(ref T source, int elementOffset)
        {
            return ref AddByteOffset(ref source, (IntPtr)(elementOffset * (int)SizeOf<T>()));
        }

        [ArduinoImplementation]
        public static void* Add<T>(void* source, int elementOffset)
        {
            return (byte*)source + (elementOffset * (int)SizeOf<T>());
        }

        /// <summary>
        /// Adds an element offset to the given reference.
        /// </summary>
        [ArduinoImplementation]
        public static ref T Add<T>(ref T source, IntPtr elementOffset)
        {
            return ref AddByteOffset(ref source, (IntPtr)((uint)elementOffset * (uint)SizeOf<T>()));
        }

        public static ref T Subtract<T>(ref T source, UIntPtr elementOffset)
        {
            return ref SubtractByteOffset(ref source, (nuint)(elementOffset * (uint)SizeOf<T>()));

            // ldarg .0
            // ldarg .1
            // sizeof !!T
            // mul
            // sub
            // ret
        }

        public static ref T SubtractByteOffset<T>(ref T source, nuint byteOffset)
        {
            return ref SubtractByteOffset(ref source, (IntPtr)(void*)byteOffset);
            // ldarg .0
            // ldarg .1
            // sub
            // ret
        }

        [ArduinoImplementation("UnsafeSubtractByteOffset", MergeGenericImplementations = true)]
        public static ref T SubtractByteOffset<T>(ref T source, IntPtr byteOffset)
        {
            throw new PlatformNotSupportedException();
        }

        [ArduinoImplementation("UnsafeAddByteOffset", 0x24, MergeGenericImplementations = true)]
        public static ref T AddByteOffset<T>(ref T source, IntPtr byteOffset)
        {
            // This method is implemented by the toolchain
            throw new PlatformNotSupportedException();

            // ldarg.0
            // ldarg.1
            // add
            // ret
        }

        [ArduinoImplementation(MergeGenericImplementations = true)]
        public static ref T AddByteOffset<T>(ref T source, uint byteOffset)
        {
            return ref AddByteOffset(ref source, (IntPtr)(void*)byteOffset);
        }

        [ArduinoImplementation]
        public static int SizeOf<T>()
        {
            return SizeOfType(typeof(T));
            // sizeof !!0
            // ret
        }

        [ArduinoImplementation("UnsafeSizeOfType", 0x25)]
        public static int SizeOfType(Type t)
        {
            throw new NotImplementedException();
        }

        [ArduinoImplementation(CompareByParameterNames = true, MergeGenericImplementations = true)]
        public static ref T AsRef<T>(void* source)
        {
            return ref As<byte, T>(ref *(byte*)source);
        }

        [ArduinoImplementation]
        public static ref T AsRef<T>(in T source)
        {
            throw new PlatformNotSupportedException();
        }

        [ArduinoImplementation("UnsafeNullRef", 0x26, MergeGenericImplementations = true)]
        public static ref T NullRef<T>()
        {
            return ref AsRef<T>(null);

            // ldc.i4.0
            // conv.u
            // ret
        }

        [ArduinoImplementation(CompareByParameterNames = true)]
        public static T Read<T>(void* source)
        {
            return As<byte, T>(ref *(byte*)source);
        }

        /// <summary>
        /// Determines whether the memory address referenced by <paramref name="left"/> is greater than
        /// the memory address referenced by <paramref name="right"/>.
        /// </summary>
        /// <remarks>
        /// This check is conceptually similar to "(void*)(&amp;left) &gt; (void*)(&amp;right)".
        /// </remarks>
        [ArduinoImplementation("UnsafeIsAddressGreaterThan", 0x18, CompareByParameterNames = true, MergeGenericImplementations = true)]
        public static bool IsAddressGreaterThan<T>(ref T left, ref T right)
        {
            throw new PlatformNotSupportedException();

            // ldarg.0
            // ldarg.1
            // cgt.un
            // ret
        }

        /// <summary>
        /// Determines whether the memory address referenced by <paramref name="left"/> is less than
        /// the memory address referenced by <paramref name="right"/>.
        /// </summary>
        /// <remarks>
        /// This check is conceptually similar to "(void*)(&amp;left) &lt; (void*)(&amp;right)".
        /// </remarks>
        [ArduinoImplementation("UnsafeIsAddressLessThan", 0x27, CompareByParameterNames = true)]
        public static bool IsAddressLessThan<T>(ref T left, ref T right)
        {
            throw new PlatformNotSupportedException();

            // ldarg.0
            // ldarg.1
            // clt.un
            // ret
        }

        [ArduinoImplementation]
        public static void InitBlockUnaligned(ref byte startAddress, byte value, uint byteCount)
        {
            for (uint i = 0; i < byteCount; i++)
            {
                AddByteOffset(ref startAddress, i) = value;
            }
        }

        /// <summary>
        /// Writes a value of type <typeparamref name="T"/> to the given location.
        /// </summary>
        [ArduinoImplementation(CompareByParameterNames = true)]
        public static void WriteUnaligned<T>(void* destination, T value)
        {
            As<byte, T>(ref *(byte*)destination) = value;
        }

        /// <summary>
        /// Writes a value of type <typeparamref name="T"/> to the given location.
        /// </summary>
        [ArduinoImplementation(CompareByParameterNames = true)]
        public static void WriteUnaligned<T>(ref byte destination, T value)
        {
            As<byte, T>(ref destination) = value;
        }

        [ArduinoImplementation]
        public static T ReadUnaligned<T>(void* source)
        {
            return As<byte, T>(ref *(byte*)source);
        }

        [ArduinoImplementation]
        public static T ReadUnaligned<T>(ref byte source)
        {
            return As<byte, T>(ref source);
        }

        [ArduinoImplementation("UnsafeSkipInit", 0x28)]
        public static void SkipInit<T>(out T value)
        {
            throw new PlatformNotSupportedException();
        }

        [ArduinoImplementation("UnsafeCopyBlockUnaligned", 0x29)]
        public static void CopyBlockUnaligned(ref byte destination, ref readonly byte source, uint byteCount)
        {
            throw new PlatformNotSupportedException();

            // ldarg .0
            // ldarg .1
            // ldarg .2
            // unaligned. 0x1
            // cpblk
            // ret
        }
    }
}
