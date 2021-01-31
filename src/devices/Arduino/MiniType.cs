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
#pragma warning disable 414, SX1309
        // This is used by firmware code directly. Do not reorder the members without checking the firmware
        // The member contains the token of the class declaration
        private Int32 m_handle;
#pragma warning restore 414

        [ArduinoImplementation(NativeMethod.TypeCtor)]
        protected MiniType()
        {
            // This ctor is never executed - the variable values are pushed directly
            m_handle = 0;
        }

        public virtual bool IsGenericType
        {
            get
            {
                // All types that have some generics return true here, whether they're open or closed. Nullable also returns true
                return (m_handle & ExecutionSet.GenericTokenMask) != 0;
            }
        }

        public virtual bool IsEnum
        {
            [ArduinoImplementation(NativeMethod.TypeIsEnum)]
            get
            {
                // Needs support in the backend
                return false;
            }
        }

        public virtual bool IsArray
        {
            [ArduinoImplementation(NativeMethod.TypeIsArray)]
            get
            {
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

        public virtual string? FullName
        {
            get
            {
                return "Undefined";
            }
        }

        public virtual RuntimeTypeHandle TypeHandle
        {
            [ArduinoImplementation(NativeMethod.TypeTypeHandle)]
            get
            {
                return default;
            }
        }

        public bool IsValueType
        {
            [ArduinoImplementation(NativeMethod.TypeIsValueType)]
            get
            {
                return false;
            }
        }

        [ArduinoImplementation(NativeMethod.TypeGetTypeFromHandle)]
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

        internal virtual RuntimeTypeHandle GetTypeHandleInternal()
        {
            return TypeHandle;
        }

        [ArduinoImplementation(NativeMethod.TypeEquals)]
        public override bool Equals(object? obj)
        {
            throw new NotImplementedException();
        }

        [ArduinoImplementation(NativeMethod.TypeGetHashCode)]
        public override int GetHashCode()
        {
            throw new NotImplementedException();
        }

        [ArduinoImplementation(NativeMethod.TypeMakeGenericType)]
        public virtual Type MakeGenericType(params Type[] types)
        {
            throw new NotImplementedException();
        }

        [ArduinoImplementation(NativeMethod.TypeIsAssignableTo)]
        public virtual bool IsAssignableTo(Type otherType)
        {
            throw new NotImplementedException();
        }

        [ArduinoImplementation(NativeMethod.TypeIsAssignableFrom)]
        public virtual bool IsAssignableFrom(Type c)
        {
            throw new NotImplementedException();
        }

        [ArduinoImplementation(NativeMethod.TypeIsSubclassOf)]
        public virtual bool IsSubclassOf(Type c)
        {
            throw new NotImplementedException();
        }

        [ArduinoImplementation(NativeMethod.TypeGetGenericTypeDefinition)]
        public virtual Type GetGenericTypeDefinition()
        {
            throw new InvalidOperationException();
        }

        [ArduinoImplementation(NativeMethod.TypeGetGenericArguments)]
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

        [ArduinoImplementation(NativeMethod.TypeGetElementType)]
        public virtual Type GetElementType()
        {
            throw new NotImplementedException();
        }

        public virtual PropertyInfo[] GetProperties()
        {
            throw new NotImplementedException();
        }

        [ArduinoImplementation(NativeMethod.TypeCreateInstanceForAnotherGenericParameter)]
        public static object? CreateInstanceForAnotherGenericParameter(Type? type1, Type? type2)
        {
            return null;
        }
    }
}
