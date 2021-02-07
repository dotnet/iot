using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.Linq;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading;
using UnitsNet;

#pragma warning disable CS1591
namespace Iot.Device.Arduino
{
    internal enum ExecutorCommand : byte
    {
        None = 0,
        DeclareMethod = 1,
        LoadIl = 3,
        StartTask = 4,
        ResetExecutor = 5,
        KillTask = 6,
        MethodSignature = 7,
        ClassDeclaration = 8,
        ClassDeclarationEnd = 9, // Last part of class declaration
        ConstantData = 10,
        Interfaces = 11,
        CopyToFlash = 12,

        WriteFlashHeader = 13,
        CheckFlashVersion = 14,
        EraseFlash = 15,

        Nack = 0x7e,
        Ack = 0x7f,
    }

    [Flags]
    public enum MethodFlags
    {
        None = 0,
        Static = 1,
        Virtual = 2,
        SpecialMethod = 4, // Method will resolve to a built-in function on the arduino
        Void = 8, // The method returns void
        Ctor = 16, // The method is a ctor (which only implicitly returns "this"); the flag is not set for static ctors.
        Abstract = 32, // The method is abstract (or an interface stub)
    }

    public enum MethodState
    {
        Stopped = 0,
        Aborted = 1,
        Running = 2,
        Killed = 3,
    }

    internal enum ExecutionError
    {
        None = 0,
        EngineBusy = 1,
        InvalidArguments = 2,
        OutOfMemory = 3,
        InternalError = 4,
    }

    [Flags]
    public enum VariableKind : byte
    {
        Void = 0, // The slot contains no data
        Uint32 = 1, // The slot contains unsigned integer data
        Int32 = 2, // The slot contains signed integer data
        Boolean = 3, // The slot contains true or false
        Object = 4, // The slot contains an object reference
        Method = 5,
        ValueArray = 6, // The slot contains a reference to an array of value types (inline)
        ReferenceArray = 7, // The slot contains a reference to an array of reference types
        Float = 8,
        LargeValueType = 9, // The slot contains a large value type
        Int64 = 16 + 1,
        Uint64 = 16 + 2,
        Double = 16 + 4,
        Reference = 32, // Address of a variable
        RuntimeFieldHandle = 33, // So far this is a pointer to a constant initializer
        RuntimeTypeHandle = 34, // A type handle. The value is a type token
        AddressOfVariable = 35, // An address pointing to a variable slot on another method's stack or arglist
        FunctionPointer = 36, // A function pointer
        StaticMember = 128, // type is defined by the first value it gets
    }

    /// <summary>
    /// A set of tokens which is always assigned to these classes, because they need to be identifiable in the firmware, i.e. the token assigned
    /// to "System.Type" is always 2
    /// </summary>
    public enum KnownTypeTokens
    {
        None = 0,
        Object = 1,
        Type = 2,
        ValueType = 3,
        String = 4,
        TypeInfo = 5,
        RuntimeType = 6,
        Nullable = 7,
        Enum = 8,
        Array = 9,
        ByReferenceByte = 10,
        Delegate = 11,
        MulticastDelegate = 12,
        LargestKnownTypeToken = 20,
    }

    public sealed class ArduinoCsCompiler : IDisposable
    {
        private const int DataVersion = 1;
        private readonly ArduinoBoard _board;
        private readonly List<IArduinoTask> _activeTasks;

        private ExecutionSet? _activeExecutionSet;

        // List of classes that have arduino-native implementations
        // These classes substitute (part of) a framework class
        private List<Type> _replacementClasses;

        private bool _disposed = false;

        public ArduinoCsCompiler(ArduinoBoard board, bool resetExistingCode = true)
        {
            _board = board;
            _board.SetCompilerCallback(BoardOnCompilerCallback);

            _activeTasks = new List<IArduinoTask>();
            _activeExecutionSet = null;

            if (resetExistingCode)
            {
                ClearAllData(true, false);
            }

            // Generate the list of all replacement classes (they're all called Mini*)
            _replacementClasses = new List<Type>();
            foreach (var type in Assembly.GetExecutingAssembly().GetTypes())
            {
                if (type.GetCustomAttribute<ArduinoReplacementAttribute>() != null)
                {
                    _replacementClasses.Add(type);
                }
            }
        }

        private string GetMethodName(ArduinoMethodDeclaration decl)
        {
            return decl.MethodBase.Name;
        }

        internal void TaskDone(IArduinoTask task)
        {
            _activeTasks.Remove(task);
        }

        private void BoardOnCompilerCallback(int codeReference, MethodState state, object args)
        {
            if (_activeExecutionSet == null)
            {
                _board.Log($"Invalid method state message. No code currently active.");
                return;
            }

            var codeRef = _activeExecutionSet.Methods().FirstOrDefault(x => x.Index == codeReference);
            if (codeRef == null)
            {
                _board.Log($"Invalid method state message. Not currently knowing any method with reference {codeReference}.");
                return;
            }

            var task = _activeTasks.FirstOrDefault(x => x.MethodInfo == codeRef && x.State == MethodState.Running);

            if (task == null)
            {
                _board.Log($"Invalid method state update. {codeRef.Index} has no active task.");
                return;
            }

            if (state == MethodState.Aborted)
            {
                _board.Log($"Execution of method {GetMethodName(codeRef)} caused an exception. Check previous messages.");
                // In this case, the data contains the exception tokens and the call stack tokens
                task.AddData(state, ((int[])args).Cast<object>().ToArray());
                return;
            }

            if (state == MethodState.Killed)
            {
                _board.Log($"Execution of method {GetMethodName(codeRef)} was forcibly terminated.");
                // Still update the task state, this will prevent a deadlock if somebody is waiting for this task to end
                task.AddData(state, new object[0]);
                return;
            }

            object[] outObjects = new object[1];
            if (state == MethodState.Stopped)
            {
                object retVal;
                byte[] data = (byte[])args;

                // The method ended, therefore we know that the only element of args is the return value and can derive its correct type
                Type returnType;
                // We sometimes also execute ctors directly, but they return void
                if (codeRef.MethodBase.MemberType == MemberTypes.Constructor)
                {
                    returnType = typeof(void);
                }
                else
                {
                    returnType = codeRef.MethodInfo!.ReturnType;
                }

                if (returnType == typeof(void))
                {
                    // Empty return set
                    task.AddData(state, new object[0]);
                    return;
                }
                else if (returnType == typeof(bool))
                {
                    retVal = data[0] != 0;
                }
                else if (returnType == typeof(UInt32))
                {
                    retVal = BitConverter.ToUInt32(data);
                }
                else if (returnType == typeof(Int32))
                {
                    retVal = BitConverter.ToInt32(data);
                }
                else if (returnType == typeof(float))
                {
                    retVal = BitConverter.ToSingle(data);
                }
                else if (returnType == typeof(double))
                {
                    retVal = BitConverter.ToDouble(data);
                }
                else if (returnType == typeof(Int64))
                {
                    retVal = BitConverter.ToInt64(data);
                }
                else if (returnType == typeof(UInt64))
                {
                    retVal = BitConverter.ToUInt64(data);
                }
                else
                {
                    throw new NotSupportedException("Unsupported return type");
                }

                outObjects[0] = retVal;
            }

            task.AddData(state, outObjects);
        }

