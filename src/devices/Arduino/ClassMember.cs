using System.Collections.Generic;

#pragma warning disable CS1591
namespace Iot.Device.Arduino
{
    public class ClassMember
    {
        public ClassMember(VariableKind variableType, int token, int sizeOfField)
        {
            VariableType = variableType;
            Token = token;
            BaseTokens = null;
            SizeOfField = sizeOfField;
        }

        public ClassMember(VariableKind variableType, int token, List<int> baseTokens)
        {
            VariableType = variableType;
            Token = token;
            BaseTokens = baseTokens;
            SizeOfField = 0;
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
    }
}
