// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System;
using System.Reflection;
using Iot.Device.Arduino;

namespace ArduinoCsCompiler.Runtime
{
    [ArduinoReplacement(typeof(System.Type), true, IncludingSubclasses = true)]
    internal class MiniType
    {
        public static readonly Type[] EmptyTypes = new Type[0];
#pragma warning disable SX1309
        // This is used by firmware code directly. Do not reorder the members without checking the firmware
        // The member contains the token of the class declaration
        private Int32 m_handle;

        [ArduinoImplementation("TypeCtor", 0x50)]
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

        /// <summary>
        /// This returns true for an open generic type only
        /// </summary>
        public virtual bool IsGenericTypeDefinition
        {
            [ArduinoImplementation("TypeIsGenericTypeDefinition", 235)]
            get
            {
                return (m_handle & ExecutionSet.GenericTokenMask) != 0;
            }
        }

        public virtual bool IsEnum
        {
            [ArduinoImplementation("TypeIsEnum", 0x51)]
            get
            {
                // Needs support in the backend
                return false;
            }
        }

        public virtual bool IsArray
        {
            [ArduinoImplementation("TypeIsArray", 0x52)]
            get
            {
                return false;
            }
        }

        /// <summary>
        /// This could probably be implemented as auto property, but we'd rather save the memory and avoid the cache.
        /// </summary>
        public object? GenericCache
        {
            [ArduinoImplementation]
            get
            {
                return null;
            }

            [ArduinoImplementation]
            set
            {
                // Nothing to do.
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
            [ArduinoImplementation("TypeName", 0x53)]
            get
            {
                return string.Empty;
            }
        }

        public virtual Type BaseType
        {
            [ArduinoImplementation("TypeGetBaseType", 0x54)]
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

        public virtual Type[] GenericTypeArguments
        {
            get
            {
                return (IsGenericType && !IsGenericTypeDefinition) ? GetGenericArguments() : Type.EmptyTypes;
            }
        }

        public virtual Type[] GenericTypeParameters
        {
            [ArduinoImplementation("TypeGetGenericTypeParameters", 233)]
            get
            {
                return new Type[0];
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
            [ArduinoImplementation("TypeContainsGenericParameters", 0x55)]
            get
            {
                throw new NotImplementedException();
            }
        }

        public bool IsSerializable => false;

        public virtual RuntimeTypeHandle TypeHandle
        {
            [ArduinoImplementation("TypeTypeHandle", 0x56)]
            get
            {
                return default;
            }
        }

        public bool IsValueType
        {
            [ArduinoImplementation("TypeIsValueType", 0x57)]
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

        public static Type GetType(string typeName, bool throwOnError)
        {
            throw new NotSupportedException($"Cannot get type {typeName} by name");
        }

        internal virtual RuntimeTypeHandle GetTypeHandleInternal()
        {
            return TypeHandle;
        }

        [ArduinoImplementation("TypeGetTypeFromHandle", 0x58)]
        public static Type GetTypeFromHandle(RuntimeTypeHandle handle)
        {
            throw new NotImplementedException();
        }

        public static TypeCode GetTypeCode(Type type)
        {
            return TypeCode.Empty;
        }

        [ArduinoImplementation("TypeCreateInstanceForAnotherGenericParameter", 0x59)]
        public static object? CreateInstanceForAnotherGenericParameter(Type? type1, Type? type2)
        {
            return null;
        }

        [ArduinoImplementation("TypeEquals", 0x5A)]
        public override bool Equals(object? obj)
        {
            throw new NotImplementedException();
        }

        [ArduinoImplementation]
        public virtual bool Equals(Type other)
        {
            return Equals((object)other);
        }

        [ArduinoImplementation("TypeGetHashCode", 0x5B)]
        public override int GetHashCode()
        {
            throw new NotImplementedException();
        }

        [ArduinoImplementation("TypeMakeGenericType", 0x5C)]
        public virtual Type MakeGenericType(params Type[] types)
        {
            throw new NotImplementedException();
        }

        [ArduinoImplementation("TypeIsAssignableTo", 0x5D)]
        public virtual bool IsAssignableTo(Type otherType)
        {
            throw new NotImplementedException();
        }

        [ArduinoImplementation("TypeIsAssignableFrom", 0x5E)]
        public virtual bool IsAssignableFrom(Type c)
        {
            throw new NotImplementedException();
        }

        [ArduinoImplementation("TypeIsSubclassOf", 0x5F)]
        public virtual bool IsSubclassOf(Type c)
        {
            throw new NotImplementedException();
        }

        [ArduinoImplementation("TypeGetGenericTypeDefinition", 0x60)]
        public virtual Type GetGenericTypeDefinition()
        {
            throw new InvalidOperationException();
        }

        [ArduinoImplementation("TypeGetGenericArguments", 0x61)]
        public virtual Type[] GetGenericArguments()
        {
            return new Type[0];
        }

        public virtual Type GetEnumUnderlyingType()
        {
            return typeof(Int32);
        }

        [ArduinoImplementation("TypeGetElementType", 0x62)]
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

        public virtual bool IsEquivalentTo(Type other)
        {
            return Equals(other);
        }

        [ArduinoImplementation("TypeGetArrayRank", 234)]
        public virtual int GetArrayRank()
        {
            return 1;
        }

        public MethodInfo? GetMethod(string name, Type[] types)
        {
            throw new PlatformNotSupportedException(name);
        }

        public MethodInfo? GetMethod(string name, BindingFlags bindingAttr)
        {
            throw new PlatformNotSupportedException(name);
        }

        [ArduinoImplementation("TypeGetFields")]
        public FieldInfo[]? GetFields(BindingFlags bindingAttr)
        {
            return null;
        }

        [ArduinoImplementation("TypeGetProperties")]
        public virtual PropertyInfo[]? GetProperties(BindingFlags bindingFlags)
        {
            return null;
        }

        public virtual Array GetEnumValues()
        {
            if (!IsEnum)
            {
                throw new ArgumentException("enumType");
            }

            // Get all of the values
            Array values = MiniEnum.GetValues(this);

            // Create a generic Array
            Array ret = Array.CreateInstance(MiniUnsafe.As<Type>(this), values.Length);

            for (int i = 0; i < values.Length; i++)
            {
                object val = Enum.ToObject(MiniUnsafe.As<Type>(this), values.GetValue(i)!);
                ret.SetValue(val, i);
            }

            return ret;
        }
    }
}
