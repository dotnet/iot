// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System.Collections.Generic;
using System.Reflection;

namespace ArduinoCsCompiler
{
    internal class IlCode
    {
        public IlCode(EquatableMethod method, byte[]? ilBytes)
        {
            Method = method;
            IlBytes = ilBytes;
            DependentMethods = new List<MethodBase>();
            DependentFields = new List<FieldInfo>();
            DependentTypes = new List<TypeInfo>();
            Name = $"{method.MethodSignature()}";
        }

        public IlCode(EquatableMethod method, byte[]? ilBytes, List<MethodBase> methods, List<FieldInfo> fields, List<TypeInfo> types, List<ExceptionClause>? exceptionClauses)
        {
            Method = method;
            IlBytes = ilBytes;
            DependentMethods = methods;
            DependentFields = fields;
            DependentTypes = types;
            ExceptionClauses = exceptionClauses;
            Name = $"{method.MethodSignature()}";
        }

        public EquatableMethod Method
        {
            get;
        }

        public byte[]? IlBytes
        {
            get;
        }

        public string Name
        {
            get;
        }

        /// <summary>
        /// Methods (and constructors) used by this method
        /// </summary>
        public List<MethodBase> DependentMethods
        {
            get;
        }

        /// <summary>
        /// Fields (static and instance, own and from other types) used by this method
        /// </summary>
        public List<FieldInfo> DependentFields
        {
            get;
        }

        /// <summary>
        /// Types used by this method
        /// </summary>
        public List<TypeInfo> DependentTypes
        {
            get;
        }

        /// <summary>
        /// Exception clauses from this method. May be null.
        /// </summary>
        public List<ExceptionClause>? ExceptionClauses { get; }

        public override string ToString()
        {
            return Name;
        }
    }
}