        /// <summary>
        /// This adds a set of low-level methods to the execution set. These are intended to be copied to flash, as they will be used
        /// by many programs. We call the method set constructed here "the kernel".
        /// </summary>
        /// <param name="set">Execution set</param>
        private void PrepareLowLevelInterface(ExecutionSet set)
        {
            if (_disposed)
            {
                throw new ObjectDisposedException(nameof(ArduinoCsCompiler));
            }

            void AddMethod(MethodInfo method, NativeMethod nativeMethod)
            {
                if (!set.HasMethod(method))
                {
                    set.GetReplacement(method.DeclaringType);
                    MethodInfo? replacement = (MethodInfo?)set.GetReplacement(method);
                    if (replacement != null)
                    {
                        method = replacement;
                        if (set.HasMethod(method))
                        {
                            return;
                        }
                    }

                    int token = set.GetOrAddMethodToken(method);
                    ArduinoMethodDeclaration decl = new ArduinoMethodDeclaration(token, method, null, MethodFlags.SpecialMethod, nativeMethod);
                    set.AddMethod(decl);
                }
            }

            Type lowLevelInterface = typeof(ArduinoHardwareLevelAccess);
            foreach (var method in lowLevelInterface.GetMethods(BindingFlags.Public | BindingFlags.DeclaredOnly))
            {
                var attr = (ArduinoImplementationAttribute)method.GetCustomAttributes(typeof(ArduinoImplementationAttribute)).First();
                AddMethod(method, attr.MethodNumber);
            }

            MethodInfo? methodToReplace;

            // And the internal classes
            foreach (var replacement in _replacementClasses)
            {
                var attribs = replacement.GetCustomAttributes(typeof(ArduinoReplacementAttribute));
                ArduinoReplacementAttribute ia = (ArduinoReplacementAttribute)attribs.Single();
                if (ia.ReplaceEntireType)
                {
                    PrepareClass(set, replacement);
                    set.AddReplacementType(ia.TypeToReplace, replacement, ia.IncludingSubclasses, ia.IncludingPrivates);
                }
                else
                {
                    foreach (var m in replacement.GetMethods(BindingFlags.Public | BindingFlags.Instance | BindingFlags.Static))
                    {
                        // Methods that have this attribute shall be replaced - if the value is ArduinoImplementation.None, the C# implementation is used,
                        // otherwise a native implementation is provided
                        attribs = m.GetCustomAttributes(typeof(ArduinoImplementationAttribute));
                        ArduinoImplementationAttribute? iaMethod = (ArduinoImplementationAttribute?)attribs.SingleOrDefault();
                        if (iaMethod != null)
                        {
                            BindingFlags flags = BindingFlags.Public | BindingFlags.Instance | BindingFlags.Static;
                            if (ia.IncludingPrivates)
                            {
                                flags |= BindingFlags.NonPublic;
                            }

                            methodToReplace = ia.TypeToReplace!.GetMethods(flags).SingleOrDefault(x => MethodsHaveSameSignature(x, m) || AreSameOperatorMethods(x, m));
                            if (methodToReplace == null)
                            {
                                // That may be ok if it is our own internal implementation
                                _board.Log($"A replacement method has nothing to replace: {m.DeclaringType} - {m}");
                            }
                            else
                            {
                                set.AddReplacementMethod(methodToReplace, m);
                            }
                        }
                    }
                }
            }

            // Some special replacements required
            Type type = typeof(System.RuntimeTypeHandle);
            MethodInfo? replacementMethodInfo;
            var methods = type.GetMethods(BindingFlags.NonPublic | BindingFlags.Static);
            methodToReplace = methods.First(x => x.Name == "CreateInstanceForAnotherGenericParameter");

            type = typeof(MiniType);
            replacementMethodInfo = type.GetMethod("CreateInstanceForAnotherGenericParameter");
            set.AddReplacementMethod(methodToReplace, replacementMethodInfo);

            // Some classes are dynamically created in the runtime - we need them anyway
            HashSet<object> hb = new HashSet<object>();
            PrepareClass(set, hb.Comparer.GetType()); // The actual instance here is ObjectEqualityComparer<object>

            PrepareClass(set, typeof(IEquatable<object>));

            PrepareClass(set, typeof(System.Span<Int32>));
            HashSet<string> hs = new HashSet<string>();
            PrepareClass(set, hs.Comparer.GetType()); // GenericEqualityComparer<string>

            HashSet<int> hi = new HashSet<int>();
            PrepareClass(set, hi.Comparer.GetType()); // GenericEqualityComparer<int>

            PrepareClass(set, typeof(IEquatable<Nullable<int>>));

            PrepareClass(set, typeof(System.Array));

            PrepareClass(set, typeof(System.Object));

            // We'll always need to provide these methods, or we'll get into trouble because they're not explicitly used before anything that depends on
            // them in the runtime
            type = typeof(System.Object);
            replacementMethodInfo = type.GetMethod("Equals", BindingFlags.Public | BindingFlags.Instance)!; // Not the static one
            AddMethod(replacementMethodInfo, NativeMethod.ObjectEquals);
            replacementMethodInfo = type.GetMethod("ToString")!;
            AddMethod(replacementMethodInfo, NativeMethod.ObjectToString);
            replacementMethodInfo = type.GetMethod("GetHashCode")!;
            AddMethod(replacementMethodInfo, NativeMethod.ObjectGetHashCode);

            if (set.CompilerSettings.CreateKernelForFlashing)
            {
                // Finally, mark this set as "the kernel"
                set.CreateKernelSnapShot();
            }
        }

        public void PrepareClass(ExecutionSet set, Type classType)
        {
            if (_disposed)
            {
                throw new ObjectDisposedException(nameof(ArduinoCsCompiler));
            }

            HashSet<Type> baseTypes = new HashSet<Type>();

            baseTypes.Add(classType);
            DetermineBaseAndMembers(baseTypes, classType);

            foreach (var cls in baseTypes)
            {
                PrepareClassDeclaration(set, cls);
            }
        }

        private void PrepareClassDeclaration(ExecutionSet set, Type classType)
        {
            if (set.HasDefinition(classType))
            {
                return;
            }

            var replacement = set.GetReplacement(classType);

            if (replacement != null)
            {
                classType = replacement;
                if (set.HasDefinition(classType))
                {
                    return;
                }
            }

            List<FieldInfo> fields = new List<FieldInfo>();
            List<MemberInfo> methods = new List<MemberInfo>();

            GetFields(classType, fields, methods);

            List<ClassMember> memberTypes = new List<ClassMember>();
            for (var index = 0; index < fields.Count; index++)
            {
                var field = fields[index];
                var fieldType = GetVariableType(field.FieldType, out var size);
                if (field.IsStatic)
                {
                    fieldType |= VariableKind.StaticMember;
                }

                // The only (known) field that can contain a function pointer. Getting the type correct here helps in type tracking and debugging
                if (field.DeclaringType == typeof(Delegate) && field.Name == "_methodPtr")
                {
                    fieldType = VariableKind.FunctionPointer;
                }

                var newvar = new ClassMember(field, fieldType, set.GetOrAddFieldToken(field), size);
                memberTypes.Add(newvar);
            }

            for (var index = 0; index < methods.Count; index++)
            {
                var m = methods[index] as ConstructorInfo;
                if (m != null)
                {
                    memberTypes.Add(new ClassMember(m, VariableKind.Method, set.GetOrAddMethodToken(m), new List<int>()));
                }
            }

            var sizeOfClass = GetClassSize(classType);

            var interfaces = classType.GetInterfaces().ToList();

            // Add this first, so we break the recursion to this class further down
            var newClass = new ClassDeclaration(classType, sizeOfClass.Dynamic, sizeOfClass.Statics, set.GetOrAddClassToken(classType.GetTypeInfo()), memberTypes, interfaces);
            set.AddClass(newClass);
            foreach (var iface in interfaces)
            {
                PrepareClassDeclaration(set, iface);
            }
        }

