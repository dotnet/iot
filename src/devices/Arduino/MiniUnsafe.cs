using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Iot.Device.Arduino
{
    [ArduinoReplacement("Internal.Runtime.CompilerServices.Unsafe", true, false, IncludingPrivates = true)]
    internal unsafe class MiniUnsafe
    {
        // The implementation of the following two methods is identical, therefore it doesn't really matter which one we match
        [ArduinoImplementation(ArduinoImplementation.UnsafeAs2, CompareByParameterNames = true)]
        public static T As<T>(object? value)
            where T : class?
        {
            throw new PlatformNotSupportedException();

            // ldarg.0
            // ret
        }

        [ArduinoImplementation(ArduinoImplementation.UnsafeAs2, CompareByParameterNames = true)]
        public static ref TTo As<TFrom, TTo>(ref TFrom source)
        {
            throw new PlatformNotSupportedException();

            // ldarg.0
            // ret
        }

        [ArduinoImplementation(ArduinoImplementation.None, CompareByParameterNames = true)]
        public static ref T Add<T>(ref T source, int elementOffset)
        {
            return ref AddByteOffset(ref source, (IntPtr)(elementOffset * (int)SizeOf<T>()));
        }

        [ArduinoImplementation(ArduinoImplementation.UnsafeAddByteOffset, CompareByParameterNames = true)]
        public static ref T AddByteOffset<T>(ref T source, IntPtr byteOffset)
        {
            // This method is implemented by the toolchain
            throw new PlatformNotSupportedException();

            // ldarg.0
            // ldarg.1
            // add
            // ret
        }

        public static int SizeOf<T>()
        {
            return SizeOfType(typeof(T));
            // sizeof !!0
            // ret
        }

        [ArduinoImplementation(ArduinoImplementation.UnsafeSizeOfType)]
        private static int SizeOfType(Type t)
        {
            throw new NotImplementedException();
        }

        public static ref T AsRef<T>(void* source)
        {
            return ref As<byte, T>(ref *(byte*)source);
        }

        public static ref T AsRef<T>(in T source)
        {
            throw new PlatformNotSupportedException();
        }

        [ArduinoImplementation(ArduinoImplementation.UnsafeNullRef)]
        public static ref T NullRef<T>()
        {
            return ref AsRef<T>(null);

            // ldc.i4.0
            // conv.u
            // ret
        }
    }
}
