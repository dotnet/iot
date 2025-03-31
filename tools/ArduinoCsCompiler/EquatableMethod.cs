// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using Iot.Device.Arduino;

namespace ArduinoCsCompiler
{
    /// <summary>
    /// This class has the sole purpose to reimplement MethodInfo.Equals, because that returns reference equality, and
    /// apparently it is not guaranteed that when the same method is retrieved via reflection multiple times, you get the identical instance.
    /// </summary>
    public class EquatableMethod : IEquatable<EquatableMethod>
    {
        public EquatableMethod(MethodBase method)
        {
            if (ReferenceEquals(method, null))
            {
                throw new ArgumentNullException(nameof(method));
            }

            Method = method;
            Name = method.Name; // Evaluate here, so that the name is stored as a string (can't evaluate MethodInfo.Name during debugging)
        }

        public string Name
        {
            get;
        }

        public MethodBase Method { get; }

        public Type? DeclaringType => Method.DeclaringType;

        public MethodAttributes Attributes => Method.Attributes;

        public bool IsStatic => Method.IsStatic;

        public bool IsPublic => Method.IsPublic;

        public bool IsAbstract => Method.IsAbstract;

        public bool IsPrivate => Method.IsPrivate;

        public bool IsVirtual => Method.IsVirtual;
        public bool IsConstructor => Method.IsConstructor;

        public CallingConventions CallingConvention => Method.CallingConvention;
        public MemberTypes MemberType => Method.MemberType;
        public bool IsConstructedGenericMethod => Method.IsConstructedGenericMethod;

        public static bool operator ==(EquatableMethod? a, EquatableMethod? b)
        {
            if (ReferenceEquals(a, b))
            {
                return true;
            }

            if (ReferenceEquals(b, null))
            {
                return false;
            }

            if (ReferenceEquals(a, null))
            {
                return false;
            }

            return AreMethodsIdentical(a.Method, b.Method);
        }

        public static bool operator !=(EquatableMethod? a, EquatableMethod? b)
        {
            if (ReferenceEquals(a, b))
            {
                return false;
            }

            if (ReferenceEquals(b, null))
            {
                return true;
            }

            if (ReferenceEquals(a, null))
            {
                return true;
            }

            return !AreMethodsIdentical(a.Method, b.Method);
        }

        public static bool operator ==(EquatableMethod? a, MethodBase? b)
        {
            if (ReferenceEquals(a, b))
            {
                return true;
            }

            if (ReferenceEquals(b, null))
            {
                return false;
            }

            if (ReferenceEquals(a, null))
            {
                return false;
            }

            return AreMethodsIdentical(a.Method, b);
        }

        public static bool operator !=(EquatableMethod? a, MethodBase? b)
        {
            if (ReferenceEquals(a, b))
            {
                return false;
            }

            if (ReferenceEquals(b, null))
            {
                return true;
            }

            if (ReferenceEquals(a, null))
            {
                return true;
            }

            return !AreMethodsIdentical(a.Method, b);
        }

        public static implicit operator EquatableMethod(MethodBase a)
        {
            return new EquatableMethod(a);
        }

        /// <summary>
        /// The two methods have the same name and signature (that means one can be replaced with another or one can override another)
        /// </summary>
        public static bool MethodsHaveSameSignature(EquatableMethod a, EquatableMethod b)
        {
            return MethodsHaveSameSignature(a.Method, b.Method, false);
        }

        /// <summary>
        /// The two methods have the same name and signature (that means one can be replaced with another or one can override another)
        /// </summary>
        public static bool MethodsHaveSameSignature(MethodBase a, MethodBase b, bool allowExplicitImplementation = false)
        {
            // A ctor can never match an ordinary method or the other way round
            if (a.GetType() != b.GetType())
            {
                return false;
            }

            if (a.IsStatic != b.IsStatic)
            {
                return false;
            }

            var n1 = a.Name;
            var n2 = b.Name;

            if (allowExplicitImplementation && n1.Contains(".", StringComparison.Ordinal))
            {
                n1 = n1.Substring(n1.LastIndexOf('.') + 1);
            }

            if (allowExplicitImplementation && n2.Contains(".", StringComparison.Ordinal))
            {
                n2 = n2.Substring(n2.LastIndexOf('.') + 1);
            }

            if (n1 != n2)
            {
                return false;
            }

            var argsa = a.GetParameters();
            var argsb = b.GetParameters();

            if (argsa.Length != argsb.Length)
            {
                return false;
            }

            if ((HasArduinoImplementationAttribute(a, out var attrib) && attrib!.CompareByParameterNames) ||
                (HasArduinoImplementationAttribute(b, out attrib) && attrib!.CompareByParameterNames))
            {
                // Even if we only compare by name, the number of generic arguments must match
                if (a.GetGenericArguments().Length != b.GetGenericArguments().Length)
                {
                    return false;
                }

                for (int i = 0; i < argsa.Length; i++)
                {
                    if (argsa[i].Name != argsb[i].Name)
                    {
                        return false;
                    }
                }

                return true;
            }

            for (int i = 0; i < argsa.Length; i++)
            {
                if (!AreSameParameterTypes(argsa[i].ParameterType, argsb[i].ParameterType))
                {
                    return false;
                }
            }

            return true;
        }

