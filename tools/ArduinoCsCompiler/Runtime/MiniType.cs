using System;
using System.Reflection;
using Iot.Device.Arduino;

namespace ArduinoCsCompiler.Runtime
{
    [ArduinoReplacement(typeof(System.Type), true, IncludingSubclasses = true)]
    internal class MiniType
    {
#pragma warning disable 414, SX1309
        // This is used by firmware code directly. Do not reorder the members without checking the firmware
        // The member contains the token of the class declaration
        private Int32 m_handle;
#pragma warning restore 414

        [ArduinoImplementation("TypeCtor")]
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
            [ArduinoImplementation("TypeIsEnum")]
            get
            {
                // Needs support in the backend
                return false;
            }
        }

        public virtual bool IsArray
        {
            [ArduinoImplementation("TypeIsArray")]
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

        public Module? Module
        {
            get
            {
                return null;
            }
        }

        public virtual string Name
        {
            [ArduinoImplementation("TypeName")]
            get
            {
                return string.Empty;
            }
        }

        public virtual Type BaseType
        {
            [ArduinoImplementation("TypeGetBaseType")]
            get
            {
                return null!;
            }
        }

        public virtual string? FullName
        {
            get
            {
                return "Undefined";
            }
        }

        public virtual string? Namespace
        {
            get
            {
                return "Namespace";
            }
        }

        public System.Reflection.MethodInfo GetMethod(System.String name)
        {
            throw new NotSupportedException();
        }

        [ArduinoImplementation(CompareByParameterNames = true)]
        internal static System.Reflection.MethodBase GetMethodBase(Type reflectedType, MethodInfo methodHandle)
        {
            throw new NotImplementedException();
        }

        internal String FormatTypeName()
        {
            return string.Empty;
        }

        public bool IsRuntimeImplemented()
        {
            return true;
        }

        public bool ContainsGenericParameters
        {
            [ArduinoImplementation("TypeContainsGenericParameters")]
            get
            {
                throw new NotImplementedException();
            }
        }

        public bool IsSerializable => false;

        public virtual RuntimeTypeHandle TypeHandle
        {
            [ArduinoImplementation("TypeTypeHandle")]
            get
            {
                return default;
            }
        }

        public bool IsValueType
        {
            [ArduinoImplementation("TypeIsValueType")]
            get
            {
                return false;
            }
        }

        public Type UnderlyingSystemType
        {
            get
            {
                return MiniUnsafe.As<Type>(this);
            }
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

        [ArduinoImplementation("TypeGetTypeFromHandle")]
        public static Type GetTypeFromHandle(RuntimeTypeHandle handle)
        {
            throw new NotImplementedException();
        }

        public static TypeCode GetTypeCode(Type type)
        {
            return TypeCode.Empty;
        }

        [ArduinoImplementation("TypeCreateInstanceForAnotherGenericParameter")]
        public static object? CreateInstanceForAnotherGenericParameter(Type? type1, Type? type2)
        {
            return null;
        }

        [ArduinoImplementation("TypeEquals")]
        public override bool Equals(object? obj)
        {
            throw new NotImplementedException();
        }

        [ArduinoImplementation("TypeGetHashCode")]
        public override int GetHashCode()
        {
            throw new NotImplementedException();
        }

        [ArduinoImplementation("TypeMakeGenericType")]
        public virtual Type MakeGenericType(params Type[] types)
        {
            throw new NotImplementedException();
        }

        [ArduinoImplementation("TypeIsAssignableTo")]
        public virtual bool IsAssignableTo(Type otherType)
        {
            throw new NotImplementedException();
        }

        [ArduinoImplementation("TypeIsAssignableFrom")]
        public virtual bool IsAssignableFrom(Type c)
        {
            throw new NotImplementedException();
        }

        [ArduinoImplementation("TypeIsSubclassOf")]
        public virtual bool IsSubclassOf(Type c)
        {
            throw new NotImplementedException();
        }

        [ArduinoImplementation("TypeGetGenericTypeDefinition")]
        public virtual Type GetGenericTypeDefinition()
        {
            throw new InvalidOperationException();
        }

        [ArduinoImplementation("TypeGetGenericArguments")]
        public virtual Type[] GetGenericArguments()
        {
            return new Type[0];
        }

        public virtual Type GetEnumUnderlyingType()
        {
            return typeof(Int32);
        }

        [ArduinoImplementation("TypeGetElementType")]
        public virtual Type GetElementType()
        {
            throw new NotImplementedException();
        }

        public virtual PropertyInfo[] GetProperties()
        {
            throw new NotImplementedException();
        }

        public virtual bool IsEnumDefined(object value)
        {
            // TODO: Should be possible to implement this using GetEnumValues
            return true;
        }

        public virtual Array GetEnumValues()
        {
            if (!IsEnum)
            {
                throw new ArgumentException("enumType");
            }

            // Get all of the values
            ulong[] values = MiniEnum.InternalGetValues(this);

            // Create a generic Array
            Array ret = Array.CreateInstance(MiniUnsafe.As<Type>(this), values.Length);

            for (int i = 0; i < values.Length; i++)
            {
                object val = Enum.ToObject(MiniUnsafe.As<Type>(this), values[i]);
                ret.SetValue(val, i);
            }

            return ret;
        }
    }
}