        /// <summary>
        /// Complete the execution set by making sure all dependencies are resolved
        /// </summary>
        internal void FinalizeExecutionSet(ExecutionSet set)
        {
            if (_disposed)
            {
                throw new ObjectDisposedException(nameof(ArduinoCsCompiler));
            }

            // Because the code below is still not water proof (there could have been virtual methods added only in the end), we do this twice
            for (int i = 0; i < 2; i++)
            {
                // Contains all classes traversed so far
                List<ClassDeclaration> declarations = new List<ClassDeclaration>(set.Classes);
                // Contains the new ones to be traversed this time (start with all)
                List<ClassDeclaration> newDeclarations = new List<ClassDeclaration>(declarations);
                while (true)
                {
                    // Sort: Interfaces first, then bases before their derived types (so that if a base rewires one virtual method to another - possibly abstract -
                    // method, the derived method's actual implementation is linked. I.e. IEqualityComparer.Equals(object,object) -> EqualityComparer.Equals(object, object) ->
                    // EqualityComparer<T>.Equals(T,T) -> -> GenericEqualityComparer<T>.Equals(T,T)
                    newDeclarations.Sort(new ClassDeclarationByInheritanceSorter());
                    DetectRequiredVirtualMethodImplementations(set, newDeclarations);
                    if (set.Classes.Count == declarations.Count)
                    {
                        break;
                    }

                    // Find the new ones
                    newDeclarations = new List<ClassDeclaration>();
                    foreach (var decl in set.Classes)
                    {
                        if (!declarations.Contains(decl))
                        {
                            newDeclarations.Add(decl);
                        }
                    }

                    declarations = new List<ClassDeclaration>(set.Classes);
                }
            }

            // Last step: Of all classes in the list, load their static cctors
            for (var i = 0; i < set.Classes.Count; i++)
            {
                // Let's hope the list no more changes, but in theory we don't know (however, creating static ctors that
                // depend on other classes might give a big problem)
                var cls = set.Classes[i];
                var cctor = cls.TheType.TypeInitializer;
                if (cctor == null || cls.SuppressInit)
                {
                    continue;
                }

                PrepareCodeInternal(set, cctor, null);
            }

            _board.Log($"Estimated program memory usage: {set.EstimateRequiredMemory()} bytes.");
        }

        /// <summary>
        /// Orders classes by inheritance (interfaces and base classes before derived classes)
        /// </summary>
        internal class ClassDeclarationByInheritanceSorter : IComparer<ClassDeclaration>
        {
            /// <inheritdoc cref="IComparer{T}.Compare"/>
            public int Compare(ClassDeclaration? x, ClassDeclaration? y)
            {
                if (x == y)
                {
                    return 0;
                }

                // No nulls expected here
                Type xt = x!.TheType;
                Type yt = y!.TheType;

                if (xt.IsInterface && !yt.IsInterface)
                {
                    return -1;
                }

                if (!xt.IsInterface && yt.IsInterface)
                {
                    return 1;
                }

                if (xt.IsInterface && yt.IsInterface)
                {
                    return string.Compare(x.Name, y.Name, StringComparison.Ordinal);
                }

                // Both x and y are not interfaces now (and not equal)
                if (xt.IsAssignableFrom(yt))
                {
                    return -1;
                }

                if (yt.IsAssignableFrom(xt))
                {
                    return 1;
                }

                return string.Compare(x.Name, y.Name, StringComparison.Ordinal);
            }
        }

        /// <summary>
        /// Detects the required (potentially used) virtual methods in the execution set
        /// </summary>
        private void DetectRequiredVirtualMethodImplementations(ExecutionSet set, List<ClassDeclaration> declarations)
        {
            for (var i = 0; i < declarations.Count; i++)
            {
                var cls = declarations[i];
                List<FieldInfo> fields = new List<FieldInfo>();
                List<MemberInfo> methods = new List<MemberInfo>();

                GetFields(cls.TheType, fields, methods);
                for (var index = 0; index < methods.Count; index++)
                {
                    var m = methods[index];
                    if (MemberLinkRequired(set, m, out var baseMethodInfos))
                    {
                        var mb = (MethodBase)m; // This cast must work

                        if (cls.Members.Any(x => x.Method == m))
                        {
                            continue;
                        }

                        // If this method is required because base implementations are called, we also need its implementation (obviously)
                        // Unfortunately, this can recursively require further classes and methods
                        PrepareCodeInternal(set, mb, null);

                        List<int> baseTokens = baseMethodInfos.Select(x => set.GetOrAddMethodToken(x)).ToList();
                        cls.AddClassMember(new ClassMember(mb, VariableKind.Method, set.GetOrAddMethodToken(mb), baseTokens));
                    }
                }
            }
        }

        /// <summary>
        /// Send all class declaration from from to to.
        /// </summary>
        /// <param name="set">Execution set</param>
        /// <param name="fromSnapShot">Elements to skip (already loaded)</param>
        /// <param name="toSnapShot">Elements to include (must be a superset of <paramref name="fromSnapShot"/>)</param>
        /// <param name="markAsReadOnly">Mark uploaded classes as readonly</param>
        internal void SendClassDeclarations(ExecutionSet set, ExecutionSet.SnapShot fromSnapShot, ExecutionSet.SnapShot toSnapShot, bool markAsReadOnly)
        {
            int idx = 0;
            // Include all elements that are not in from but in to. Do not include elements in neither collection.
            var list = set.Classes.Where(x => !fromSnapShot.AlreadyAssignedTokens.Contains(x.NewToken) && toSnapShot.AlreadyAssignedTokens.Contains(x.NewToken));
            foreach (var c in list.OrderBy(x => x.NewToken))
            {
                var cls = c.TheType;
                Int32 parentToken = 0;
                Type parent = cls.BaseType!;
                if (parent != null)
                {
                    parentToken = set.GetOrAddClassToken(parent.GetTypeInfo());
                }

                int token = set.GetOrAddClassToken(cls.GetTypeInfo());

                // separated for debugging purposes (the debugger cannot evaluate Type.ToString() on a conditional breakpoint)
                string className = cls.Name;

                _board.Log($"Sending class declaration for {className} (Token 0x{token:x8}). Number of members: {c.Members.Count}, Dynamic size {c.DynamicSize} Bytes, Static Size {c.StaticSize} Bytes. Class {idx + 1} / {set.Classes.Count}");
                _board.Firmata.SendClassDeclaration(token, parentToken, (c.DynamicSize, c.StaticSize), cls.IsValueType, c.Members);

                _board.Firmata.SendInterfaceImplementations(token, c.Interfaces.Select(x => set.GetOrAddClassToken(x.GetTypeInfo())).ToArray());

                if (markAsReadOnly)
                {
                    c.ReadOnly = true;
                }

                idx++;
            }
        }

        public void SendConstants(IEnumerable<(int Token, byte[] InitializerData)> constElements)
        {
            foreach (var e in constElements)
            {
                if (e.InitializerData == null)
                {
                    continue;
                }

                _board.Firmata.SendConstant(e.Token, e.InitializerData);
            }
        }

        internal void SendMethods(ExecutionSet set)
        {
            var cnt = set.Methods().Count;
            foreach (var me in set.Methods())
            {
                MethodBase methodInfo = me.MethodBase;
                _board.Log($"Loading Method {me.Index + 1} of {cnt} (NewToken 0x{me.Token:X}), named {methodInfo.DeclaringType} - {methodInfo.Name}.");
                SendMethod(set, me);
            }
        }

        /// <summary>
        /// Detects whether the method must be known by the class declaration.
        /// This is used a) to find the class to construct from a newobj instruction (which provides the ctor token only)
        /// and b) to resolve virtual method calls on a concrete class.
        /// </summary>
        /// <param name="set">The current execution set</param>
        /// <param name="method">The method instance</param>
        /// <param name="methodsBeingImplemented">Returns the list of methods (from interfaces or base classes) that this method implements</param>
        /// <returns>True if the method shall be part of the class declaration</returns>
        private static bool MemberLinkRequired(ExecutionSet set, MemberInfo method, out List<MethodInfo> methodsBeingImplemented)
        {
            methodsBeingImplemented = new List<MethodInfo>();

            if (method is MethodInfo m)
            {
                // Static methods, do not need a link to the class (so far)
                if (m.IsStatic)
                {
                    return false;
                }

                // For ordinary methods, it gets more complicated.
                // We need to find out whether this method overrides some other method or implements an interface method
                if (m.IsAbstract)
                {
                    // An abstract method can never be called, so it is never the real call target of a callvirt instruction
                    return false;
                }

                CollectBaseImplementations(m, methodsBeingImplemented);

                // We need the implementation if at least one base implementation is being called and is used
                return methodsBeingImplemented.Count > 0 && methodsBeingImplemented.Any(x => set.HasMethod(x));
            }

            return false;
        }

