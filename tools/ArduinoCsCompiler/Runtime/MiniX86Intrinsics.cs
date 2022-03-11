// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System;
using Iot.Device.Arduino;

namespace ArduinoCsCompiler.Runtime
{
    [ArduinoReplacement("System.Runtime.Intrinsics.X86.X86Base", null, true, IncludingPrivates = true, IncludingSubclasses = true)]
    internal class MiniX86Intrinsics
    {
        /// <summary>
        /// No intrinsics are ever supported in this runtime
        /// </summary>
        public static bool IsSupported
        {
            [ArduinoImplementation]
            get
            {
                return false;
            }
        }

        /// <summary>
        /// This method replaces all implementations of this intrinsic. Since <see cref="IsSupported"/> is false, these should never be called.
        /// </summary>
        /// <remarks>
        /// Do not change the method name (is used in code)
        /// </remarks>
        public static void NotSupportedException()
        {
            throw new PlatformNotSupportedException("This operation should never be executed");
        }

        [ArduinoReplacement("System.Runtime.Intrinsics.X86.X86Base+X64", null, true, IncludingPrivates = true, IncludingSubclasses = true)]
        public class MiniX64
        {
            /// <summary>
            /// No intrinsics are ever supported in this runtime
            /// </summary>
            public static bool IsSupported
            {
                [ArduinoImplementation]
                get
                {
                    return false;
                }
            }

            // Do not change the method name
            public static void NotSupportedException()
            {
                throw new PlatformNotSupportedException("This operation should never be executed");
            }
        }
    }
}
