// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

namespace ArduinoCsCompiler.Runtime
{
    [ArduinoReplacement("System.IO.PathHelper", "System.Private.CoreLib.dll", false)]
    internal static class MiniPathHelper
    {
    }
}