        private static bool IsOverriddenImplementation(MethodInfo candidate, MethodInfo self, bool candidateIsFromInterface)
        {
            if (candidate.Name != self.Name)
            {
                return false;
            }

            // If we're declared new, we're not overriding anything (that does not apply for interfaces, though)
            if (self.Attributes.HasFlag(MethodAttributes.NewSlot) && !candidateIsFromInterface)
            {
                return false;
            }

            // if the base is neither virtual nor abstract, we're not overriding
            if (!candidate.IsVirtual && !candidate.IsAbstract)
            {
                return false;
            }

            // private methods cannot be virtual
            // TODO: Check how explicitly interface implementations are handled in IL
            if (self.IsPrivate || candidate.IsPrivate)
            {
                return false;
            }

            return MethodsHaveSameSignature(self, candidate);
        }

        internal static void CollectBaseImplementations(MethodInfo method, List<MethodInfo> methodsBeingImplemented)
        {
            Type? cls = method.DeclaringType?.BaseType;
            while (cls != null)
            {
                foreach (var candidate in cls.GetMethods(BindingFlags.Public | BindingFlags.Instance | BindingFlags.NonPublic))
                {
                    if (IsOverriddenImplementation(candidate, method, false))
                    {
                        methodsBeingImplemented.Add(candidate);
                    }
                }

                cls = cls.BaseType;
            }

            cls = method.DeclaringType;
            if (cls == null)
            {
                return;
            }

            foreach (var interf in cls.GetInterfaces())
            {
                foreach (var candidate in interf.GetMethods(BindingFlags.Public | BindingFlags.Instance | BindingFlags.NonPublic))
                {
                    if (IsOverriddenImplementation(candidate, method, true))
                    {
                        methodsBeingImplemented.Add(candidate);
                    }
                }
            }
        }

        private static void GetFields(Type classType, List<FieldInfo> fields, List<MemberInfo> methods)
        {
            foreach (var m in classType.GetMembers(BindingFlags.Instance | BindingFlags.Public | BindingFlags.Static |
                                                   BindingFlags.NonPublic | BindingFlags.DeclaredOnly))
            {
                if (m is FieldInfo field)
                {
                    fields.Add(field);
                }
                else if (m is MethodInfo method)
                {
                    methods.Add(method);
                }
                else if (m is ConstructorInfo ctor)
                {
                    methods.Add(ctor);
                }
            }
        }

        /// <summary>
        /// Returns the size of the variable on the target platform. This is currently the largest of sizeof(void*) and sizeof(int64).
        /// </summary>
        /// <returns>See above</returns>
        private static int SizeOfVoidPointer()
        {
            return 4;
        }

        /// <summary>
        /// Calculates the size of the class instance in bytes, excluding the management information (such as the vtable)
        /// </summary>
        /// <param name="classType">The class type</param>
        /// <returns>A tuple with the size of an instance and the size of the static part of the class</returns>
        private (int Dynamic, int Statics) GetClassSize(Type classType)
        {
            List<FieldInfo> fields = new List<FieldInfo>();
            List<MemberInfo> methods = new List<MemberInfo>();
            GetFields(classType, fields, methods);
            int sizeDynamic = 0;
            int sizeStatic = 0;
            foreach (var f in fields)
            {
                GetVariableType(f.FieldType, out int sizeOfMember);
                if (f.IsStatic)
                {
                    if (classType.IsValueType)
                    {
                        sizeStatic += sizeOfMember;
                    }
                    else
                    {
                        sizeStatic += (sizeOfMember <= 4) ? 4 : 8;
                    }
                }
                else
                {
                    if (classType.IsValueType)
                    {
                        sizeDynamic += sizeOfMember;
                    }
                    else if (f.FieldType.IsValueType)
                    {
                        // Storing a value type field in a (non-value-type) class shall use the value size rounded up to 4 or 8
                        if (sizeOfMember <= 4)
                        {
                            sizeDynamic += 4;
                        }
                        else
                        {
                            if (sizeOfMember % 8 != 0)
                            {
                                sizeOfMember = (sizeOfMember + 8) & ~0x7;
                            }

                            sizeDynamic += sizeOfMember;
                        }
                    }
                    else
                    {
                        sizeDynamic += (sizeOfMember <= 4) ? 4 : 8;
                    }
                }
            }

            if (classType.BaseType != null)
            {
                var baseSizes = GetClassSize(classType.BaseType);
                // Static sizes are not inherited (but do we need to care about accessing a static field via a derived class?)
                sizeDynamic += baseSizes.Dynamic;
            }

            return (sizeDynamic, sizeStatic);
        }

        private bool AddClassDependency(HashSet<Type> allTypes, Type newType)
        {
            return allTypes.Add(newType);
        }

        /// <summary>
        /// Calculates the transitive hull of all types we need to instantiate this class and run its methods
        /// This can be a lengthy list!
        /// </summary>
        private void DetermineBaseAndMembers(HashSet<Type> allTypesToLoad, Type classType)
        {
            if (classType.BaseType != null)
            {
                if (AddClassDependency(allTypesToLoad, classType.BaseType))
                {
                    DetermineBaseAndMembers(allTypesToLoad, classType.BaseType);
                }
            }

            foreach (var t in classType.GetInterfaces())
            {
                if (AddClassDependency(allTypesToLoad, t))
                {
                    DetermineBaseAndMembers(allTypesToLoad, t);
                }
            }

            // This causes a lot of classes to be added, but we'll probably not need them - unless any of their ctors is in the call chain
            // This is detected separately.
            // Maybe we still need any value types involved, though
            ////foreach (var m in classType.GetMembers(BindingFlags.Instance | BindingFlags.Public | BindingFlags.Static | BindingFlags.NonPublic))
            ////{
            ////    if (m is FieldInfo field)
            ////    {
            ////        if (AddClassDependency(allTypesToLoad, field.FieldType))
            ////        {
            ////            DetermineBaseAndMembers(allTypesToLoad, field.FieldType);
            ////        }
            ////    }
            ////}
        }

        private void SendMethodDeclaration(ArduinoMethodDeclaration declaration)
        {
            ClassMember[] localTypes = new ClassMember[declaration.MaxLocals];
            var body = declaration.MethodBase.GetMethodBody();
            int i;
            // This is null in case of a method without implementation (an interface or abstract method). In this case, there are no locals, either
            if (body != null)
            {
                for (i = 0; i < declaration.MaxLocals; i++)
                {
                    var type = GetVariableType(body.LocalVariables[i].LocalType, out var size);
                    ClassMember local = new ClassMember($"Local #{i}", type, 0, (ushort)size);
                    localTypes[i] = local;
                }
            }

            ClassMember[] argTypes = new ClassMember[declaration.ArgumentCount];
            int startOffset = 0;
            // If the method is not static, the fist argument is the "this" pointer, which is not explicitly mentioned in the parameter list. It is of type object
            // for reference types and usually of type reference for value types (but depends whether the method is virtual or not, analyzation of these cases is underway)
            if ((declaration.MethodBase.CallingConvention & CallingConventions.HasThis) != 0)
            {
                startOffset = 1;
                argTypes[0] = new ClassMember($"Argument 0: this", VariableKind.Object, 0, 4);
            }

            var parameters = declaration.MethodBase.GetParameters();
            for (i = startOffset; i < declaration.ArgumentCount; i++)
            {
                var type = GetVariableType(parameters[i - startOffset].ParameterType, out var size);
                ClassMember arg = new ClassMember($"Argument {i}", type, 0, size);
                argTypes[i] = arg;
            }

            // Stopwatch w = Stopwatch.StartNew();
            _board.Firmata.SendMethodDeclaration(declaration.Index, declaration.Token, declaration.Flags, (byte)declaration.MaxStack,
                (byte)declaration.ArgumentCount, declaration.NativeMethod, localTypes, argTypes);

            // _board.Log($"Loading took {w.Elapsed}.");
        }

