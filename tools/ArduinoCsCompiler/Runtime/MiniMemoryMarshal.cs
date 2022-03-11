// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

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

        [ArduinoImplementation("MemoryMarshalGetArrayDataReferenceByte", CompareByParameterNames = true, IgnoreGenericTypes = true)]
        public static unsafe ref byte GetArrayDataReference(Array array)
        {
            throw new NotImplementedException();
        }
    }
}
