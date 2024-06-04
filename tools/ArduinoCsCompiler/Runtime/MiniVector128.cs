// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Diagnostics;
using System.Globalization;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Runtime.Intrinsics.X86;
using System.Runtime.Intrinsics;
using System.Text;
using System.Threading.Tasks;

#pragma warning disable SA1306 // Naming convention
#pragma warning disable SA1204 // Method ordering

namespace ArduinoCsCompiler.Runtime
{
    [StructLayout(LayoutKind.Sequential, Size = 16)]
    public readonly struct Vector128<T> : IEquatable<Vector128<T>>
        where T : struct
    {
        // These fields exist to ensure the alignment is 8, rather than 1.
        // This also allows the debug view to work https://github.com/dotnet/runtime/issues/9495)
        private readonly ulong _00;
        private readonly ulong _01;

        public Vector128(ulong f1, ulong f2)
        {
            _00 = f1;
            _01 = f2;
        }

        /// <summary>Gets the number of <typeparamref name="T" /> that are in a <see cref="Vector128{T}" />.</summary>
        /// <exception cref="NotSupportedException">The type of the current instance (<typeparamref name="T" />) is not supported.</exception>
        public static int Count
        {
            get
            {
                return 16 / Unsafe.SizeOf<T>();
            }
        }

        /// <summary>Gets a new <see cref="Vector128{T}" /> with all elements initialized to zero.</summary>
        /// <exception cref="NotSupportedException">The type of the current instance (<typeparamref name="T" />) is not supported.</exception>
        public static Vector128<T> Zero
        {
            get
            {
                return default;
            }
        }

        /// <summary>Gets a new <see cref="Vector128{T}" /> with all bits set to 1.</summary>
        /// <exception cref="NotSupportedException">The type of the current instance (<typeparamref name="T" />) is not supported.</exception>
        public static Vector128<T> AllBitsSet
        {
            get
            {
                Vector128<T> newVector = new Vector128<T>(UInt64.MaxValue, UInt64.MaxValue);
                return newVector;
            }
        }

        internal string DisplayString
        {
            get
            {
                if (IsSupported)
                {
                    return ToString();
                }
                else
                {
                    return string.Empty;
                }
            }
        }

        internal static bool IsSupported
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get => (typeof(T) == typeof(byte)) ||
                   (typeof(T) == typeof(sbyte)) ||
                   (typeof(T) == typeof(short)) ||
                   (typeof(T) == typeof(ushort)) ||
                   (typeof(T) == typeof(int)) ||
                   (typeof(T) == typeof(uint)) ||
                   (typeof(T) == typeof(long)) ||
                   (typeof(T) == typeof(ulong)) ||
                   (typeof(T) == typeof(float)) ||
                   (typeof(T) == typeof(double));
        }

        /// <summary>Determines whether the specified <see cref="Vector128{T}" /> is equal to the current instance.</summary>
        /// <param name="other">The <see cref="Vector128{T}" /> to compare with the current instance.</param>
        /// <returns><c>true</c> if <paramref name="other" /> is equal to the current instance; otherwise, <c>false</c>.</returns>
        /// <exception cref="NotSupportedException">The type of the current instance (<typeparamref name="T" />) is not supported.</exception>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool Equals(Vector128<T> other)
        {
            return SoftwareFallback(in this, other);

            static bool SoftwareFallback(in Vector128<T> vector, Vector128<T> other)
            {
                for (int i = 0; i < Count; i++)
                {
                    if (!((IEquatable<T>)(GetElement(vector, i))).Equals(GetElement(other, i)))
                    {
                        return false;
                    }
                }

                return true;
            }
        }

        /// <summary>Determines whether the specified object is equal to the current instance.</summary>
        /// <param name="obj">The object to compare with the current instance.</param>
        /// <returns><c>true</c> if <paramref name="obj" /> is a <see cref="Vector128{T}" /> and is equal to the current instance; otherwise, <c>false</c>.</returns>
        /// <exception cref="NotSupportedException">The type of the current instance (<typeparamref name="T" />) is not supported.</exception>
        public override bool Equals([NotNullWhen(true)] object? obj)
        {
            return (obj is Vector128<T>) && Equals((Vector128<T>)(obj));
        }

        public static T1 GetElement<T1>(Vector128<T1> vector, int index)
            where T1 : struct
        {
            if ((uint)(index) >= (uint)(Vector128<T1>.Count))
            {
                throw new ArgumentOutOfRangeException(nameof(index));
            }

            ref T1 e0 = ref Unsafe.As<Vector128<T1>, T1>(ref vector);
            return Unsafe.Add(ref e0, index);
        }

        /// <summary>Gets the hash code for the instance.</summary>
        /// <returns>The hash code for the instance.</returns>
        /// <exception cref="NotSupportedException">The type of the current instance (<typeparamref name="T" />) is not supported.</exception>
        public override int GetHashCode()
        {
            HashCode hashCode = default;

            for (int i = 0; i < Count; i++)
            {
                hashCode.Add(GetElement(this, i).GetHashCode());
            }

            return hashCode.ToHashCode();
        }

        /// <summary>Converts the current instance to an equivalent string representation.</summary>
        /// <returns>An equivalent string representation of the current instance.</returns>
        /// <exception cref="NotSupportedException">The type of the current instance (<typeparamref name="T" />) is not supported.</exception>
        public override string ToString()
        {
            int lastElement = Count - 1;
            var sb = new StringBuilder();
            CultureInfo invariant = CultureInfo.InvariantCulture;

            sb.Append('<');
            for (int i = 0; i < lastElement; i++)
            {
                sb.Append(((IFormattable)GetElement(this, i)).ToString("G", invariant));
                sb.Append(',');
                sb.Append(' ');
            }

            sb.Append(((IFormattable)GetElement(this, lastElement)).ToString("G", invariant));
            sb.Append('>');

            return sb.ToString();
        }
    }
}
