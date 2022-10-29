// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using ArduinoCsCompiler.Runtime;

namespace ArduinoCsCompiler
{
    /// <summary>
    /// This class contains interop declarations.
    /// Important: These are called by the compiler, these are not Arduino Runtime method replacements,
    /// They're used to resolve dynamic values (such as locale and time zone information) at compile time without
    /// having to convert Win32 types back and forth
    /// </summary>
    internal class Interop
    {
        [DllImport("kernel32.dll", CharSet = CharSet.Unicode, SetLastError = true, ExactSpelling = true)]
        internal static extern uint GetTimeZoneInformation(out MiniInterop.TIME_ZONE_INFORMATION lpTimeZoneInformation);

        [DllImport("kernel32.dll", CharSet = CharSet.Unicode, SetLastError = true, ExactSpelling = true)]
        internal static extern uint GetDynamicTimeZoneInformation(out MiniInterop.TIME_DYNAMIC_ZONE_INFORMATION pTimeZoneInformation);

    }
}
