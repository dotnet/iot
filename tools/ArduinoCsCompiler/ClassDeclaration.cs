// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System;
using System.Collections.Generic;

#pragma warning disable CS1591
namespace ArduinoCsCompiler
{
    public class ClassDeclaration
    {
        private readonly List<ClassMember> _members;
        private readonly List<Type> _interfaces;

        public ClassDeclaration(Type type, int dynamicSize, int staticSize, int newToken, List<ClassMember> members, List<Type> interfaces)
        {
            TheType = type;
            DynamicSize = dynamicSize;
            StaticSize = staticSize;
            _members = members;
            NewToken = newToken;
            _interfaces = interfaces;
            Name = type.ClassSignature(true);
            ReadOnly = false;
        }

        public Type TheType
        {
            get;
        }

        /// <summary>
        /// This is set to true if the class is made read-only (i.e copied to flash). No further members can be added in this case
        /// </summary>
        public bool ReadOnly
        {
            get;
            internal set;
        }

        public int NewToken
        {
            get;
        }

        public string Name
        {
            get;
        }

        public int DynamicSize { get; }
        public int StaticSize { get; }

        public IList<ClassMember> Members => _members.AsReadOnly();

        public IEnumerable<Type> Interfaces => _interfaces;

        public bool SuppressInit
        {
            get
            {
                // Type initializers of open generic types are pointless to execute
                if (TheType.ContainsGenericParameters)
                {
                    return true;
                }

                // Don't run these init functions, to complicated or depend on native functions
                return TheType.FullName == "System.SR";
            }
        }

        public void AddClassMember(ClassMember member)
        {
            if (ReadOnly)
            {
                throw new NotSupportedException($"Cannot update class {Name}, as it is read-only");
            }

            _members.Add(member);
        }

        public void RemoveMemberAt(int index)
        {
            _members.RemoveAt(index);
        }

        public override string ToString()
        {
            return Name;
        }
    }
}
