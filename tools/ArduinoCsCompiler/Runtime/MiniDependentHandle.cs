// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime;
using System.Text;
using System.Threading.Tasks;
using Iot.Device.Arduino;

namespace ArduinoCsCompiler.Runtime
{
    [ArduinoReplacement(typeof(DependentHandle), false, IncludingPrivates = true)]
    internal struct MiniDependentHandle
    {
        [ArduinoImplementation("DependentHandle_InternalInitialize")]
        public static IntPtr InternalAlloc(object target, object dependent)
        {
            throw new NotImplementedException();
        }

        [ArduinoImplementation("DependentHandle_InternalGetTarget")]
        public static object InternalGetTarget(IntPtr dependentHandle)
        {
            return MiniUnsafe.As<IntPtr, object>(ref dependentHandle);
        }

        [ArduinoImplementation("DependentHandle_InternalGetDependent")]
        public static object InternalGetDependent(IntPtr dependentHandle)
        {
            throw new NotImplementedException();
        }

        [ArduinoImplementation("DependentHandle_InternalGetTargetAndDependent")]
        public static object InternalGetTargetAndDependent(
            IntPtr dependentHandle,
            out object dependent)
        {
            throw new NotImplementedException();
        }

        [ArduinoImplementation("DependentHandle_InternalSetDependent")]
        public static void InternalSetDependent(IntPtr dependentHandle, object dependent)
        {
            throw new NotImplementedException();
        }

        [ArduinoImplementation("DependentHandle_InternalSetTarget")]
        public static void InternalSetTargetToNull(IntPtr dependentHandle)
        {
            throw new NotImplementedException();
        }

        [ArduinoImplementation]
        public static bool InternalFree(IntPtr dependentHandle)
        {
            FreeHandle(dependentHandle);
            return true;
        }

        [ArduinoImplementation("DependentHandle_InternalFree", InternalCall = true)]
        public static void FreeHandle(IntPtr dependentHandle)
        {
            throw new NotImplementedException();
        }
    }
}
