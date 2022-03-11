// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using Iot.Device.Arduino;

#pragma warning disable SA1204 // Static members should appear before non-static members (justification: Order is from original .NET implementation)
namespace ArduinoCsCompiler.Runtime
{
    /// <summary>
    /// Mini-Implementation for the System.String type. Uses unicode characters (16 bits per char). To avoid including rarely used functions with the kernel, this
    /// does not implement <see cref="IConvertible"/>, but is otherwise equivalent to the original implementation.
    /// </summary>
    [ArduinoReplacement(typeof(System.String), false, IncludingPrivates = true)]
    internal unsafe partial class MiniString : ICloneable, IComparable, IComparable<string>, IEquatable<string>, System.Collections.Generic.IEnumerable<char>
    {
#pragma warning disable SA1122 // Use string.Empty for empty strings (This is the definition of an empty string!)
        public static string Empty = "";
#pragma warning restore SA1122

#pragma warning disable CS0649 // Used internally
        private int _stringLength;

        // This is the first char of the data this instance holds. The address of this field is the char* pointer, meaning that the actual string
        // data is stored inline, and the object has a dynamic size!
        private char _firstChar;
#pragma warning restore CS0649

        /// <summary>
        /// This needs support in the backend as well, because the size of the allocated object must be determined beforehand
        /// </summary>
        [ArduinoImplementation("StringCtorSpan", 7)]
        public MiniString(ReadOnlySpan<char> value)
        {
            throw new NotImplementedException();
        }

        [ArduinoImplementation("StringCtorCharCount", 8)]
        public MiniString(char c, int count)
        {
            throw new NotImplementedException();
        }

        [ArduinoImplementation("StringCtorCharArray", 9)]
        public MiniString(char[] value, int startIndex, int length)
        {
            throw new NotImplementedException();
        }

        [ArduinoImplementation("StringCtorCharPtr", 23)]
        public unsafe MiniString(char* value)
        {
            throw new NotImplementedException();
        }

        [ArduinoImplementation("StringCtorCharPtr3", 19)]
        public unsafe MiniString(char* value, int startIndex, int length)
        {
            throw new NotImplementedException();
        }

        // This is a bit odd: All of these default constructors on System::String are never actually called, because the runtime handles string construction specially.
        /*
        [ArduinoImplementation("StringCtor0")]
        public MiniString()
        {
            _stringLength = 0;
            _firstChar = '\0';
        }

        [ArduinoImplementation("StringCtor1")]
        public unsafe MiniString(char* buf)
        {
            throw new NotImplementedException();
        }

        [ArduinoImplementation]
        public MiniString(char[]? value)
        {
            if (value == null || value.Length == 0)
            {
                return;
            }

            InternalAllocateString(value.Length);
            for (int i = 0; i < value.Length; i++)
            {
                SetElem(i, value[i]);
            }
        }
        */
        /////// <summary>
        /////// The purpose of this operator is to syntactically correctly perform a conversion from String to MiniString. The Implementation is a no-op.
        /////// </summary>
        ////[ArduinoImplementation("StringImplicitConversion")]
        ////public static implicit operator MiniString(string other)
        ////{
        ////    throw new NotImplementedException();
        ////}

        [ArduinoImplementation]
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static unsafe implicit operator ReadOnlySpan<char>(MiniString? value)
        {
            if (!ReferenceEquals(value, null))
            {
                // Unfortunately, the ctor we would need here is internal, but
                // this should give the same result. Since we keep the reference to value,
                // there is no problem with exiting out of the fixed block.
                fixed (void* ptr = &value._firstChar)
                {
                    return new ReadOnlySpan<char>(ptr, value.Length);
                }
            }
            else
            {
                return default;
            }
        }

        public int Length
        {
            [ArduinoImplementation]
            get
            {
                return _stringLength;
            }
        }

        [IndexerName("Chars")]
        public Char this[int index]
        {
            [ArduinoImplementation]
            get
            {
                return GetElem(index);
            }
        }

        [ArduinoImplementation("StringCompareTo", 10)]
        public int CompareTo(string? other)
        {
            throw new NotImplementedException();
        }

        [ArduinoImplementation]
        public IEnumerator<char> GetEnumerator()
        {
            throw new NotImplementedException();
        }

        [ArduinoImplementation("StringEquals", 11)]
        public override bool Equals(object? obj)
        {
            throw new NotImplementedException();
        }

        [ArduinoImplementation]
        public bool Equals(string? other)
        {
            return Equals((object?)other);
        }

        [ArduinoImplementation("StringEqualsStringComparison", 12)]
        public bool Equals(string value, StringComparison comparisonType)
        {
            throw new NotImplementedException();
        }

        [ArduinoImplementation("StringGetHashCode", 13)]
        public override int GetHashCode()
        {
            throw new NotImplementedException();
        }