        /// <summary>
        /// Returns the type of a variable for the IL. This merely distinguishes signed from unsigned types, since
        /// the execution stack auto-extends smaller types.
        /// </summary>
        /// <param name="t">Type to query</param>
        /// <param name="sizeOfMember">Returns the actual size of the member, used for value-type arrays (because byte[] should use just one byte per entry)</param>
        /// <returns></returns>
        internal static VariableKind GetVariableType(Type t, out int sizeOfMember)
        {
            if (t == typeof(sbyte))
            {
                sizeOfMember = 1;
                return VariableKind.Int32;
            }

            if (t == typeof(Int32))
            {
                sizeOfMember = 4;
                return VariableKind.Int32;
            }

            if (t == typeof(UInt32))
            {
                sizeOfMember = 4;
                return VariableKind.Uint32;
            }

            if (t == typeof(Int16))
            {
                sizeOfMember = 2;
                return VariableKind.Int32;
            }

            if (t == typeof(UInt16))
            {
                sizeOfMember = 2;
                return VariableKind.Uint32;
            }

            if (t == typeof(Char))
            {
                sizeOfMember = 2;
                return VariableKind.Uint32;
            }

            if (t == typeof(byte))
            {
                sizeOfMember = 1;
                return VariableKind.Uint32;
            }

            if (t == typeof(bool))
            {
                sizeOfMember = 1;
                return VariableKind.Boolean;
            }

            if (t == typeof(Int64))
            {
                sizeOfMember = 8;
                return VariableKind.Int64;
            }

            if (t == typeof(UInt64))
            {
                sizeOfMember = 8;
                return VariableKind.Uint64;
            }

            if (t == typeof(float))
            {
                sizeOfMember = 4;
                return VariableKind.Float;
            }

            if (t == typeof(double))
            {
                sizeOfMember = 8;
                return VariableKind.Double;
            }

            if (t == typeof(DateTime) || t == typeof(TimeSpan))
            {
                sizeOfMember = 8;
                return VariableKind.Uint64;
            }

            if (t.IsArray)
            {
                var elemType = t.GetElementType();
                if (elemType!.IsValueType)
                {
                    GetVariableType(elemType, out sizeOfMember);
                    return VariableKind.ValueArray;
                }
                else
                {
                    sizeOfMember = SizeOfVoidPointer();
                    return VariableKind.ReferenceArray;
                }
            }

            if (t.IsEnum)
            {
                sizeOfMember = 4;
                return VariableKind.Uint32;
            }

            if (t.IsValueType && !t.IsGenericTypeParameter)
            {
                if (t.IsGenericType)
                {
                    // There are a few special types for which CreateInstance always throws an exception
                    var openType = t.GetGenericTypeDefinition();
                    if (openType.Name.StartsWith("ByReference", StringComparison.Ordinal))
                    {
                        sizeOfMember = 4;
                        return VariableKind.Reference;
                    }

                    // This one is special anyway (and usually explicitly created on the stack using a LOCALLOC instruction)
                    if (openType == typeof(Span<>))
                    {
                        sizeOfMember = SizeOfVoidPointer();
                        return VariableKind.ValueArray;
                    }
                }

                // Calculate class size (Note: Can't use GetClassSize here, as this would be recursive)
                sizeOfMember = 0;
                // If this attribute is given, use its size property (which is applied ie for empty structures)
                var attrib = t.StructLayoutAttribute;
                int attribSize = 0;
                if (attrib != null && attrib.Size > 0)
                {
                    // Minimum size is 4
                    attribSize = Math.Max(attrib.Size, 4);
                }

                foreach (var f in t.GetFields(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance)) // Not the static ones
                {
                    GetVariableType(f.FieldType, out var s);
                    sizeOfMember += s;
                }

                // If the StructLayoutAttribute gives a bigger size than the field combination, use that one. It seems sometimes the field combination is bigger, maybe due to some unioning, but
                // that feature is not supported yet.
                if (attribSize > sizeOfMember)
                {
                    sizeOfMember = attribSize;
                }

                if (sizeOfMember <= 4)
                {
                    sizeOfMember = 4;
                    return VariableKind.Uint32;
                }

                if (sizeOfMember <= 8)
                {
                    sizeOfMember = 8;
                    return VariableKind.Uint64;
                }
                else
                {
                    // Round up to next 4 bytes
                    if ((sizeOfMember & 4) != 0)
                    {
                        sizeOfMember += 3;
                        sizeOfMember = sizeOfMember & ~0x3;
                    }

                    return VariableKind.LargeValueType;
                }
            }

            sizeOfMember = SizeOfVoidPointer();
            return VariableKind.Object;
        }

        /// <summary>
        /// Returns true if the given method shall be internalized (has a native implementation on the arduino)
        /// </summary>
        internal static bool HasArduinoImplementationAttribute(MethodBase method,
            out ArduinoImplementationAttribute? attribute)
        {
            var attribs = method.GetCustomAttributes(typeof(ArduinoImplementationAttribute));
            ArduinoImplementationAttribute? iaMethod = (ArduinoImplementationAttribute?)attribs.SingleOrDefault();
            if (iaMethod != null)
            {
                attribute = iaMethod;
                return true;
            }

            attribute = null;
            return false;
        }

        internal static bool HasIntrinsicAttribute(MethodBase method)
        {
            var attribute = Type.GetType("System.Runtime.CompilerServices.IntrinsicAttribute", true)!;
            var attribs = method.GetCustomAttributes(attribute);
            return attribs.Any();
        }

        internal static bool HasReplacementAttribute(Type type, out ArduinoReplacementAttribute? attribute)
        {
            var repl = type.GetCustomAttribute<ArduinoReplacementAttribute>();
            if (repl != null)
            {
                attribute = repl;
                return true;
            }

            attribute = null!;
            return false;
        }

        public void CollectDependendentMethods(ExecutionSet set, MethodBase methodInfo, HashSet<MethodBase> newMethods)
        {
            List<MethodBase> methodsRequired = new List<MethodBase>();
            List<FieldInfo> fieldsRequired = new List<FieldInfo>();
            List<TypeInfo> typesRequired = new List<TypeInfo>();
            if (methodInfo.IsAbstract)
            {
                // This is a method that will never be called directly, so we can safely skip it.
                // There won't be code for it, anyway.
                return;
            }

            // If this is true, we don't have to parse the implementation
            if (HasArduinoImplementationAttribute(methodInfo, out var attrib) && attrib!.MethodNumber != NativeMethod.None)
            {
                return;
            }

            IlCodeParser.FindAndPatchTokens(set, methodInfo, methodsRequired, typesRequired, fieldsRequired);

            foreach (var method in methodsRequired)
            {
                // Do we need to replace this method?
                set.GetReplacement(method.DeclaringType);
                var finalMethod = set.GetReplacement(method);
                if (finalMethod == null)
                {
                    finalMethod = method;
                }

                if (finalMethod is MethodInfo me)
                {
                    // Ensure we're not scanning the same implementation twice, as this would result
                    // in a stack overflow when a method is recursive (even indirect)
                    if (!set.HasMethod(me) && newMethods.Add(me))
                    {
                        CollectDependendentMethods(set, me, newMethods);
                    }
                }
                else if (finalMethod is ConstructorInfo co)
                {
                    if (!set.HasMethod(co) && newMethods.Add(co))
                    {
                        CollectDependendentMethods(set, co, newMethods);
                    }
                }
                else
                {
                    throw new InvalidOperationException($"Token {method} is not a valid method.");
                }
            }

            /*
            // TODO: Something is very fishy about these... check later.
            // We see in EqualityComparer<T>::get_Default the token 0x0a000a50, but ILDasm says it is 0A000858. None of them
            // match the class field, which is 04001895. Is the mess because this is a static field of a generic type?
            // Similarly, the field tokens we see in the HashTable`1 ctor do not match the class definitions
            foreach (var s in fieldsRequired)
            {
                var resolved = ResolveMember(methodInfo, s);
                if (resolved == null)
                {
                    // Warning only, since might be an incorrect match
                    _board.Log($"Unable to resolve token {s}.");
                    continue;
                }
            }
            */
        }

