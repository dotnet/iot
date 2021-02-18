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

        public static ExecutionSet? CompiledKernel = null;

        private readonly ArduinoCsCompiler _compiler;
        private readonly List<ArduinoMethodDeclaration> _methods;
        private readonly List<ClassDeclaration> _classes;
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
        // String data, already UTF-8 encoded
        private readonly List<(int Token, byte[] EncodedString)> _strings;
        private readonly CompilerSettings _compilerSettings;

        private static readonly SnapShot EmptySnapShot = new SnapShot(null, new List<int>());

        private int _numDeclaredMethods;
        private ArduinoTask _entryPoint;
        private int _nextToken;
        private int _nextGenericToken;
        private int _nextStringToken;
        private SnapShot _kernelSnapShot;

        internal ExecutionSet(ArduinoCsCompiler compiler, CompilerSettings compilerSettings)
        {
            _compiler = compiler;
            _methods = new List<ArduinoMethodDeclaration>();
            _classes = new List<ClassDeclaration>();
            _patchedTypeTokens = new Dictionary<TypeInfo, int>();
            _patchedMethodTokens = new Dictionary<MethodBase, int>();
            _patchedFieldTokens = new Dictionary<FieldInfo, (int Token, byte[]? InitializerData)>();
            _inversePatchedMethodTokens = new Dictionary<int, MethodBase>();
            _inversePatchedTypeTokens = new Dictionary<int, TypeInfo>();
            _inversePatchedFieldTokens = new Dictionary<int, FieldInfo>();
            _classesReplaced = new HashSet<(Type Original, Type Replacement, bool Subclasses)>();
            _methodsReplaced = new List<(MethodBase, MethodBase?)>();
            _classesToSuppress = new List<Type>();
            _strings = new();

            _nextToken = (int)KnownTypeTokens.LargestKnownTypeToken + 1;
            _nextGenericToken = GenericTokenStep;
            _nextStringToken = StringTokenStep; // The lower 16 bit are the length

            _numDeclaredMethods = 0;
            _entryPoint = null!;
            _kernelSnapShot = EmptySnapShot;
            _compilerSettings = compilerSettings.Clone();
            MainEntryPointInternal = null;
        }

        internal ExecutionSet(ExecutionSet setToClone, ArduinoCsCompiler compiler, CompilerSettings compilerSettings)
        {
            _compiler = compiler;
            if (setToClone._compilerSettings != compilerSettings)
            {
                throw new NotSupportedException("Target compiler settings must be equal to existing");
            }

            _methods = new List<ArduinoMethodDeclaration>(setToClone._methods);
            _classes = new List<ClassDeclaration>(setToClone._classes);

            _patchedTypeTokens = new Dictionary<TypeInfo, int>(setToClone._patchedTypeTokens);
            _patchedMethodTokens = new Dictionary<MethodBase, int>(setToClone._patchedMethodTokens);
            _patchedFieldTokens = new Dictionary<FieldInfo, (int Token, byte[]? InitializerData)>(setToClone._patchedFieldTokens);
            _inversePatchedMethodTokens = new Dictionary<int, MethodBase>(setToClone._inversePatchedMethodTokens);
            _inversePatchedTypeTokens = new Dictionary<int, TypeInfo>(setToClone._inversePatchedTypeTokens);
            _inversePatchedFieldTokens = new Dictionary<int, FieldInfo>(setToClone._inversePatchedFieldTokens);
            _classesReplaced = new HashSet<(Type Original, Type Replacement, bool Subclasses)>(setToClone._classesReplaced);
            _methodsReplaced = new List<(MethodBase, MethodBase?)>(setToClone._methodsReplaced);
            _classesToSuppress = new List<Type>(setToClone._classesToSuppress);
            _strings = new(setToClone._strings);

            _nextToken = setToClone._nextToken;
            _nextGenericToken = setToClone._nextGenericToken;
            _nextStringToken = setToClone._nextStringToken;

            _numDeclaredMethods = setToClone._numDeclaredMethods;
            _entryPoint = setToClone._entryPoint;
            _kernelSnapShot = setToClone._kernelSnapShot;
            _compilerSettings = compilerSettings.Clone();
            MainEntryPointInternal = setToClone.MainEntryPointInternal;
        }

        internal IList<ClassDeclaration> Classes => _classes;

        public ArduinoTask MainEntryPoint
        {
            get => _entryPoint;
            internal set => _entryPoint = value;
        }

        internal MethodInfo? MainEntryPointInternal
        {
            get;
            set;
        }

        public CompilerSettings CompilerSettings => _compilerSettings;

        public void Load()
        {
            if (CompilerSettings.UseFlash)
            {
                if (!_compiler.BoardHasKernelLoaded(_kernelSnapShot))
                {
                    // Perform a full flash erase (since the above also returns false if a wrong kernel is loaded)
                    _compiler.ClearAllData(true, true);
                    _compiler.SendClassDeclarations(this, EmptySnapShot, _kernelSnapShot, true);
                    _compiler.SendMethods(this, EmptySnapShot, _kernelSnapShot, true);
                    _compiler.CopyToFlash();
                    _compiler.WriteFlashHeader(_kernelSnapShot);
                }
            }
            else
            {
                // If flash is not used, we must make sure it's empty. Otherwise there will be conflicts.
                _compiler.ClearAllData(true, true);
            }

            Load(_kernelSnapShot, CreateSnapShot());
        }

        private void Load(SnapShot from, SnapShot to)
        {
            if (MainEntryPointInternal == null)
            {
                throw new InvalidOperationException("Main entry point not defined");
            }

            // TODO: This should not be necessary later
            _compiler.ClearAllData(true, false);
            _compiler.SetExecutionSetActive(this);
            _compiler.SendClassDeclarations(this, from, to, false);
            _compiler.SendMethods(this, from, to, false);
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
            _compiler.SendConstants(_strings.ToList());

            MainEntryPoint = _compiler.GetTask(this, MainEntryPointInternal);

            // Execute all static ctors
            _compiler.ExecuteStaticCtors(this);
        }

        public ArduinoTask GetTaskForMethod<T>(T mainEntryPoint)
            where T : Delegate
        {
            return _compiler.GetTask(this, mainEntryPoint.Method);
        }

        /// <summary>
        /// Creates a snapshot of the execution set. Used to identify parts that are pre-loaded (or shall be?)
        /// </summary>
        internal SnapShot CreateSnapShot()
        {
            List<int> tokens = new List<int>();
            // Can't use this, because the list may contain replacement tokens for methods we haven't actually picked as part of this snapshot
            // tokens.AddRange(_patchedMethodTokens.Values);
            tokens.AddRange(_methods.Select(x => x.Token));
            // TODO: Uncomment once these can also be stored to flash
            // tokens.AddRange(_patchedFieldTokens.Values.Select(x => x.Token));
            tokens.AddRange(_patchedTypeTokens.Values);
            return new SnapShot(this, tokens);
        }

        internal void CreateKernelSnapShot()
        {
            _compiler.FinalizeExecutionSet(this, true);

            _kernelSnapShot = CreateSnapShot();
        }

        internal void SuppressType(Type t)
        {
            _classesToSuppress.Add(t);
        }

        internal void SuppressType(string name)
        {
            var t = Type.GetType(name, true);
            _classesToSuppress.Add(t!);
        }

        public long EstimateRequiredMemory()
        {
            return EstimateRequiredMemory(out _);
        }

        public long EstimateRequiredMemory(out List<KeyValuePair<Type, ClassStatistics>> details)
        {
            const int MethodBodyMinSize = 40;
            Dictionary<Type, ClassStatistics> classSizes = new Dictionary<Type, ClassStatistics>();
            long bytesUsed = 0;
            foreach (var cls in Classes)
            {
                int classBytes = 40;
                classBytes += cls.StaticSize;
                classBytes += cls.Members.Count * 8; // Assuming 32 bit target system for now
                foreach (var field in cls.Members)
                {
                    if (_inversePatchedFieldTokens.TryGetValue(field.Token, out FieldInfo? value))
                    {
                        if (_patchedFieldTokens.TryGetValue(value, out var data))
                        {
                            classBytes += data.InitializerData?.Length ?? 0;
                        }
                    }
                }

                bytesUsed += classBytes;
                classSizes[cls.TheType] = new ClassStatistics(cls, classBytes);
            }

            foreach (var method in _methods)
            {
                int methodBytes = MethodBodyMinSize;
                methodBytes += MethodBodyMinSize;
                methodBytes += method.ArgumentCount * 4;
                methodBytes += method.MaxLocals * 4;

                methodBytes += method.IlBytes != null ? method.IlBytes.Length : 0;

                var type = method.MethodBase.DeclaringType!;
                if (classSizes.TryGetValue(type, out _))
                {
                    classSizes[type].MethodBytes += methodBytes;
                    classSizes[type].Methods.Add((method, methodBytes));
                }
                else
                {
                    classSizes[type] = new ClassStatistics(new ClassDeclaration(type, 0, 0, 0, new List<ClassMember>(), new List<Type>()), 0);
                    classSizes[type].MethodBytes += methodBytes;
                    classSizes[type].Methods.Add((method, methodBytes));
                }

                bytesUsed += methodBytes;
            }

            foreach (var stat in classSizes.Values)
            {
                stat.TotalBytes = stat.ClassBytes + stat.MethodBytes;
            }

            details = classSizes.OrderByDescending(x => x.Value.TotalBytes).ToList();

            foreach (var constant in _strings)
            {
                bytesUsed += constant.EncodedString.Length + 4;
            }

            return bytesUsed;
        }

        public sealed class ClassStatistics
        {
            public ClassStatistics(ClassDeclaration type, int classBytes)
            {
                Type = type;
                ClassBytes = classBytes;
                MethodBytes = 0;
                TotalBytes = 0;
                Methods = new List<(ArduinoMethodDeclaration, int)>();
            }

            public ClassDeclaration Type
            {
                get;
            }

            public int ClassBytes
            {
                get;
            }

            public int MethodBytes
            {
                get;
                set;
            }

            public int TotalBytes
            {
                get;
                set;
            }

            public List<(ArduinoMethodDeclaration Method, int Size)> Methods
            {
                get;
            }

            public override string ToString()
            {
                return $"Class {Type.Name} uses {MethodBytes} for code and {ClassBytes} for fields and metadata. Total {MethodBytes}.";
            }
        }

        internal int GetOrAddMethodToken(MethodBase methodBase)
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

        internal int GetOrAddFieldToken(FieldInfo field)
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

        internal int GetOrAddFieldToken(FieldInfo field, byte[] initializerData)
        {
            if (_patchedFieldTokens.TryGetValue(field, out var token))
            {
                // Even though the token can already exist, we need to add the data (this happens typically only once, but the field may have several uses)
                var newEntry = (token.Token, initializerData);
                _patchedFieldTokens[field] = newEntry;
                return newEntry.Token;
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
        internal int GetOrAddClassToken(TypeInfo typeInfo)
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
            else if (typeInfo == typeof(System.Delegate))
            {
                token = (int)KnownTypeTokens.Delegate;
            }
            else if (typeInfo == typeof(System.MulticastDelegate))
            {
                token = (int)KnownTypeTokens.MulticastDelegate;
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
            else if (typeInfo == typeof(Array))
            {
                token = (int)KnownTypeTokens.Array;
            }
            else if (typeInfo.FullName != null &&
                     typeInfo.FullName.StartsWith("System.ByReference`1[[System.Byte, System.Private.CoreLib,", StringComparison.Ordinal)) // Ignore version of library
            {
                token = (int)KnownTypeTokens.ByReferenceByte;
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

        internal bool AddClass(ClassDeclaration type)
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

        internal bool HasDefinition(Type classType)
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

        internal bool HasMethod(MemberInfo m)
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

            return _methods.Any(x => AreMethodsIdentical(x.MethodBase, (MethodBase)m));
        }

        private bool AreMethodsIdentical(MethodBase a, MethodBase b)
        {
            if (ReferenceEquals(a, b))
            {
                return true;
            }

            if (!ArduinoCsCompiler.MethodsHaveSameSignature(a, b))
            {
                return false;
            }

            if (a.IsGenericMethod && b.IsGenericMethod)
            {
                var typeParamsa = a.GetGenericArguments();
                var typeParamsb = b.GetGenericArguments();
                if (typeParamsa.Length != typeParamsb.Length)
                {
                    return false;
                }

                for (int i = 0; i < typeParamsa.Length; i++)
                {
                    if (typeParamsa[i] != typeParamsb[i])
                    {
                        return false;
                    }
                }
            }

            if (a.DeclaringType!.IsConstructedGenericType && b.DeclaringType!.IsConstructedGenericType)
            {
                var typeParamsa = a.DeclaringType.GetGenericArguments();
                var typeParamsb = b.DeclaringType.GetGenericArguments();
                if (typeParamsa.Length != typeParamsb.Length)
                {
                    return false;
                }

                for (int i = 0; i < typeParamsa.Length; i++)
                {
                    if (typeParamsa[i] != typeParamsb[i])
                    {
                        return false;
                    }
                }
            }

            if (a.MetadataToken == b.MetadataToken && a.Module.FullyQualifiedName == b.Module.FullyQualifiedName)
            {
                return true;
            }

            return false;
        }

        internal bool AddMethod(ArduinoMethodDeclaration method)
        {
            if (_numDeclaredMethods >= Math.Pow(2, 14) - 1)
            {
                // In practice, the maximum will be much less on most Arduino boards, due to ram limits
                throw new NotSupportedException("To many methods declared. Only 2^14 supported.");
            }

            // These conditions allow some memory optimization in the runtime. It's very rare that methods exceed these limitations.
            if (method.MaxLocals > 255)
            {
                throw new NotSupportedException("Methods with more than 255 local variables are unsupported");
            }

            if (method.MaxStack > 255)
            {
                throw new NotSupportedException("The maximum execution stack size is 255");
            }

            if (_methods.Any(x => AreMethodsIdentical(x.MethodBase, method.MethodBase)))
            {
                return false;
            }

            if (_methodsReplaced.Any(x => AreMethodsIdentical(x.Item1, method.MethodBase)))
            {
                throw new InvalidOperationException($"Method {method} should have been replaced by its replacement");
            }

            if (_methodsReplaced.Any(x => AreMethodsIdentical(x.Item1, method.MethodBase) && x.Item2 == null))
            {
                throw new InvalidOperationException($"The method {method} should be replaced, but has no new implementation. This program will not execute");
            }

            _methods.Add(method);
            method.Index = _numDeclaredMethods;
            _numDeclaredMethods++;

            return true;
        }

        internal IList<ArduinoMethodDeclaration> Methods()
        {
            return _methods;
        }

        internal MemberInfo? InverseResolveToken(int token)
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

        internal void AddReplacementType(Type? typeToReplace, Type replacement, bool includingSubclasses, bool includingPrivates)
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

            if (!includingSubclasses)
            {
                flags |= BindingFlags.DeclaredOnly;
            }

            List<MethodInfo> methodsNeedingReplacement = typeToReplace.GetMethods(flags).ToList();

            if (!includingPrivates)
            {
                // We can't include internal methods by the filter above, so (unless we need all) remove all privates here, keeping public and internals
                methodsNeedingReplacement = methodsNeedingReplacement.Where(x => !x.IsPrivate).ToList();
            }

            foreach (var methoda in replacement.GetMethods(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Static | BindingFlags.Instance))
            {
                // Above, we only check the public methods, here we also look at the private ones
                BindingFlags otherFlags = BindingFlags.Public | BindingFlags.Static | BindingFlags.Instance | BindingFlags.NonPublic;
                if (!includingSubclasses)
                {
                    otherFlags |= BindingFlags.DeclaredOnly;
                }

                foreach (var methodb in typeToReplace.GetMethods(otherFlags))
                {
                    if (!methodsNeedingReplacement.Contains(methodb))
                    {
                        // This is not the one in need of replacement (or already has one, if the parameters matched a similar implementation as well)
                        continue;
                    }

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

        internal Type? GetReplacement(Type? original)
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

        internal MethodBase? GetReplacement(MethodBase original)
        {
            // Odd: I'm pretty sure that previously equality on MethodBase instances worked, but for some reason not all instances pointing to the same method are Equal().
            var elem = _methodsReplaced.FirstOrDefault(x => AreMethodsIdentical(x.Item1, original));
            ////var elemTest = _methodsReplaced.FirstOrDefault(x => ReferenceEquals(x.Item1, original));
            ////if (!ReferenceEquals(elem.Item1, elemTest.Item1))
            ////{
            ////    throw new NotSupportedException();
            ////}

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
        internal MethodBase? GetReplacement(MethodBase methodInfo, Type classToSearch)
        {
            foreach (var replacementMethod in classToSearch.GetMethods(BindingFlags.Public | BindingFlags.Static | BindingFlags.Instance | BindingFlags.NonPublic))
            {
                if (ArduinoCsCompiler.MethodsHaveSameSignature(replacementMethod, methodInfo) || ArduinoCsCompiler.AreSameOperatorMethods(replacementMethod, methodInfo))
                {
                    if (!replacementMethod.IsGenericMethodDefinition)
                    {
                        return replacementMethod;
                    }
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

        internal void AddReplacementMethod(MethodBase? toReplace, MethodBase? replacement)
        {
            if (toReplace == null)
            {
                throw new ArgumentNullException(nameof(toReplace));
            }

            if (replacement != null && AreMethodsIdentical(toReplace, replacement))
            {
                // Replacing a method with itself may happen if virtual resolution points back to the same base class. Should fix itself later.
                return;
            }

            string name = toReplace.Name;
            _methodsReplaced.Add((toReplace, replacement));
        }

        internal int GetOrAddString(string data)
        {
            var encoded = Encoding.UTF8.GetBytes(data);
            var existing = _strings.FirstOrDefault(x => x.EncodedString.SequenceEqual(encoded));
            if (existing.Token != 0)
            {
                return existing.Token;
            }

            int token = _nextStringToken + encoded.Length;
            _nextStringToken += StringTokenStep;
            _strings.Add((token, encoded));
            return token;
        }

        internal ArduinoMethodDeclaration GetMethod(MethodBase methodInfo)
        {
            return _methods.First(x => AreMethodsIdentical(x.MethodBase, methodInfo));
        }

        private static int Xor(IEnumerable<int> inputs)
        {
            int result = 0;
            foreach (int i in inputs)
            {
                result ^= i;
            }

            return result;
        }

        public sealed class SnapShot
        {
            private readonly ExecutionSet? _set;

            public SnapShot(ExecutionSet? set, List<int> alreadyAssignedTokens)
            {
                AlreadyAssignedTokens = alreadyAssignedTokens;
                _set = set;
            }

            public List<int> AlreadyAssignedTokens
            {
                get;
            }

            public override int GetHashCode()
            {
                int ret = AlreadyAssignedTokens.Count;
                ret ^= Xor(AlreadyAssignedTokens);
                if (AlreadyAssignedTokens.Count > 0 && _set != null)
                {
                    // Add the original token from the last entry in the list (could also add all of them)
                    var element = _set.InverseResolveToken(AlreadyAssignedTokens[AlreadyAssignedTokens.Count - 1]);
                    ret ^= element!.MetadataToken;
                }

                return ret;
            }
        }
    }
}
