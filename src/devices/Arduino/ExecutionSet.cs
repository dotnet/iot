using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

#pragma warning disable CS1591
namespace Iot.Device.Arduino
{
    public class ExecutionSet
    {
        private const int GenericTokenStep = 0x0100_0000;
        private readonly ArduinoCsCompiler _compiler;
        private readonly List<ArduinoMethodDeclaration> _methods;
        private readonly List<Class> _classes;
        private readonly Dictionary<TypeInfo, int> _patchedTypeTokens;
        private readonly Dictionary<MethodBase, int> _patchedMethodTokens;
        private readonly Dictionary<FieldInfo, (int Token, byte[]? InitializerData)> _patchedFieldTokens;
        private readonly HashSet<(Type Original, Type Replacement, bool Subclasses)> _classesReplaced;
        private readonly List<(MethodBase, MethodBase?)> _methodsReplaced;

        private int _numDeclaredMethods;
        private ArduinoTask _entryPoint;
        private int _nextToken;
        private int _nextGenericToken;

        public ExecutionSet(ArduinoCsCompiler compiler)
        {
            _compiler = compiler;
            _methods = new List<ArduinoMethodDeclaration>();
            _classes = new List<Class>();
            _patchedTypeTokens = new Dictionary<TypeInfo, int>();
            _patchedMethodTokens = new Dictionary<MethodBase, int>();
            _patchedFieldTokens = new Dictionary<FieldInfo, (int Token, byte[]? InitializerData)>();
            _classesReplaced = new HashSet<(Type Original, Type Replacement, bool Subclasses)>();
            _methodsReplaced = new List<(MethodBase, MethodBase?)>();

            _nextToken = (int)KnownTypeTokens.LargestKnownTypeToken + 1;
            _nextGenericToken = GenericTokenStep;

            _numDeclaredMethods = 0;
            _entryPoint = null!;
            MainEntryPointInternal = null;
        }

        public IList<Class> Classes => _classes;

        public ArduinoTask EntryPoint
        {
            get => _entryPoint;
            internal set => _entryPoint = value;
        }

        internal MethodInfo? MainEntryPointInternal
        {
            get;
            set;
        }

        public void Load()
        {
            if (MainEntryPointInternal == null)
            {
                throw new InvalidOperationException("Main entry point not defined");
            }

            // TODO: This should not be necessary later
            _compiler.ClearAllData(true);
            _compiler.SendClassDeclarations(this);
            _compiler.SendMethods(this);
            _compiler.SendConstants(_patchedFieldTokens.Values);

            EntryPoint = _compiler.GetTask(this, MainEntryPointInternal);

            // Execute all static ctors
            _compiler.ExecuteStaticCtors(this);
        }

        public long EstimateRequiredMemory()
        {
            long bytesUsed = 0;
            foreach (var cls in Classes)
            {
                bytesUsed += 40;
                bytesUsed += cls.StaticSize;
                bytesUsed += cls.Members.Count * 8; // Assuming 32 bit target system for now
                foreach (var fld in cls.Members.Where(x => x.InitialValue != null))
                {
                    if (fld.InitialValue is Array arr)
                    {
                        bytesUsed += arr.Length;
                    }
                }
            }

            foreach (var method in _methods)
            {
                bytesUsed += 48;
                bytesUsed += method.ArgumentCount * 8;

                if (method.IlBytes != null)
                {
                    bytesUsed += method.IlBytes.Length;
                }
            }

            return bytesUsed;
        }

        public int GetOrAddMethodToken(MethodBase methodBase)
        {
            int token;
            if (_patchedMethodTokens.TryGetValue(methodBase, out token))
            {
                return token;
            }

            var replacement = GetReplacement(methodBase);
            if (replacement != null)
            {
                return GetOrAddMethodToken(replacement);
            }

            token = _nextToken++;
            _patchedMethodTokens.Add(methodBase, token);
            return token;
        }

        public int GetOrAddFieldToken(FieldInfo field)
        {
            if (_patchedFieldTokens.TryGetValue(field, out var token))
            {
                return token.Token;
            }

            int tk = _nextToken++;
            _patchedFieldTokens.Add(field, (tk, null));
            return tk;
        }

