// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Runtime.Versioning;
using System.Text;
using System.Threading;
using ArduinoCsCompiler.Runtime;
using Iot.Device.Common;
using Microsoft.Extensions.Logging;

#pragma warning disable CS1591
namespace ArduinoCsCompiler
{
    public class ExecutionSet
    {
        internal const int GenericTokenStep = 0x0100_0000;
        private const int StringTokenStep = 0x0001_0000;
        private const int NullableToken = 0x0080_0000;
        public const int GenericTokenMask = -8_388_608; // 0xFF80_0000 as signed

        public static ExecutionSet? CompiledKernel = null;

        private static readonly SnapShot EmptySnapShot = new SnapShot(null, new List<int>(), new List<int>(), new List<int>());

        private static readonly Dictionary<Type, KnownTypeTokens> KnownTypeTokensMap;

        private readonly MicroCompiler _compiler;
        private readonly Dictionary<EquatableMethod, ArduinoMethodDeclaration> _methods;
        private readonly List<ClassDeclaration> _classes;
        private readonly Dictionary<TypeInfo, int> _patchedTypeTokens;
        private readonly Dictionary<int, TypeInfo> _inversePatchedTypeTokens;
        private readonly Dictionary<EquatableMethod, int> _patchedMethodTokens;
        private readonly Dictionary<int, EquatableMethod> _inversePatchedMethodTokens; // Same as the above, but the other way round
        private readonly Dictionary<EquatableField, (int Token, byte[]? InitializerData)> _patchedFieldTokens;
        private readonly Dictionary<int, EquatableField> _inversePatchedFieldTokens;
        private readonly HashSet<(Type Original, Type Replacement, bool Subclasses)> _classesReplaced;
        private readonly Dictionary<EquatableMethod, IlCode> _codeCache;

        /// <summary>
        /// For each method name, the list of replacements. The outer dictionary is to speed up lookups
        /// </summary>
        private readonly Dictionary<string, List<(EquatableMethod Original, EquatableMethod? Replacement)>> _methodsReplaced;
        // These classes (and any of their methods) will not be loaded, even if they seem in use. This should speed up testing
        private readonly List<Type> _classesToSuppress;
        // String data, already UTF-8 encoded. The StringData value is actually only used for debugging purposes
        private readonly List<(int Token, byte[] EncodedString, string StringData)> _strings;
        private readonly CompilerSettings _compilerSettings;
        private readonly List<(int Token, TypeInfo? Class)> _specialTypeList;
        private readonly ILogger _logger;

        private int _numDeclaredMethods;
        private ArduinoTask _entryPoint;
        private int _nextToken;
        private int _nextGenericToken;
        private int _nextStringToken;
        private SnapShot _kernelSnapShot;
        private Dictionary<Type, MethodInfo> _arrayListImpl;
        private List<(int, ClassMember)> _staticFieldsUsed;

        static ExecutionSet()
        {
            KnownTypeTokensMap = new Dictionary<Type, KnownTypeTokens>();
            KnownTypeTokensMap.Add(typeof(Object), KnownTypeTokens.Object);
            KnownTypeTokensMap.Add(typeof(UInt32), KnownTypeTokens.Uint32);
            KnownTypeTokensMap.Add(typeof(Int32), KnownTypeTokens.Int32);
            KnownTypeTokensMap.Add(typeof(UInt64), KnownTypeTokens.Uint64);
            KnownTypeTokensMap.Add(typeof(Int64), KnownTypeTokens.Int64);
            KnownTypeTokensMap.Add(typeof(byte), KnownTypeTokens.Byte);
            KnownTypeTokensMap.Add(typeof(System.Delegate), KnownTypeTokens.Delegate);
            KnownTypeTokensMap.Add(typeof(System.MulticastDelegate), KnownTypeTokens.MulticastDelegate);
            KnownTypeTokensMap.Add(typeof(TypeInfo), KnownTypeTokens.TypeInfo);
            KnownTypeTokensMap.Add(typeof(String), KnownTypeTokens.String);
            KnownTypeTokensMap.Add(Type.GetType("System.RuntimeType", true)!, KnownTypeTokens.RuntimeType);
            KnownTypeTokensMap.Add(typeof(Type), KnownTypeTokens.Type);
            KnownTypeTokensMap.Add(typeof(MiniType), KnownTypeTokens.Type); // Same value for different keys is ok
            KnownTypeTokensMap.Add(typeof(Array), KnownTypeTokens.Array);
            KnownTypeTokensMap.Add(typeof(MiniArray), KnownTypeTokens.Array);
            KnownTypeTokensMap.Add(typeof(Exception), KnownTypeTokens.Exception);
            KnownTypeTokensMap.Add(typeof(Thread), KnownTypeTokens.Thread);
            KnownTypeTokensMap.Add(typeof(ArithmeticException), KnownTypeTokens.ArithmeticException);
            KnownTypeTokensMap.Add(typeof(DivideByZeroException), KnownTypeTokens.DivideByZeroException);
            KnownTypeTokensMap.Add(typeof(NullReferenceException), KnownTypeTokens.NullReferenceException);
            KnownTypeTokensMap.Add(typeof(MissingMethodException), KnownTypeTokens.MissingMethodException);
            KnownTypeTokensMap.Add(typeof(IndexOutOfRangeException), KnownTypeTokens.IndexOutOfRangeException);
            KnownTypeTokensMap.Add(typeof(ArrayTypeMismatchException), KnownTypeTokens.ArrayTypeMismatchException);
            KnownTypeTokensMap.Add(typeof(InvalidOperationException), KnownTypeTokens.InvalidOperationException);
            KnownTypeTokensMap.Add(typeof(TypeInitializationException), KnownTypeTokens.ClassNotFoundException);
            KnownTypeTokensMap.Add(typeof(InvalidCastException), KnownTypeTokens.InvalidCastException);
            KnownTypeTokensMap.Add(typeof(NotSupportedException), KnownTypeTokens.NotSupportedException);
            KnownTypeTokensMap.Add(typeof(FieldAccessException), KnownTypeTokens.FieldAccessException);
            KnownTypeTokensMap.Add(typeof(OverflowException), KnownTypeTokens.OverflowException);
            KnownTypeTokensMap.Add(typeof(IOException), KnownTypeTokens.IoException);
        }

        internal ExecutionSet(MicroCompiler compiler, CompilerSettings compilerSettings)
        {
            _compiler = compiler;
            _logger = this.GetCurrentClassLogger();
            _methods = new();
            _classes = new List<ClassDeclaration>();
            _patchedTypeTokens = new Dictionary<TypeInfo, int>();
            _patchedMethodTokens = new Dictionary<EquatableMethod, int>();
            _patchedFieldTokens = new Dictionary<EquatableField, (int Token, byte[]? InitializerData)>();
            _inversePatchedMethodTokens = new Dictionary<int, EquatableMethod>();
            _inversePatchedTypeTokens = new Dictionary<int, TypeInfo>();
            _inversePatchedFieldTokens = new Dictionary<int, EquatableField>();
            _classesReplaced = new HashSet<(Type Original, Type Replacement, bool Subclasses)>();
            _methodsReplaced = new();
            _classesToSuppress = new List<Type>();
            _arrayListImpl = new();
            _strings = new();
            _specialTypeList = new();
            _codeCache = new();

            _nextToken = (int)KnownTypeTokens.LargestKnownTypeToken + 1;
            _nextGenericToken = GenericTokenStep * 4; // The first entries are reserved (see KnownTypeTokens)
            _nextStringToken = StringTokenStep; // The lower 16 bit are the length
            _staticFieldsUsed = new List<(int, ClassMember)>();

            _numDeclaredMethods = 0;
            _entryPoint = null!;
            _kernelSnapShot = EmptySnapShot;
            _compilerSettings = compilerSettings.Clone();
            MainEntryPointMethod = null;
            TokenOfStartupMethod = 0;
        }

