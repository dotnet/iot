using System;
using Iot.Device.Arduino;

namespace ArduinoCsCompiler.Runtime
{
    [ArduinoReplacement(typeof(System.Runtime.InteropServices.MemoryMarshal), false, IncludingPrivates = true)]
    internal static class MiniMemoryMarshal
    {
        [ArduinoImplementation("MemoryMarshalGetArrayDataReference", CompareByParameterNames = true, IgnoreGenericTypes = true)]
        public static ref T GetArrayDataReference<T>(T[] array)
        {
            throw new NotImplementedException();
        }
    }
}
