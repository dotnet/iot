// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System;
using System.Diagnostics;
using Iot.Device.Arduino;

namespace ArduinoCsCompiler
{
    [ArduinoReplacement(typeof(Debug), true, IncludingPrivates = true)]
    internal class MiniDebug
    {
        [ArduinoImplementation("DebugWriteLine")]
        public static void WriteLine(string? message)
        {
            throw new NotImplementedException();
        }

        public static void Fail(string? message)
        {
            throw new NotImplementedException(message);
        }
    }
}