        internal ExecutionSet(ExecutionSet setToClone, MicroCompiler compiler, CompilerSettings compilerSettings)
        {
            _compiler = compiler;
            _logger = this.GetCurrentClassLogger();
            if (setToClone._compilerSettings != compilerSettings)
            {
                throw new NotSupportedException("Target compiler settings must be equal to existing");
            }

            _methods = new(setToClone._methods);
            _classes = new List<ClassDeclaration>(setToClone._classes);

            _patchedTypeTokens = new Dictionary<TypeInfo, int>(setToClone._patchedTypeTokens);
            _patchedMethodTokens = new Dictionary<EquatableMethod, int>(setToClone._patchedMethodTokens);
            _patchedFieldTokens = new Dictionary<EquatableField, (int Token, byte[]? InitializerData)>(setToClone._patchedFieldTokens);
            _inversePatchedMethodTokens = new Dictionary<int, EquatableMethod>(setToClone._inversePatchedMethodTokens);
            _inversePatchedTypeTokens = new Dictionary<int, TypeInfo>(setToClone._inversePatchedTypeTokens);
            _inversePatchedFieldTokens = new Dictionary<int, EquatableField>(setToClone._inversePatchedFieldTokens);
            _classesReplaced = new HashSet<(Type Original, Type Replacement, bool Subclasses)>(setToClone._classesReplaced);
            _methodsReplaced = new(setToClone._methodsReplaced);
            _classesToSuppress = new List<Type>(setToClone._classesToSuppress);
            _strings = new(setToClone._strings);
            _arrayListImpl = new Dictionary<Type, MethodInfo>(setToClone._arrayListImpl);
            _specialTypeList = new(setToClone._specialTypeList);
            _codeCache = new(setToClone._codeCache);

            _nextToken = setToClone._nextToken;
            _nextGenericToken = setToClone._nextGenericToken;
            _nextStringToken = setToClone._nextStringToken;

            _numDeclaredMethods = setToClone._numDeclaredMethods;
            _entryPoint = setToClone._entryPoint;
            _kernelSnapShot = setToClone._kernelSnapShot;
            _compilerSettings = compilerSettings.Clone();
            _staticFieldsUsed = new List<(int, ClassMember)>(setToClone._staticFieldsUsed);
            if (setToClone.FirmwareStartupSequence != null)
            {
                FirmwareStartupSequence = new List<IlCode>(setToClone.FirmwareStartupSequence);
            }

            MainEntryPointMethod = setToClone.MainEntryPointMethod;
            TokenOfStartupMethod = setToClone.TokenOfStartupMethod;
        }

        internal IList<ClassDeclaration> Classes => _classes;

        public ArduinoTask MainEntryPoint
        {
            get => _entryPoint;
            internal set => _entryPoint = value;
        }

        public MethodInfo? MainEntryPointMethod
        {
            get;
            set;
        }

        public long? ExpectedMemoryUsage
        {
            get;
            set;
        }

        public long? StaticMemberSize { get; set; }

        public List<KeyValuePair<Type, ClassStatistics>>? Statistics
        {
            get;
            set;
        }

        internal Dictionary<Type, MethodInfo> ArrayListImplementation
        {
            get
            {
                return _arrayListImpl;
            }
        }

        public CompilerSettings CompilerSettings => _compilerSettings;

        /// <summary>
        /// The list of methods that need to be called to start the code (static constructors and the main method)
        /// The sequence is combined into a startup method if the program is to run directly from flash, otherwise <see cref="MicroCompiler.ExecuteStaticCtors"/> takes care of the
        /// sequencing.
        /// </summary>
        internal List<IlCode>? FirmwareStartupSequence { get; set; }

        public int TokenOfStartupMethod { get; set; }

        private static int CalculateTotalStringSize(List<(int Token, byte[] EncodedString, string StringData)> strings, SnapShot fromSnapShot, SnapShot toSnapShot)
        {
            int totalSize = sizeof(int); // we need a trailing empty entry
            var list = strings.Where(x => !fromSnapShot.AlreadyAssignedStringTokens.Contains(x.Token) && toSnapShot.AlreadyAssignedStringTokens.Contains(x.Token));
            foreach (var elem in list)
            {
                totalSize += sizeof(int);
                totalSize += elem.EncodedString.Length;
            }

            return totalSize;
        }

        public void Load(bool runStaticCtors)
        {
            if (CompilerSettings.ForceFlashWrite)
            {
                _compiler.ClearAllData(true, true);
            }

            if (CompilerSettings.CreateKernelForFlashing)
            {
                if (!_compiler.BoardHasKernelLoaded(_kernelSnapShot))
                {
                    // Must have been calculated
                    EstimateRequiredMemory();
                    Progress<double> dummy = new Progress<double>(); // Not used here, since probably no console available
                    // Perform a full flash erase (since the above also returns false if a wrong kernel is loaded)
                    _compiler.ClearAllData(true, true);
                    _compiler.SendClassDeclarations(dummy, this, EmptySnapShot, _kernelSnapShot, true);
                    _compiler.SendMethods(dummy, this, EmptySnapShot, _kernelSnapShot, true);
                    List<(int Token, byte[] Data, string NoData)> converted = new();
                    // Need to do this manually, due to stupid nullability conversion restrictions
                    foreach (var elem in _patchedFieldTokens.Values)
                    {
                        if (elem.InitializerData != null)
                        {
                            converted.Add((elem.Token, elem.InitializerData, string.Empty));
                        }
                    }

                    _compiler.SendConstants(dummy, converted, EmptySnapShot, _kernelSnapShot, true);
                    _compiler.CopyToFlash();

                    int totalStringSize = CalculateTotalStringSize(_strings, EmptySnapShot, _kernelSnapShot);
                    _compiler.PrepareStringLoad(0, totalStringSize); // The first argument is currently unused
                    _compiler.SendStrings(dummy, _strings.ToList(), EmptySnapShot, _kernelSnapShot, true);
                    _compiler.SendSpecialTypeList(dummy, _specialTypeList.Select(x => x.Token).ToList(), EmptySnapShot, _kernelSnapShot, true);
                    _compiler.SendGlobalMetadata((UInt32)StaticMemberSize.GetValueOrDefault(0));
                    _compiler.CopyToFlash();

                    // The kernel contains no startup method, therefore don't use one
                    _compiler.WriteFlashHeader(_kernelSnapShot, 0, CodeStartupFlags.None);
                }
            }
            else if (!CompilerSettings.UseFlashForProgram && !CompilerSettings.UseFlashForKernel)
            {
                // If flash is not used, we must make sure it's empty. Otherwise there will be conflicts.
                _compiler.ClearAllData(true, true);
            }

            Load(_kernelSnapShot, CreateSnapShot(), runStaticCtors);
        }

