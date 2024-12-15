// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using Iot.Device.Arduino;

namespace ArduinoCsCompiler.Runtime
{
    [ArduinoReplacement(typeof(System.Exception), IncludingPrivates = true)]
    internal class MiniException
    {
        [ArduinoImplementation]
        public static void GetStackTracesDeepCopy(System.Exception exception, ref System.Byte[] currentStackTrace, ref System.Object[] dynamicMethodArray)
        {
            dynamicMethodArray = new object[0];
            currentStackTrace = new byte[0];
        }

        [ArduinoImplementation]
        public static bool IsImmutableAgileException(System.Exception e)
        {
            return false;
        }

        [ArduinoImplementation]
        public static void SaveStackTracesFromDeepCopy(System.Exception exception, System.Byte[] currentStackTrace, System.Object[] dynamicMethodArray)
        {
            // We currently don't generate runtime stack traces
        }

        [ArduinoImplementation]
        public static void PrepareForForeignExceptionRaise()
        {
        }

        [ArduinoImplementation(CompareByParameterNames = true)]
        public void RestoreDispatchState(int dispatchState) // Argument is a struct
        {
            // Nothing to do, ExceptionDispatchInfo.Throw is not really supported (and probably not useful in our EE)
        }
    }
}