        public ArduinoTask GetTask(ExecutionSet set, MethodBase methodInfo)
        {
            if (_disposed)
            {
                throw new ObjectDisposedException(nameof(ArduinoCsCompiler));
            }

            if (set.HasMethod(methodInfo))
            {
                var tsk = new ArduinoTask(this, set.GetMethod(methodInfo));
                _activeTasks.Add(tsk);
                return tsk;
            }

            throw new InvalidOperationException($"Method {methodInfo} not loaded");
        }

        private ExecutionSet PrepareProgram(MethodInfo mainEntryPoint, CompilerSettings compilerSettings)
        {
            if (_disposed)
            {
                throw new ObjectDisposedException(nameof(ArduinoCsCompiler));
            }

            ExecutionSet exec;

            if (ExecutionSet.CompiledKernel == null || ExecutionSet.CompiledKernel.CompilerSettings != compilerSettings)
            {
                exec = new ExecutionSet(this, compilerSettings);
                // We never want these types in our execution set - reflection is not supported, except in very specific cases
                exec.SuppressType("System.Reflection.MethodBase");
                exec.SuppressType("System.Reflection.MethodInfo");
                exec.SuppressType("System.Reflection.ConstructorInfo");
                exec.SuppressType("System.Reflection.Module");
                exec.SuppressType("System.Reflection.Assembly");
                exec.SuppressType("System.Reflection.RuntimeAssembly");
                exec.SuppressType("System.Globalization.HebrewNumber");
                exec.SuppressType(typeof(System.Globalization.HebrewCalendar));
                exec.SuppressType(typeof(System.Globalization.JapaneseCalendar));
                exec.SuppressType(typeof(System.Globalization.JapaneseLunisolarCalendar));
                exec.SuppressType(typeof(System.Globalization.ChineseLunisolarCalendar));

                exec.SuppressType("System.Runtime.Serialization.SerializationInfo"); // Serialization is not currently supported

                PrepareLowLevelInterface(exec);
                if (compilerSettings.CreateKernelForFlashing)
                {
                    // Clone the kernel and save as static member
                    ExecutionSet.CompiledKernel = new ExecutionSet(exec, this, compilerSettings);
                }
                else
                {
                    ExecutionSet.CompiledKernel = null;
                }
            }
            else
            {
                // Another clone, to leave the static member alone. Replace the compiler in that kernel with the current one.
                exec = new ExecutionSet(ExecutionSet.CompiledKernel, this, compilerSettings);
            }

            if (mainEntryPoint.DeclaringType != null)
            {
                PrepareClass(exec, mainEntryPoint.DeclaringType);
            }

            PrepareCodeInternal(exec, mainEntryPoint, null);

            exec.MainEntryPointInternal = mainEntryPoint;
            FinalizeExecutionSet(exec);
            return exec;
        }

        /// <summary>
        /// Creates and loads an execution set (a program to be executed on a remote microcontroller)
        /// </summary>
        /// <typeparam name="T">The type of the main entry method. Typically something like <code>Func{int, int, int}</code></typeparam>
        /// <param name="mainEntryPoint">The main entry method for the program</param>
        /// <returns>The execution set. Use it's <see cref="ExecutionSet.MainEntryPoint"/> property to get a callable reference to the remote code.</returns>
        /// <exception cref="Exception">This may throw exceptions in case the execution of some required static constructors (type initializers) fails.</exception>
        public ExecutionSet CreateExecutionSet<T>(T mainEntryPoint)
            where T : Delegate
        {
            return CreateExecutionSet(mainEntryPoint, new CompilerSettings());
        }

        /// <summary>
        /// Creates and loads an execution set (a program to be executed on a remote microcontroller)
        /// </summary>
        /// <typeparam name="T">The type of the main entry method. Typically something like <code>Func{int, int, int}</code></typeparam>
        /// <param name="mainEntryPoint">The main entry method for the program</param>
        /// <param name="settings">Custom compiler settings</param>
        /// <returns>The execution set. Use it's <see cref="ExecutionSet.MainEntryPoint"/> property to get a callable reference to the remote code.</returns>
        /// <exception cref="Exception">This may throw exceptions in case the execution of some required static constructors (type initializers) fails.</exception>
        public ExecutionSet CreateExecutionSet<T>(T mainEntryPoint, CompilerSettings settings)
        where T : Delegate
        {
            var exec = PrepareProgram(mainEntryPoint.Method, settings);
            try
            {
                exec.Load();
            }
            catch (Exception)
            {
                ClearAllData(true);
                throw;
            }

            return exec;
        }

        /// <summary>
        /// Creates and loads an execution set (a program to be executed on a remote microcontroller).
        /// </summary>
        /// <param name="mainEntryPoint">The main entry method for the program</param>
        /// <param name="settings">Custom compiler settings</param>
        /// <returns>The execution set. Use it's <see cref="ExecutionSet.MainEntryPoint"/> property to get a callable reference to the remote code.</returns>
        /// <exception cref="Exception">This may throw exceptions in case the execution of some required static constructors (type initializers) fails.</exception>
        public ExecutionSet CreateExecutionSet(MethodInfo mainEntryPoint, CompilerSettings settings)
        {
            var exec = PrepareProgram(mainEntryPoint, settings);
            try
            {
                exec.Load();
            }
            catch (Exception)
            {
                ClearAllData(true);
                throw;
            }

            return exec;
        }

