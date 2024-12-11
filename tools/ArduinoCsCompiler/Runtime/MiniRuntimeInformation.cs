// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Runtime.Versioning;
using System.Text;
using System.Threading.Tasks;

namespace ArduinoCsCompiler.Runtime
{
    [ArduinoReplacement(typeof(System.Runtime.InteropServices.RuntimeInformation), true)]
    public class MiniRuntimeInformation
    {
        public static string FrameworkDescription
        {
            get
            {
                // We hardcode the value to what we last tested with
                return ".NET 8.0.5";
            }
        }

        /// <summary>
        /// Returns an opaque string that identifies the platform on which an app is running.
        /// </summary>
        /// <remarks>
        /// The property returns a string that identifies the operating system, typically including version,
        /// and processor architecture of the currently executing process.
        /// Since this string is opaque, it is not recommended to parse the string into its constituent parts.
        ///
        /// For more information, see https://docs.microsoft.com/dotnet/core/rid-catalog.
        /// </remarks>
        public static string RuntimeIdentifier => "RISC-V-32";

        /// <summary>
        /// Indicates whether the current application is running on the specified platform.
        /// </summary>
        public static bool IsOSPlatform(OSPlatform osPlatform) => OperatingSystem.IsOSPlatform(osPlatform.ToString());

        public static Architecture ProcessArchitecture => Architecture.Arm;

        public static string OSDescription => "MiniRuntime, Emulating Win32 behavior";

        public static Architecture OSArchitecture => Architecture.X86;
    }
}