        /// <summary>
        /// Returns true if the given method shall be internalized (has a native implementation on the arduino)
        /// </summary>
        internal static bool HasArduinoImplementationAttribute(EquatableMethod method,
            [NotNullWhen(true)] out ArduinoImplementationAttribute attribute)
        {
            return HasArduinoImplementationAttribute(method.Method, out attribute);
        }

        /// <summary>
        /// Returns true if the given method shall be internalized (has a native implementation on the arduino)
        /// </summary>
        internal static bool HasArduinoImplementationAttribute(MethodBase method,
            [NotNullWhen(true)]
            out ArduinoImplementationAttribute attribute)
        {
            return HasAttribute(method, out attribute);
        }

        internal static bool HasAttribute<T>(EquatableMethod method, [NotNullWhen(true)] out T attribute)
            where T : Attribute
        {
            return HasAttribute<T>(method.Method, out attribute);
        }

        internal static bool HasAttribute<T>(MethodBase method, [NotNullWhen(true)] out T attribute)
            where T : Attribute
        {
            var attribs = method.GetCustomAttributes(typeof(T));
            T? iaMethod = (T?)attribs.SingleOrDefault();
            if (iaMethod != null)
            {
                attribute = iaMethod;
                return true;
            }

            attribute = null!;
            return false;
        }

        private static bool AreSameParameterTypes(Type parameterA, Type parameterB)
        {
            if (parameterA == parameterB)
            {
                return true;
            }

            // FullName is null for generic type arguments, since they have no namespace
            if (parameterA.FullName == parameterB.FullName && parameterA.Name == parameterB.Name)
            {
                return true;
            }

            // UintPtr/IntPtr have a platform specific width, that means they're different whether we run in 32 bit or in 64 bit mode on the local(!) computer.
            // But since we know that the target platform is 32 bit, we can assume them to be equal
            if (parameterA == typeof(UIntPtr) && parameterB == typeof(uint))
            {
                return true;
            }

            if (parameterA == typeof(IntPtr) && parameterB == typeof(int))
            {
                return true;
            }

            if (parameterA == typeof(uint) && parameterB == typeof(UIntPtr))
            {
                return true;
            }

            if (parameterA == typeof(int) && parameterB == typeof(IntPtr))
            {
                return true;
            }

            return false;
        }

        /// <summary>
        /// Checks whether "candidate" is overriding "self" in a derived class.
        /// </summary>
        /// <param name="candidate">Method that could be an override</param>
        /// <param name="self">Method that could be overriden</param>
        /// <param name="candidateIsFromInterface">True if the candidate is from an interface (that means we're testing
        /// whether the method is an implementation for the candidate</param>
        /// <returns></returns>
        public static bool IsOverriddenImplementation(EquatableMethod candidate, EquatableMethod self, bool candidateIsFromInterface)
        {
            var interf = candidate.DeclaringType;
            if (interf != null && interf.IsInterface && self.DeclaringType != null && self.DeclaringType.IsArray == false && !self.DeclaringType.IsInterface)
            {
                // The interface map can be used to check whether a method (self) implements a method from an interface. For this
                // the names need not match (and will eventually not, if the method is implemented explicitly).
                // Note that self cannot be an interface in itself.
                var map = self.DeclaringType.GetInterfaceMap(interf);
                for (int i = 0; i < map.InterfaceMethods.Length; i++)
                {
                    if (candidate == map.InterfaceMethods[i] && self == map.TargetMethods[i])
                    {
                        return true;
                    }
                }

                // If we can use the interface map, don't try further, or we'll end up with wrong associations when there are multiple overloads for the same method (i.e. List<T>.GetEnumerator())
                return false;
            }

            if (candidate.Name != self.Name)
            {
                return false;
            }

            // If we're declared new, we're not overriding anything (that does not apply for interfaces, though)
            if (self.Attributes.HasFlag(MethodAttributes.NewSlot) && !candidateIsFromInterface)
            {
                return false;
            }

            // if the base is neither virtual nor abstract, we're not overriding
            if (!candidate.IsVirtual && !candidate.IsAbstract)
            {
                return false;
            }

            // private methods cannot be virtual
            if (self.IsPrivate || candidate.IsPrivate)
            {
                return false;
            }

            return MethodsHaveSameSignature(self.Method, candidate.Method);
        }

