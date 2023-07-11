// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System;
using System.Reflection;
using Iot.Device.Arduino;

namespace ArduinoCsCompiler
{
    /// <summary>
    /// This attribute is applied to classes that contain implementations that should replace original implementations.
    /// There are several usage possibilities and use cases:
    /// - Provide an implementation to methods that are in the original CLR implementation calling into native code (declared extern, implemented in the runtime itself)
    /// - Provide an implementation to methods that are to complex to use or shall do something else (i.e. Console.WriteLine)
    /// - Implement methods that query hardware (GpioDriver, Environment.TickCount)
    ///
    /// The individual methods of the replacement class should be attributed with <see cref="ArduinoImplementationAttribute"/>.
    /// If <see cref="ReplaceEntireType"/> is true, the whole source class is replaced, and an error is thrown if a method is used that does not have a replacement. If it is false,
    /// only provided methods are replaced. Replacement methods in that case *must* be attributed and *must* be public (regardless of what visibility the original method has).
    /// </summary>
    [AttributeUsage(AttributeTargets.Class | AttributeTargets.Struct)]
    internal class ArduinoReplacementAttribute : Attribute
    {
        private bool _includingPrivates;

        /// <summary>
        /// The attribute ctor
        /// </summary>
        public ArduinoReplacementAttribute(Type typeToReplace, bool replaceEntireType = false)
        {
            if (typeToReplace == null)
            {
                throw new ArgumentNullException(nameof(typeToReplace));
            }

            TypeToReplace = typeToReplace;
            ReplaceEntireType = replaceEntireType;
            TypeNameToReplace = typeToReplace.FullName!;
        }

        /// <summary>
        /// Use this overload if you need to replace a class that is not publicly visible (i.e. an internal implementation class in the framework)
        /// </summary>
        public ArduinoReplacementAttribute(string typeNameToReplace, string? assemblyName = null, bool replaceEntireType = false, Type? typeInSameAssembly = null)
        {
            TypeNameToReplace = typeNameToReplace;
            if (assemblyName != null)
            {
                Assembly assembly;
                if (typeInSameAssembly != null)
                {
                    assembly = Assembly.GetAssembly(typeInSameAssembly)!;
                }
                else
                {
                    assembly = Assembly.Load(assemblyName);
                }

                if (typeNameToReplace.Contains("+")) // Special marker giving the parent class (we can't access the object the attribute is on from here)
                {
                    string parent = typeNameToReplace.Substring(0, typeNameToReplace.IndexOf("+", StringComparison.OrdinalIgnoreCase));
                    var parentType = assembly.GetType(parent);
                    string sub = typeNameToReplace.Substring(typeNameToReplace.IndexOf("+", StringComparison.OrdinalIgnoreCase) + 1);
                    TypeToReplace = parentType!.GetNestedType(sub, BindingFlags.Public | BindingFlags.NonPublic);
                }
                else
                {
                    TypeToReplace = assembly.GetType(typeNameToReplace, true);
                }
            }
            else
            {
                TypeToReplace = Type.GetType(typeNameToReplace, true);
            }

            ReplaceEntireType = replaceEntireType;
        }

        public Type? TypeToReplace
        {
            get;
        }

        /// <summary>
        /// If the whole type shall be replaced (Any methods not declared in the replacement will throw a MissingMethodException if required)
        /// Otherwise only specific methods are replaced
        /// </summary>
        public bool ReplaceEntireType { get; }

        /// <summary>
        /// True if all subclasses should be replaced the same way (used i.e. to replace all Exception types at once)
        /// </summary>
        public bool IncludingSubclasses { get; set; }

        public bool IncludingPrivates
        {
            get { return _includingPrivates; }
            set { _includingPrivates = value; }
        }

        public string TypeNameToReplace
        {
            get;
        }
    }
}