        internal void PrepareCodeInternal(ExecutionSet set, MethodBase methodInfo, ArduinoMethodDeclaration? parent)
        {
            // Ensure the class is known, if it needs replacement
            var classReplacement = set.GetReplacement(methodInfo.DeclaringType);
            MethodBase? replacement = set.GetReplacement(methodInfo);
            if (classReplacement != null && replacement == null)
            {
                // See below, this is the fix for it
                replacement = set.GetReplacement(methodInfo, classReplacement);
            }

            if (replacement != null)
            {
                methodInfo = replacement;
            }

            if (set.HasMethod(methodInfo))
            {
                return;
            }

            if (classReplacement != null && replacement == null)
            {
                // If the class requires full replacement, all methods must be replaced (or throw an error inside GetReplacement, if it is not defined), but it must
                // never return null. Seen during development, because generic parameter types did not match.
                throw new InvalidOperationException($"Internal error: The class {classReplacement} should fully replace {methodInfo.DeclaringType}, however method {methodInfo} has no replacement (and no error either)");
            }

            if (HasArduinoImplementationAttribute(methodInfo, out var implementation) && implementation!.MethodNumber != NativeMethod.None)
            {
                int tk1 = set.GetOrAddMethodToken(methodInfo);
                var newInfo1 = new ArduinoMethodDeclaration(tk1, methodInfo, parent, MethodFlags.SpecialMethod, implementation!.MethodNumber);
                set.AddMethod(newInfo1);
                return;
            }

            if (HasIntrinsicAttribute(methodInfo))
            {
                // If the method is marked with [Intrinsic] (an internal attribute supporting the JIT compiler), we need to check whether it requires special handling as well.
                // We cannot use the normal replacement technique with generic classes such as ByReference<T>, because Type.GetType doesn't allow open generic classes.
                if (methodInfo.Name == ".ctor" && methodInfo.DeclaringType!.Name == "ByReference`1")
                {
                    int tk1 = set.GetOrAddMethodToken(methodInfo);
                    var newInfo1 = new ArduinoMethodDeclaration(tk1, methodInfo, parent, MethodFlags.SpecialMethod, NativeMethod.ByReferenceCtor);
                    set.AddMethod(newInfo1);
                    return;
                }

                if (methodInfo.Name == "get_Value" && methodInfo.DeclaringType!.Name == "ByReference`1")
                {
                    int tk1 = set.GetOrAddMethodToken(methodInfo);
                    var newInfo1 = new ArduinoMethodDeclaration(tk1, methodInfo, parent, MethodFlags.SpecialMethod, NativeMethod.ByReferenceValue);
                    set.AddMethod(newInfo1);
                    return;
                }
            }

            var body = methodInfo.GetMethodBody();
            bool hasBody = !methodInfo.IsAbstract;

            var ilBytes = body?.GetILAsByteArray()!.ToArray();

            bool constructedCode = false;
            MethodFlags construcedFlags = MethodFlags.None;
            if (body == null && !methodInfo.IsAbstract)
            {
                Type multicastType = typeof(MulticastDelegate);
                if (multicastType.IsAssignableFrom(methodInfo.DeclaringType))
                {
                    // The compiler inserts auto-generated code for the methods of the specific delegate.
                    // We generate this code here.
                    hasBody = true;
                    if (methodInfo.IsConstructor)
                    {
                        // find the matching constructor in MulticastDelegate. Actually, we're not using a real constructor, but a method that acts on behalf of it
                        var methods = multicastType.GetMethods(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance);
                        var baseCtor = methods.Single(x => x.Name == "CtorClosedStatic"); // Implementation is same for static and instance, except for a null test

                        // Make sure this stub method is in memory
                        PrepareCodeInternal(set, baseCtor, parent);
                        int token = baseCtor.MetadataToken;

                        // the code we need to generate is
                        // LDARG.0
                        // LDARG.1
                        // LDARG.2
                        // CALL MulticastDelegate.baseCtor // with the original ctor token!
                        // RET
                        byte[] code = new byte[]
                        {
                            02, // LDARG.0
                            03, // LDARG.1
                            04, // LDARG.2
                            0x28, // CALL
                            (byte)(token & 0xFF),
                            (byte)((token >> 8) & 0xFF),
                            (byte)((token >> 16) & 0xFF),
                            (byte)((token >> 24) & 0xFF),
                            0x2A, // RET
                        };
                        ilBytes = code;
                        constructedCode = true;
                        construcedFlags = MethodFlags.Ctor;
                    }
                    else
                    {
                        var args = methodInfo.GetParameters();
                        Type t = methodInfo.DeclaringType!;
                        var methodDetail = (MethodInfo)methodInfo;
                        var targetField = t.GetField("_target", BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance)!;
                        var methodPtrField = t.GetField("_methodPtr", BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance)!;
                        List<byte> code = new List<byte>();
                        int numargs = args.Length;
                        if (methodInfo.IsStatic)
                        {
                            throw new InvalidOperationException("The Invoke() method of a delegate cannot be static");
                        }

                        code.Add((byte)OpCode.CEE_LDARG_0); // This is the this pointer of the delegate. We need to get its _target and _methodPtr references

                        // Leaves the target object on the stack (null for static methods). We'll have to decide in the EE whether we need it or not (meaning whether
                        // the actual target is static or not)
                        AddCallWithToken(code, OpCode.CEE_LDFLD, targetField.MetadataToken);

                        // Push all remaining arguments to the stack -> they'll be the arguments to the method
                        for (int i = 1; i < numargs; i++)
                        {
                            code.Add((byte)OpCode.CEE_LDARG_S);
                            code.Add((byte)i);
                        }

                        code.Add((byte)OpCode.CEE_LDARG_0);

                        // Leaves the target (of type method ptr) on the stack. This shall be the final argument to the calli instruction
                        AddCallWithToken(code, OpCode.CEE_LDFLD, methodPtrField.MetadataToken);

                        AddCallWithToken(code, OpCode.CEE_CALLI, 0); // The argument is irrelevant, the EE knows the calling convention to the target method, and we hope it matches

                        code.Add((byte)OpCode.CEE_RET);
                        ilBytes = code.ToArray();
                        constructedCode = true;
                        construcedFlags = MethodFlags.Virtual;
                        if (methodDetail.ReturnType == typeof(void))
                        {
                            construcedFlags |= MethodFlags.Void;
                        }
                    }
                }
                else
                {
                    // TODO: There are a bunch of methods currently getting here because they're not implemented
                    // throw new MissingMethodException($"{methodInfo.DeclaringType}.{methodInfo} has no implementation");
                    _board.Log($"Error: {methodInfo.DeclaringType} - {methodInfo} has no visible implementation");
                    return;
                }
            }

            if (ilBytes == null && hasBody)
            {
                throw new MissingMethodException($"{methodInfo.DeclaringType} - {methodInfo} has no visible implementation");
            }

            List<MethodBase> foreignMethodsRequired = new List<MethodBase>();
            List<FieldInfo> fieldsRequired = new List<FieldInfo>();
            List<TypeInfo> typesRequired = new List<TypeInfo>();

            if (ilBytes != null && ilBytes.Length > Math.Pow(2, 14) - 1)
            {
                throw new InvalidProgramException($"Max IL size of real time method is 2^14 Bytes. Actual size is {ilBytes.Length}.");
            }

            if (hasBody)
            {
                ilBytes = IlCodeParser.FindAndPatchTokens(set, methodInfo, ilBytes!, foreignMethodsRequired, typesRequired, fieldsRequired);

                foreach (var type in typesRequired.Distinct())
                {
                    if (!set.HasDefinition(type))
                    {
                        PrepareClass(set, type);
                    }
                }
            }

            int tk = set.GetOrAddMethodToken(methodInfo);

            ArduinoMethodDeclaration newInfo;
            if (constructedCode)
            {
                newInfo = new ArduinoMethodDeclaration(tk, methodInfo, parent, construcedFlags, 0, Math.Max(8, methodInfo.GetParameters().Length + 3), ilBytes);
            }
            else
            {
                newInfo = new ArduinoMethodDeclaration(tk, methodInfo, parent, ilBytes);
            }

            if (set.AddMethod(newInfo))
            {
                _board.Log($"Method {methodInfo.DeclaringType} - {methodInfo} added to the execution set");
                // If the class containing this method contains statics, we need to send its declaration
                // TODO: Parse code to check for LDSFLD or STSFLD instructions and skip if none found.
                if (methodInfo.DeclaringType != null && GetClassSize(methodInfo.DeclaringType).Statics > 0)
                {
                    PrepareClass(set, methodInfo.DeclaringType);
                }

                HashSet<MethodBase> methods = new HashSet<MethodBase>();

                CollectDependendentMethods(set, methodInfo, methods);

                var list = methods.ToList();
                for (var index = 0; index < list.Count; index++)
                {
                    var dep = list[index];
                    // If we have a ctor in the call chain we need to ensure we have its class loaded.
                    // This happens if the created object is only used in local variables but not as a class member
                    // seen so far.
                    if (dep.IsConstructor && dep.DeclaringType != null)
                    {
                        PrepareClass(set, dep.DeclaringType);
                    }
                    else if (dep.DeclaringType != null && HasStaticFields(dep.DeclaringType))
                    {
                        // Also load the class declaration if it contains static fields.
                        // TODO: We currently assume that no class is accessing static fields of another class.
                        PrepareClass(set, dep.DeclaringType);
                    }

                    PrepareCodeInternal(set, dep, newInfo);
                }
            }
        }

        private void AddCallWithToken(List<byte> code, OpCode opCode, int token)
        {
            if ((int)opCode < 0x100)
            {
                code.Add((byte)opCode);
            }
            else
            {
                code.Add(254);
                code.Add((byte)opCode);
            }

            code.Add((byte)(token & 0xFF));
            code.Add((byte)((token >> 8) & 0xFF));
            code.Add((byte)((token >> 16) & 0xFF));
            code.Add((byte)((token >> 24) & 0xFF));
        }

        private static bool HasStaticFields(Type cls)
        {
            foreach (var fld in cls.GetFields())
            {
                if (fld.IsStatic)
                {
                    return true;
                }
            }

            return false;
        }

        private void SendMethod(ExecutionSet set, ArduinoMethodDeclaration decl)
        {
            SendMethodDeclaration(decl);
            if (decl.HasBody && decl.NativeMethod == NativeMethod.None)
            {
                _board.Firmata.SendMethodIlCode(decl.Index, decl.IlBytes!);
            }
        }

