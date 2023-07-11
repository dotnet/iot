// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System;

namespace ArduinoCsCompiler.Runtime
{
    internal partial class MiniInterop
    {
        [ArduinoReplacement("Interop+Ole32", null, true)]
        internal class Ole32
        {
            public static Int32 CoCreateGuid(out System.Guid guid)
            {
                guid = new Guid(Environment.TickCount, 0xbc, 0xde, 1, 2, 3, 4, 5, 6, 7, 8);
                return 0;
            }
        }
    }
}
