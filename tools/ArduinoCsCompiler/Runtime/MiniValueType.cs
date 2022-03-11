// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System;
using Iot.Device.Arduino;

namespace ArduinoCsCompiler.Runtime
{
    [ArduinoReplacement(typeof(System.ValueType), true)]
    internal class MiniValueType
    {
        public MiniValueType()
        {
        }

        [ArduinoImplementation("ValueTypeEquals")]
        public override bool Equals(object? other)
        {
            return true;
        }

        [ArduinoImplementation("ValueTypeGetHashCode")]
        public override int GetHashCode()
        {
            return 0;
        }

        [ArduinoImplementation("ValueTypeToString")]
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
