using System;
using System.Runtime.InteropServices;
using Iot.Device.Arduino;

namespace ArduinoCsCompiler.Runtime
{
    [ArduinoReplacement(typeof(Marshal), true)]
    internal class MiniMarshal
    {
        [ArduinoImplementation("Interop_Kernel32SetLastError", 0x205)]
        public static void SetLastWin32Error(int error)
        {
        }

        [ArduinoImplementation("Interop_Kernel32GetLastError", 0x206)]
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
