// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.Numerics;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using Iot.Device.Arduino;

namespace ArduinoCsCompiler.Runtime
{
    [ArduinoReplacement(typeof(System.Enum), IncludingPrivates = true)]
    internal class MiniEnum
    {
        public static object ToObject(Type enumType, sbyte value) =>
            InternalBoxEnum(ValidateRuntimeType(enumType), value);

        public static object ToObject(Type enumType, short value) =>
            InternalBoxEnum(ValidateRuntimeType(enumType), value);

        public static object ToObject(Type enumType, int value) =>
            InternalBoxEnum(ValidateRuntimeType(enumType), value);

        public static object ToObject(Type enumType, byte value) =>
            InternalBoxEnum(ValidateRuntimeType(enumType), value);

        public static object ToObject(Type enumType, ushort value) =>
            InternalBoxEnum(ValidateRuntimeType(enumType), value);

        public static object ToObject(Type enumType, uint value) =>
            InternalBoxEnum(ValidateRuntimeType(enumType), value);

        public static object ToObject(Type enumType, long value) =>
            InternalBoxEnum(ValidateRuntimeType(enumType), value);

        public static object ToObject(Type enumType, ulong value) =>
            InternalBoxEnum(ValidateRuntimeType(enumType), unchecked((long)value));

        public static object ToObject(Type enumType, char value) =>
            InternalBoxEnum(ValidateRuntimeType(enumType), value);

        public static object ToObject(Type enumType, bool value) =>
            InternalBoxEnum(ValidateRuntimeType(enumType), value ? 1 : 0);

        private static MiniType ValidateRuntimeType(Type enumType)
        {
            if (enumType == null)
            {
                throw new ArgumentNullException(nameof(enumType));
            }

            if (!enumType.IsEnum)
            {
                throw new ArgumentException("Invalid argument", nameof(enumType));
            }

            return MiniUnsafe.As<MiniType>(enumType);
        }

        [ArduinoImplementation("EnumInternalBoxEnum", CompareByParameterNames = true)]
        public static object InternalBoxEnum(MiniType enumType, long value)
        {
            throw new NotImplementedException();
        }

        [ArduinoImplementation("EnumToUInt64")]
        public static ulong ToUInt64(object value)
        {
            throw new NotImplementedException();
        }

        [ArduinoImplementation("EnumInternalGetValues")]
        public static Array GetValues(System.Type enumType)
        {
            throw new NotImplementedException();
        }

        public static Array GetValues(MiniType enumType)
        {
            return GetValues(Unsafe.As<Type>(enumType));
        }

        public static Array GetValues<T>()
        {
            return GetValues(typeof(T));
        }

        public static bool TryFormatUnconstrained<TEnum>(TEnum value, Span<char> destination, out int charsWritten, [StringSyntax(StringSyntaxAttribute.EnumFormat)] ReadOnlySpan<char> format = default)
        {
            throw new NotImplementedException();
        }

        [ArduinoImplementation(CompareByParameterNames = true)]
        public static CorElementType InternalGetCorElementType(Type rt)
        {
            if (rt == typeof(byte))
            {
                return CorElementType.ELEMENT_TYPE_U1;
            }

            if (rt == typeof(sbyte))
            {
                return CorElementType.ELEMENT_TYPE_I1;
            }

            if (rt == typeof(short))
            {
                return CorElementType.ELEMENT_TYPE_I2;
            }

            if (rt == typeof(ushort))
            {
                return CorElementType.ELEMENT_TYPE_U2;
            }

            if (rt == typeof(int))
            {
                return CorElementType.ELEMENT_TYPE_I4;
            }

            if (rt == typeof(uint))
            {
                return CorElementType.ELEMENT_TYPE_U4;
            }

            if (rt == typeof(ulong))
            {
                return CorElementType.ELEMENT_TYPE_U8;
            }

            if (rt == typeof(long))
            {
                return CorElementType.ELEMENT_TYPE_I8;
            }

            return CorElementType.ELEMENT_TYPE_I; // Something else
        }

        [ArduinoImplementation(CompareByParameterNames = true)]
        public static MiniEnumInfo<TStorage> GetEnumInfo<TStorage>(Type enumType, bool getNames)
            where TStorage : struct, INumber<TStorage>
        {
            Array values = GetValues(enumType);
            TStorage[] array = new TStorage[values.Length];
            string[] names = new string[array.Length];
            for (int i = 0; i < array.Length; i++)
            {
                array[i] = (TStorage)values.GetValue(i)!;
                names[i] = array[i].ToString()!;
            }

            var ret = new MiniEnumInfo<TStorage>(false, array, names);
            return ret;
        }

        [ArduinoImplementation]
        public override string ToString()
        {
            // We don't have the metadata to print the enums as strings, so use their underlying value instead.
            return ToUInt64(this).ToString();
        }

        [ArduinoImplementation]
        public string ToString(string? format)
        {
            return ToUInt64(this).ToString();
        }

        [ArduinoImplementation]
        public string? ToString(string format, IFormatProvider provider)
        {
            return ToUInt64(this).ToString();
        }

        [ArduinoImplementation("EnumGetHashCode")]
        public override int GetHashCode()
        {
            throw new NotImplementedException();
        }

        [ArduinoImplementation]
        public override bool Equals(object? obj)
        {
            if (obj == null)
            {
                return false;
            }

            if (obj.GetType() != GetType())
            {
                return false;
            }

            return MiniRuntimeHelpers.EnumEqualsInternal(this, obj);
        }

        [ArduinoReplacement("System.Enum+EnumInfo`1", "System.Private.Corelib.dll", true, typeof(System.Enum), IncludingPrivates = true)]
        internal sealed class MiniEnumInfo<TStorage>
            where TStorage : struct, INumber<TStorage>
        {
            public readonly bool HasFlagsAttribute;
            public readonly bool ValuesAreSequentialFromZero;
            public readonly TStorage[] Values;
            public readonly string[] Names;

            // Each entry contains a list of sorted pair of enum field names and values, sorted by values
            public MiniEnumInfo(bool hasFlagsAttribute, TStorage[] values, string[] names)
            {
                HasFlagsAttribute = hasFlagsAttribute;
                Values = values;
                Names = names;
                ValuesAreSequentialFromZero = false;
            }

            /// <summary>Create a copy of <see cref="Values"/>.</summary>
            public TResult[] CloneValues<TResult>()
                where TResult : struct
            {
                return MemoryMarshal.Cast<TStorage, TResult>(Values).ToArray();
            }
        }
    }
}
