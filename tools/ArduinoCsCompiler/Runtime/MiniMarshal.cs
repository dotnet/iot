using System;
using System.ComponentModel;
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

        [ArduinoImplementation("Interop_Kernel32SetLastError", 0x205)]
        public static void SetLastPInvokeError(int error)
        {
            throw new NotImplementedException();
        }

        [ArduinoImplementation("Interop_Kernel32GetLastError", 0x206)]
        public static int GetLastWin32Error()
        {
            throw new NotImplementedException();
        }

        [ArduinoImplementation("Interop_Kernel32GetLastError", 0x206)]
        public static int GetLastPInvokeError()
        {
            throw new NotImplementedException();
        }

        public static int GetHRForLastWin32Error()
        {
            throw new NotImplementedException();
        }

        public static void ThrowExceptionForHR(Int32 errorCode)
        {
            throw new Win32Exception(errorCode);
        }

        public static bool IsPinnable(object obj)
        {
            return false;
        }

        [ArduinoImplementation("Interop_Kernel32AllocHGlobal")]
        public static IntPtr AllocHGlobal(IntPtr cb)
        {
            throw new NotSupportedException();
        }

        [ArduinoImplementation("Interop_Kernel32AllocHGlobal")]
        public static IntPtr AllocHGlobal(Int32 cb)
        {
            throw new NotSupportedException();
        }

        [ArduinoImplementation("Interop_Kernel32FreeHGlobal")]
        public static void FreeHGlobal(IntPtr hGlobal)
        {
            throw new NotImplementedException();
        }

        public static unsafe string? PtrToStringUni(IntPtr ptr)
        {
            if (ptr == IntPtr.Zero)
            {
                return null;
            }

            return new string((char*)ptr);
        }

        public static unsafe string PtrToStringUni(IntPtr ptr, int len)
        {
            if (ptr == IntPtr.Zero)
            {
                throw new ArgumentNullException(nameof(ptr));
            }

            if (len < 0)
            {
                throw new ArgumentOutOfRangeException(nameof(len));
            }

            return new string((char*)ptr, 0, len);
        }

        public static unsafe string PtrToStringAnsi(IntPtr ptr)
        {
            if (ptr == IntPtr.Zero)
            {
                throw new ArgumentNullException(nameof(ptr));
            }

            return new string((sbyte*)ptr);
        }

        public static Int32 SizeOf<T>()
        {
            return MiniUnsafe.SizeOf<T>();
        }

        public static int SizeOf(Type t)
        {
            return MiniUnsafe.SizeOfType(t);
        }
    }
}
