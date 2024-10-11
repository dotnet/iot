// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System;
using Iot.Device.Arduino;

namespace ArduinoCsCompiler.Runtime
{
    [ArduinoReplacement(typeof(DateTime), false, IncludingPrivates = true)]
    internal struct MiniDateTime
    {
        public static DateTime UtcNow
        {
            [ArduinoImplementation("DateTimeUtcNow")]
            get
            {
                throw new NotImplementedException();
            }
        }

        [ArduinoImplementation]
        public static bool IsValidTimeWithLeapSeconds(
            int year,
            int month,
            int day,
            int hour,
            int minute,
            DateTimeKind kind)
        {
            return true;
        }

        [ArduinoImplementation(CompareByParameterNames = true)]
        public static IntPtr GetGetSystemTimeAsFileTimeFnPtr()
        {
            return IntPtr.Zero; // The returned function pointer(!) should not be used, because we have overriden UtcNow
        }

        [ArduinoImplementation]
        public static bool GetSystemSupportsLeapSeconds()
        {
            return false;
        }
    }
}