        private void Load(SnapShot from, SnapShot to, bool runStaticCtos)
        {
            if (MainEntryPointMethod == null)
            {
                throw new InvalidOperationException("Main entry point not defined");
            }

            EstimateRequiredMemory();
            SynchronousProgress<double> progress = new SynchronousProgress<double>(1);
            double previous = 0;

            if (ErrorManager.ShowProgress)
            {
                progress.ProgressChanged += (sender, d) =>
                {
                    if (previous >= 1)
                    {
                        // We already printed "100%"
                        return;
                    }

                    if (d > previous + 0.01 || d >= 1)
                    {
                        Console.Write($"\r{d * 100:F0}%...");
                        if (d >= 1)
                        {
                            Console.WriteLine();
                        }

                        previous = d;
                    }
                };
            }

            bool doWriteProgramToFlash = CompilerSettings.DoCopyToFlash(false);

            if (!_compiler.BoardHasKernelLoaded(to))
            {
                if (from == EmptySnapShot || doWriteProgramToFlash)
                {
                    _compiler.ClearAllData(true, true);
                }
                else
                {
                    _compiler.ClearAllData(true, false);
                }

                _compiler.SetExecutionSetActive(this);
                _logger.LogInformation("1/5 Uploading class declarations...");
                _compiler.SendClassDeclarations(progress, this, from, to, false);
                progress.Done();
                previous = 0;

                _logger.LogInformation("2/5 Uploading methods..");
                _compiler.SendMethods(progress, this, from, to, false);
                progress.Done();
                previous = 0;
                List<(int Token, byte[] Data, string NoData)> converted = new();
                // Need to do this manually, due to stupid nullability conversion restrictions
                foreach (var elem in _patchedFieldTokens.Values)
                {
                    if (elem.InitializerData != null)
                    {
                        converted.Add((elem.Token, elem.InitializerData, string.Empty));
                    }
                }

                _logger.LogInformation("3/5 Uploading constants...");
                _compiler.SendConstants(progress, converted, from, to, false);
                if (doWriteProgramToFlash)
                {
                    _compiler.CopyToFlash();
                }

                progress.Done();
                previous = 0;
                int totalStringSize = CalculateTotalStringSize(_strings, from, to);
                _compiler.PrepareStringLoad(0, totalStringSize); // The first argument is currently unused
                _logger.LogInformation("4/5 Uploading strings...");
                _compiler.SendStrings(progress, _strings.ToList(), from, to, false);
                progress.Done();
                previous = 0;
                _logger.LogInformation("5/5 Uploading special types...");
                _compiler.SendSpecialTypeList(progress, _specialTypeList.Select(x => x.Token).ToList(), from, to, false);
                _compiler.SendGlobalMetadata((UInt32)StaticMemberSize.GetValueOrDefault(0));
                _logger.LogInformation("Finalizing...");
                if (doWriteProgramToFlash)
                {
                    _compiler.WriteFlashHeader(to, TokenOfStartupMethod, CompilerSettings.AutoRestartProgram ? CodeStartupFlags.AutoRestartAfterCrash : CodeStartupFlags.None);
                }

                progress.Done();
                previous = 0;

                _logger.LogInformation("Upload successfully completed");
            }
            else
            {
                // We need to activate this execution set even if we don't need to load anything
                _compiler.SetExecutionSetActive(this);
            }

            MainEntryPoint = _compiler.GetTask(this, MainEntryPointMethod);

            if (runStaticCtos)
            {
                // Execute all static ctors
                _compiler.ExecuteStaticCtors(this);
            }
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
            List<int> stringTokens = new List<int>();
            // Can't use this, because the list may contain replacement tokens for methods we haven't actually picked as part of this snapshot
            // tokens.AddRange(_patchedMethodTokens.Values);
            tokens.AddRange(_methods.Values.Select(x => x.Token));
            tokens.AddRange(_patchedFieldTokens.Values.Where(x => x.InitializerData != null).Select(x => x.Token));
            tokens.AddRange(_patchedTypeTokens.Values);
            stringTokens.AddRange(_strings.Select(x => x.Token));

            return new SnapShot(this, tokens, stringTokens, _specialTypeList.Select(x => x.Token).ToList());
        }

        internal void CreateKernelSnapShot()
        {
            _compiler.FinalizeExecutionSet(this, CompilerSettings, new AnalysisStack(), true);

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
            if (ExpectedMemoryUsage.HasValue)
            {
                return ExpectedMemoryUsage.Value;
            }

            return EstimateRequiredMemory(out _);
        }

