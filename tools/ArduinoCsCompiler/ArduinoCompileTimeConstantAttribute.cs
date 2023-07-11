// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ArduinoCsCompiler
{
    /// <summary>
    /// Applying this attribute to a function evaluates said function at compile time, replacing the contents
    /// with whatever it returned at compile time.
    /// This can be used to insert e.g. culture-dependent values from compile-time into the code.
    /// The attribute can only be used on functions taking no arguments.
    /// </summary>
    [AttributeUsage(AttributeTargets.Method, AllowMultiple = false, Inherited = false)]
    public class ArduinoCompileTimeConstantAttribute : Attribute
    {
        public ArduinoCompileTimeConstantAttribute()
        {
            MethodName = string.Empty;
        }

        internal ArduinoCompileTimeConstantAttribute(string methodName)
        {
            MethodName = methodName;
        }

        public string MethodName
        {
            get;
            set;
        }
    }
}
