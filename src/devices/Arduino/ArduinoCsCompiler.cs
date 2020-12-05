using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Reflection;
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
        SetMethodTokens = 2,
        LoadIl = 3,
        StartTask = 4,
        ResetExecutor = 5,
        KillTask = 6,
        MethodSignature = 7,
        ClassDeclaration = 8,
        SendObject = 9,
        ConstantData = 10,

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
        VoidOrCtor = 8, // The method returns void or is a ctor (which only implicitly returns "this")
        Abstract = 16, // The method is abstract (or an interface stub)
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
        OutOfMemory = 3
    }

    public enum VariableKind : byte
    {
        Void = 0,
        Uint32 = 1,
        Int32 = 2,
        Boolean = 3,
        Object = 4,
        Method = 5,
        ValueArray = 6,
        ReferenceArray = 7,
        StaticMember = 8 // type is defined by the first value it gets
    }

    public enum SystemException
    {
        None = 0,
        StackOverflow = 1,
        NullReference = 2,
        MissingMethod = 3,
        InvalidOpCode = 4,
        DivideByZero = 5,
        IndexOutOfRange = 6,
        OutOfMemory = 7,
        ArrayTypeMismatch = 8,
        InvalidOperation = 9,
    }

    public sealed class ArduinoCsCompiler : IDisposable
    {
        private readonly List<Type> _rootClasses = new List<Type>()
        {
            typeof(System.Object), typeof(System.Type), typeof(System.String),
            typeof(Array), typeof(Monitor), typeof(Exception),
        };

        // These classes substitute (part of) a framework class
        private readonly List<Type> _replacementClasses = new List<Type>()
        {
            typeof(MiniObject), typeof(MiniArray), typeof(MiniString), typeof(MiniMonitor),
            typeof(MiniException), typeof(MiniHashSet<int>), typeof(MiniEqualityComparer<int>), typeof(MiniThread),
            typeof(MiniEnvironment), typeof(MiniRuntimeHelpers)
        };

        /// <summary>
        /// A list of known classes whose static ctor should not be run (because it is currently unsupported and
        /// its implementation not needed/patched)
        /// </summary>
        private readonly List<Type> _staticInitializersToSuppress = new List<Type>()
        {
            typeof(EqualityComparer<>), typeof(EqualityComparer<int>), typeof(Type),
        };

        private readonly ArduinoBoard _board;
        private readonly Dictionary<MethodBase, ArduinoMethodDeclaration> _methodInfos;
        private readonly List<IArduinoTask> _activeTasks;

        public ArduinoCsCompiler(ArduinoBoard board, bool resetExistingCode = true)
        {
            _board = board;
            _methodInfos = new Dictionary<MethodBase, ArduinoMethodDeclaration>();
            _board.SetCompilerCallback(BoardOnCompilerCallback);

            _activeTasks = new List<IArduinoTask>();

            if (resetExistingCode)
            {
                ClearAllData(true);
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

        private void BoardOnCompilerCallback(int codeReference, MethodState state, int[] args)
        {
            object[] outObjects = new object[args.Length];
            var codeRef = _methodInfos.Values.FirstOrDefault(x => x.Index == codeReference);
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
                // Still update the task state, this will prevent a deadlock if somebody is waiting for this task to end
                task.AddData(state, args.Cast<object>().ToArray());
                return;
            }

            if (state == MethodState.Killed)
            {
                _board.Log($"Execution of method {GetMethodName(codeRef)} was forcibly terminated.");
                // Still update the task state, this will prevent a deadlock if somebody is waiting for this task to end
                task.AddData(state, new object[0]);
                return;
            }

            if (state == MethodState.Stopped)
            {
                object retVal;
                int inVal = (int)args[0]; // initially, the list contains only ints
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
                    retVal = inVal != 0;
                }
                else if (returnType == typeof(UInt32))
                {
                    retVal = (uint)inVal;
                }
                else
                {
                    retVal = inVal;
                }

                outObjects[0] = retVal;
            }

            task.AddData(state, outObjects);
        }

        public ExecutionSet CreateExecutionSet()
        {
            return new ExecutionSet(this);
        }

        public void PrepareLowLevelInterface(ExecutionSet set)
        {
            Type lowLevelInterface = typeof(IArduinoHardwareLevelAccess);
            foreach (var method in lowLevelInterface.GetMethods())
            {
                if (!_methodInfos.ContainsKey(method))
                {
                    var attr = (ArduinoImplementationAttribute)method.GetCustomAttributes(typeof(ArduinoImplementationAttribute)).First();

                    int token = set.GetOrAddMethodToken(method);
                    ArduinoMethodDeclaration decl = new ArduinoMethodDeclaration(token, method, MethodFlags.SpecialMethod, attr.MethodNumber);
                    // Todo: Move this member to the ExecutionSet
                    _methodInfos.Add(method, decl);
                    set.AddMethod(decl);
                }
            }

            // And the internal classes
            foreach (var replacement in _replacementClasses)
            {
                var attribs = replacement.GetCustomAttributes(typeof(ArduinoReplacementAttribute));
                ArduinoReplacementAttribute ia = (ArduinoReplacementAttribute)attribs.Single();
                if (ia.ReplaceEntireType)
                {
                    PrepareClass(set, replacement);
                    set.AddReplacementType(ia.TypeToReplace, replacement, ia.IncludingSubclasses);
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
                            var methodToReplace = ia.TypeToReplace!.GetMethods(BindingFlags.Public | BindingFlags.Instance | BindingFlags.Static).SingleOrDefault(x => MethodsHaveSameSignature(x, m));
                            if (methodToReplace == null)
                            {
                                throw new InvalidOperationException("A replacement method has nothing to replace");
                            }

                            set.AddReplacementMethod(methodToReplace, m);
                        }
                    }
                }
            }
        }

        public void PrepareClass(ExecutionSet set, Type classType)
        {
            if (!ValueTypeSupported(classType))
            {
                throw new NotSupportedException("Value types with sizeof(Type) > sizeof(int32) not supported");
            }

            HashSet<Type> baseTypes = new HashSet<Type>();

            baseTypes.Add(classType);
            DetermineBaseAndMembers(baseTypes, classType);

            foreach (var cls in baseTypes.Where(x => x.IsArray == false))
            {
                PrepareClassDeclaration(set, cls);
            }
        }

        private bool ValueTypeSupported(Type classType)
        {
            // TODO: Should be sizeof(Variant)
            // return !(classType.IsValueType && GetClassSize(classType).Dynamic > sizeof(int));
            return true;
        }

        private void PrepareClassDeclaration(ExecutionSet set, Type classType)
        {
            if (set.HasDefinition(classType))
            {
                return;
            }

            if (classType.ContainsGenericParameters)
            {
                // Don't inspect unresolved generic types
                // TODO: Need to add some magic if these are really instantiated dynamically somewhere
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

            List<ExecutionSet.VariableOrMethod> memberTypes = new List<ExecutionSet.VariableOrMethod>();
            for (var index = 0; index < fields.Count; index++)
            {
                var fieldType = GetVariableType(fields[index].FieldType, out _);
                if (fields[index].IsStatic)
                {
                    fieldType = VariableKind.StaticMember;
                }

                var newvar = new ExecutionSet.VariableOrMethod(fieldType, set.GetOrAddFieldToken(fields[index]), new List<int>());
                if (fields[index].IsStatic)
                {
                    var t = fields[index].DeclaringType;
                    if (t == null)
                    {
                        throw new InvalidOperationException("field without a class???");
                    }
                    else if (t.GenericTypeArguments.Length == 0)
                    {
                        newvar.InitialValue = fields[index].GetValue(null);
                    }
                    else
                    {
                        // If this is a static field of a generic class, we have to go trough the class to obtain its value, since each concrete type has a different
                        // instance of the field
                        string fName = fields[index].Name;
                        if (fName.Contains("<")) // A backing field - use its get accessor instead
                        {
                            fName = fName.Substring(1, fName.IndexOf(">", StringComparison.Ordinal) - 1);
                            var prop = t.GetProperty(fName);
                            newvar.InitialValue = prop?.GetValue(null, BindingFlags.Public | BindingFlags.NonPublic, null, null, CultureInfo.InvariantCulture);
                        }
                        else
                        {
                            var fld = t.GetField(fName, BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Static);
                            newvar.InitialValue = fld?.GetValue(null);
                        }
                    }
                }

                memberTypes.Add(newvar);
            }

            for (var index = 0; index < methods.Count; index++)
            {
                var m = methods[index] as ConstructorInfo;
                if (m != null)
                {
                    memberTypes.Add(new ExecutionSet.VariableOrMethod(VariableKind.Method, set.GetOrAddMethodToken(m), new List<int>()));
                }
            }

            var sizeOfClass = GetClassSize(classType);

            // Add this first, so we break the recursion to this class further down
            var newClass = new ExecutionSet.Class(classType, sizeOfClass.Dynamic, sizeOfClass.Statics, set.GetOrAddClassToken(classType.GetTypeInfo()), memberTypes);
            set.AddClass(newClass);
        }

        /// <summary>
        /// Complete the execution set by making sure all dependencies are resolved
        /// </summary>
        internal void FinalizeExecutionSet(ExecutionSet set)
        {
            // This list might change during iteration
            for (var i = 0; i < set.Classes.Count; i++)
            {
                var cls = set.Classes[i];
                List<FieldInfo> fields = new List<FieldInfo>();
                List<MemberInfo> methods = new List<MemberInfo>();

                GetFields(cls.Cls, fields, methods);
                string name = cls.Cls.Name;
                for (var index = 0; index < methods.Count; index++)
                {
                    var m = methods[index];
                    if (MemberLinkRequired(set, m, out var baseMethodInfos))
                    {
                        // If this method is required because base implementations are called, we also need its implementation (obviously)
                        // Unfortunately, this can recursively require further classes and methods
                        PrepareCodeInternal(set, (MethodBase)m); // This cast must work

                        List<int> baseTokens = baseMethodInfos.Select(x => set.GetOrAddMethodToken(x)).ToList();
                        cls.Members.Add(new ExecutionSet.VariableOrMethod(VariableKind.Method, set.GetOrAddMethodToken((MethodBase)m), baseTokens));
                    }
                }
            }

            // Last step: Of all classes in the list, load their static cctors
            for (var i = 0; i < set.Classes.Count; i++)
            {
                // Let's hope the list no more changes, but in theory we don't know (however, creating static ctors that
                // depend on other classes might give a big problem)
                var cls = set.Classes[i];
                var cctor = cls.Cls.TypeInitializer;
                if (cctor == null)
                {
                    continue;
                }

                if (_staticInitializersToSuppress.Contains(cls.Cls))
                {
                    cls.SuppressInit = true;
                    continue;
                }

                PrepareCodeInternal(set, cctor);
            }
        }

        internal void SendClassDeclarations(ExecutionSet set)
        {
            foreach (var c in set.Classes)
            {
                var cls = c.Cls;
                Int32 parentToken = 0;
                Type parent = cls.BaseType!;
                if (parent != null)
                {
                    parentToken = set.GetOrAddClassToken(parent.GetTypeInfo());
                }

                // Extend token with assembly identifier, to make sure it is unique
                int token = set.GetOrAddClassToken(cls.GetTypeInfo());

                // separated for debugging purposes (the debugger cannot evaluate Type.ToString() on a conditional breakpoint)
                string className = cls.Name;

                _board.Log($"Sending class declaration for {className} (Token 0x{token:x8}). Number of members: {c.Members.Count}, Dynamic size {c.DynamicSize} Bytes, Static Size {c.StaticSize} Bytes.");
                _board.Firmata.SendClassDeclaration(token, parentToken, (c.DynamicSize, c.StaticSize), cls.IsValueType, c.Members);
            }
        }

        public void SendConstants(IEnumerable<(int Token, byte[]? InitializerData)> constElements)
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
            foreach (var me in set.Methods())
            {
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

        private static void CollectBaseImplementations(MethodInfo method, List<MethodInfo> methodsBeingImplemented)
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
        /// Returns the size of the variable on the target platform. This is currently the largest of sizeof(void*) and sizeof(int32).
        /// TODO: Extend when 64-Bit variables become available (double on SAM3X based boards is 64 bit, DateTime and TimeSpan also require 64 bit)
        /// </summary>
        /// <returns>See above</returns>
        private int SizeOfVariableField()
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
                var varType = GetVariableType(f.FieldType, out int sizeOfMember);
                // Currently, this is always true
                if (varType == VariableKind.Boolean || varType == VariableKind.Int32 || varType == VariableKind.Uint32 || varType == VariableKind.Object ||
                    varType == VariableKind.ValueArray || varType == VariableKind.ReferenceArray)
                {
                    if (f.IsStatic)
                    {
                        sizeStatic += SizeOfVariableField();
                    }
                    else
                    {
                        if (classType.IsValueType)
                        {
                            sizeDynamic += sizeOfMember;
                        }
                        else
                        {
                            sizeDynamic += SizeOfVariableField();
                        }
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
            // No support for structs right now. And basic value types (such as int) are always declared.
            if (!ValueTypeSupported(newType))
            {
                return false;
            }

            // If any of these are found, we add them once, but we don't search further
            if (_rootClasses.Contains(newType))
            {
                allTypes.Add(newType);
                return false;
            }

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
            byte[] localTypes = new byte[declaration.MaxLocals];
            var body = declaration.MethodBase.GetMethodBody();
            int i;
            // This is null in case of a method without implementation (an interface or abstract method). In this case, there are no locals, either
            if (body != null)
            {
                for (i = 0; i < declaration.MaxLocals; i++)
                {
                    localTypes[i] = (byte)GetVariableType(body.LocalVariables[i].LocalType, out _);
                }
            }

            byte[] argTypes = new byte[declaration.ArgumentCount];
            int startOffset = 0;
            // If the method is not static, the fist argument is the "this" pointer, which is not explicitly mentioned in the parameter list. It is always of type object.
            if ((declaration.MethodBase.CallingConvention & CallingConventions.HasThis) != 0)
            {
                startOffset = 1;
                argTypes[0] = (byte)VariableKind.Object;
            }

            for (i = startOffset; i < declaration.ArgumentCount; i++)
            {
                argTypes[i] = (byte)GetVariableType(declaration.MethodBase.GetParameters()[i - startOffset].ParameterType, out _);
            }

            _board.Firmata.SendMethodDeclaration(declaration.Index, declaration.Token, declaration.Flags, (byte)declaration.MaxLocals, (byte)declaration.ArgumentCount, declaration.NativeMethod, localTypes, argTypes);
        }

        /// <summary>
        /// Returns the type of a variable for the IL. This merely distinguishes signed from unsigned types, since
        /// the execution stack auto-extends smaller types.
        /// </summary>
        /// <param name="t">Type to query</param>
        /// <param name="sizeOfMember">Returns the actual size of the member, used for value-type arrays (because byte[] should use just one byte per entry)</param>
        /// <returns></returns>
        private VariableKind GetVariableType(Type t, out int sizeOfMember)
        {
            if (t == typeof(Int16))
            {
                sizeOfMember = 2;
                return VariableKind.Int32;
            }

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

            if (t == typeof(UInt16))
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
                    sizeOfMember = SizeOfVariableField();
                    return VariableKind.ReferenceArray;
                }
            }

            sizeOfMember = SizeOfVariableField();
            return VariableKind.Object;
        }

        /// <summary>
        /// Returns true if the given method shall be internalized (has a native implementation on the arduino)
        /// </summary>
        private bool HasArduinoImplementationAttribute(MethodBase method)
        {
            var attribs = method.GetCustomAttributes(typeof(ArduinoImplementationAttribute));
            ArduinoImplementationAttribute? iaMethod = (ArduinoImplementationAttribute?)attribs.SingleOrDefault();
            if (iaMethod != null && iaMethod.MethodNumber != ArduinoImplementation.None)
            {
                return true;
            }

            return false;
        }

        public void CollectDependencies(ExecutionSet set, MethodBase methodInfo, HashSet<MethodBase> newMethods)
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
            if (HasArduinoImplementationAttribute(methodInfo))
            {
                return;
            }

            GetMethodDependencies(set, methodInfo, methodsRequired, typesRequired, fieldsRequired);

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
                        CollectDependencies(set, me, newMethods);
                    }
                }
                else if (finalMethod is ConstructorInfo co)
                {
                    if (!set.HasMethod(co) && newMethods.Add(co))
                    {
                        CollectDependencies(set, co, newMethods);
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
            if (_methodInfos.ContainsKey(methodInfo))
            {
                // Nothing to do, already loaded
                var tsk = new ArduinoTask(this, _methodInfos[methodInfo]);
                _activeTasks.Add(tsk);
                return tsk;
            }

            throw new InvalidOperationException($"Method {methodInfo} not loaded");
        }

        public ExecutionSet PrepareProgram<T>(Type mainClass, T mainEntryPoint)
            where T : Delegate
        {
            var exec = CreateExecutionSet();
            PrepareLowLevelInterface(exec);
            PrepareClass(exec, mainClass);
            PrepareCodeInternal(exec, mainEntryPoint.Method);

            exec.MainEntryPointInternal = mainEntryPoint.Method;
            _board.Log($"Estimated program memory usage before finalization: {exec.EstimateRequiredMemory()} bytes.");
            FinalizeExecutionSet(exec);
            _board.Log($"Estimated program memory usage: {exec.EstimateRequiredMemory()} bytes.");
            return exec;
        }

        public ArduinoTask PrepareAndLoadSimpleMethod<T>(T mainEntryPoint)
            where T : Delegate
        {
            if (mainEntryPoint == null)
            {
                throw new ArgumentNullException(nameof(mainEntryPoint));
            }

            var set = CreateExecutionSet();
            PrepareCodeInternal(set, mainEntryPoint.Method);
            _board.Log($"Estimated program memory usage before finalization: {set.EstimateRequiredMemory()} bytes.");
            FinalizeExecutionSet(set);
            _board.Log($"Estimated program memory usage: {set.EstimateRequiredMemory()} bytes.");
            set.MainEntryPointInternal = mainEntryPoint.Method;
            set.Load();
            return GetTask(set, mainEntryPoint.Method);
        }

        public ArduinoTask AddSimpleMethod<T>(ExecutionSet set, T mainEntryPoint)
            where T : Delegate
        {
            if (mainEntryPoint == null)
            {
                throw new ArgumentNullException(nameof(mainEntryPoint));
            }

            PrepareCodeInternal(set, mainEntryPoint.Method);
            _board.Log($"Estimated program memory usage before finalization: {set.EstimateRequiredMemory()} bytes.");
            FinalizeExecutionSet(set);
            _board.Log($"Estimated program memory usage: {set.EstimateRequiredMemory()} bytes.");
            set.MainEntryPointInternal = mainEntryPoint.Method;
            set.Load();
            return GetTask(set, mainEntryPoint.Method);
        }

        public ArduinoTask AddSimpleMethod(ExecutionSet set, MethodInfo mainEntryPoint)
        {
            if (mainEntryPoint == null)
            {
                throw new ArgumentNullException(nameof(mainEntryPoint));
            }

            PrepareCodeInternal(set, mainEntryPoint);
            _board.Log($"Estimated program memory usage before finalization: {set.EstimateRequiredMemory()} bytes.");
            FinalizeExecutionSet(set);
            _board.Log($"Estimated program memory usage: {set.EstimateRequiredMemory()} bytes.");
            set.MainEntryPointInternal = mainEntryPoint;
            set.Load();
            return GetTask(set, mainEntryPoint);
        }

        public void PrepareCodeInternal(ExecutionSet set, MethodBase methodInfo)
        {
            /* if (set.GetReplacement(methodInfo.DeclaringType!) != null)
            {
                throw new InvalidOperationException($"{methodInfo.DeclaringType} - {methodInfo} should have been replaced.");
            }*/

            // Ensure the class is known, if it needs replacement
            set.GetReplacement(methodInfo.DeclaringType);
            MethodBase? replacement = set.GetReplacement(methodInfo);
            if (replacement != null)
            {
                methodInfo = replacement;
                if (set.HasMethod(methodInfo))
                {
                    return;
                }
            }

            var body = methodInfo.GetMethodBody();
            if (body == null && !methodInfo.IsAbstract)
            {
                // throw new MissingMethodException($"{methodInfo.DeclaringType}.{methodInfo} has no implementation");
                _board.Log($"Error: {methodInfo.DeclaringType} - {methodInfo} has no visible implementation");
                return;
            }

            bool hasBody = !methodInfo.IsAbstract;

            var ilBytes = body?.GetILAsByteArray();
            if (ilBytes == null && hasBody)
            {
                throw new MissingMethodException($"{methodInfo.DeclaringType} has no visible implementation");
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
                ilBytes = GetMethodDependencies(set, methodInfo, foreignMethodsRequired, typesRequired, fieldsRequired);

                foreach (var type in typesRequired.Distinct())
                {
                    if (!set.HasDefinition(type))
                    {
                        PrepareClass(set, type);
                    }
                }
            }

            int tk = set.GetOrAddMethodToken(methodInfo);
            var newInfo = new ArduinoMethodDeclaration(tk, methodInfo, new List<(int First, int Second)>(), ilBytes);

            if (set.AddMethod(newInfo))
            {
                _board.Log($"Method {methodInfo.DeclaringType} - {methodInfo} added to the execution set");
                // If the class containing this method contains statics, we need to send its declaration
                // TODO: Parse code to check for LDSFLD or STSFLD instructions and skip if none found.
                if (methodInfo.DeclaringType != null && GetClassSize(methodInfo.DeclaringType).Statics > 0)
                {
                    PrepareClass(set, methodInfo.DeclaringType);
                }

                PrepareDependencies(set, methodInfo);
            }
        }

        private void PrepareDependencies(ExecutionSet set, MethodBase method)
        {
            HashSet<MethodBase> methods = new HashSet<MethodBase>();

            CollectDependencies(set, method, methods);

            var list = methods.ToList();
            for (var index = 0; index < list.Count; index++)
            {
                var dep = list[index];
                // If we have a ctor in the call chain we need to ensure we have its class loaded.
                // This happens if the created object is only used in local variables but not as a class member
                // seen so far.
                if (dep.IsConstructor && dep.DeclaringType != null && ValueTypeSupported(dep.DeclaringType))
                {
                    PrepareClass(set, dep.DeclaringType);
                }
                else if (dep.DeclaringType != null && HasStaticFields(dep.DeclaringType))
                {
                    // Also load the class declaration if it contains static fields.
                    // TODO: We currently assume that no class is accessing static fields of another class.
                    PrepareClass(set, dep.DeclaringType);
                }

                PrepareCodeInternal(set, dep);
            }
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
            MethodBase methodInfo = decl.MethodBase;
            _methodInfos.Add(methodInfo, decl);
            _board.Log($"Method Index {decl.Index} (NewToken 0x{decl.Token:X}) is named {methodInfo.DeclaringType} - {methodInfo.Name}.");
            SendMethodDeclaration(decl);
            if (decl.TokenMap != null)
            {
                SendTokenMap(decl.Index, decl.TokenMap);
            }

            if (decl.HasBody && decl.NativeMethod == ArduinoImplementation.None)
            {
                _board.Firmata.SendMethodIlCode(decl.Index, decl.IlBytes!);
            }
        }

        private void SendTokenMap(int codeReference, List<(int Foreign, int Own)> tokenMap)
        {
            if (tokenMap.Count == 0)
            {
                return;
            }

            int[] data = new int[tokenMap.Count * 2];
            int idx = 0;
            foreach (var entry in tokenMap)
            {
                data[idx] = entry.Foreign;
                data[idx + 1] = entry.Own;
                idx += 2;
            }

            _board.Firmata.SendTokenMap(codeReference, data);
        }

        internal void ExecuteStaticCtors(ExecutionSet set)
        {
            foreach (var cls in set.Classes)
            {
                if (!cls.SuppressInit && cls.Cls.TypeInitializer != null)
                {
                    var task = GetTask(set, cls.Cls.TypeInitializer);
                    task.Invoke(CancellationToken.None);
                    task.WaitForResult();
                    if (task.GetMethodResults(set, out _, out var state) == false || state != MethodState.Stopped)
                    {
                        throw new InvalidProgramException($"Error executing static ctor of class {cls.Cls}");
                    }
                }
            }
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

            for (int i = 0; i < argsa.Length; i++)
            {
                if (argsa[i].ParameterType != argsb[i].ParameterType)
                {
                    return false;
                }
            }

            return true;
        }

        /// <summary>
        /// Executes the given method with the provided arguments asynchronously
        /// </summary>
        /// <remarks>Argument count/type not checked yet</remarks>
        /// <param name="method">Handle to method to invoke.</param>
        /// <param name="arguments">Argument list</param>
        internal void Invoke(MethodBase method, params object[] arguments)
        {
            if (!_methodInfos.TryGetValue(method, out var decl))
            {
                throw new InvalidOperationException("Method must be loaded first.");
            }

            _board.Log($"Starting execution on {decl}...");
            _board.Firmata.ExecuteIlCode(decl.Index, arguments);
        }

        public void KillTask(MethodBase methodInfo)
        {
            if (!_methodInfos.TryGetValue(methodInfo, out var decl))
            {
                throw new InvalidOperationException("No such method known.");
            }

            _board.Firmata.SendKillTask(decl.Index);
        }

        private byte[]? GetMethodDependencies(ExecutionSet set, MethodBase methodInstance, List<MethodBase> methodsUsed, List<TypeInfo> typesUsed, List<FieldInfo> fieldsUsed)
        {
            if (methodInstance.ContainsGenericParameters)
            {
                throw new InvalidProgramException("No generics supported");
            }

            MethodBody? body = methodInstance.GetMethodBody();
            if (body == null)
            {
                // Method has no (visible) implementation, so it certainly has no code dependencies as well
                return null;
            }

            /* if (body.ExceptionHandlingClauses.Count > 0)
            {
                throw new InvalidProgramException("Methods with exception handling are not supported");
            } */

            IlCodeParser parser = new IlCodeParser();
            return parser.FindAndPatchTokens(set, methodInstance, body, methodsUsed, typesUsed, fieldsUsed);
        }

        /// <summary>
        /// Clears all execution data from the arduino, so that the memory is freed again.
        /// </summary>
        /// <param name="force">True to also kill the current task. If false and code is being executed, nothing happens.</param>
        public void ClearAllData(bool force)
        {
            _board.Firmata.SendIlResetCommand(force);
            _activeTasks.Clear();
            _methodInfos.Clear();
        }

        public void Dispose()
        {
            _board.SetCompilerCallback(null!);
        }
    }
}
