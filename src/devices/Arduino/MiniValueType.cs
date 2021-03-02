using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Iot.Device.Arduino
{
    [ArduinoReplacement(typeof(System.ValueType), true)]
    internal class MiniValueType
    {
        public MiniValueType()
        {
        }

        [ArduinoImplementation(NativeMethod.ValueTypeEquals)]
        public override bool Equals(object? other)
        {
            return true;
        }

        [ArduinoImplementation(NativeMethod.ValueTypeGetHashCode)]
        public override int GetHashCode()
        {
            return 0;
        }

        [ArduinoImplementation(NativeMethod.ValueTypeToString)]
        public override string ToString()
        {
            return string.Empty;
        }

        internal static int GetHashCodeOfPtr(IntPtr ptr)
        {
            return (int)ptr;
        }
    }
}