        public int GetOrAddFieldToken(FieldInfo field, byte[] initializerData)
        {
            if (_patchedFieldTokens.TryGetValue(field, out var token))
            {
                return token.Token;
            }

            int tk = _nextToken++;

            _patchedFieldTokens.Add(field, (tk, initializerData));
            return tk;
        }

        public int GetOrAddClassToken(TypeInfo typeInfo)
        {
            int token;
            if (_patchedTypeTokens.TryGetValue(typeInfo, out token))
            {
                return token;
            }

            var replacement = GetReplacement(typeInfo);
            if (replacement != null)
            {
                return GetOrAddClassToken(replacement.GetTypeInfo());
            }

            if (typeInfo == typeof(object))
            {
                token = (int)KnownTypeTokens.Object;
            }
            else if (typeInfo.Name == "System.Enum")
            {
                // TODO: Check handling of enums. Will probably have to provide this flag to the runtime.
                // Note that enums are not value types.
                token = (int)KnownTypeTokens.Enum;
            }
            else if (typeInfo == typeof(TypeInfo))
            {
                token = (int)KnownTypeTokens.TypeInfo;
            }
            else if (typeInfo.FullName == "System.RuntimeType")
            {
                token = (int)KnownTypeTokens.RuntimeType;
            }
            else if (typeInfo == typeof(Type) || typeInfo == typeof(MiniType))
            {
                token = (int)KnownTypeTokens.Type;
            }
            else if (typeInfo.IsGenericTypeDefinition)
            {
                // Type with (at least one) incomplete type argument.
                // We use token values > 24 bit, so that we can add it to the base type to implement MakeGenericType(), at least for single type arguments
                token = _nextGenericToken;
                _nextGenericToken += GenericTokenStep;
            }
            else if (typeInfo.IsGenericType)
            {
                // complete generic type, find whether the base definition has been defined already
                var definition = typeInfo.GetGenericTypeDefinition();
                int definitionToken = GetOrAddClassToken(definition.GetTypeInfo());
                Type[] typeArguments = typeInfo.GetGenericArguments();
                Type firstArg = typeArguments.First();
                int firstArgToken = GetOrAddClassToken(firstArg.GetTypeInfo());
                // Our token is the combination of the generic type and the first argument. This allows a simple implementation for Type.MakeGenericType() with
                // generic types with a single argument
                token = firstArgToken + definitionToken;
            }
            else
            {
                token = _nextToken++;
            }

            _patchedTypeTokens.Add(typeInfo, token);
            return token;
        }

