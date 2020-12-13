using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace Iot.Device.Arduino
{
    [ArduinoReplacement(typeof(System.Type), true, true)]
    internal class MiniType
    {
#pragma warning disable 414
        // This is used by firmware code directly. Do not reorder the members without checking the firmware
        // The member contains the address of the class declaration
        private UInt32 _internalType;
#pragma warning restore 414

        [ArduinoImplementation(ArduinoImplementation.TypeCtor)]
        protected MiniType()
        {
            // This ctor is never executed - the variable values are pushed directly
            _internalType = 0;
        }

        public virtual bool IsGenericType
        {
            get
            {
                // We do not support incomplete generic types, so we don't really care
                return false;
            }
        }

        public virtual bool IsEnum
        {
            [ArduinoImplementation(ArduinoImplementation.TypeIsEnum)]
            get
            {
                // Needs support in the backend
                return false;
            }
        }

        public Assembly? Assembly
        {
            get
            {
                return null;
            }
        }

        public virtual RuntimeTypeHandle TypeHandle
        {
            [ArduinoImplementation(ArduinoImplementation.TypeTypeHandle)]
            get
            {
                return default;
            }
        }

        public bool IsValueType
        {
            [ArduinoImplementation(ArduinoImplementation.TypeIsValueType)]
            get
            {
                return false;
            }
        }

        [ArduinoImplementation(ArduinoImplementation.TypeGetTypeFromHandle)]
        public static Type GetTypeFromHandle(RuntimeTypeHandle handle)
        {
            throw new NotImplementedException();
        }

        public static bool operator ==(MiniType? a, MiniType? b)
        {
            if (ReferenceEquals(a, null))
            {
                return ReferenceEquals(b, null);
            }

            return a.Equals(b);
        }

        public static bool operator !=(MiniType? a, MiniType? b)
        {
            if (ReferenceEquals(a, null))
            {
                return !ReferenceEquals(b, null);
            }

            return !a.Equals(b);
        }

        [ArduinoImplementation(ArduinoImplementation.TypeEquals)]
        public override bool Equals(object? obj)
        {
            throw new NotImplementedException();
        }

        [ArduinoImplementation(ArduinoImplementation.TypeGetHashCode)]
        public override int GetHashCode()
        {
            throw new NotImplementedException();
        }

        [ArduinoImplementation(ArduinoImplementation.TypeMakeGenericType)]
        public virtual Type MakeGenericType(params Type[] types)
        {
            throw new NotImplementedException();
        }

        [ArduinoImplementation(ArduinoImplementation.TypeIsAssignableTo)]
        public virtual bool IsAssignableTo(Type otherType)
        {
            throw new NotImplementedException();
        }

        [ArduinoImplementation(ArduinoImplementation.TypeIsAssignableFrom)]
        public virtual bool IsAssignableFrom(Type c)
        {
            throw new NotImplementedException();
        }

        [ArduinoImplementation(ArduinoImplementation.TypeIsSubclassOf)]
        public virtual bool IsSubclassOf(Type c)
        {
            throw new NotImplementedException();
        }

        [ArduinoImplementation(ArduinoImplementation.TypeGetGenericTypeDefinition)]
        public virtual Type GetGenericTypeDefinition()
        {
            throw new InvalidOperationException();
        }

        [ArduinoImplementation(ArduinoImplementation.TypeGetGenericArguments)]
        public virtual Type[] GetGenericArguments()
        {
            return new Type[0];
        }

        public static TypeCode GetTypeCode(Type type)
        {
            return TypeCode.Empty;
        }

        public virtual Type GetEnumUnderlyingType()
        {
            return typeof(Int32);
        }

        [ArduinoImplementation(ArduinoImplementation.CreateInstanceForAnotherGenericParameter)]
        public static object? CreateInstanceForAnotherGenericParameter(Type? type1, Type? type2)
        {
            return null;
        }
    }
}
