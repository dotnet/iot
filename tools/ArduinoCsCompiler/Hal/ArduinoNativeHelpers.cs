// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ArduinoCsCompiler
{
    /// <summary>
    /// Contains helper functions usable when running on the Arduino.
    /// None of these functions work when called on the host PC directly.
    /// </summary>
    public class ArduinoNativeHelpers
    {
        /// <summary>
        /// Gets the number of microseconds since board was switched on.
        /// This wraps around every about 71 minutes
        /// </summary>
        /// <returns>The current number of microseconds</returns>
        [ArduinoImplementation("ArduinoNativeHelpersGetMicroseconds", 0x110)]
        public static UInt32 GetMicroseconds()
        {
            throw new PlatformNotSupportedException("This method works on the Arduino only");
        }

        /// <summary>
        /// Sleeps the given number of microseconds
        /// </summary>
        /// <param name="micros">Number of microseconds to sleep</param>
        [ArduinoImplementation("ArduinoNativeHelpersSleepMicroseconds", 0x111)]
        public static void SleepMicroseconds(UInt32 micros)
        {
            throw new PlatformNotSupportedException("This method works on the Arduino only");
        }

        /// <summary>
        /// This method serves as stub for the main startup code. The body will be dynamically generated (from the static ctors and the actual main method)
        /// </summary>
        internal static void MainStub()
        {
            throw new NotImplementedException();
        }
    }
}
