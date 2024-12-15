// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System.Collections.Generic;
using System.Reflection;

#pragma warning disable CS1591
namespace ArduinoCsCompiler
{
    public class ClassMember
    {
        public ClassMember(string name, VariableKind variableType, int token, int sizeOfField)
        {
            VariableType = variableType;
            Token = token;
            BaseTokens = null;
            SizeOfField = sizeOfField;
            Name = name;
        }

        public ClassMember(FieldInfo field, VariableKind variableType, int token, int sizeOfField, int staticFieldSize)
        {
            VariableType = variableType;
            Token = token;
            BaseTokens = null;
            SizeOfField = sizeOfField;
            Field = field;
            StaticFieldSize = staticFieldSize;
            Name = $"Field: {field.MemberInfoSignature()}";
        }

        public ClassMember(MethodBase method, VariableKind variableType, int token, List<int> baseTokens)
        {
            VariableType = variableType;
            Token = token;
            BaseTokens = baseTokens;
            SizeOfField = 0;
            Method = method;
            Name = $"Method: {method.MethodSignature()}";
        }

        public string Name
        {
            get;
        }

        public MethodBase? Method
        {
            get;
        }

        public FieldInfo? Field
        {
            get;
        }

        /// <summary>
        /// This value is non-zero for static fields. The length is the size of the root field required, so it is
        /// the type length for value types but 4 (sizeof(void*)) for reference types.
        /// </summary>
        public int StaticFieldSize
        {
            get;
        }

        public VariableKind VariableType
        {
            get;
        }

        public int Token
        {
            get;
        }

        public List<int>? BaseTokens
        {
            get;
        }

        public int SizeOfField
        {
            get;
            set;
        }

        /// <summary>
        /// Field offset. Only for non-static fields.
        /// </summary>
        public int Offset
        {
            get;
            set;
        }

        public override string ToString()
        {
            return Name;
        }
    }
}