        [ArduinoImplementation("StringToString", 14)]
        public override string ToString()
        {
            // This should simply do a "return this", but we can't do that here, because the types don't match
            throw new NotImplementedException();
        }

        [ArduinoImplementation]
        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }

        public int CompareTo(object? obj)
        {
            if (obj == null)
            {
                return 1;
            }

            return CompareTo((string)obj);
        }

        [ArduinoImplementation]
        public object Clone()
        {
            return this; // String is immutable
        }

        [ArduinoImplementation("StringSetElem", 15)]
        private void SetElem(int idx, char c)
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// Write access to an immutable string!
        /// </summary>
        private static void SetElem(String str, int idx, char c)
        {
            throw new NotImplementedException();
        }

        [ArduinoImplementation("StringGetElem", 16)]
        private char GetElem(int idx)
        {
            throw new NotImplementedException();
        }

        [ArduinoImplementation("StringGetPinnableReference", 17)]
        public ref char GetPinnableReference()
        {
            throw new NotImplementedException();
        }

        [ArduinoImplementation("StringEqualsStatic", 18)]
        public static bool Equals(string? a, string? b)
        {
            throw new NotImplementedException();
        }

        [ArduinoImplementation("StringEqualsStatic", 18)]
        public static bool operator ==(MiniString? a, MiniString? b)
        {
            throw new NotImplementedException();
        }

        [ArduinoImplementation("StringUnEqualsStatic", 20)]
        public static bool operator !=(MiniString? a, MiniString? b)
        {
            throw new NotImplementedException();
        }

        [ArduinoImplementation]
        public TypeCode GetTypeCode()
        {
            return TypeCode.String;
        }

        public bool ToBoolean(IFormatProvider? provider)
        {
            return Convert.ToBoolean(ToString(), provider);
        }

        public byte ToByte(IFormatProvider? provider)
        {
            return Convert.ToByte(ToString(), provider);
        }

        public char ToChar(IFormatProvider? provider)
        {
            return Convert.ToChar(ToString(), provider);
        }

        public DateTime ToDateTime(IFormatProvider? provider)
        {
            return Convert.ToDateTime(ToString(), provider);
        }

        public decimal ToDecimal(IFormatProvider? provider)
        {
            return Convert.ToDecimal(ToString(), provider);
        }

        public double ToDouble(IFormatProvider? provider)
        {
            return Convert.ToDouble(ToString(), provider);
        }

        public short ToInt16(IFormatProvider? provider)
        {
            return Convert.ToInt16(ToString(), provider);
        }

        public int ToInt32(IFormatProvider? provider)
        {
            return Convert.ToInt32(ToString(), provider);
        }

        public long ToInt64(IFormatProvider? provider)
        {
            return Convert.ToInt64(ToString(), provider);
        }

        public sbyte ToSByte(IFormatProvider? provider)
        {
            return Convert.ToSByte(ToString(), provider);
        }

        public float ToSingle(IFormatProvider? provider)
        {
            return Convert.ToSingle(ToString(), provider);
        }

        public string ToString(IFormatProvider? provider)
        {
            return ToString();
        }

        public object ToType(Type? conversionType, IFormatProvider? provider)
        {
            throw new NotImplementedException();
        }

        public ushort ToUInt16(IFormatProvider? provider)
        {
            throw new NotImplementedException();
        }

        public uint ToUInt32(IFormatProvider? provider)
        {
            throw new NotImplementedException();
        }

        public ulong ToUInt64(IFormatProvider? provider)
        {
            throw new NotImplementedException();
        }

        [ArduinoImplementation("StringFastAllocateString", 21)]
        public static String FastAllocateString(int length)
        {
            throw new NotImplementedException();
        }

        public ref char GetRawStringData()
        {
            return ref _firstChar;
        }

        // This is only intended to be used by char.ToString.
        // It is necessary to put the code in this class instead of Char, since _firstChar is a private member.
        // Making _firstChar internal would be dangerous since it would make it much easier to break String's immutability.
        internal static String CreateFromChar(char c)
        {
            String result = FastAllocateString(1);
            SetElem(result, 0, c);
            return result;
        }

        /// <summary>
        /// Allocates memory for the internal _data pointer
        /// </summary>
        /// <param name="length">Length, in chars</param>
        [ArduinoImplementation("StringInternalAllocateString", 22)]
        private void InternalAllocateString(int length)
        {
            throw new NotImplementedException();
        }

        internal static unsafe bool TryGetSpan(MiniString value, int startIndex, int count, out ReadOnlySpan<char> slice)
        {
            if ((uint)startIndex > (uint)value.Length || (uint)count > (uint)(value.Length - startIndex))
            {
                slice = default;
                return false;
            }

            // Hope this does the same as the CLR implementation
            fixed (void* ptr = &value._firstChar)
            {
                slice = new ReadOnlySpan<char>(ptr, count);
            }

            return true;
        }
    }
}