        internal void ExecuteStaticCtors(ExecutionSet set)
        {
            List<ClassDeclaration> classes = set.Classes.Where(x => !x.SuppressInit && x.TheType.TypeInitializer != null).ToList();
            // We need to figure out dependencies between the cctors (i.e. we know that System.Globalization.JapaneseCalendar..ctor depends on System.DateTime..cctor)
            // For now, we just do that by "knowledge" (analyzing the code manually showed these dependencies)
            BringToFront(classes, typeof(Stopwatch));
            BringToFront(classes, GetSystemPrivateType("System.Collections.Generic.NonRandomizedStringEqualityComparer"));
            BringToFront(classes, typeof(System.DateTime));
            for (var index = 0; index < classes.Count; index++)
            {
                ClassDeclaration? cls = classes[index];
                if (!cls.SuppressInit && cls.TheType.TypeInitializer != null)
                {
                    _board.Log($"Running static initializer of {cls}. Step {index + 1}/{classes.Count}...");
                    var task = GetTask(set, cls.TheType.TypeInitializer);
                    task.Invoke(CancellationToken.None);
                    task.WaitForResult();
                    if (task.GetMethodResults(set, out _, out var state) == false || state != MethodState.Stopped)
                    {
                        throw new InvalidProgramException($"Error executing static ctor of class {cls.TheType}");
                    }
                }
            }
        }

        private void BringToFront(List<ClassDeclaration> classes, Type type)
        {
            if (type == null)
            {
                throw new ArgumentNullException(nameof(type));
            }

            int idx = classes.FindIndex(x => x.TheType == type);
            if (idx < 0)
            {
                return;
            }

            var temp = classes[idx];
            classes[idx] = classes[0];
            classes[0] = temp;
        }

        /// <summary>
        /// The two methods have the same name and signature (that means one can be replaced with another or one can override another)
        /// </summary>
        public static bool MethodsHaveSameSignature(MethodBase a, MethodBase b)
        {
            // A ctor can never match an ordinary method or the other way round
            if (a.GetType() != b.GetType())
            {
                return false;
            }

            if (a.IsStatic != b.IsStatic)
            {
                return false;
            }

            if (a.Name != b.Name)
            {
                return false;
            }

            var argsa = a.GetParameters();
            var argsb = b.GetParameters();

            if (argsa.Length != argsb.Length)
            {
                return false;
            }

            if ((HasArduinoImplementationAttribute(a, out var attrib) && attrib!.CompareByParameterNames) ||
                (HasArduinoImplementationAttribute(b, out attrib) && attrib!.CompareByParameterNames))
            {
                for (int i = 0; i < argsa.Length; i++)
                {
                    if (argsa[i].Name != argsb[i].Name)
                    {
                        return false;
                    }
                }

                return true;
            }

            for (int i = 0; i < argsa.Length; i++)
            {
                if (!AreSameParameterTypes(argsa[i].ParameterType, argsb[i].ParameterType))
                {
                    return false;
                }
            }

            return true;
        }

        private static bool AreSameParameterTypes(Type parameterA, Type parameterB)
        {
            if (parameterA == parameterB)
            {
                return true;
            }

            // FullName is null for generic type arguments, since they have no namespace
            if (parameterA.FullName == parameterB.FullName && parameterA.Name == parameterB.Name)
            {
                return true;
            }

            // UintPtr/IntPtr have a platform specific width, that means they're different whether we run in 32 bit or in 64 bit mode on the local(!) computer.
            // But since we know that the target platform is 32 bit, we can assume them to be equal
            if (parameterA == typeof(UIntPtr) && parameterB == typeof(uint))
            {
                return true;
            }

            if (parameterA == typeof(IntPtr) && parameterB == typeof(int))
            {
                return true;
            }

            if (parameterA == typeof(uint) && parameterB == typeof(UIntPtr))
            {
                return true;
            }

            if (parameterA == typeof(int) && parameterB == typeof(IntPtr))
            {
                return true;
            }

            return false;
        }

        /// <summary>
        /// Returns true if the two methods denote the same operator.
        /// We need to handle this a bit special because it is not possible to declare i.e. operator==(Type a, Type b) outside "Type" if we want to replace it.
        /// </summary>
        public static bool AreSameOperatorMethods(MethodBase a, MethodBase b)
        {
            // A ctor can never match an ordinary method or the other way round
            if (a.GetType() != b.GetType())
            {
                return false;
            }

            if (a.Name != b.Name)
            {
                return false;
            }

            if (a.IsStatic != b.IsStatic)
            {
                return false;
            }

            var argsa = a.GetParameters();
            var argsb = b.GetParameters();

            if (argsa.Length != argsb.Length)
            {
                return false;
            }

            // Same name and named "op_*". These are both operators, so we decide they're equal.
            // Note that this is not necessarily true, because it is possible to define two op_equality members with different argument sets,
            // but this is very discouraged and is hopefully not the case in the System libs.
            if (a.Name.StartsWith("op_"))
            {
                return true;
            }

            return false;
        }

        /// <summary>
        /// Executes the given method with the provided arguments asynchronously
        /// </summary>
        /// <remarks>Argument count/type not checked yet</remarks>
        /// <param name="method">Handle to method to invoke.</param>
        /// <param name="arguments">Argument list</param>
        internal void Invoke(MethodBase method, params object[] arguments)
        {
            if (_activeExecutionSet == null)
            {
                throw new InvalidOperationException("No execution set loaded");
            }

            var decl = _activeExecutionSet.GetMethod(method);
            _board.Log($"Starting execution on {decl}...");
            _board.Firmata.ExecuteIlCode(decl.Index, arguments);
        }

        public void KillTask(MethodBase methodInfo)
        {
            if (_activeExecutionSet == null)
            {
                throw new InvalidOperationException("No execution set loaded");
            }

            var decl = _activeExecutionSet.GetMethod(methodInfo);

            _board.Firmata.SendKillTask(decl.Index);
        }

        public static Type GetSystemPrivateType(string typeName)
        {
            var ret = Type.GetType(typeName);
            if (ret == null)
            {
                throw new InvalidOperationException($"Type {typeName} not found");
            }

            return ret;
        }

        /// <summary>
        /// Clears all execution data from the arduino, so that the memory is freed again.
        /// </summary>
        /// <param name="force">True to also kill the current task. If false and code is being executed, nothing happens.</param>
        /// <param name="includingFlash">Clear the flash, so a complete new kernel can be loaded</param>
        public void ClearAllData(bool force, bool includingFlash = false)
        {
            if (includingFlash)
            {
                _board.Log("Erasing flash.");
                _board.Firmata.ClearFlash();
            }

            _board.Log("Resetting execution engine.");
            _board.Firmata.SendIlResetCommand(force);
            _activeTasks.Clear();
            _activeExecutionSet = null;
        }

        public void Dispose()
        {
            _board.SetCompilerCallback(null!);
        }

        public void SetExecutionSetActive(ExecutionSet executionSet)
        {
            if (_activeExecutionSet != null)
            {
                throw new InvalidOperationException("An execution set is already active. Perform a clear first");
            }

            _activeExecutionSet = executionSet;
        }

        public void CopyToFlash()
        {
            _board.Firmata.CopyToFlash();
        }

        /// <summary>
        /// Returns true if the given kernel snapshot is already installed on the board.
        /// </summary>
        /// <param name="snapShot">Kernel snapshot to verify</param>
        /// <returns>True if the given snapshot is loaded, false if either no kernel is loaded or its checksum doesn't match</returns>
        public bool BoardHasKernelLoaded(ExecutionSet.SnapShot snapShot)
        {
            return _board.Firmata.IsMatchingFirmwareLoaded(DataVersion, snapShot.GetHashCode());
        }

        public void WriteFlashHeader(ExecutionSet.SnapShot snapShot)
        {
            _board.Firmata.WriteFlashHeader(DataVersion, snapShot.GetHashCode());
        }
    }
}
