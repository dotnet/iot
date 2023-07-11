// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System;
using Iot.Device.Arduino;

namespace ArduinoCsCompiler.Runtime
{
    [ArduinoReplacement(typeof(System.Enum), IncludingPrivates = true)]
    internal class MiniEnum
    {
        [ArduinoImplementation(CompareByParameterNames = true)]
        public static string? InternalFormat(Type enumType, ulong value)
        {
            return value.ToString();
        }

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
                throw new ArgumentException("Type is not an enum", nameof(enumType));
            }

            return MiniUnsafe.As<MiniType>(enumType);
        }

        [ArduinoImplementation("EnumInternalBoxEnum", CompareByParameterNames = true)]
        public static object InternalBoxEnum(MiniType enumType, long value)
        {
            throw new NotImplementedException();
        }

        [ArduinoImplementation("EnumInternalGetValues", CompareByParameterNames = true)]
        public static ulong[] InternalGetValues(MiniType enumType)
        {
            throw new NotImplementedException();
        }

        [ArduinoImplementation]
        public override string ToString()
        {
            // We don't have the metadata to print the enums as strings, so use their underlying value instead.
            return ToUInt64().ToString();
        }

        [ArduinoImplementation]
        public string ToString(string? format)
        {
            return ToUInt64().ToString();
        }

        [ArduinoImplementation]
        public string? ToString(string format, IFormatProvider provider)
        {
            return ToUInt64().ToString();
        }

        [ArduinoImplementation("EnumGetHashCode")]
        public override int GetHashCode()
        {
            throw new NotImplementedException();
        }

        [ArduinoImplementation("EnumToUInt64")]
        public ulong ToUInt64()
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

            UInt64 other = (UInt64)obj;
            return ToUInt64() == other;
        }
    }
}
