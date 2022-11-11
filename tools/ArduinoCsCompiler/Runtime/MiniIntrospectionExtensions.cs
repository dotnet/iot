// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System;
using System.Reflection;
using Iot.Device.Arduino;

namespace ArduinoCsCompiler.Runtime
{
    [ArduinoReplacement(typeof(IntrospectionExtensions), true)]
    internal class MiniIntrospectionExtensions
    {
        [ArduinoImplementation(CompareByParameterNames = true)]
        public static Type GetTypeInfo(Type type)
        {
            return type;
        }
    }
}
