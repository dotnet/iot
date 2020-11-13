using System;
using System.Collections.Generic;
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
        Void = 8,
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

    internal enum VariableKind : byte
    {
        Void = 0,
        Uint32 = 1,
        Int32 = 2,
        Boolean = 3,
        Object = 4,
        Method = 5,
    }

    public sealed class ArduinoCsCompiler : IDisposable
    {
        private readonly List<Type> _rootClasses = new List<Type>()
        {
            typeof(System.Object), typeof(System.Type), typeof(System.String),
            typeof(Array)
        };

        private readonly ArduinoBoard _board;
        private readonly Dictionary<MethodBase, ArduinoMethodDeclaration> _methodInfos;
        private readonly List<IArduinoTask> _activeTasks;

        private int _numDeclaredMethods;

        public ArduinoCsCompiler(ArduinoBoard board, bool resetExistingCode = true)
        {
            _board = board;
            _numDeclaredMethods = 0;
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

        private void BoardOnCompilerCallback(int codeReference, MethodState state, object[] args)
        {
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
                task.AddData(state, new object[0]);
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
                Type returnType = codeRef.MethodInfo!.ReturnType;
                if (returnType == typeof(void))
                {
                    args = new object[0]; // Empty return set
                    task.AddData(state, args);
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

                args[0] = retVal;
            }

            task.AddData(state, args);
        }

        public ArduinoTask<T> LoadCode<T>(T method)
            where T : Delegate
        {
            return LoadCode<T>(method.Method);
        }

        private MemberInfo? ResolveMember(MethodBase method, int metadataToken)
        {
            Type type = method.DeclaringType!;
            Type[] typeArgs = new Type[0];
            Type[] methodArgs = new Type[0];

            if (type.IsGenericType || type.IsGenericTypeDefinition)
            {
                typeArgs = type.GetGenericArguments();
            }

            if (method.IsGenericMethod || method.IsGenericMethodDefinition)
            {
                methodArgs = method.GetGenericArguments();
            }

            try
            {
                return type.Module.ResolveMember(metadataToken, typeArgs, methodArgs);
            }
            catch (ArgumentException)
            {
                // Due to our simplistic parsing below, we might find matching metadata tokens that aren't really tokens
                return null;
            }
        }

        public void LoadLowLevelInterface()
        {
            Type lowLevelInterface = typeof(IArduinoHardwareLevelAccess);
            foreach (var method in lowLevelInterface.GetMethods())
            {
                if (!_methodInfos.ContainsKey(method))
                {
                    var attr = (ArduinoImplementationAttribute)method.GetCustomAttributes(typeof(ArduinoImplementationAttribute)).First();

                    ArduinoMethodDeclaration decl = new ArduinoMethodDeclaration(_numDeclaredMethods++, method.MetadataToken, method, MethodFlags.SpecialMethod, attr.MethodNumber);
                    _methodInfos.Add(method, decl);
                    LoadMethodDeclaration(decl);
                }
            }

            // Also load the core methods
            LoadCode(new Action<IArduinoHardwareLevelAccess, int>(ArduinoRuntimeCore.Sleep));
        }

        public void LoadClass(Type classType)
        {
            if (classType.IsValueType)
            {
                throw new NotSupportedException("Only reference types supported");
            }

            HashSet<Type> allTypesToLoad = new HashSet<Type>();

            // The method may also collect base types (such as int or bool). We'll drop them later
            DetermineReferencedClasses(allTypesToLoad, classType);

            HashSet<Type> baseTypes = new HashSet<Type>();

            baseTypes.Add(classType);
            DetermineBaseAndMembers(baseTypes, classType);

            foreach (var cls in baseTypes.Where(x => x.IsValueType == false && x.IsArray == false))
            {
                SendClassDeclaration(cls);
            }
        }

        private void SendClassDeclaration(Type classType)
        {
            List<FieldInfo> fields = new List<FieldInfo>();
            List<MethodInfo> methods = new List<MethodInfo>();
            GetFields(classType, fields, methods);

            List<(VariableKind, Int32)> memberTypes = new List<(VariableKind, Int32)>();
            for (var index = 0; index < fields.Count; index++)
            {
                var fieldType = GetVariableType(fields[index].FieldType);
                memberTypes.Add((fieldType, fields[index].MetadataToken));
            }

            for (var index = 0; index < methods.Count; index++)
            {
                memberTypes.Add((VariableKind.Method, methods[index].MetadataToken));
            }

            Int32 parentToken = 0;
            Type parent = classType.BaseType!;
            if (parent != null)
            {
                parentToken = parent.MetadataToken;
            }

            short sizeOfClass = (short)GetClassSize(classType);

            sizeOfClass += 4; // vtable

            _board.Firmata.SendClassDeclaration(classType.MetadataToken, parentToken, sizeOfClass, memberTypes);
        }

        private static void GetFields(Type classType, List<FieldInfo> fields, List<MethodInfo> methods)
        {
            foreach (var m in classType.GetMembers(BindingFlags.Instance | BindingFlags.Public | BindingFlags.Static |
                                                   BindingFlags.NonPublic))
            {
                if (m is FieldInfo field)
                {
                    fields.Add(field);
                }
                else if (m is MethodInfo method)
                {
                    // TODO: Do we really need all, or is the ctor sufficient?
                    methods.Add(method);
                }
            }
        }

        private int GetClassSize(Type classType)
        {
            List<FieldInfo> fields = new List<FieldInfo>();
            List<MethodInfo> methods = new List<MethodInfo>();
            GetFields(classType, fields, methods);
            int size = 0;
            foreach (var f in fields)
            {
                var varType = GetVariableType(f.FieldType);
                if (varType != VariableKind.Method)
                {
                    // TODO: Need to query some properties from the board (i.e. sizeof(void*))
                    size += 4;
                }
            }

            if (classType.BaseType != null)
            {
                size += GetClassSize(classType.BaseType);
            }

            return size;
        }

        private bool AddClassDependency(HashSet<Type> allTypes, Type newType)
        {
            // No support for structs right now. And basic value types (such as int) are always declared.
            if (newType.IsValueType)
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
        private void DetermineReferencedClasses(HashSet<Type> allTypesToLoad, Type classType)
        {
            if (classType.BaseType != null)
            {
                if (AddClassDependency(allTypesToLoad, classType.BaseType))
                {
                    DetermineReferencedClasses(allTypesToLoad, classType.BaseType);
                }
            }

            foreach (var t in classType.GetInterfaces())
            {
                if (AddClassDependency(allTypesToLoad, t))
                {
                    DetermineReferencedClasses(allTypesToLoad, t);
                }
            }

            foreach (var m in classType.GetMembers(BindingFlags.Instance | BindingFlags.Public | BindingFlags.Static | BindingFlags.NonPublic))
            {
                if (m is FieldInfo field)
                {
                    if (AddClassDependency(allTypesToLoad, field.FieldType))
                    {
                        DetermineReferencedClasses(allTypesToLoad, field.FieldType);
                    }
                }
                else if (m is MethodInfo method)
                {
                    foreach (var argument in method.GetParameters())
                    {
                        if (AddClassDependency(allTypesToLoad, argument.ParameterType))
                        {
                            DetermineReferencedClasses(allTypesToLoad, argument.ParameterType);
                        }
                    }

                    var il = method.GetMethodBody();
                    if (il == null)
                    {
                        continue;
                    }

                    foreach (var argument in il.LocalVariables)
                    {
                        if (AddClassDependency(allTypesToLoad, argument.LocalType))
                        {
                            DetermineReferencedClasses(allTypesToLoad, argument.LocalType);
                        }
                    }
                }
            }
        }

        /// <summary>
        /// Similar to the above, but only returns parent classes/interfaces and member types.
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

            foreach (var m in classType.GetMembers(BindingFlags.Instance | BindingFlags.Public | BindingFlags.Static | BindingFlags.NonPublic))
            {
                if (m is FieldInfo field)
                {
                    if (AddClassDependency(allTypesToLoad, field.FieldType))
                    {
                        DetermineBaseAndMembers(allTypesToLoad, field.FieldType);
                    }
                }
            }
        }

        private void LoadMethodDeclaration(ArduinoMethodDeclaration declaration)
        {
            byte[] localTypes = new byte[declaration.MaxLocals];
            var body = declaration.MethodBase.GetMethodBody();
            int i;
            // This is null in case of a method without implementation (an interface or abstract method). In this case, there are no locals, either
            if (body != null)
            {
                for (i = 0; i < declaration.MaxLocals; i++)
                {
                    localTypes[i] = (byte)GetVariableType(body.LocalVariables[i].LocalType);
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
                argTypes[i] = (byte)GetVariableType(declaration.MethodBase.GetParameters()[i - startOffset].ParameterType);
            }

            _board.Firmata.SendMethodDeclaration((byte)declaration.Index, declaration.Token, declaration.Flags, (byte)declaration.MaxLocals, (byte)declaration.ArgumentCount, localTypes, argTypes);
        }

        private VariableKind GetVariableType(Type t)
        {
            if (t == typeof(Int32) || t == typeof(Int16) || t == typeof(sbyte))
            {
                return VariableKind.Int32;
            }

            if (t == typeof(UInt32) || t == typeof(UInt16) || t == typeof(byte))
            {
                return VariableKind.Uint32;
            }

            if (t == typeof(bool))
            {
                return VariableKind.Boolean;
            }

            return VariableKind.Object;
        }

        public List<MethodBase> CollectDependencies(MethodBase methodInfo)
        {
            List<int> foreignMethodsRequired = new List<int>();
            List<int> ownMethodsRequired = new List<int>();
            List<MethodBase> ret = new List<MethodBase>();
            GetMethodDependencies(methodInfo, foreignMethodsRequired, ownMethodsRequired);
            List<int> combined = foreignMethodsRequired;
            combined.AddRange(ownMethodsRequired);
            combined = combined.Distinct().ToList();
            foreach (var method in combined)
            {
                var resolved = ResolveMember(methodInfo, method);
                if (resolved == null)
                {
                    // Warning only, since might be an incorrect match
                    _board.Log($"Unable to resolve token {method}.");
                    continue;
                }

                if (resolved is MethodInfo me)
                {
                    ret.Add(me);
                }
                else if (resolved is ConstructorInfo co)
                {
                    ret.Add(co);
                }
                else
                {
                    _board.Log($"Token {method} is not a MethodInfo token, but a {resolved.GetType()}.");
                }
            }

            return ret;
        }

        public ArduinoTask<T> LoadCode<T>(MethodBase methodInfo)
            where T : Delegate
        {
            var body = methodInfo.GetMethodBody();
            if (body == null)
            {
                throw new MissingMethodException($"{methodInfo.DeclaringType} has no implementation");
            }

            var ilBytes = body.GetILAsByteArray();
            if (ilBytes == null)
            {
                throw new MissingMethodException($"{methodInfo.DeclaringType} has no implementation");
            }

            List<int> foreignMethodsRequired = new List<int>();
            List<int> ownMethodsRequired = new List<int>();
            // Maps methodDef to memberRef tokens (for methods declared outside the assembly of the executing code)
            Dictionary<int, int> tokenMap = new Dictionary<int, int>();
            if (ilBytes.Length > Math.Pow(2, 14) - 1)
            {
                throw new InvalidProgramException($"Max IL size of real time method is 2^14 Bytes. Actual size is {ilBytes.Length}.");
            }

            if (_methodInfos.ContainsKey(methodInfo))
            {
                // Nothing to do, already loaded
                var tsk = new ArduinoTask<T>(this, _methodInfos[methodInfo]);
                _activeTasks.Add(tsk);
                return tsk;
            }

            GetMethodDependencies(methodInfo, foreignMethodsRequired, ownMethodsRequired);

            foreach (int token in foreignMethodsRequired.Distinct())
            {
                var resolved = ResolveMember(methodInfo, token);
                if (resolved == null)
                {
                    continue;
                }

                tokenMap.Add(resolved.MetadataToken, token);
            }

            if (_numDeclaredMethods >= 255)
            {
                // In practice, the maximum will be much less on most Arduino boards
                throw new NotSupportedException("To many methods declared. Only 255 supported.");
            }

            var newInfo = new ArduinoMethodDeclaration(_numDeclaredMethods++, methodInfo);
            _methodInfos.Add(methodInfo, newInfo);

            _board.Log($"Method Index {newInfo.Index} is named {methodInfo.Name}.");
            LoadMethodDeclaration(newInfo);
            LoadTokenMap((byte)newInfo.Index, tokenMap);
            _board.Firmata.SendMethodIlCode((byte)newInfo.Index, ilBytes);

            var ret = new ArduinoTask<T>(this, newInfo);
            _activeTasks.Add(ret);
            return ret;
        }

        private void LoadTokenMap(byte codeReference, Dictionary<int, int> tokenMap)
        {
            if (tokenMap.Count == 0)
            {
                return;
            }

            int[] data = new int[tokenMap.Count * 2];
            int idx = 0;
            foreach (var entry in tokenMap)
            {
                data[idx] = entry.Key;
                data[idx + 1] = entry.Value;
                idx += 2;
            }

            _board.Firmata.SendTokenMap(codeReference, data);
        }

        /// <summary>
        /// Executes the given method with the provided arguments asynchronously
        /// </summary>
        /// <remarks>Argument count/type not checked yet</remarks>
        /// <param name="method">Handle to method to invoke.</param>
        /// <param name="arguments">Argument list</param>
        internal void Invoke(MethodInfo method, params object[] arguments)
        {
            if (!_methodInfos.TryGetValue(method, out var decl))
            {
                throw new InvalidOperationException("Method must be loaded first.");
            }

            _board.Firmata.ExecuteIlCode((byte)decl.Index, arguments);
        }

        public void KillTask(MethodInfo methodInfo)
        {
            if (!_methodInfos.TryGetValue(methodInfo, out var decl))
            {
                throw new InvalidOperationException("No such method known.");
            }

            _board.Firmata.SendKillTask((byte)decl.Index);
        }

        private void GetMethodDependencies(MethodBase methodInstance, List<int> foreignMethodTokens, List<int> ownMethodTokens)
        {
            if (methodInstance.ContainsGenericParameters)
            {
                throw new InvalidProgramException("No generics supported");
            }

            MethodBody? body = methodInstance.GetMethodBody();
            if (body == null)
            {
                throw new InvalidProgramException($"Method {methodInstance.Name} has no implementation.");
            }

            if (body.ExceptionHandlingClauses.Count > 0)
            {
                throw new InvalidProgramException("Methods with exception handling are not supported");
            }

            // TODO: Check argument count, Check parameter types, etc., etc.
            var byteCode = body.GetILAsByteArray();
            if (byteCode == null)
            {
                throw new InvalidProgramException("Method has no implementation");
            }

            if (byteCode.Length >= ushort.MaxValue - 1)
            {
                // If you hit this limit, some refactoring should be considered...
                throw new InvalidProgramException("Maximum method size is 64kb");
            }

            // TODO: This is very simplistic so we do not need another parser. But this might have false positives
            int idx = 0;
            while (idx < byteCode.Length - 5)
            {
                // Decode token first (number is little endian!)
                int token = byteCode[idx + 1] | byteCode[idx + 2] << 8 | byteCode[idx + 3] << 16 | byteCode[idx + 4] << 24;
                if ((byteCode[idx] == 0x6F || byteCode[idx] == 0x28 || (byteCode[idx] == 0x73)) && (token >> 24 == 0x0A))
                {
                    // The tokens we're interested in have the form 0x0A XX XX XX preceded by a call, callvirt or newinst instruction
                    foreignMethodTokens.Add(token);
                }

                if ((byteCode[idx] == 0x6F || byteCode[idx] == 0x28 || (byteCode[idx] == 0x73)) && (token >> 24 == 0x06))
                {
                    // Call to another method of the same assembly
                    ownMethodTokens.Add(token);
                }

                idx++;
            }
        }

        /// <summary>
        /// Clears all execution data from the arduino, so that the memory is freed again.
        /// </summary>
        /// <param name="force">True to also kill the current task. If false and code is being executed, nothing happens.</param>
        public void ClearAllData(bool force)
        {
            _board.Firmata.SendIlResetCommand(force);
            _numDeclaredMethods = 0;
            _activeTasks.Clear();
            _methodInfos.Clear();
        }

        public void Dispose()
        {
            _board.SetCompilerCallback(null!);
        }
    }
}
