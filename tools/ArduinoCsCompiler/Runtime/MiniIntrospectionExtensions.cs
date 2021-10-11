using System;
using System.Reflection;
using Iot.Device.Arduino;

namespace ArduinoCsCompiler.Runtime
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
