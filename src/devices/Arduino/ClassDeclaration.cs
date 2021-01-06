using System;
using System.Collections.Generic;

#pragma warning disable CS1591
namespace Iot.Device.Arduino
{
    public class ClassDeclaration
    {
        public ClassDeclaration(Type type, int dynamicSize, int staticSize, int newToken, List<ClassMember> members)
        {
            TheType = type;
            DynamicSize = dynamicSize;
            StaticSize = staticSize;
            Members = members;
            NewToken = newToken;
            Interfaces = new List<Type>();
            Name = type.ToString();
        }

        public Type TheType
        {
            get;
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

        public List<ClassMember> Members
        {
            get;
            internal set;
        }

        public List<Type> Interfaces
        {
            get;
        }

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

        public override string ToString()
        {
            return Name;
        }
    }
}
