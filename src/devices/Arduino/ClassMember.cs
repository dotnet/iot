using System.Collections.Generic;
using System.Reflection;

#pragma warning disable CS1591
namespace Iot.Device.Arduino
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

        public ClassMember(FieldInfo field, VariableKind variableType, int token, int sizeOfField)
        {
            VariableType = variableType;
            Token = token;
            BaseTokens = null;
            SizeOfField = sizeOfField;
            Name = $"Field: {field.DeclaringType} - {field}";
        }

        public ClassMember(MethodBase method, VariableKind variableType, int token, List<int> baseTokens)
        {
            VariableType = variableType;
            Token = token;
            BaseTokens = baseTokens;
            SizeOfField = 0;
            Name = $"Method: {method.DeclaringType} - {method}";
        }

        public string Name
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
        }

        public override string ToString()
        {
            return Name;
        }
    }
}
