// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System;
using System.Linq;
using System.Reflection;
using System.Text;

namespace ArduinoCsCompiler
{
    /// <summary>
    /// Contains pretty-print methods for <see cref="MethodInfo"/>
    /// </summary>
    public static class MethodInfoExtensions
    {
        /// <summary>
        /// Returns the signature of the method in C# syntax
        /// </summary>
        /// <param name="me">A MethodInfo instance</param>
        /// <param name="useFullNamespaces">True to print full namespace info, false to omit them</param>
        /// <returns></returns>
        public static string MethodSignature(this MethodBase me, bool useFullNamespaces = true)
        {
            StringBuilder b = new StringBuilder();
            if (me.IsPublic)
            {
                b.Append("public ");
            }

            if (me.IsFamily)
            {
                b.Append("protected ");
            }

            if (me.IsPrivate)
            {
                b.Append("private ");
            }

            if (me.IsAssembly)
            {
                b.Append("internal ");
            }

            if (me.IsStatic)
            {
                b.Append("static ");
            }

            if (me.IsAbstract)
            {
                b.Append("abstract ");
            }

            if (me.IsVirtual)
            {
                b.Append("virtual ");
            }

            if (me is MethodInfo mi)
            {
                if (mi.ReturnType == typeof(void))
                {
                    b.Append("void ");
                }
                else
                {
                    b.Append(ClassSignature(mi.ReturnType, useFullNamespaces));
                    b.Append(' ');
                }
            }
            else
            {
                // Is a ctor
            }

            if (me.DeclaringType != null)
            {
                b.Append(ClassSignature(me.DeclaringType, useFullNamespaces));
                b.Append(".");
            }

            b.Append(me.Name);
            if (me.IsGenericMethod || me.IsGenericMethodDefinition)
            {
                var genericParametersToUse = me.GetGenericArguments();
                if (genericParametersToUse.Length > 0)
                {
                    b.Append('<');
                    int genericParameterLength = genericParametersToUse.Length;
                    for (var index = 0; index < genericParameterLength; index++)
                    {
                        var typeParam = genericParametersToUse[index];
                        b.Append(ClassSignature(typeParam, useFullNamespaces));
                        if (index < genericParameterLength - 1)
                        {
                            b.Append(", ");
                        }
                    }

                    b.Append(">");
                }
            }

            b.Append('(');
            int paramLength = me.GetParameters().Length;
            for (var index = 0; index < paramLength; index++)
            {
                var param = me.GetParameters()[index];
                b.Append(ClassSignature(param.ParameterType, useFullNamespaces));
                b.AppendFormat(" {0}", param.Name);
                if (index < paramLength - 1)
                {
                    b.Append(", ");
                }
            }

            b.Append(')');
            return b.ToString();
        }

        public static string MethodSignature(this EquatableMethod em, bool useFullNamespaces = true)
        {
            return MethodSignature(em.Method, useFullNamespaces);
        }

        public static string MemberInfoSignature(this EquatableMethod em, bool useFullNamespaces = true)
        {
            return MemberInfoSignature((MemberInfo)em.Method, useFullNamespaces);
        }

        /// <summary>
        /// Prints the signature of an arbitrary member
        /// </summary>
        public static string MemberInfoSignature(this MemberInfo mi, bool useFullNamespaces = true)
        {
            if (mi is Type t)
            {
                return t.ClassSignature(useFullNamespaces);
            }

            if (mi is FieldInfo fi)
            {
                if (fi.DeclaringType != null)
                {
                    return $"{fi.FieldType.ClassSignature(useFullNamespaces)} {fi.DeclaringType.ClassSignature(useFullNamespaces)}.{fi.Name}";
                }
                else
                {
                    return fi.Name;
                }
            }

            if (mi is MethodBase me)
            {
                return me.MethodSignature(useFullNamespaces);
            }

            throw new NotSupportedException("Missing case");
        }

        /// <summary>
        /// Returns the type as a string in C# syntax.
        /// This is similar to <see cref="Type.ToString"/>, but returns the type name in a form known to C# programmers, instead of the form defined by the ECMA standard
        /// </summary>
        /// <param name="t">The type to print</param>
        /// <param name="useFullNamespaces">True (the default) to print full namespace info, otherwise namespaces are omitted</param>
        /// <returns>A string representing the type</returns>
        public static string ClassSignature(this Type t, bool useFullNamespaces = true)
        {
            return ClassSignature(t, t.GenericTypeArguments, t.GetGenericArguments(), useFullNamespaces);
        }

        private static string ClassSignature(this Type t, Type[] genericParametersToUse, Type[] genericArguments, bool useFullNamespaces = true)
        {
            if (t.IsUnmanagedFunctionPointer)
            {
                // The name of these is null, thus invent something
                return "fnptr*";
            }

            if (t.IsGenericType == false)
            {
                if (useFullNamespaces)
                {
                    return t.FullName ?? t.Name;
                }

                return t.Name;
            }

            StringBuilder b = new StringBuilder();
            if (t.Namespace != null && useFullNamespaces && !t.IsNested)
            {
                b.Append(t.Namespace + ".");
            }

            if (t.IsNested && t.DeclaringType != null)
            {
                // If this is an internal class to a generic outer class, the type arguments for the outer class are on this type.
                int elementsOnParent = 0;
                if (t.DeclaringType.IsGenericType)
                {
                    elementsOnParent = t.DeclaringType.GetGenericArguments().Length;
                }

                b.Append(t.DeclaringType.ClassSignature(t.GenericTypeArguments.Take(elementsOnParent).ToArray(), t.GetGenericArguments().Take(elementsOnParent).ToArray(), useFullNamespaces));
                genericParametersToUse = genericParametersToUse.Skip(elementsOnParent).ToArray();
                genericArguments = genericArguments.Skip(elementsOnParent).ToArray();
                b.Append('+');
            }

            string nameSimple = t.Name; // Contains the number of generic arguments in the form `N at the end (unless it's an internal class to a generic class)
            int idx = nameSimple.IndexOf('`');
            if (idx >= 0)
            {
                nameSimple = nameSimple.Substring(0, idx);
            }

            b.Append(nameSimple);
            if (genericParametersToUse.Length > 0 || genericArguments.Length > 0)
            {
                b.Append('<');
                int genericParameterLength = genericParametersToUse.Length;
                for (var index = 0; index < genericParameterLength; index++)
                {
                    var typeParam = genericParametersToUse[index];
                    b.Append(ClassSignature(typeParam, useFullNamespaces));
                    if (index < genericParameterLength - 1)
                    {
                        b.Append(", ");
                    }
                }

                // If this is an open generic type, print the generic type names instead
                if (genericParameterLength == 0)
                {
                    var args = genericArguments;
                    for (var index = 0; index < args.Length; index++)
                    {
                        var typeParam = args[index];
                        b.Append(typeParam.Name);
                        if (index < args.Length - 1)
                        {
                            b.Append(", ");
                        }
                    }
                }

                b.Append(">");
            }

            return b.ToString();
        }
    }
}
