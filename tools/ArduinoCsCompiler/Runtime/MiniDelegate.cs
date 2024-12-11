// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System;
using Iot.Device.Arduino;

namespace ArduinoCsCompiler.Runtime
{
    [ArduinoReplacement(typeof(System.Delegate), IncludingPrivates = true)]
    internal class MiniDelegate
    {
        [ArduinoImplementation("DelegateInternalEqualTypes")]
        public static bool InternalEqualTypes(object a, object b)
        {
            throw new NotImplementedException();
        }

        // The following two methods are implemented the same. I haven't found a test case for verifying them, though
        [ArduinoImplementation("DelegateInternalEqualMethodHandle")]
        public static Boolean InternalEqualMethodHandles(System.Delegate left, System.Delegate right)
        {
            throw new NotImplementedException();
        }
    }
}
