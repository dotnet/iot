using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Iot.Device.Arduino
{
    [ArduinoReplacement(typeof(System.Runtime.CompilerServices.RuntimeHelpers), true)]
    internal static class MiniRuntimeHelpers
    {
        [ArduinoImplementation(NativeMethod.RuntimeHelpersInitializeArray)]
        public static void InitializeArray(Array array, RuntimeFieldHandle fldHandle)
        {
            throw new NotImplementedException();
        }

        [ArduinoImplementation(NativeMethod.RuntimeHelpersRunClassConstructor)]
        public static void RunClassConstructor(RuntimeTypeHandle rtHandle)
        {
            throw new NotImplementedException();
        }

        public static int OffsetToStringData
        {
            get
            {
                // TODO: Will depend on our string implementation
                return 8;
            }
        }

        [ArduinoImplementation(NativeMethod.RuntimeHelpersGetHashCode)]
        public static int GetHashCode(object? obj)
        {
            return 0;
        }

        public static bool IsReferenceOrContainsReferences<T>()
        {
            return IsReferenceOrContainsReferencesCore(typeof(T));
        }

        [ArduinoImplementation(NativeMethod.RuntimeHelpersIsReferenceOrContainsReferencesCore)]
        private static bool IsReferenceOrContainsReferencesCore(Type t)
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// This uses an implementation in the EE to get rid of all type tests (and all possible casts)
        /// </summary>
        [ArduinoImplementation(NativeMethod.RuntimeHelpersEnumEquals)]
        public static bool EnumEquals<T>(T x, T y)
            where T : struct, Enum
        {
            return x.Equals(y);
        }

        internal static bool IsBitwiseEquatable<T>()
        {
            return IsBitwiseEquatableCore(typeof(T));
        }

        [ArduinoImplementation(NativeMethod.RuntimeHelpersIsBitwiseEquatable)]
        private static bool IsBitwiseEquatableCore(Type t)
        {
            throw new NotImplementedException();
        }

        [ArduinoImplementation(NativeMethod.RuntimeHelpersGetMethodTable)]
        public static unsafe void* GetMethodTable(object obj)
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// This is expected to return a specific structure describing the array bounds - our implementation works differently,
        /// so we might eventually keep this without implementation and make sure it's not called
        /// </summary>
        [ArduinoImplementation(NativeMethod.RuntimeHelpersGetMultiDimensionalArrayBounds)]
        internal static unsafe ref int GetMultiDimensionalArrayBounds(Array array)
        {
            throw new NotImplementedException();
        }

        // [ArduinoImplementation(NativeMethod.RuntimeHelpersGetMultiDimensionalArrayRank)]
        internal static int GetMultiDimensionalArrayRank(Array array)
        {
            // We don't support the GetLowerBounds/GetUpperBounds methods for multi-dimensional arrays
            return 0;
        }

        [ArduinoImplementation(NativeMethod.RuntimeHelpersGetRawArrayData)]
        internal static unsafe ref byte GetRawArrayData(this Array array)
        {
            throw new NotImplementedException();
        }

        internal static ref byte GetRawData(this object obj) =>
            ref MiniUnsafe.As<RawData>(obj).Data;

        // Helper class to assist with unsafe pinning of arbitrary objects.
        // It's used by VM code (for what?)
        internal class RawData
        {
            public byte Data;
        }

        [ArduinoImplementation]
        internal static new bool Equals(object? o1, object? o2)
        {
            if (ReferenceEquals(o1, o2))
            {
                return true;
            }

            if (o1 == null || o2 == null)
            {
                return false;
            }

            return o1.Equals(o2);
        }
    }
}
