using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;

namespace Iot.Device.Arduino
{
    [ArduinoReplacement(typeof(System.String), IncludingPrivates = true)]
    internal unsafe partial class MiniString : ICloneable, IComparable, IComparable<string>, IConvertible, IEquatable<string>, System.Collections.Generic.IEnumerable<char>
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

        [ArduinoImplementation(ArduinoImplementation.StringCtor0)]
        public MiniString()
        {
            _stringLength = 0;
            _firstChar = '\0';
        }

        [ArduinoImplementation(ArduinoImplementation.StringCtor1)]
        public unsafe MiniString(char* buf)
        {
            throw new NotImplementedException();
        }

        [ArduinoImplementation(ArduinoImplementation.None)]
        public MiniString(ReadOnlySpan<char> value)
            : this(' ', value.Length)
        {
            for (int i = 0; i < value.Length; i++)
            {
                SetElem(i, value[i]);
            }
        }

        [ArduinoImplementation(ArduinoImplementation.StringCtor2)]
        public MiniString(char c, int count)
        {
            throw new NotImplementedException();
        }

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

        public static implicit operator ReadOnlySpan<char>(MiniString? value)
        {
            if (ReferenceEquals(value, null))
            {
                throw new ArgumentNullException(nameof(value));
            }

            char[] chars = new char[value.Length];
            for (int i = 0; i < value.Length; i++)
            {
                chars[i] = value[i];
            }

            Span<char> s = new Span<char>(chars, 0, value.Length);
            return s;
        }

        public int Length
        {
            get
            {
                return _stringLength;
            }
        }

        [IndexerName("Chars")]
        public Char this[int index]
        {
            get
            {
                return GetElem(index);
            }
        }

        public int CompareTo(string? other)
        {
            throw new NotImplementedException();
        }

        public int IndexOf(char c)
        {
            throw new NotImplementedException();
        }

        public IEnumerator<char> GetEnumerator()
        {
            throw new NotImplementedException();
        }

        [ArduinoImplementation(ArduinoImplementation.StringEquals)]
        public override bool Equals(object? obj)
        {
            throw new NotImplementedException();
        }

        [ArduinoImplementation(ArduinoImplementation.None)]
        public bool Equals(string? other)
        {
            return Equals((object?)other);
        }

        [ArduinoImplementation(ArduinoImplementation.StringEqualsStringComparison)]
        public bool Equals(string value, StringComparison comparisonType)
        {
            throw new NotImplementedException();
        }

        [ArduinoImplementation(ArduinoImplementation.StringGetHashCode)]
        public override int GetHashCode()
        {
            throw new NotImplementedException();
        }

        [ArduinoImplementation(ArduinoImplementation.StringToString)]
        public override string ToString()
        {
            // This should simply do a "return this", but we can't do that here, because the types don't match
            throw new NotImplementedException();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }

        public int CompareTo(object? obj)
        {
            throw new NotImplementedException();
        }

        public object Clone()
        {
            return this; // String is immutable
        }

        [ArduinoImplementation(ArduinoImplementation.StringSetElem)]
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

        [ArduinoImplementation(ArduinoImplementation.StringGetElem)]
        private char GetElem(int idx)
        {
            throw new NotImplementedException();
        }

        [ArduinoImplementation(ArduinoImplementation.StringGetPinnableReference)]
        public ref char GetPinnableReference()
        {
            throw new NotImplementedException();
        }

        [ArduinoImplementation(ArduinoImplementation.StringEqualsStatic)]
        public static bool Equals(string a, string b)
        {
            throw new NotImplementedException();
        }

        [ArduinoImplementation(ArduinoImplementation.StringEqualsStatic)]
        public static bool operator ==(MiniString a, MiniString b)
        {
            throw new NotImplementedException();
        }

        [ArduinoImplementation(ArduinoImplementation.StringUnEqualsStatic)]
        public static bool operator !=(MiniString a, MiniString b)
        {
            throw new NotImplementedException();
        }

        [ArduinoImplementation(ArduinoImplementation.None)]
        public TypeCode GetTypeCode()
        {
            return TypeCode.String;
        }

        public bool ToBoolean(IFormatProvider? provider)
        {
            throw new NotImplementedException();
        }

        public byte ToByte(IFormatProvider? provider)
        {
            throw new NotImplementedException();
        }

        public char ToChar(IFormatProvider? provider)
        {
            throw new NotImplementedException();
        }

        public DateTime ToDateTime(IFormatProvider? provider)
        {
            throw new NotImplementedException();
        }

        public decimal ToDecimal(IFormatProvider? provider)
        {
            throw new NotImplementedException();
        }

        public double ToDouble(IFormatProvider? provider)
        {
            throw new NotImplementedException();
        }

        public short ToInt16(IFormatProvider? provider)
        {
            throw new NotImplementedException();
        }

        public int ToInt32(IFormatProvider? provider)
        {
            throw new NotImplementedException();
        }

        public long ToInt64(IFormatProvider? provider)
        {
            throw new NotImplementedException();
        }

        public sbyte ToSByte(IFormatProvider? provider)
        {
            throw new NotImplementedException();
        }

        public float ToSingle(IFormatProvider? provider)
        {
            throw new NotImplementedException();
        }

        public string ToString(IFormatProvider? provider)
        {
            throw new NotImplementedException();
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

        [ArduinoImplementation(ArduinoImplementation.StringFastAllocateString)]
        public static String FastAllocateString(int length)
        {
            throw new NotImplementedException();
        }

        [ArduinoImplementation(ArduinoImplementation.StringGetRawStringData)]
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
        [ArduinoImplementation(ArduinoImplementation.StringInternalAllocateString)]
        private void InternalAllocateString(int length)
        {
            throw new NotImplementedException();
        }
    }
}
