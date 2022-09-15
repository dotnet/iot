// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System;

namespace ArduinoCsCompiler.Runtime
{
    internal static partial class MiniInterop
    {
        // This should only be called already from our wrappers, because the class doesn't really exist anywhere, it seems
        // [ArduinoReplacement("Interop+Sys", "System.Private.CoreLib.dll", true)]
        internal static class Sys
        {
            private static string s_currentDirectory = "/";

            internal static IntPtr MksTemps(
                byte[] template,
                int suffixlen)
            {
                throw new NotImplementedException();
            }

            internal static int Close(IntPtr fd)
            {
                throw new NotImplementedException();
            }

            internal static string GetCwd()
            {
                return s_currentDirectory;
            }
        }
    }
}
