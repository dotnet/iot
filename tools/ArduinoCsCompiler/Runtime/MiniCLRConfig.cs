// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using Iot.Device.Arduino;

namespace ArduinoCsCompiler.Runtime
{
    // This type appears to no longer exist in net6.0
    // [ArduinoReplacement("System.CLRConfig", "System.Private.Corelib.dll", true, IncludingPrivates = true)]
    internal static class MiniCLRConfig
    {
        [ArduinoImplementation]
        public static bool GetBoolValueWithFallbacks(string switchName, string environmentName, bool defaultValue)
        {
            return defaultValue;
        }
    }
}
