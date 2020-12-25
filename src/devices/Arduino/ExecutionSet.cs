using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.ComponentModel;
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
        private const int StringTokenStep = 0x0001_0000;
        private const int NullableToken = 0x0080_0000;
        public const int GenericTokenMask = -8_388_608; // 0xFF80_0000 as signed
        private readonly ArduinoCsCompiler _compiler;
        private readonly List<ArduinoMethodDeclaration> _methods;
        private readonly List<Class> _classes;
        private readonly Dictionary<TypeInfo, int> _patchedTypeTokens;
        private readonly Dictionary<int, TypeInfo> _inversePatchedTypeTokens;
        private readonly Dictionary<MethodBase, int> _patchedMethodTokens;
        private readonly Dictionary<int, MethodBase> _inversePatchedMethodTokens; // Same as the above, but the other way round
        private readonly Dictionary<FieldInfo, (int Token, byte[]? InitializerData)> _patchedFieldTokens;
        private readonly Dictionary<int, FieldInfo> _inversePatchedFieldTokens;
        private readonly HashSet<(Type Original, Type Replacement, bool Subclasses)> _classesReplaced;
        private readonly List<(MethodBase, MethodBase?)> _methodsReplaced;
        // These classes (and any of their methods) will not be loaded, even if they seem in use. This should speed up testing
        private readonly List<Type> _classesToSuppress;
        private readonly Dictionary<int, string> _strings;

        private int _numDeclaredMethods;
        private ArduinoTask _entryPoint;
        private int _nextToken;
        private int _nextGenericToken;
        private int _nextStringToken;

        public ExecutionSet(ArduinoCsCompiler compiler)
        {
            _compiler = compiler;
            _methods = new List<ArduinoMethodDeclaration>();
            _classes = new List<Class>();
            _patchedTypeTokens = new Dictionary<TypeInfo, int>();
            _patchedMethodTokens = new Dictionary<MethodBase, int>();
            _patchedFieldTokens = new Dictionary<FieldInfo, (int Token, byte[]? InitializerData)>();
            _inversePatchedMethodTokens = new Dictionary<int, MethodBase>();
            _inversePatchedTypeTokens = new Dictionary<int, TypeInfo>();
            _inversePatchedFieldTokens = new Dictionary<int, FieldInfo>();
            _classesReplaced = new HashSet<(Type Original, Type Replacement, bool Subclasses)>();
            _methodsReplaced = new List<(MethodBase, MethodBase?)>();
            _classesToSuppress = new List<Type>();
            _strings = new Dictionary<int, string>();

            _nextToken = (int)KnownTypeTokens.LargestKnownTypeToken + 1;
            _nextGenericToken = GenericTokenStep;
            _nextStringToken = StringTokenStep; // The lower 16 bit are the length

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
            List<(int Token, byte[] Data)> converted = new List<(int Token, byte[] Data)>();
            // Need to do this manually, due to stupid nullability conversion restrictions
            foreach (var elem in _patchedFieldTokens.Values)
            {
                if (elem.InitializerData != null)
                {
                    converted.Add((elem.Token, elem.InitializerData));
                }
            }

            _compiler.SendConstants(converted);
            _compiler.SendConstants(_strings.Select(x => (x.Key, Encoding.Default.GetBytes(x.Value))));

            EntryPoint = _compiler.GetTask(this, MainEntryPointInternal);

            // Execute all static ctors
            _compiler.ExecuteStaticCtors(this);
        }

        public void SuppressType(Type t)
        {
            _classesToSuppress.Add(t);
        }

        public void SuppressTypes(IEnumerable<Type>? types)
        {
            if (types != null)
            {
                foreach (var t in types)
                {
                    SuppressType(t);
                }
            }
        }

        public long EstimateRequiredMemory()
        {
            Dictionary<Type, long> classSizes = new Dictionary<Type, long>();
            long bytesUsed = 0;
            foreach (var cls in Classes)
            {
                bytesUsed += 40;
                bytesUsed += cls.StaticSize;
                bytesUsed += cls.Members.Count * 8; // Assuming 32 bit target system for now
            }

            foreach (var method in _methods)
            {
                bytesUsed += 48;
                bytesUsed += method.ArgumentCount * 8;

                int methodBytes = method.IlBytes != null ? method.IlBytes.Length : 0;
                if (method.IlBytes != null)
                {
                    bytesUsed += methodBytes;
                }

                var type = method.MethodBase.DeclaringType!;
                if (classSizes.TryGetValue(type, out var classBytes))
                {
                    classSizes[type] += methodBytes;
                }
                else
                {
                    classSizes[type] = methodBytes;
                }
            }

            var orderedBySize = classSizes.OrderByDescending(x => x.Value).ToArray();

            foreach (var constant in _strings)
            {
                bytesUsed += constant.Value.Length * sizeof(char);
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

            var classReplacement = GetReplacement(methodBase.DeclaringType);
            if (classReplacement != null && replacement == null)
            {
                replacement = GetReplacement(methodBase, classReplacement);
                return GetOrAddMethodToken(replacement ?? throw new InvalidOperationException($"Internal error: Expected replacement not found for {methodBase.DeclaringType} - {methodBase}"));
            }

            token = _nextToken++;
            _patchedMethodTokens.Add(methodBase, token);
            _inversePatchedMethodTokens.Add(token, methodBase);
            return token;
        }

        public int GetOrAddFieldToken(FieldInfo field)
        {
            string temp = field.Name;
            if (_patchedFieldTokens.TryGetValue(field, out var token))
            {
                return token.Token;
            }

            // If both the original class and the replacement have fields, match them and define the original as the "correct" ones
            // There shouldn't be a problem if only either one contains a field (but check size calculation!)
            if (ArduinoCsCompiler.HasReplacementAttribute(field.DeclaringType!, out var attrib))
            {
                var replacementType = attrib!.TypeToReplace!;
                var replacementField = replacementType.GetField(field.Name, BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Static | BindingFlags.Instance);
                if (replacementField != null)
                {
                    if (_patchedFieldTokens.TryGetValue(replacementField, out token))
                    {
                        return token.Token;
                    }

                    field = replacementField;
                }
            }

            int tk = _nextToken++;
            _patchedFieldTokens.Add(field, (tk, null));
            _inversePatchedFieldTokens.Add(tk, field);
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
            _inversePatchedFieldTokens.Add(tk, field);
            return tk;
        }

        /// <summary>
        /// Creates the new, application-global token for a class. Some bits have special meaning:
        /// 0..23 Type id for ordinary classes (neither generics nor nullables)
        /// 24 True if this is a Nullable{T}
        /// 25..31 Type id for generic classes
        /// Combinations are constructed: if the token 0x32 means "int", 0x0200_0000 means "IEquatable{T}", then 0x0200_0032 is IEquatable{int} and
        /// 0x0280_0032 is IEquatable{Nullable{int}}
        /// </summary>
        /// <param name="typeInfo">Original type to add to list</param>
        /// <returns>A new token for the given type, or the existing token if it is already in the list</returns>
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
            else if (typeInfo == typeof(string) || typeInfo == typeof(MiniString))
            {
                token = (int)KnownTypeTokens.String;
            }
            else if (typeInfo.FullName == "System.RuntimeType")
            {
                token = (int)KnownTypeTokens.RuntimeType;
            }
            else if (typeInfo == typeof(Type) || typeInfo == typeof(MiniType))
            {
                token = (int)KnownTypeTokens.Type;
            }
            else if (typeInfo == typeof(Nullable<>))
            {
                token = NullableToken;
            }
            else if (typeInfo.IsGenericType && typeInfo.GetGenericTypeDefinition() == typeof(Nullable<>))
            {
                // Nullable<T>, but with a defined T
                int baseToken = GetOrAddClassToken(typeInfo.GetGenericArguments()[0].GetTypeInfo());
                token = baseToken + NullableToken;
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
            if (!_inversePatchedTypeTokens.ContainsKey(token))
            {
                _inversePatchedTypeTokens.Add(token, typeInfo);
            }
            else
            {
                // This can only happen for replacement classes that won't be fully replaced. InverseResolveToken shall return the original in this case(?)
                if (typeInfo.GetCustomAttributes((typeof(ArduinoReplacementAttribute)), false).Length == 0)
                {
                    _inversePatchedTypeTokens[token] = typeInfo;
                }
            }

            return token;
        }

        public bool AddClass(Class type)
        {
            if (_classesToSuppress.Contains(type.TheType))
            {
                return false;
            }

            if (_classes.Any(x => x.TheType == type.TheType))
            {
                return false;
            }

            if (_classesReplaced.Any(x => x.Original == type.TheType))
            {
                throw new InvalidOperationException($"Class {type} should have been replaced by its replacement");
            }

            _classes.Add(type);
            return true;
        }

        public bool HasDefinition(Type classType)
        {
            if (_classesToSuppress.Contains(classType))
            {
                return true;
            }

            if (_classes.Any(x => x.TheType == classType))
            {
                return true;
            }

            return false;
        }

        public bool HasMethod(MemberInfo m)
        {
            if (_classesToSuppress.Contains(m.DeclaringType!))
            {
                return true;
            }

            var replacement = GetReplacement((MethodBase)m);
            if (replacement != null)
            {
                m = replacement;
            }

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
            if (_inversePatchedMethodTokens.TryGetValue(token, out var method))
            {
                return method;
            }

            if (_inversePatchedFieldTokens.TryGetValue(token, out var field))
            {
                return field;
            }

            if (_inversePatchedTypeTokens.TryGetValue(token, out var t))
            {
                return t;
            }

            // Try whether the input token is a constructed generic token, which was not expected in this combination
            if (_inversePatchedTypeTokens.TryGetValue((int)(token & 0xFF00_0000), out t))
            {
                try
                {
                    if (t != null && t.IsGenericTypeDefinition)
                    {
                        if (_inversePatchedTypeTokens.TryGetValue((int)(token & 0x00FF_FFFF), out var t2))
                        {
                            return t.MakeGenericType(t2);
                        }
                    }
                }
                catch (ArgumentException)
                {
                    // Ignore, try next approach (if any)
                }
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
            public Class(Type type, int dynamicSize, int staticSize, int newToken, List<VariableOrMethod> members)
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

            public List<VariableOrMethod> Members
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

        public void AddReplacementType(Type? typeToReplace, Type replacement, bool includingSubclasses, bool includingPrivates)
        {
            if (typeToReplace == null)
            {
                throw new ArgumentNullException(nameof(typeToReplace));
            }

            if (!_classesReplaced.Add((typeToReplace, replacement, includingSubclasses)))
            {
                return;
            }

            BindingFlags flags = BindingFlags.Public | BindingFlags.Static | BindingFlags.Instance | BindingFlags.NonPublic;
            List<MethodInfo> methodsNeedingReplacement = typeToReplace.GetMethods(flags).ToList();

            if (!includingPrivates)
            {
                // We can't include internal methods by the filter above, so (unless we need all) remove all privates here, keeping public and internals
                methodsNeedingReplacement = methodsNeedingReplacement.Where(x => !x.IsPrivate).ToList();
            }

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
                    AddReplacementType(original, x.Replacement, true, false);
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

        /// <summary>
        /// Try to find a replacement for the given method in the given class
        /// </summary>
        /// <param name="methodInfo">The method to replace</param>
        /// <param name="classToSearch">With a method in this class</param>
        /// <returns></returns>
        public MethodBase? GetReplacement(MethodBase methodInfo, Type classToSearch)
        {
            foreach (var replacementMethod in classToSearch.GetMethods(BindingFlags.Public | BindingFlags.Static | BindingFlags.Instance | BindingFlags.NonPublic))
            {
                if (ArduinoCsCompiler.MethodsHaveSameSignature(replacementMethod, methodInfo) || ArduinoCsCompiler.AreSameOperatorMethods(replacementMethod, methodInfo))
                {
                    return replacementMethod;
                }

                if (replacementMethod.Name == methodInfo.Name && replacementMethod.GetParameters().Length == methodInfo.GetParameters().Length &&
                    methodInfo.IsConstructedGenericMethod && replacementMethod.IsGenericMethodDefinition &&
                    methodInfo.GetGenericArguments().Length == replacementMethod.GetGenericArguments().Length)
                {
                    // The replacement method is likely the correct one, but we need to instantiate it.
                    var repl = replacementMethod.MakeGenericMethod(methodInfo.GetGenericArguments());
                    if (ArduinoCsCompiler.MethodsHaveSameSignature(repl, methodInfo) || ArduinoCsCompiler.AreSameOperatorMethods(repl, methodInfo))
                    {
                        return repl;
                    }
                }
            }

            return null; // this is now likely an error
        }

        public void AddReplacementMethod(MethodBase? toReplace, MethodBase? replacement)
        {
            if (toReplace == null)
            {
                throw new ArgumentNullException(nameof(toReplace));
            }

            _methodsReplaced.Add((toReplace, replacement));
        }

        public int GetOrAddString(string data)
        {
            var existing = _strings.FirstOrDefault(x => x.Value == data);
            if (existing.Key != 0)
            {
                return existing.Key;
            }

            int token = _nextStringToken + data.Length;
            _nextStringToken += StringTokenStep;
            _strings[token] = data;
            return token;
        }
    }
}
