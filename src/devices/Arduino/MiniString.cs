using System;
using System.Runtime.CompilerServices;

namespace Iot.Device.Arduino
{
    [ArduinoReplacement(typeof(System.String), true)]
    internal class MiniString
    {
        [ArduinoImplementation(ArduinoImplementation.StringCtor0)]
        public MiniString()
        {
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

        public int Length
        {
            [ArduinoImplementation(ArduinoImplementation.StringLength)]
            get;
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

        [ArduinoImplementation(ArduinoImplementation.StringEquals)]
        public override bool Equals(object? obj)
        {
            throw new NotImplementedException();
        }

        [ArduinoImplementation(ArduinoImplementation.None)]
        public bool Equals(string other)
        {
            return Equals((object)other);
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
    }
}
