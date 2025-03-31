// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System;
using System.Threading;
using Iot.Device.Arduino;

namespace ArduinoCsCompiler.Runtime
{
    [ArduinoReplacement(typeof(System.Environment), true)]
    internal static class MiniEnvironment
    {
        public static int CurrentManagedThreadId => Thread.CurrentThread.ManagedThreadId;

        public static int TickCount
        {
            [ArduinoImplementation("EnvironmentTickCount")]
            get
            {
                throw new NotImplementedException();
            }
        }

        public static int TickCount64
        {
            [ArduinoImplementation("EnvironmentTickCount64")]
            get
            {
                throw new NotImplementedException();
            }
        }

        public static int ProcessorCount
        {
            [ArduinoImplementation("EnvironmentProcessorCount")]
            get
            {
                return 1;
            }
        }

        public static int ProcessId
        {
            get
            {
                // Some magic number
                return 0x1BBAEFFE;
            }
        }

        public static bool IsSingleProcessor
        {
            get
            {
                return ProcessorCount == 1;
            }
        }

        public static string SystemDirectory
        {
            get
            {
                return "/"; // At the moment, we do not have a file system at all
            }
        }

        public static string NewLine
        {
            get
            {
                return "\n"; // We'll have our "Arduino-OS" look like an unix style system
            }
        }

        internal static System.Boolean IsWindows8OrAbove
        {
            get
            {
                return true;
            }
        }

        public static OperatingSystem OSVersion
        {
            get
            {
                // This does not have a "anything else" option...
                return new OperatingSystem(PlatformID.Unix, new Version(1, 0));
            }
        }

        public static int SystemPageSize
        {
            get
            {
                return 0x1000; // 4kb typical
            }
        }

        public static bool Is64BitProcess
        {
            get
            {
                return false;
            }
        }

        [ArduinoImplementation("EnvironmentFailFast1")]
        public static void FailFast(string message)
        {
            throw new NotImplementedException();
        }

        [ArduinoImplementation("EnvironmentFailFast2")]
        public static void FailFast(string message, Exception exception)
        {
            throw new NotImplementedException();
        }

        public static string? GetEnvironmentVariable(string variable)
        {
            return null;
        }

        public static string ExpandEnvironmentVariables(string input)
        {
            return input;
        }

        public static string? GetEnvironmentVariableCore_NoArrayPool(string variable)
        {
            return null;
        }
    }
}