        public long EstimateRequiredMemory(out List<KeyValuePair<Type, ClassStatistics>> details)
        {
            if (ExpectedMemoryUsage.HasValue && Statistics != null)
            {
                details = Statistics;
                return ExpectedMemoryUsage.Value;
            }

            const int MethodBodyMinSize = 40;
            const int StaticsPerFieldMinSize = 8; // 4 bytes for field token, 1 byte variable type, 1 byte marker, 2 bytes length

            Dictionary<Type, ClassStatistics> classSizes = new Dictionary<Type, ClassStatistics>();
            List<byte[]> methodBodies = new List<byte[]>();
            long bytesUsed = 0;
            long staticSize = 0;
            List<(int, ClassMember)> tempFields = new();
            _staticFieldsUsed = new List<(int, ClassMember)>();
            foreach (var cls in Classes.OrderBy(x => (uint)x.NewToken))
            {
                int classBytes = 40;
                classBytes += cls.StaticSize;

                classBytes += cls.Members.Count * 8; // Assuming 32 bit target system for now
                foreach (var field in cls.Members)
                {
                    if (_inversePatchedFieldTokens.TryGetValue(field.Token, out EquatableField? value))
                    {
                        if (_patchedFieldTokens.TryGetValue(value, out var data))
                        {
                            classBytes += data.InitializerData?.Length ?? 0;
                        }
                    }

                    if (field.Field == null || field.Field.DeclaringType!.IsEnum)
                    {
                        // A constant field or an enum member. Don't count these.
                        continue;
                    }

                    if (field.SizeOfField > 0
                        && (field.VariableType & VariableKind.StaticMember) == VariableKind.StaticMember)
                    {
                        // We need room for the initializers, but only for the small ones
                        if (field.Name.Contains(MicroCompiler.PrivateImplementationDetailsName, StringComparison.Ordinal) && field.StaticFieldSize > 8)
                        {
                            continue;
                        }

                        staticSize += StaticsPerFieldMinSize;
                        int fieldSizeToUse = 4;
                        // They all still have the static bit set
                        if (field.VariableType == (VariableKind.Double | VariableKind.StaticMember) ||
                            field.VariableType == (VariableKind.Int64 | VariableKind.StaticMember) ||
                            field.VariableType == (VariableKind.Uint64 | VariableKind.StaticMember))
                        {
                            fieldSizeToUse = 8;
                        }
                        else if (field.VariableType == (VariableKind.LargeValueType | VariableKind.StaticMember))
                        {
                            fieldSizeToUse = Math.Max(4, field.StaticFieldSize);
                        }

                        staticSize += fieldSizeToUse;
                        _staticFieldsUsed.Add((fieldSizeToUse + StaticsPerFieldMinSize, field));
                    }
                }

                bytesUsed += classBytes;
                classSizes[cls.TheType] = new ClassStatistics(cls, classBytes);
            }

            foreach (var method in _methods.Values)
            {
                int methodBytes = MethodBodyMinSize;
                methodBytes += method.ArgumentCount * 4;
                methodBytes += method.MaxLocals * 4;

                if (method.Code.IlBytes != null)
                {
                    methodBytes += method.Code.IlBytes.Length;
                    methodBodies.Add(method.Code.IlBytes);
                }

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

            ////long totalBodyLength = methodBodies.Sum(x => x.Length);
            ////var compressedBodies = methodBodies.Distinct(new ByteArrayEqualityComparer());
            ////long compressedBodyLength = compressedBodies.Sum(x => x.Length);

            ExpectedMemoryUsage = bytesUsed;
            Statistics = details;
            StaticMemberSize = staticSize;
            return bytesUsed;
        }

        public class ByteArrayEqualityComparer : IEqualityComparer<byte[]>
        {
            public bool Equals(byte[]? x, byte[]? y)
            {
                if (ReferenceEquals(x, y))
                {
                    return true;
                }

                if (ReferenceEquals(x, null))
                {
                    return false;
                }

                if (ReferenceEquals(y, null))
                {
                    return false;
                }

                if (x.Length != y.Length)
                {
                    return false;
                }

                for (int i = 0; i < x.Length; i++)
                {
                    if (x[i] != y[i])
                    {
                        return false;
                    }
                }

                return true;
            }

            public int GetHashCode(byte[] obj)
            {
                int ret = 0;
                for (int i = 0; i < Math.Min(obj.Length, 6); i++)
                {
                    ret = obj[i] | ret << 6;
                }

                return ret;
            }
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

            internal List<(ArduinoMethodDeclaration Method, int Size)> Methods
            {
                get;
            }

            public override string ToString()
            {
                return $"Class {Type.Name} uses {MethodBytes} for code and {ClassBytes} for fields and metadata. Total {MethodBytes}.";
            }
        }

        internal int GetOrAddMethodToken(EquatableMethod methodBase, AnalysisStack analysisStack)
        {
            int token;
            if (_patchedMethodTokens.TryGetValue(methodBase, out token))
            {
                return token;
            }

            var replacement = GetReplacement(methodBase, analysisStack);
            if (replacement != null)
            {
                return GetOrAddMethodToken(replacement, analysisStack);
            }

            // If the class is being replaced, search the replacement class
            var classReplacement = GetReplacement(methodBase.DeclaringType);
            if (classReplacement != null && replacement == null)
            {
                replacement = GetReplacement(methodBase, analysisStack, classReplacement);
                if (replacement == null)
                {
                    // If the replacement class has a static method named "NotSupportedException", we call this instead (expecting that this will never be called).
                    // This is used so we can remove all the unsupported implementations for compiler intrinsics.
                    MethodInfo? dummyMethod = GetNotSupportedExceptionMethod(classReplacement);
                    if (dummyMethod != null)
                    {
                        return GetOrAddMethodToken(dummyMethod, analysisStack);
                    }

                    throw new InvalidOperationException($"Internal error: Expected replacement not found for {methodBase.MemberInfoSignature()}. CallStack {analysisStack}");
                }

                return GetOrAddMethodToken(replacement, analysisStack);
            }

            if (methodBase.DeclaringType == typeof(Thread) && methodBase.Name == "StartCallback")
            {
                // We need to be able to recognize this method in the backend
                token = (int)KnownTypeTokens.ThreadStartCallback;
            }
            else if (methodBase.DeclaringType != null && methodBase.DeclaringType.FullName == "System.Threading.TimerQueue" && methodBase.Name == "AppDomainTimerCallback")
            {
                token = (int)KnownTypeTokens.AppDomainTimerCallback;
            }
            else
            {
                token = _nextToken++;
            }

            _patchedMethodTokens.Add(methodBase, token);
            _inversePatchedMethodTokens.Add(token, methodBase);
            return token;
        }

        /// <summary>
        /// Returns non-null when the whole method should be removed, because it's assumed that it's never going to be called
        /// </summary>
        private static MethodInfo? GetNotSupportedExceptionMethod(Type classReplacement)
        {
            var dummyMethod = classReplacement.GetMethod(nameof(MiniX86Intrinsics.NotSupportedException), BindingFlags.Public | BindingFlags.Static);
            return dummyMethod;
        }

        internal int GetOrAddFieldToken(EquatableField field)
        {
            string temp = field.Name;
            if (_patchedFieldTokens.TryGetValue(field, out var token))
            {
                return token.Token;
            }

            // If both the original class and the replacement have fields, match them and define the original as the "correct" ones
            // There shouldn't be a problem if only either one contains a field (but check size calculation!)
            if (MicroCompiler.HasReplacementAttribute(field.DeclaringType!, out var attrib))
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
        /// 0x0280_0032 is Nullable{IEquatable{int}} (not IEquatable{Nullable{int}}!) Since Nullable{T} only works with value types, this isn't normally used.
        /// There's a special list of very complex tokens that is used for complex combinations that can't be mapped with the above bit combinations, namely
        /// objects with multiple template arguments such as List{List{T}} or Dictionary{TKey, TValue}
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

            if (KnownTypeTokensMap.TryGetValue(typeInfo, out var knownToken))
            {
                token = (int)knownToken;
            }
            else if (typeInfo.FullName == "System.Enum")
            {
                // Note that enums are value types, but "System.Enum" itself is not.
                token = (int)KnownTypeTokens.Enum;
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
            else if (typeInfo == typeof(IEnumerable<>))
            {
                token = (int)KnownTypeTokens.IEnumerableOfT;
            }
            else if (typeInfo == typeof(Span<>))
            {
                token = (int)KnownTypeTokens.SpanOfT;
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
                if (firstArg.IsGenericType || typeArguments.Length > 1)
                {
                    // If the first argument is itself generic or there is more than one generic argument, we need to create extended metadata
                    // The list consists of a length element, then the master token (which we create here) and then the tokens of the type arguments
                    List<(int Token, TypeInfo? Element)> entries = new List<(int Token, TypeInfo? Element)>(); // Element is only for debugging purposes

                    // one entry for the length, one entry for the combined token, one for the left part.
                    // We can't use the FF's as marker, because the list itself might contain them when combining very complex tokens.
                    entries.Add((typeArguments.Length + 3, null));
                    token = (int)(0xFF000000 | _nextToken++); // Create a new token, marked "special" (top 8 bits set).
                    // Note: While in theory, this element could again be wrapped in a Nullable<>, this is probably really rare, as generic types are almost never structs, therefore
                    // a token such as 0x02800079 is rather IList<Nullable<int>> rather than Nullable<IList<int>>, but getting that right everywhere is difficult
                    entries.Add((token, typeInfo)); // own type
                    entries.Add((definitionToken, definition.GetTypeInfo())); // The generic type
                    foreach (var t in typeArguments)
                    {
                        var info = t.GetTypeInfo();
                        int token2 = GetOrAddClassToken(info);
                        entries.Add((token2, info));
                    }

                    ClearStatistics();
                    _specialTypeList.AddRange(entries);
                }
                else
                {
                    // Our token is the combination of the generic type and the only argument. This allows a simple implementation for Type.MakeGenericType() with
                    // generic types with a single argument
                    token = definitionToken + firstArgToken;
                }
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

            // Unless this compiler setting is enabled, we automatically suppress all preview features (in .NET 6.0 for instance the INumber<T> interfaces)
            if (!_compilerSettings.UsePreviewFeatures && type.TheType.GetCustomAttributes(typeof(RequiresPreviewFeaturesAttribute), true).Any())
            {
                return false;
            }

            if (_classes.Any(x => x.TheType == type.TheType))
            {
                return false;
            }

            if (_classesReplaced.Any(x => x.Original.AssemblyQualifiedName == type.TheType.AssemblyQualifiedName))
            {
                throw new InvalidOperationException($"Class {type} should have been replaced by its replacement");
            }

            ClearStatistics();
            _classes.Add(type);
            _logger.LogDebug($"Class {type.TheType.MemberInfoSignature(true)} added to the execution set with token 0x{type.NewToken:X}");
            PrintProgress();
            return true;
        }

        internal bool IsSuppressed(Type t)
        {
            return _classesToSuppress.Contains(t);
        }

        internal bool HasDefinition(Type classType)
        {
            if (_classesToSuppress.Contains(classType))
            {
                return true;
            }

            var result = _classes.FirstOrDefault(x => x.TheType == classType);
            if (result != null)
            {
                return true;
            }

            return false;
        }

        internal bool HasMethod(EquatableMethod m, AnalysisStack analysisStack, out IlCode? found, out int newToken)
        {
            newToken = 0;
            if (_classesToSuppress.Contains(m.DeclaringType!))
            {
                found = null;
                return true;
            }

            var replacement = GetReplacement(m, analysisStack);
            if (replacement != null)
            {
                m = replacement;
            }

            if (_methods.TryGetValue(m, out var find))
            {
                found = find.Code;
                newToken = find.Token;
                return true;
            }

            found = null;
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

            if (_methods.TryGetValue(method.MethodBase, out var m1))
            {
                return false;
            }

            if (_methodsReplaced.TryGetValue(method.Name, out var list))
            {
                if (list.Any(x => EquatableMethod.AreMethodsIdentical(x.Original, method.MethodBase)))
                {
                    throw new InvalidOperationException(
                        $"Method {method} should have been replaced by its replacement");
                }

                if (list.Any(x => EquatableMethod.AreMethodsIdentical(x.Original, method.MethodBase) && x.Replacement == null))
                {
                    throw new InvalidOperationException(
                        $"The method {method} should be replaced, but has no new implementation. This program will not execute");
                }
            }

            ClearStatistics();
            _methods.Add(method.MethodBase, method);
            method.Index = _numDeclaredMethods;
            _numDeclaredMethods++;

            if ((method.Flags & MethodFlags.SpecialMethod) == MethodFlags.SpecialMethod)
            {
                _logger.LogDebug($"Internally implemented method {method.MethodBase.MethodSignature(false)} added to the execution set with token 0x{method.Token:X}");
            }
            else
            {
                _logger.LogDebug($"Method {method.MethodBase.MethodSignature(false)} added to the execution set with token 0x{method.Token:X}");
            }

            PrintProgress();

            return true;
        }

        internal IDictionary<EquatableMethod, ArduinoMethodDeclaration> Methods()
        {
            return _methods;
        }

        internal MemberInfo? InverseResolveToken(int token)
        {
            // Todo: This is very slow - consider inversing the dictionaries during data prep
            if (_inversePatchedMethodTokens.TryGetValue(token, out var method))
            {
                return method.Method;
            }

            if (_inversePatchedFieldTokens.TryGetValue(token, out var field))
            {
                return field.Field;
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

            BindingFlags flags = BindingFlags.Public | BindingFlags.Static | BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.DeclaredOnly;

            List<MethodInfo> methodsNeedingReplacement = typeToReplace.GetMethods(flags).ToList();

            if (!includingPrivates)
            {
                // We can't include internal methods by the filter above, so (unless we need all) remove all privates here, keeping public and internals
                methodsNeedingReplacement = methodsNeedingReplacement.Where(x => !x.IsPrivate).ToList();
            }

            foreach (var methoda in replacement.GetMethods(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Static | BindingFlags.Instance | BindingFlags.DeclaredOnly))
            {
                // Above, we only check the public methods, here we also look at the private ones
                BindingFlags otherFlags = BindingFlags.Public | BindingFlags.Static | BindingFlags.Instance | BindingFlags.NonPublic;
                if (!includingSubclasses)
                {
                    otherFlags |= BindingFlags.DeclaredOnly;
                }

                bool replacementFound = false;
                foreach (var methodb in typeToReplace.GetMethods(otherFlags))
                {
                    if (!methodsNeedingReplacement.Contains(methodb))
                    {
                        // This is not the one in need of replacement (or already has one, if the parameters matched a similar implementation as well)
                        continue;
                    }

                    if (EquatableMethod.MethodsHaveSameSignature(methoda, methodb) || EquatableMethod.AreSameOperatorMethods(methoda, methodb, false))
                    {
                        // Method A shall replace Method B
                        AddReplacementMethod(methodb, methoda);
                        // Remove from the list - so we see in the end what is missing
                        methodsNeedingReplacement.Remove(methodb);
                        replacementFound = true;
                        break;
                    }
                }

                if (!replacementFound)
                {
                    _logger.LogDebug($"Method {methoda.MemberInfoSignature()} has nothing to replace");
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
                    if (EquatableMethod.MethodsHaveSameSignature(methoda, methodb))
                    {
                        // Method A shall replace Method B
                        AddReplacementMethod(methodb, methoda);
                        // Remove from the list - so we see in the end what is missing
                        ctorsNeedingReplacement.Remove(methodb);
                        break;
                    }
                }
            }

            // If the replacement has a static ctor, also replace it
            foreach (var methoda in replacement.GetConstructors(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Static))
            {
                // Replace these only if explicitly requested (would need testing of impact otherwise)
                if (!EquatableMethod.HasArduinoImplementationAttribute(methoda, out _))
                {
                    continue;
                }

                bool found = false;
                // Above, we only check the public methods, here we also look at the private ones
                foreach (var methodb in typeToReplace.GetConstructors(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Static))
                {
                    if (EquatableMethod.MethodsHaveSameSignature(methoda, methodb))
                    {
                        // Method A shall replace Method B
                        AddReplacementMethod(methodb, methoda);
                        found = true;
                        break;
                    }
                }

                if (!found)
                {
                    ErrorManager.AddWarning("ACS0008", $"{replacement.FullName} specifies a static ctor to replace, but the original class has none");
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
                // Only exact matches, including assembly (there is a type named Interop.Kernel32 in several assemblies)
                if (x.Original.AssemblyQualifiedName == original.AssemblyQualifiedName)
                {
                    return x.Replacement;
                }
                else if (original.IsConstructedGenericType)
                {
                    // This is for the case where we do a full-replacement of a generic class
                    var deconstructed = original.GetGenericTypeDefinition();
                    var deconstructedReplacement = GetReplacement(deconstructed);
                    if (deconstructedReplacement == null)
                    {
                        continue;
                    }

                    var args = original.GetGenericArguments();
                    var result = deconstructedReplacement.MakeGenericType(args);
                    return result;
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

        private bool MethodsBelongToSameClassOrToReplacementClass(EquatableMethod first, EquatableMethod second)
        {
            if (first.DeclaringType == second.DeclaringType)
            {
                return true;
            }

            var r1 = GetReplacement(first.DeclaringType);
            if (r1 == second.DeclaringType)
            {
                return true;
            }

            var r2 = GetReplacement(second.DeclaringType);
            if (r2 == first.DeclaringType)
            {
                return true;
            }

            return false;
        }

        /// <summary>
        /// Checks whether the arguments have the same type or at least the same qualifiers.
        /// A method A(ref T a) must not match another method A(T* a)
        /// </summary>
        private bool MethodArgumentsHaveSameQualifiers(EquatableMethod first, EquatableMethod second)
        {
            var p1 = first.GetParameters();
            var p2 = second.GetParameters();
            if (p1.Length != p2.Length)
            {
                return false;
            }

            for (int i = 0; i < p1.Length; i++)
            {
                var p1Arg = p1[i].ParameterType;
                var p2Arg = p2[i].ParameterType;
                if (p1Arg == p2Arg)
                {
                    continue;
                }

                if (p1Arg.IsGenericParameter && p2Arg.IsGenericParameter)
                {
                    if (p1Arg.IsByRef != p2Arg.IsByRef || p1Arg.IsPointer != p2Arg.IsPointer)
                    {
                        return false;
                    }

                    continue;
                }

                return false;
            }

            return true;
        }

        internal EquatableMethod? GetReplacement(EquatableMethod original, AnalysisStack analysisStack)
        {
            // Odd: I'm pretty sure that previously equality on MethodBase instances worked, but for some reason not all instances pointing to the same method are Equal().
            if (!_methodsReplaced.TryGetValue(original.Name, out var methodsToConsider))
            {
                return null;
            }

            var elem = methodsToConsider.FirstOrDefault(x =>
            {
                if (x.Original.DeclaringType != original.DeclaringType)
                {
                    // The _methodsReplaced dictionary sorts by names only, so the potential candidates could be from entirely different classes
                    return false;
                }

                if (x.Replacement != null && EquatableMethod.HasArduinoImplementationAttribute(x.Replacement, out var attrib) && attrib.IgnoreGenericTypes)
                {
                    // There are only very few methods with the IgnoreGenericTypes attribute. Therefore a simple test is enough
                    if (x.Original.Name == original.Name && x.Original.GetParameters().Length == original.GetParameters().Length
                        && MicroCompiler.HasReplacementAttribute(x.Replacement.DeclaringType!, out var replacementAttribute)
                        && replacementAttribute.TypeToReplace == original.DeclaringType)
                    {
                        return true;
                    }
                }

                return EquatableMethod.AreMethodsIdentical(x.Original, original);
            });

            if (elem.Original == default)
            {
                // There's possibly a replacement required, so check whether we have a matching generic implementation replacement
                var openGenerics = methodsToConsider.FirstOrDefault(x => x.Original.IsConstructedGenericMethod == false
                                                                         && x.Original.IsConstructor == false && x.Original.GetGenericArguments().Length > 0
                                                                         && MethodsBelongToSameClassOrToReplacementClass(x.Original, original)
                                                                         && MethodArgumentsHaveSameQualifiers(x.Original, original)
                                                                         && x.Replacement != null && EquatableMethod.HasArduinoImplementationAttribute(x.Replacement, out var attr));

                // The second term prevents that we're trying to replace a replacement method,
                // as this causes a stack overflow.
                if (openGenerics.Replacement != null && !EquatableMethod.HasArduinoImplementationAttribute(original, out _))
                {
                    var replacement = GetGenericMethodReplacement(original, (MethodInfo)openGenerics.Replacement!.Method, true);
                    if (replacement == null)
                    {
                        ErrorManager.AddError("ACS0010", $"Cannot construct the generic method replacement for {original.MethodSignature()}.");
                    }

                    methodsToConsider.Add((original, replacement));
                    return replacement;
                }

                return null;
            }
            else if (elem.Replacement == null)
            {
                // There should be a replacement, but there isn't
                var classReplacement = GetReplacement(elem.Original.DeclaringType);
                // If it's about a missing replacement with a generic argument, elem.Original may already be a replacement object, so the
                // above returns null.
                if (GetNotSupportedExceptionMethod(classReplacement ?? elem.Original.DeclaringType!) != null)
                {
                    return null;
                }

                ErrorManager.AddError("ACS0007", $"Should have a replacement for {original.MethodSignature()}, but it is missing. CallStack: \r\n{analysisStack} " +
                                                    $"Original implementation is in {original.DeclaringType!.AssemblyQualifiedName}");
                return null;
            }

            return elem.Replacement;
        }

        /// <summary>
        /// Try to find a replacement for the given method in the given class
        /// </summary>
        /// <param name="methodInfo">The method to replace</param>
        /// <param name="analysisStack">The stack of the current analyzer queue</param>
        /// <param name="classToSearch">With a method in this class</param>
        /// <returns></returns>
        internal EquatableMethod? GetReplacement(EquatableMethod methodInfo, AnalysisStack analysisStack, Type classToSearch)
        {
            string n1 = classToSearch.FullName ?? string.Empty;
            foreach (var replacementMethod in classToSearch.GetMethods(BindingFlags.Public | BindingFlags.Static | BindingFlags.Instance | BindingFlags.NonPublic))
            {
                if (EquatableMethod.MethodsHaveSameSignature(replacementMethod, methodInfo) || EquatableMethod.AreSameOperatorMethods(replacementMethod, methodInfo, false))
                {
                    if (!replacementMethod.IsGenericMethodDefinition)
                    {
                        return replacementMethod;
                    }
                }

                var genericReplacement = GetGenericMethodReplacement(methodInfo, replacementMethod, methodInfo.Method.IsConstructedGenericMethod);
                if (genericReplacement != null)
                {
                    return genericReplacement;
                }
            }

            return null; // this is now likely an error
        }

        private EquatableMethod? GetGenericMethodReplacement(EquatableMethod original, MethodInfo replacementMethod, bool originalIsClosed)
        {
            if (replacementMethod.Name == original.Name && replacementMethod.GetParameters().Length == original.GetParameters().Length &&
                original.IsConstructedGenericMethod == originalIsClosed && replacementMethod.IsGenericMethodDefinition &&
                original.GetGenericArguments().Length == replacementMethod.GetGenericArguments().Length)
            {
                // The replacement method is likely the correct one, but we need to instantiate it.
                var repl = replacementMethod.MakeGenericMethod(original.GetGenericArguments());
                if (EquatableMethod.MethodsHaveSameSignature(repl, original) || EquatableMethod.AreSameOperatorMethods(repl, original, false))
                {
                    if (EquatableMethod.HasArduinoImplementationAttribute(replacementMethod, out var attr) && attr.MergeGenericImplementations)
                    {
                        // If we don't care about the types of the generic arguments, return the generic method signature instead
                        return replacementMethod;
                    }

                    return repl;
                }
            }

            return null;
        }

        internal void AddReplacementMethod(MethodBase? toReplace, MethodBase? replacement)
        {
            if (toReplace == null)
            {
                throw new ArgumentNullException(nameof(toReplace));
            }

            if (replacement != null && EquatableMethod.AreMethodsIdentical(toReplace, replacement))
            {
                // Replacing a method with itself may happen if virtual resolution points back to the same base class. Should fix itself later.
                return;
            }

            string name = toReplace.Name;
            if (_methodsReplaced.TryGetValue(name, out var list))
            {
                list.Add((toReplace, (replacement == null) ? (EquatableMethod?)null : new EquatableMethod(replacement)));
            }
            else
            {
                list = new List<(EquatableMethod, EquatableMethod?)>();
                list.Add((toReplace, (replacement == null) ? (EquatableMethod?)null : new EquatableMethod(replacement)));
                _methodsReplaced.Add(name, list);
            }
        }

        // Same implementation as on the C++ side, to make sure our encoding is correctly reversible
        private char Utf8ToUnicode(byte[] data, ref int index)
        {
            int charcode = 0;
            int t = data[index];
            index++;
            if (t < 128)
            {
                return (char)t;
            }

            int high_bit_mask = (1 << 6) - 1;
            int high_bit_shift = 0;
            int total_bits = 0;
            const int other_bits = 6;
            while ((t & 0xC0) == 0xC0)
            {
                t <<= 1;
                t &= 0xff;
                total_bits += 6;
                high_bit_mask >>= 1;
                high_bit_shift++;
                charcode <<= other_bits;
                charcode |= data[index] & ((1 << other_bits) - 1);
                index++;
            }

            charcode |= ((t >> high_bit_shift) & high_bit_mask) << total_bits;
            if (charcode > 0xFFFF)
            {
                charcode = 0x3F; // The Question Mark
            }

            return (char)charcode;
        }

        /// <summary>
        /// Decodes an UTF8 string to unicode, using the same algorithm the C++ code uses, to verify our data integrity.
        /// Idealy, this should be equivalent to Encoding.UTF8.FromBytes();
        /// </summary>
        /// <param name="data">Input array, UTF8</param>
        /// <returns>A .NET string instance (unicode)</returns>
        private string DecodeUtf8(byte[] data)
        {
            int index = 0;
            StringBuilder b = new StringBuilder();
            while (index < data.Length)
            {
                b.Append(Utf8ToUnicode(data, ref index));
            }

            return b.ToString();
        }

        internal int GetOrAddString(string data)
        {
            byte[] encoded = Encoding.UTF8.GetBytes(data);

            string decoded = DecodeUtf8(encoded);
            if (decoded != data)
            {
                throw new NotSupportedException($"Unable to properly encode string {data} for transmission");
            }

            var existing = _strings.FirstOrDefault(x => x.EncodedString.SequenceEqual(encoded));
            if (existing.Token != 0)
            {
                return existing.Token;
            }

            ClearStatistics();
            int token = _nextStringToken + encoded.Length;
            _nextStringToken += StringTokenStep;
            _strings.Add((token, encoded, data));
            return token;
        }

        internal String GetString(int token)
        {
            var entry = _strings.FirstOrDefault(x => x.Token == token);
            return entry.StringData;
        }

        internal ArduinoMethodDeclaration GetMethod(EquatableMethod methodInfo)
        {
            if (_methods.TryGetValue(methodInfo, out var m))
            {
                return m;
            }

            throw new InvalidOperationException($"No such method: {methodInfo.MemberInfoSignature()}");
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

            public SnapShot(ExecutionSet? set, List<int> alreadyAssignedTokens, List<int> alreadyAssignedStringTokens, List<int> specialTypes)
            {
                AlreadyAssignedTokens = alreadyAssignedTokens;
                AlreadyAssignedStringTokens = alreadyAssignedStringTokens;
                SpecialTypes = specialTypes;
                _set = set;
            }

            public List<int> AlreadyAssignedTokens
            {
                get;
            }

            public List<int> AlreadyAssignedStringTokens
            {
                get;
            }

            public List<int> SpecialTypes
            {
                get;
            }

            public override int GetHashCode()
            {
                int ret = AlreadyAssignedTokens.Count;
                ret ^= Xor(AlreadyAssignedTokens);
                ret ^= Xor(AlreadyAssignedStringTokens);
                if (AlreadyAssignedTokens.Count > 0 && _set != null)
                {
                    // Add the original token from the last entry in the list (could also add all of them)
                    var element = _set.InverseResolveToken(AlreadyAssignedTokens[AlreadyAssignedTokens.Count - 1]);
                    ret ^= element!.MetadataToken;
                }

                return ret;
            }
        }

        /// <summary>
        /// This is required for the special implementation of Array.GetEnumerator that is an implementation of IList{T}
        /// See I.8.9.1 Array types. T[] implements IList{T}.
        /// </summary>
        public void AddArrayImplementation(TypeInfo arrayType, MethodInfo getEnumeratorCall)
        {
            _arrayListImpl[arrayType] = getEnumeratorCall;
        }

        public void SuppressNamespace(string namespacePrefix, bool includingSubNamespaces)
        {
            var assembly = typeof(System.Runtime.GCSettings).Assembly;
            foreach (var t in assembly.GetTypes())
            {
                if (t.Namespace == null)
                {
                    continue;
                }

                if (includingSubNamespaces)
                {
                    if (t.Namespace.StartsWith(namespacePrefix, StringComparison.InvariantCulture))
                    {
                        SuppressType(t);
                    }
                }
                else
                {
                    if (t.Namespace == namespacePrefix)
                    {
                        SuppressType(t);
                    }
                }
            }
        }

        public void WriteMapFile(string tokenMapFile, IlCapabilities ilCapabilities)
        {
            using StreamWriter w = new StreamWriter(tokenMapFile);

            w.WriteLine("Program data summary");
            w.WriteLine("====================");
            w.WriteLine($"Program Name: {CompilerSettings.ProcessName ?? "N/A"}");
            w.WriteLine(ilCapabilities);
            w.WriteLine($"Total number of classes: {_classes.Count}");
            w.WriteLine($"Total number of methods: {_methods.Count}");
            w.WriteLine($"Total number of static fields: {_staticFieldsUsed.Count}");
            w.WriteLine($"Total number of string constants: {_strings.Count}");
            w.WriteLine();

            w.WriteLine("Token map file, ordered by token number");
            w.WriteLine("=======================================");
            w.WriteLine();
            List<uint> tokens = new List<uint>();
            tokens.AddRange(_patchedMethodTokens.Select(x => (uint)x.Value));
            tokens.AddRange(_patchedTypeTokens.Select(x => (uint)x.Value));
            tokens.AddRange(_patchedFieldTokens.Select(x => (uint)x.Value.Token));

            tokens.Sort();

            foreach (var token in tokens)
            {
                var c = _classes.FirstOrDefault(x => (uint)x.NewToken == token);
                if (c != null)
                {
                    string type = "(Class)";
                    if (c.TheType.IsValueType)
                    {
                        type = "(Struct)";
                    }

                    w.WriteLine($"0x{c.NewToken:X8} {type} {c.Name} (Dynamic size: {c.DynamicSize}, static size: {c.StaticSize})");
                    continue;
                }

                var m = _methods.Values.FirstOrDefault(x => (uint)x.Token == token);
                if (m != null)
                {
                    w.WriteLine($"0x{m.Token:X8} (Method) {m.ToString()}");
                    continue;
                }

                var pc = _patchedTypeTokens.FirstOrDefault(x => (uint)x.Value == token);
                if (pc.Value != 0)
                {
                    w.WriteLine($"0x{token:X8} (Class, not loaded) {pc.Key.FullName}");
                    continue;
                }

                var pm = _patchedMethodTokens.FirstOrDefault(x => (uint)x.Value == token);
                if (pm.Value != 0)
                {
                    w.WriteLine($"0x{token:X8} (Method, not loaded or no implementation present) {pm.Key.MemberInfoSignature()}");
                    continue;
                }

                var fld = _patchedFieldTokens.FirstOrDefault(x => (uint)x.Value.Token == token);
                if (fld.Value.Token != 0)
                {
                    string className;
                    var cls = _classes.FirstOrDefault(x => x.Members.Any(y => y.Token == token));
                    string name = "(Field)";
                    int s;
                    int o = -1;
                    if (cls != null)
                    {
                        var f = cls.Members.First(y => y.Token == token);
                        s = f.SizeOfField;
                        o = f.Offset;
                        name = "(Dynamic field)";
                        if (f.StaticFieldSize > 0)
                        {
                            name = "(Static field)";
                            s = f.StaticFieldSize;
                        }

                        className = cls.Name;
                    }
                    else
                    {
                        className = fld.Key.Name;
                        s = -1;
                    }

                    if (o >= 0)
                    {
                        w.WriteLine($"0x{token:X8} {name} {fld.Key.Name} of {className}. Size {s} Offset {o}");
                    }
                    else
                    {
                        w.WriteLine($"0x{token:X8} {name} {fld.Key.Name} of {className}. Size {s}");
                    }
                }
            }

            w.WriteLine("================================================");
            long size = EstimateRequiredMemory(out var details);
            w.WriteLine($"Total estimated flash space required: {size} bytes ({size / 1024} kb)");
            w.WriteLine($"Total number of classes in execution set: {_classes.Count}.");
            w.WriteLine($"Total number of methods in execution set: {_methods.Count}.");

            long stringBytesUsed = 0;
            foreach (var constant in _strings)
            {
                stringBytesUsed += constant.EncodedString.Length + 4;
            }

            w.WriteLine($"Total number of string constants in execution set: {_strings.Count} with a total size of {stringBytesUsed} bytes ({stringBytesUsed / 1024} kb)");

            List<(int Token, byte[] Data, string NoData)> converted = new();
            // Need to do this manually, due to stupid nullability conversion restrictions
            long constDataUsed = 0;
            foreach (var elem in _patchedFieldTokens.Values)
            {
                if (elem.InitializerData != null)
                {
                    converted.Add((elem.Token, elem.InitializerData, string.Empty));
                    constDataUsed += elem.InitializerData.Length;
                }
            }

            w.WriteLine($"Total number of data blobs in execution set: {converted.Count} with a total size of {constDataUsed} bytes ({constDataUsed / 1024} kb)");
            w.WriteLine();
            w.WriteLine("Classes, sorted by size");
            w.WriteLine("=======================");

            foreach (var stat in details)
            {
                var value = stat.Value;
                w.WriteLine($"Class {value.Type.Name}: Total {value.TotalBytes} Bytes, {value.MethodBytes} for methods and {value.ClassBytes} for member metadata");
            }

            w.WriteLine();
            w.WriteLine("Static root fields");
            w.WriteLine("==================");

            int offset = 0;
            foreach (var f in _staticFieldsUsed)
            {
                w.WriteLine($"0x{f.Item2.Token:X8}: {f.Item2.Name} reserved {f.Item1} bytes at offset {offset}");
                offset += f.Item1;
            }

            w.WriteLine($"Total static root size required {StaticMemberSize} bytes for {_staticFieldsUsed.Count} fields.");

            w.Close();
        }

        private void ClearStatistics()
        {
            ExpectedMemoryUsage = null;
            Statistics = null;
        }

        private void PrintProgress()
        {
            if ((_methods.Count + _classes.Count) % 100 == 0)
            {
                _logger.LogInformation($"Collected {_classes.Count} classes and {_methods.Count} methods. And counting...");
            }
        }

        /// <summary>
        /// Remove initializer fields (in PrivateImplementationDetails) that are unused.
        /// We initialize the class as a whole, but only in the end we know which fields are actually used.
        /// </summary>
        public void RemoveUnusedDataFields()
        {
            ClearStatistics();
            foreach (var c in _classes.Where(x => x.Name.Contains(MicroCompiler.PrivateImplementationDetailsName, StringComparison.Ordinal)))
            {
                for (var index = 0; index < c.Members.Count; index++)
                {
                    var field = c.Members[index];
                    var f = field.Field;
                    if (f != null && (f.IsStatic || f.IsLiteral))
                    {
                        if (!_patchedFieldTokens.TryGetValue(f, out var value) || value.InitializerData == null)
                        {
                            c.RemoveMemberAt(index);
                            index--;
                        }
                    }
                }
            }
        }

        /// <summary>
        /// Create a table that allows fast lookups for ResolveClassFromFieldToken and ResolveClassFromCtorToken
        /// </summary>
        public void AddReverseFieldLookupTable()
        {
            Dictionary<int, ClassDeclaration> fieldOrCtorTokenToClass = new Dictionary<int, ClassDeclaration>();
            foreach (var c in Classes)
            {
                foreach (var f in c.Members)
                {
                    // Tokens might be found in multiple classes, due to class replacements (fields in replacement classes shadow the original counterpart)
                    if (f.Field != null)
                    {
                        fieldOrCtorTokenToClass[f.Token] = c;
                    }

                    if (f.Method is ConstructorInfo)
                    {
                        fieldOrCtorTokenToClass[f.Token] = c;
                    }
                }
            }

            _logger.LogDebug($"Got {fieldOrCtorTokenToClass.Count} members in reverse lookup index");
        }

        internal bool TryGetCachedCode(EquatableMethod method, [NotNullWhen(true)]out IlCode? ilCode)
        {
            if (_codeCache.TryGetValue(method, out ilCode))
            {
                return true;
            }

            return false;
        }

        internal bool TryAddCachedCode(EquatableMethod method, IlCode code)
        {
            return _codeCache.TryAdd(method, code);
        }
    }
}
