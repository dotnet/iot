// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System;

namespace ArduinoCsCompiler.Runtime
{
    [ArduinoReplacement("System.Threading.Tasks.LoggingExtensions", "System.Private.CoreLib.dll", false, typeof(string))]
    internal static class MiniTaskLoggingExtensions
    {
        [ArduinoImplementation]
        public static string GetMethodName(Delegate @delegate)
        {
            ArgumentNullException.ThrowIfNull(@delegate);
            // DiagnosticMethodInfo.Create returns null when stack traces are unsupported.
            return "<unknown>";
        }
    }
}
