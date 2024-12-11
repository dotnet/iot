// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using Iot.Device.Arduino;

namespace ArduinoCsCompiler.Runtime
{
    [ArduinoReplacement("System.SR", IncludingPrivates = true)]
    internal class MiniSR
    {
        public static string GetResourceString(string resourceKey, string? defaultString)
        {
            if (ReferenceEquals(defaultString, null))
            {
                return resourceKey;
            }

            return defaultString;
        }

        [ArduinoImplementation]
        public static string GetResourceString(string resourceKey)
        {
            return resourceKey;
        }
    }
}
