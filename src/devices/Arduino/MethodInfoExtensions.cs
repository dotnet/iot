using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace Iot.Device.Arduino
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

        /// <summary>
        /// Returns the type as a string in C# syntax.
        /// This is similar to <see cref="Type.ToString"/>, but returns the type name in a form known to C# programmers, instead of the form defined by the ECMA standard
        /// </summary>
        /// <param name="t">The type to print</param>
        /// <param name="useFullNamespaces">True (the default) to print full namespace info, otherwise namespaces are omitted</param>
        /// <returns>A string representing the type</returns>
        public static string ClassSignature(this Type t, bool useFullNamespaces = true)
        {
            if (t.IsGenericType == false)
            {
                if (useFullNamespaces)
                {
                    return t.FullName ?? t.Name;
                }

                return t.Name;
            }

            StringBuilder b = new StringBuilder();
            if (t.Namespace != null && useFullNamespaces)
            {
                b.Append(t.Namespace + ".");
            }

            string nameSimple = t.Name; // Contains the number of generic arguments in the form `N at the end
            int idx = nameSimple.IndexOf('`');
            nameSimple = nameSimple.Substring(0, idx);
            b.Append(nameSimple);
            b.Append('<');
            int genericParameterLength = t.GenericTypeArguments.Length;
            for (var index = 0; index < genericParameterLength; index++)
            {
                var typeParam = t.GenericTypeArguments[index];
                b.Append(ClassSignature(typeParam, useFullNamespaces));
                if (index < genericParameterLength - 1)
                {
                    b.Append(", ");
                }
            }

            // If this is an open generic type, print the generic type names instead
            if (genericParameterLength == 0)
            {
                var args = t.GetGenericArguments();
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

            return b.ToString();
        }
    }
}
