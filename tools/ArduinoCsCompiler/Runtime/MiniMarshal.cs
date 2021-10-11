using System;
using System.Runtime.InteropServices;
using Iot.Device.Arduino;

namespace ArduinoCsCompiler.Runtime
{
    [ArduinoReplacement(typeof(Marshal), true)]
    internal class MiniMarshal
    {
        [ArduinoImplementation(NativeMethod.Interop_Kernel32SetLastError)]
        public static void SetLastWin32Error(int error)
        {
        }

        [ArduinoImplementation(NativeMethod.Interop_Kernel32GetLastError)]
        public static int GetLastWin32Error()
        {
            throw new NotImplementedException();
        }

        public static bool IsPinnable(object obj)
        {
            return false;
        }
    }
}
