using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

#pragma warning disable CS1591
namespace Iot.Device.Arduino
{
    public class ExecutionSet
    {
        private readonly ArduinoCsCompiler _compiler;
        private readonly List<ArduinoMethodDeclaration> _methods;
        private readonly List<Class> _classes;
        private int _numDeclaredMethods;

        public ExecutionSet(ArduinoCsCompiler compiler)
        {
            _compiler = compiler;
            _methods = new List<ArduinoMethodDeclaration>();
            _classes = new List<Class>();
            _numDeclaredMethods = 0;
        }

        public IList<Class> Classes => _classes;

        public void FinalizeSet()
        {
            _compiler.FinalizeExecutionSet(this);
        }

        public void Load()
        {
            // TODO: This should not be necessary later
            _compiler.ClearAllData(true);
            _compiler.SendClassDeclarations(this);
            _compiler.SendMethods(this);
        }

        public long EstimateRequiredMemory()
        {
            long bytesUsed = 0;
            foreach (var cls in Classes)
            {
                bytesUsed += 20;
                bytesUsed += cls.StaticSize;
                bytesUsed += cls.Members.Count * 8; // Assuming 32 bit target system for now
            }

            foreach (var method in _methods)
            {
                bytesUsed += 12;
                bytesUsed += method.ArgumentCount * 8;
                if (method.TokenMap != null)
                {
                    bytesUsed += method.TokenMap.Count;
                }

                if (method.IlBytes != null)
                {
                    bytesUsed += method.IlBytes.Length;
                }
            }

            return bytesUsed;
        }

        public bool AddClass(Class type)
        {
            if (_classes.Any(x => x.Cls == type.Cls))
            {
                return false;
            }

            _classes.Add(type);
            return true;
        }

        public bool HasDefinition(Type classType)
        {
            if (_classes.Any(x => x.Cls == classType))
            {
                return true;
            }

            return false;
        }

        public bool HasMethod(MemberInfo m)
        {
            return _methods.Any(x => x.MethodBase == m);
        }

        public bool AddMethod(ArduinoMethodDeclaration method)
        {
            if (_numDeclaredMethods >= Math.Pow(2, 14) - 1)
            {
                // In practice, the maximum will be much less on most Arduino boards, due to ram limits
                throw new NotSupportedException("To many methods declared. Only 2^14 supported.");
            }

            if (_methods.Any(x => x.MethodBase == method.MethodBase))
            {
                return false;
            }

            _methods.Add(method);
            method.Index = _numDeclaredMethods;
            _numDeclaredMethods++;

            return true;
        }

        public IEnumerable<ArduinoMethodDeclaration> Methods()
        {
            return _methods;
        }

        public class VariableOrMethod
        {
            public VariableOrMethod(VariableKind variableType, int token, List<int> baseTokens)
            {
                VariableType = variableType;
                Token = token;
                BaseTokens = baseTokens;
            }

            public VariableKind VariableType
            {
                get;
            }

            public int Token
            {
                get;
            }

            public List<int> BaseTokens
            {
                get;
            }
        }

        public class Class
        {
            public Class(Type cls, int dynamicSize, int staticSize, List<VariableOrMethod> members)
            {
                Cls = cls;
                DynamicSize = dynamicSize;
                StaticSize = staticSize;
                Members = members;
            }

            public Type Cls
            {
                get;
            }

            public int DynamicSize { get; }
            public int StaticSize { get; }

            public List<VariableOrMethod> Members
            {
                get;
                internal set;
            }
        }

    }
}
