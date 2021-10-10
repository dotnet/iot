using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace Iot.Device.Arduino.Runtime
{
    [ArduinoReplacement(typeof(IntrospectionExtensions), true)]
    internal class MiniIntrospectionExtensions
    {
        [ArduinoImplementation(NativeMethod.None, CompareByParameterNames = true)]
        public static Type GetTypeInfo(Type type)
        {
            return type;
        }
    }
}