        public static bool AreMethodsIdentical(EquatableMethod a, EquatableMethod b)
        {
            return AreMethodsIdentical(a.Method, b.Method);
        }

        public static bool AreMethodsIdentical(MethodBase a, MethodBase b)
        {
            if (ReferenceEquals(a, b))
            {
                return true;
            }

            if (!MethodsHaveSameSignature(a, b))
            {
                return false;
            }

            if (a.IsGenericMethod && b.IsGenericMethod)
            {
                var typeParamsa = a.GetGenericArguments();
                var typeParamsb = b.GetGenericArguments();
                if (typeParamsa.Length != typeParamsb.Length)
                {
                    return false;
                }

                for (int i = 0; i < typeParamsa.Length; i++)
                {
                    if (typeParamsa[i] != typeParamsb[i])
                    {
                        return false;
                    }
                }
            }

            if (a.DeclaringType!.IsConstructedGenericType && b.DeclaringType!.IsConstructedGenericType)
            {
                var typeParamsa = a.DeclaringType.GetGenericArguments();
                var typeParamsb = b.DeclaringType.GetGenericArguments();
                if (typeParamsa.Length != typeParamsb.Length)
                {
                    return false;
                }

                for (int i = 0; i < typeParamsa.Length; i++)
                {
                    if (typeParamsa[i] != typeParamsb[i])
                    {
                        return false;
                    }
                }
            }

            if (a.MetadataToken == b.MetadataToken && a.Module.FullyQualifiedName == b.Module.FullyQualifiedName)
            {
                return true;
            }

            return false;
        }

        /// <summary>
        /// Returns true if the two methods denote the same operator.
        /// We need to handle this a bit special because it is not possible to declare i.e. operator==(Type a, Type b) outside "Type" if we want to replace it.
        /// </summary>
        public static bool AreSameOperatorMethods(EquatableMethod a, EquatableMethod b, bool allowExplicitImplementations)
        {
            return AreSameOperatorMethods(a.Method, b.Method, allowExplicitImplementations);
        }

        /// <summary>
        /// Returns true if the two methods denote the same operator.
        /// We need to handle this a bit special because it is not possible to declare i.e. operator==(Type a, Type b) outside "Type" if we want to replace it.
        /// </summary>
        public static bool AreSameOperatorMethods(MethodBase a, MethodBase b, bool allowExplicitImplementation)
        {
            // A ctor can never match an ordinary method or the other way round
            if (a.GetType() != b.GetType())
            {
                return false;
            }

            var n1 = a.Name;
            var n2 = b.Name;

            if (allowExplicitImplementation && n1.Contains(".", StringComparison.Ordinal))
            {
                n1 = n1.Substring(n1.LastIndexOf('.') + 1);
            }

            if (allowExplicitImplementation && n2.Contains(".", StringComparison.Ordinal))
            {
                n2 = n2.Substring(n2.LastIndexOf('.') + 1);
            }

            if (n1 != n2)
            {
                return false;
            }

            if (a.IsStatic != b.IsStatic)
            {
                return false;
            }

            var argsa = a.GetParameters();
            var argsb = b.GetParameters();

            if (argsa.Length != argsb.Length)
            {
                return false;
            }

            // Same name and named "op_*". These are both operators, so we decide they're equal.
            // Note that this is not necessarily true, because it is possible to define two op_equality members with different argument sets,
            // but this is very discouraged and is hopefully not the case in the System libs.
            if (n1.StartsWith("op_"))
            {
                return true;
            }

            return false;
        }

        public ParameterInfo[] GetParameters()
        {
            return Method.GetParameters();
        }

        public MethodBody? GetMethodBody()
        {
            return Method.GetMethodBody();
        }

        public Type[] GetGenericArguments()
        {
            return Method.GetGenericArguments();
        }

        public IEnumerable<Attribute> GetCustomAttributes(Type attribute)
        {
            return Method.GetCustomAttributes(attribute);
        }

        public bool Equals(EquatableMethod? other)
        {
            if (ReferenceEquals(null, other))
            {
                return false;
            }

            if (ReferenceEquals(this, other))
            {
                return true;
            }

            return AreMethodsIdentical(this, other);
        }

        public override bool Equals(object? obj)
        {
            if (ReferenceEquals(null, obj))
            {
                return false;
            }

            if (ReferenceEquals(this, obj))
            {
                return true;
            }

            if (obj.GetType() != GetType())
            {
                return false;
            }

            return Equals((EquatableMethod)obj);
        }

        public override int GetHashCode()
        {
            return ArduinoImplementationAttribute.GetStaticHashCode(this.MethodSignature(true));
        }

        public override string ToString()
        {
            return this.MethodSignature(false);
        }

        public MethodImplAttributes GetMethodImplementationFlags()
        {
            return Method.GetMethodImplementationFlags();
        }
    }
}
