using System;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.CompilerServices;

#pragma warning disable SA1204 // Static members should appear before non-static members (justification: Order is from original .NET implementation)
namespace Iot.Device.Arduino.Runtime
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
        private readonly int _stringLength;

        // This is the first char of the data this instance holds. The address of this field is the char* pointer, meaning that the actual string
        // data is stored inline, and the object has a dynamic size!
#pragma warning disable 414 // Used internally
        private char _firstChar;
#pragma warning restore 414

        [ArduinoImplementation(NativeMethod.StringCtor0)]
        public MiniString()
        {
            _stringLength = 0;
            _firstChar = '\0';
        }

        [ArduinoImplementation(NativeMethod.StringCtor1)]
        public unsafe MiniString(char* buf)
        {
            throw new NotImplementedException();
        }

        [ArduinoImplementation(NativeMethod.None)]
        public MiniString(ReadOnlySpan<char> value)
            : this(' ', value.Length)
        {
            for (int i = 0; i < value.Length; i++)
            {
                SetElem(i, value[i]);
            }
        }

        [ArduinoImplementation(NativeMethod.StringCtor2)]
        public MiniString(char c, int count)
        {
            throw new NotImplementedException();
        }

        [ArduinoImplementation]
        public MiniString(char[] value, int startIndex, int length)
        {
            if (value == null)
            {
                throw new ArgumentNullException(nameof(value));
            }

            if (startIndex < 0)
            {
                throw new ArgumentOutOfRangeException(nameof(startIndex));
            }

            if (length < 0)
            {
                throw new ArgumentOutOfRangeException(nameof(length));
            }

            if (startIndex > value.Length - length)
            {
                throw new ArgumentOutOfRangeException(nameof(startIndex));
            }

            if (length == 0)
            {
                return;
            }

            InternalAllocateString(length);
            for (int i = startIndex; i < startIndex + length; i++)
            {
                SetElem(i - startIndex, value[i]);
            }
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

        /////// <summary>
        /////// The purpose of this operator is to syntactically correctly perform a conversion from String to MiniString. The Implementation is a no-op.
        /////// </summary>
        ////[ArduinoImplementation(NativeMethod.StringImplicitConversion)]
        ////public static implicit operator MiniString(string other)
        ////{
        ////    throw new NotImplementedException();
        ////}

        [ArduinoImplementation]
        public static implicit operator ReadOnlySpan<char>(MiniString? value)
        {
            if (ReferenceEquals(value, null))
            {
                throw new ArgumentNullException(nameof(value));
            }

            char[] chars = new char[value.Length];
            for (int i = 0; i < value.Length; i++)
            {
                chars[i] = value.GetElem(i);
            }

            Span<char> s = new Span<char>(chars, 0, value.Length);
            return s;
        }

        public int Length
        {
            [ArduinoImplementation(NativeMethod.None)]
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

        [ArduinoImplementation(NativeMethod.StringCompareTo)]
        public int CompareTo(string? other)
        {
            throw new NotImplementedException();
        }

        [ArduinoImplementation]
        public int IndexOf(char c)
        {
            throw new NotImplementedException();
        }

        [ArduinoImplementation]
        public IEnumerator<char> GetEnumerator()
        {
            throw new NotImplementedException();
        }

        [ArduinoImplementation(NativeMethod.StringEquals)]
        public override bool Equals(object? obj)
        {
            throw new NotImplementedException();
        }

        [ArduinoImplementation(NativeMethod.None)]
        public bool Equals(string? other)
        {
            return Equals((object?)other);
        }

        [ArduinoImplementation(NativeMethod.StringEqualsStringComparison)]
        public bool Equals(string value, StringComparison comparisonType)
        {
            throw new NotImplementedException();
        }

        [ArduinoImplementation(NativeMethod.StringGetHashCode)]
        public override int GetHashCode()
        {
            throw new NotImplementedException();
        }

        [ArduinoImplementation(NativeMethod.StringToString)]
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

        [ArduinoImplementation(NativeMethod.StringSetElem)]
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

        [ArduinoImplementation(NativeMethod.StringGetElem)]
        private char GetElem(int idx)
        {
            throw new NotImplementedException();
        }

        [ArduinoImplementation(NativeMethod.StringGetPinnableReference)]
        public ref char GetPinnableReference()
        {
            throw new NotImplementedException();
        }

        [ArduinoImplementation(NativeMethod.StringEqualsStatic)]
        public static bool Equals(string? a, string? b)
        {
            throw new NotImplementedException();
        }

        [ArduinoImplementation(NativeMethod.StringEqualsStatic)]
        public static bool operator ==(MiniString a, MiniString b)
        {
            throw new NotImplementedException();
        }

        [ArduinoImplementation(NativeMethod.StringUnEqualsStatic)]
        public static bool operator !=(MiniString a, MiniString b)
        {
            throw new NotImplementedException();
        }

        [ArduinoImplementation(NativeMethod.None)]
        public TypeCode GetTypeCode()
        {
            return TypeCode.String;
        }

        public bool ToBoolean(IFormatProvider? provider)
        {
            // Our MiniString.ToString() operation is a no-op, but it does the implicit conversion from MiniString to String
            return Convert.ToBoolean(ToString(), provider);
            // throw new NotImplementedException();
        }

        public byte ToByte(IFormatProvider? provider)
        {
            return Convert.ToByte(ToString(), provider);
            // throw new NotImplementedException();
        }

        public char ToChar(IFormatProvider? provider)
        {
            return Convert.ToChar(ToString(), provider);
            // throw new NotImplementedException();
        }

        public DateTime ToDateTime(IFormatProvider? provider)
        {
            return Convert.ToDateTime(ToString(), provider);
            // throw new NotImplementedException();
        }

        public decimal ToDecimal(IFormatProvider? provider)
        {
            return Convert.ToDecimal(ToString(), provider);
            // throw new NotImplementedException();
        }

        public double ToDouble(IFormatProvider? provider)
        {
            return Convert.ToDouble(ToString(), provider);
            // throw new NotImplementedException();
        }

        public short ToInt16(IFormatProvider? provider)
        {
            return Convert.ToInt16(ToString(), provider);
            // throw new NotImplementedException();
        }

        public int ToInt32(IFormatProvider? provider)
        {
            return Convert.ToInt32(ToString(), provider);
            // throw new NotImplementedException();
        }

        public long ToInt64(IFormatProvider? provider)
        {
            return Convert.ToInt64(ToString(), provider);
            // throw new NotImplementedException();
        }

        public sbyte ToSByte(IFormatProvider? provider)
        {
            return Convert.ToSByte(ToString(), provider);
            // throw new NotImplementedException();
        }

        public float ToSingle(IFormatProvider? provider)
        {
            return Convert.ToSingle(ToString(), provider);
            // throw new NotImplementedException();
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

        [ArduinoImplementation(NativeMethod.StringFastAllocateString)]
        public static String FastAllocateString(int length)
        {
            throw new NotImplementedException();
        }

        [ArduinoImplementation(NativeMethod.StringGetRawStringData)]
        public ref char GetRawStringData()
        {
            throw new NotImplementedException();
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
        [ArduinoImplementation(NativeMethod.StringInternalAllocateString)]
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