        public bool AddClass(Class type)
        {
            if (_classes.Any(x => x.Cls == type.Cls))
            {
                return false;
            }

            if (_classesReplaced.Any(x => x.Original == type.Cls))
            {
                throw new InvalidOperationException($"Class {type} should have been replaced by its replacement");
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

            if (_methodsReplaced.Any(x => x.Item1 == method.MethodBase))
            {
                throw new InvalidOperationException($"Method {method} should have been replaced by its replacement");
            }

            if (_methodsReplaced.Any(x => x.Item1 == method.MethodBase && x.Item2 == null))
            {
                throw new InvalidOperationException($"The method {method} should be replaced, but has no new implementation. This program will not execute");
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

        public MemberInfo? InverseResolveToken(int token)
        {
            // Todo: This is very slow - consider inversing the dictionaries during data prep
            var method = _patchedMethodTokens.FirstOrDefault(x => x.Value == token);
            if (method.Key != null)
            {
                return method.Key;
            }

            var field = _patchedFieldTokens.FirstOrDefault(x => x.Value.Token == token);
            if (field.Key != null)
            {
                return field.Key;
            }

            var t = _patchedTypeTokens.FirstOrDefault(x => x.Value == token);
            if (t.Key != null)
            {
                return t.Key;
            }

            return null;
        }

        public class VariableOrMethod
        {
            public VariableOrMethod(VariableKind variableType, int token, List<int> baseTokens)
            {
                VariableType = variableType;
                Token = token;
                BaseTokens = baseTokens;
                InitialValue = null;
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

            public object? InitialValue
            {
                get;
                set;
            }
        }

        public class Class
        {
            private bool _suppressInit;

            public Class(Type cls, int dynamicSize, int staticSize, int newToken, List<VariableOrMethod> members)
            {
                Cls = cls;
                DynamicSize = dynamicSize;
                StaticSize = staticSize;
                Members = members;
                NewToken = newToken;
            }

            public Type Cls
            {
                get;
            }

            public int NewToken
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

            public bool SuppressInit
            {
                get { return _suppressInit; }
                set { _suppressInit = value; }
            }

            public override string ToString()
            {
                return Cls.ToString();
            }
        }

        public void AddReplacementType(Type? typeToReplace, Type replacement, bool includingSubclasses)
        {
            if (typeToReplace == null)
            {
                throw new ArgumentNullException(nameof(typeToReplace));
            }

            if (!_classesReplaced.Add((typeToReplace, replacement, includingSubclasses)))
            {
                return;
            }

            List<MethodInfo> methodsNeedingReplacement = typeToReplace.GetMethods(BindingFlags.Public | BindingFlags.Static | BindingFlags.Instance).ToList();

            foreach (var methoda in replacement.GetMethods(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Static | BindingFlags.Instance))
            {
                // Above, we only check the public methods, here we also look at the private ones
                foreach (var methodb in typeToReplace.GetMethods(BindingFlags.Public | BindingFlags.Static | BindingFlags.Instance | BindingFlags.NonPublic))
                {
                    if (ArduinoCsCompiler.MethodsHaveSameSignature(methoda, methodb) || ArduinoCsCompiler.AreSameOperatorMethods(methoda, methodb))
                    {
                        // Method A shall replace Method B
                        AddReplacementMethod(methodb, methoda);
                        // Remove from the list - so we see in the end what is missing
                        methodsNeedingReplacement.Remove(methodb);
                        break;
                    }
                }
            }

            // Add these as "not implemented" to the list, so we can figure out what we actually need
            foreach (var m in methodsNeedingReplacement)
            {
                AddReplacementMethod(m, null);
            }

            // And do the same as above for all (public) ctors
            var ctorsNeedingReplacement = typeToReplace.GetConstructors(BindingFlags.Public | BindingFlags.Instance).ToList();

            foreach (var methoda in replacement.GetConstructors(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance))
            {
                // Above, we only check the public methods, here we also look at the private ones
                foreach (var methodb in typeToReplace.GetConstructors(BindingFlags.Public | BindingFlags.Instance | BindingFlags.NonPublic))
                {
                    if (ArduinoCsCompiler.MethodsHaveSameSignature(methoda, methodb))
                    {
                        // Method A shall replace Method B
                        AddReplacementMethod(methodb, methoda);
                        // Remove from the list - so we see in the end what is missing
                        ctorsNeedingReplacement.Remove(methodb);
                        break;
                    }
                }
            }

            foreach (var m in ctorsNeedingReplacement)
            {
                AddReplacementMethod(m, null);
            }
        }

        public Type? GetReplacement(Type? original)
        {
            if (original == null)
            {
                return null;
            }

            foreach (var x in _classesReplaced)
            {
                if (x.Original == original)
                {
                    return x.Replacement;
                }
                else if (x.Subclasses && original.IsSubclassOf(x.Original))
                {
                    // If we need to replace all subclasses of x as well, we need to add them here to the replacement list, because
                    // we initially didn't know which classes will be in this set.
                    AddReplacementType(original, x.Replacement, true);
                    return x.Replacement;
                }
            }

            return null;
        }

        public MethodBase? GetReplacement(MethodBase original)
        {
            var elem = _methodsReplaced.FirstOrDefault(x => x.Item1 == original);
            if (elem.Item1 == default)
            {
                return null;
            }
            else if (elem.Item2 == null)
            {
                throw new InvalidOperationException($"Should have a replacement for {original.DeclaringType} - {original}, but it is missing.");
            }

            return elem.Item2;
        }

        public void AddReplacementMethod(MethodBase? toReplace, MethodBase? replacement)
        {
            if (toReplace == null)
            {
                throw new ArgumentNullException(nameof(toReplace));
            }

            _methodsReplaced.Add((toReplace, replacement));
        }
    }
}
