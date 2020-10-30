using System;
using System.Reflection;

#pragma warning disable CS1591
namespace Iot.Device.Arduino
{
    public sealed class ArduinoMethodDeclaration
    {
        public ArduinoMethodDeclaration(int index, MethodInfo methodInfo)
        {
            Index = index;
            MethodInfo = methodInfo;
            Flags = MethodFlags.None;
            var body = methodInfo.GetMethodBody();
            Token = methodInfo.MetadataToken;
            MaxLocals = body.LocalVariables.Count;
            MaxStack = body.MaxStackSize;
            ArgumentCount = methodInfo.GetParameters().Length;
            if (methodInfo.CallingConvention.HasFlag(CallingConventions.HasThis))
            {
                ArgumentCount += 1;
            }

            if (methodInfo.IsStatic)
            {
                Flags |= MethodFlags.Static;
            }

            if (methodInfo.IsVirtual)
            {
                Flags |= MethodFlags.Virtual;
            }

            if (methodInfo.ReturnParameter == null || methodInfo.ReturnParameter.ParameterType == typeof(void))
            {
                Flags |= MethodFlags.Void;
            }
        }

        public ArduinoMethodDeclaration(int index, int token, MethodInfo methodInfo, MethodFlags flags, int maxStack)
        {
            Index = index;
            Token = token;
            MethodInfo = methodInfo;
            Flags = flags;
            MaxLocals = MaxStack = maxStack;
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
        public MethodInfo MethodInfo { get; }

        public MethodFlags Flags
        {
            get;
        }

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
