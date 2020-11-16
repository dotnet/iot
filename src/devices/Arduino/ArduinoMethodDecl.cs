using System;
using System.Reflection;

#pragma warning disable CS1591
namespace Iot.Device.Arduino
{
    public sealed class ArduinoMethodDeclaration
    {
        public ArduinoMethodDeclaration(int index, MethodBase methodBase)
        {
            Index = index;
            MethodBase = methodBase;
            Flags = MethodFlags.None;
            var body = methodBase.GetMethodBody();
            if (body == null)
            {
                throw new InvalidOperationException("Use this ctor only for methods that have a body");
            }

            Token = methodBase.MetadataToken;
            MaxLocals = body.LocalVariables.Count;
            MaxStack = body.MaxStackSize;
            NativeMethod = ArduinoImplementation.None;
            ArgumentCount = methodBase.GetParameters().Length;
            if (methodBase.CallingConvention.HasFlag(CallingConventions.HasThis))
            {
                ArgumentCount += 1;
            }

            if (methodBase.IsStatic)
            {
                Flags |= MethodFlags.Static;
            }

            if (methodBase.IsVirtual)
            {
                Flags |= MethodFlags.Virtual;
            }

            if (methodBase is MethodInfo methodInfo)
            {
                if (methodInfo.ReturnParameter == null || methodInfo.ReturnParameter.ParameterType == typeof(void))
                {
                    Flags |= MethodFlags.Void;
                }
            }
        }

        public ArduinoMethodDeclaration(int index, int token, MethodInfo methodInfo, MethodFlags flags, ArduinoImplementation nativeMethod)
        {
            Index = index;
            Token = token;
            MethodBase = methodInfo;
            Flags = flags;
            NativeMethod = nativeMethod;
            MaxLocals = MaxStack = 0;
            ArgumentCount = methodInfo.GetParameters().Length;
            if (methodInfo.CallingConvention.HasFlag(CallingConventions.HasThis))
            {
                ArgumentCount += 1;
            }

            if (methodInfo.ReturnParameter.ParameterType == typeof(void))
            {
                Flags |= MethodFlags.Void;
            }
        }

        public int Index { get; }
        public int Token { get; }
        public MethodBase MethodBase { get; }

        public MethodInfo MethodInfo
        {
            get
            {
                if (MethodBase is MethodInfo info)
                {
                    return info;
                }

                // Can't directly call ctors or fields
                throw new InvalidOperationException("Not callable Method");
            }
        }

        public MethodFlags Flags
        {
            get;
        }

        public ArduinoImplementation NativeMethod { get; }

        public int MaxLocals
        {
            get;
        }

        public int MaxStack
        {
            get;
        }

        public int ArgumentCount
        {
            get;
        }
    }
}
