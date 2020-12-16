using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;

namespace Iot.Device.Arduino
{
    [ArduinoReplacement(typeof(System.String), true)]
    internal class MiniString : ICloneable, IComparable, IComparable<string>, IConvertible, IEquatable<string>, System.Collections.Generic.IEnumerable<char>
    {
#pragma warning disable SA1122 // Use string.Empty for empty strings
        private static string s_emptyString = "";
#pragma warning restore SA1122 // Use string.Empty for empty strings
        private int _length;
        private int _data; // Internal char* pointer

        [ArduinoImplementation(ArduinoImplementation.StringCtor0)]
        public MiniString()
        {
            _length = 0;
            _data = 0;
        }

        public static string Empty
        {
            get
            {
                return s_emptyString;
            }
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

        public static implicit operator ReadOnlySpan<char>(MiniString? value)
        {
            if (ReferenceEquals(value, null))
            {
                throw new ArgumentNullException(nameof(value));
            }

            char[] chars = value.ToArray();
            Span<char> s = new Span<char>(chars, 0, value.Length);
            return s;
        }

        public int Length
        {
            get
            {
                if (_data == 0)
                {
                    return 0;
                }

                return _length;
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

        [ArduinoImplementation(ArduinoImplementation.StringFormat2)]
        public static string Format(string format, object arg0)
        {
            return string.Empty;
        }

        [ArduinoImplementation(ArduinoImplementation.StringFormat3)]
        public static string Format(string format, object arg0, object arg1)
        {
            return string.Empty;
        }

        [ArduinoImplementation(ArduinoImplementation.StringFormat2b)]
        public static string Format(string format, object[] args)
        {
            return string.Empty;
        }

        public int CompareTo(string? other)
        {
            throw new NotImplementedException();
        }

        public int IndexOf(char c)
        {
            throw new NotImplementedException();
        }

        public static string? Concat(ReadOnlySpan<char> str0, ReadOnlySpan<char> str1, ReadOnlySpan<char> str2, ReadOnlySpan<char> str3)
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

        [ArduinoImplementation(ArduinoImplementation.StringConcat2)]
        public static string Concat(string string1, string string2)
        {
            throw new NotImplementedException();
        }

        [ArduinoImplementation(ArduinoImplementation.None)]
        public static string Join(string? separator, params object[]? values)
        {
            return string.Empty;
        }

        [ArduinoImplementation(ArduinoImplementation.StringSetElem)]
        private void SetElem(int idx, char c)
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

        [ArduinoImplementation(ArduinoImplementation.None)]
        public static bool IsNullOrEmpty(string? value)
        {
            return (value == null || value.Length == 0);
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

        [ArduinoImplementation(ArduinoImplementation.StringEqualsStatic)]
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
    }
}
