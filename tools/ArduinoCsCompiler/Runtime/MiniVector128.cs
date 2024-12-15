// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.Globalization;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Runtime.Intrinsics;
using System.Runtime.Intrinsics.X86;
using System.Text;
using System.Threading.Tasks;

#pragma warning disable SA1306 // Naming convention
#pragma warning disable SA1204 // Method ordering

namespace ArduinoCsCompiler.Runtime
{
    [ArduinoReplacement(typeof(System.Runtime.Intrinsics.Vector128), false)]
    public static class Vector128
    {
        public static bool IsHardwareAccelerated
        {
            [ArduinoImplementation]
            get
            {
                // The original implementation has an infinite recursion
                return false;
            }
        }
    }
}
