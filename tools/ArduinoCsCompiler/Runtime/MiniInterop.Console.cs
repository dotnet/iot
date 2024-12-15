// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

// Our predefined handles are:
// 1-0xBFFF File handles
// 0xCEED Standard input
// 0xCEEE Standard output
// 0xCEEF Standard error (see also FirmataIlExecutor.h)
// 0xF000-0xFFFF Network handles
namespace ArduinoCsCompiler.Runtime
{
    internal partial class MiniInterop
    {
        internal partial class Kernel32
        {
            [ArduinoImplementation]
            public static uint GetConsoleOutputCP()
            {
                return 1200; // Unicode, little endian
            }

            public static IntPtr GetStdHandle(int nStdHandle)
            {
                if (nStdHandle == -10)
                {
                    // Standard input
                    return new IntPtr(0xCEED); // Some obvious marker handle
                }
                else if (nStdHandle == -11)
                {
                    // Standard output
                    return new IntPtr(0xCEEE);
                }
                else
                {
                    // standard error
                    return new IntPtr(0xCEEF);
                }
            }

            internal static bool IsGetConsoleModeCallSuccessful(IntPtr handle)
            {
                return true;
            }

            [ArduinoImplementation("Kernel32_WriteConsole")]
            internal static unsafe bool WriteConsole(System.IntPtr handle, System.Byte* bytes, System.Int32 numBytesToWrite, ref System.Int32 numBytesWritten, System.IntPtr mustBeZero)
            {
                return true;
            }

            [ArduinoImplementation("Kernel32_WideCharToMultiByte", CompareByParameterNames = true)]
            public static unsafe Int32 WideCharToMultiByte(System.UInt32 CodePage, System.UInt32 dwFlags, System.Char* lpWideCharStr, System.Int32 cchWideChar, System.Byte* lpMultiByteStr, System.Int32 cbMultiByte, System.IntPtr lpDefaultChar, System.IntPtr lpUsedDefaultChar)
            {
                return 0;
            }
        }
    }
}
