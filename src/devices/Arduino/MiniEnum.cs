using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Iot.Device.Arduino
{
    [ArduinoReplacement(typeof(System.Enum), IncludingPrivates = true)]
    internal class MiniEnum
    {
        [ArduinoImplementation(NativeMethod.None, CompareByParameterNames = true)]
        public static string? InternalFormat(Type enumType, ulong value)
        {
            return value.ToString();
        }

        [ArduinoImplementation(NativeMethod.None)]
        public override string ToString()
        {
            // We don't have the metadata to print the enums as strings, so use their underlying value instead.
            return ToUInt64().ToString();
        }

        [ArduinoImplementation(NativeMethod.None)]
        public string ToString(string? format)
        {
            return ToUInt64().ToString();
        }

        [ArduinoImplementation(NativeMethod.None)]
        public string? ToString(string format, IFormatProvider provider)
        {
            return ToUInt64().ToString();
        }

        [ArduinoImplementation(NativeMethod.EnumGetHashCode)]
        public override int GetHashCode()
        {
            throw new NotImplementedException();
        }

        [ArduinoImplementation(NativeMethod.EumToUInt64)]
        public ulong ToUInt64()
        {
            throw new NotImplementedException();
        }
    }
}
