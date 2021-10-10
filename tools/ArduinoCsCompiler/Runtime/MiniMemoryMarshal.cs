using System;
using System.Buffers;
using System.Diagnostics;
using System.Runtime.CompilerServices;

namespace Iot.Device.Arduino.Runtime
{
    [ArduinoReplacement(typeof(System.Runtime.InteropServices.MemoryMarshal), false, IncludingPrivates = true)]
    internal static class MiniMemoryMarshal
    {
        [ArduinoImplementation(NativeMethod.MemoryMarshalGetArrayDataReference, CompareByParameterNames = true, IgnoreGenericTypes = true)]
        public static ref T GetArrayDataReference<T>(T[] array)
        {
            throw new NotImplementedException();
        }
    }
}
