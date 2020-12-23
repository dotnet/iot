using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Iot.Device.Arduino
{
    [ArduinoReplacement(typeof(System.Runtime.CompilerServices.RuntimeHelpers), true)]
    internal class MiniRuntimeHelpers
    {
        [ArduinoImplementation(ArduinoImplementation.RuntimeHelpersInitializeArray)]
        public static void InitializeArray(Array array, RuntimeFieldHandle fldHandle)
        {
            throw new NotImplementedException();
        }

        [ArduinoImplementation(ArduinoImplementation.RuntimeHelpersRunClassConstructor)]
        public static void RunClassConstructor(RuntimeTypeHandle rtHandle)
        {
            throw new NotImplementedException();
        }

        public static int OffsetToStringData
        {
            get
            {
                // TODO: Will depend on our string implementation
                return 12;
            }
        }

        [ArduinoImplementation(ArduinoImplementation.RuntimeHelpersGetHashCode)]
        public static int GetHashCode(object? obj)
        {
            return 0;
        }

        public static bool IsReferenceOrContainsReferences<T>()
        {
            return IsReferenceOrContainsReferencesCore(typeof(T));
        }

        [ArduinoImplementation((ArduinoImplementation.RuntimeHelpersIsReferenceOrContainsReferencesCore))]
        private static bool IsReferenceOrContainsReferencesCore(Type t)
        {
            throw new NotImplementedException();
        }

        internal static bool IsBitwiseEquatable<T>()
        {
            return IsBitwiseEquatableCore(typeof(T));
        }

        [ArduinoImplementation((ArduinoImplementation.RuntimeHelpersIsBitwiseEquatable))]
        private static bool IsBitwiseEquatableCore(Type t)
        {
            throw new NotImplementedException();
        }
    }
}
