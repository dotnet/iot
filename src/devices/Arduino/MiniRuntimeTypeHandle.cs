using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;

#pragma warning disable SA1405
#pragma warning disable SA1503
#pragma warning disable SA1513

namespace Iot.Device.Arduino
{
    internal enum CorElementType : byte
    {
        ELEMENT_TYPE_END = 0x00,
        ELEMENT_TYPE_VOID = 0x01,
        ELEMENT_TYPE_BOOLEAN = 0x02,
        ELEMENT_TYPE_CHAR = 0x03,
        ELEMENT_TYPE_I1 = 0x04,
        ELEMENT_TYPE_U1 = 0x05,
        ELEMENT_TYPE_I2 = 0x06,
        ELEMENT_TYPE_U2 = 0x07,
        ELEMENT_TYPE_I4 = 0x08,
        ELEMENT_TYPE_U4 = 0x09,
        ELEMENT_TYPE_I8 = 0x0A,
        ELEMENT_TYPE_U8 = 0x0B,
        ELEMENT_TYPE_R4 = 0x0C,
        ELEMENT_TYPE_R8 = 0x0D,
        ELEMENT_TYPE_STRING = 0x0E,
        ELEMENT_TYPE_PTR = 0x0F,
        ELEMENT_TYPE_BYREF = 0x10,
        ELEMENT_TYPE_VALUETYPE = 0x11,
        ELEMENT_TYPE_CLASS = 0x12,
        ELEMENT_TYPE_VAR = 0x13,
        ELEMENT_TYPE_ARRAY = 0x14,
        ELEMENT_TYPE_GENERICINST = 0x15,
        ELEMENT_TYPE_TYPEDBYREF = 0x16,
        ELEMENT_TYPE_I = 0x18,
        ELEMENT_TYPE_U = 0x19,
        ELEMENT_TYPE_FNPTR = 0x1B,
        ELEMENT_TYPE_OBJECT = 0x1C,
        ELEMENT_TYPE_SZARRAY = 0x1D,
        ELEMENT_TYPE_MVAR = 0x1E,
        ELEMENT_TYPE_CMOD_REQD = 0x1F,
        ELEMENT_TYPE_CMOD_OPT = 0x20,
        ELEMENT_TYPE_INTERNAL = 0x21,
        ELEMENT_TYPE_MAX = 0x22,
        ELEMENT_TYPE_MODIFIER = 0x40,
        ELEMENT_TYPE_SENTINEL = 0x41,
        ELEMENT_TYPE_PINNED = 0x45,
    }

    [ArduinoReplacement("System.RuntimeTypeHandle", "System.Private.CoreLib.dll", true, false, IncludingPrivates = true)]
    // Note: Be aware that this replaces a value type
    internal struct MiniRuntimeTypeHandle : ISerializable
    {
        // Returns handle for interop with EE. The handle is guaranteed to be non-null.
        internal MiniRuntimeTypeHandle GetNativeHandle()
        {
            // Create local copy to avoid a race condition
            Type type = m_type;
            if (type == null)
                throw new ArgumentNullException();
            return new MiniRuntimeTypeHandle(type);
        }

        // Returns type for interop with EE. The type is guaranteed to be non-null.
        internal Type GetTypeChecked()
        {
            // Create local copy to avoid a race condition
            Type type = m_type;
            if (type == null)
                throw new ArgumentNullException(null);
            return type;
        }

        internal static bool IsInstanceOfType(Type type, object? o)
        {
            throw new NotImplementedException();
        }

        internal static Type GetTypeHelper(Type typeStart, Type[]? genericArgs, IntPtr pModifiers, int cModifiers)
        {
            Type type = typeStart;

            if (genericArgs != null)
            {
                type = type.MakeGenericType(genericArgs);
            }

            if (cModifiers > 0)
            {
                throw new NotImplementedException();
                ////int* arModifiers = (int*)pModifiers.ToPointer();
                ////for (int i = 0; i < cModifiers; i++)
                ////{
                ////    if ((CorElementType)Marshal.ReadInt32((IntPtr)arModifiers, i * sizeof(int)) == CorElementType.ELEMENT_TYPE_PTR)
                ////        type = type.MakePointerType();
                ////    else if ((CorElementType)Marshal.ReadInt32((IntPtr)arModifiers, i * sizeof(int)) == CorElementType.ELEMENT_TYPE_BYREF)
                ////        type = type.MakeByRefType();
                ////    else if ((CorElementType)Marshal.ReadInt32((IntPtr)arModifiers, i * sizeof(int)) == CorElementType.ELEMENT_TYPE_SZARRAY)
                ////        type = type.MakeArrayType();
                ////    else
                ////        type = type.MakeArrayType(Marshal.ReadInt32((IntPtr)arModifiers, ++i * sizeof(int)));
                ////}
            }

            return type;
        }

        public static bool operator ==(MiniRuntimeTypeHandle left, object? right) => left.Equals(right);

        public static bool operator ==(object? left, MiniRuntimeTypeHandle right) => right.Equals(left);

        public static bool operator !=(MiniRuntimeTypeHandle left, object? right) => !left.Equals(right);

        public static bool operator !=(object? left, MiniRuntimeTypeHandle right) => !right.Equals(left);

        // This is the RuntimeType for the type
#pragma warning disable SA1307 // Original name
        internal Type m_type;
#pragma warning restore SA1307

        public override int GetHashCode()
        {
            return m_type != null ? m_type.GetHashCode() : 0;
        }

        public override bool Equals(object? obj)
        {
            if (!(obj is MiniRuntimeTypeHandle))
                return false;

            MiniRuntimeTypeHandle handle = (MiniRuntimeTypeHandle)obj;
            return handle.m_type == m_type;
        }

        public bool Equals(MiniRuntimeTypeHandle handle)
        {
            return handle.m_type == m_type;
        }

        public IntPtr Value
        {
            [ArduinoImplementation(ArduinoImplementation.RuntimeTypeHandleValue)]
            get
            {
                throw new NotImplementedException();
            }
        }

        internal static IntPtr GetValueInternal(RuntimeTypeHandle handle)
        {
            throw new NotImplementedException();
        }

        internal MiniRuntimeTypeHandle(Type type)
        {
            m_type = type;
        }

        internal static bool IsTypeDefinition(Type type)
        {
            CorElementType corElemType = GetCorElementType(type);
            if (!((corElemType >= CorElementType.ELEMENT_TYPE_VOID && corElemType < CorElementType.ELEMENT_TYPE_PTR) ||
                  corElemType == CorElementType.ELEMENT_TYPE_VALUETYPE ||
                  corElemType == CorElementType.ELEMENT_TYPE_CLASS ||
                  corElemType == CorElementType.ELEMENT_TYPE_TYPEDBYREF ||
                  corElemType == CorElementType.ELEMENT_TYPE_I ||
                  corElemType == CorElementType.ELEMENT_TYPE_U ||
                  corElemType == CorElementType.ELEMENT_TYPE_OBJECT))
                return false;

            if (HasInstantiation(type) && !IsGenericTypeDefinition(type))
                return false;

            return true;
        }

        internal static bool IsPrimitive(Type type)
        {
            CorElementType corElemType = GetCorElementType(type);
            return (corElemType >= CorElementType.ELEMENT_TYPE_BOOLEAN && corElemType <= CorElementType.ELEMENT_TYPE_R8) ||
                   corElemType == CorElementType.ELEMENT_TYPE_I ||
                   corElemType == CorElementType.ELEMENT_TYPE_U;
        }

        internal static bool IsByRef(Type type)
        {
            CorElementType corElemType = GetCorElementType(type);
            return corElemType == CorElementType.ELEMENT_TYPE_BYREF;
        }

        internal static bool IsPointer(Type type)
        {
            CorElementType corElemType = GetCorElementType(type);
            return corElemType == CorElementType.ELEMENT_TYPE_PTR;
        }

        internal static bool IsArray(Type type)
        {
            CorElementType corElemType = GetCorElementType(type);
            return corElemType == CorElementType.ELEMENT_TYPE_ARRAY || corElemType == CorElementType.ELEMENT_TYPE_SZARRAY;
        }

        internal static bool IsSZArray(Type type)
        {
            CorElementType corElemType = GetCorElementType(type);
            return corElemType == CorElementType.ELEMENT_TYPE_SZARRAY;
        }

        internal static bool HasElementType(Type type)
        {
            CorElementType corElemType = GetCorElementType(type);

            return corElemType == CorElementType.ELEMENT_TYPE_ARRAY || corElemType == CorElementType.ELEMENT_TYPE_SZARRAY // IsArray
                                                                    || (corElemType == CorElementType.ELEMENT_TYPE_PTR) // IsPointer
                                                                    || (corElemType == CorElementType.ELEMENT_TYPE_BYREF); // IsByRef
        }

        internal static IntPtr[]? CopyRuntimeTypeHandles(RuntimeTypeHandle[]? inHandles, out int length)
        {
            if (inHandles == null || inHandles.Length == 0)
            {
                length = 0;
                return null;
            }

            IntPtr[] outHandles = new IntPtr[inHandles.Length];
            for (int i = 0; i < inHandles.Length; i++)
            {
                outHandles[i] = inHandles[i].Value;
            }

            length = outHandles.Length;
            return outHandles;
        }

        internal static IntPtr[]? CopyRuntimeTypeHandles(Type[]? inHandles, out int length)
        {
            if (inHandles == null || inHandles.Length == 0)
            {
                length = 0;
                return null;
            }

            IntPtr[] outHandles = new IntPtr[inHandles.Length];
            for (int i = 0; i < inHandles.Length; i++)
            {
                outHandles[i] = inHandles[i].TypeHandle.Value;
            }

            length = outHandles.Length;
            return outHandles;
        }

        [ArduinoImplementation(ArduinoImplementation.None, CompareByParameterNames = true)]
        internal static object Allocate(Type type)
        {
            throw new NotImplementedException();
        }

        [ArduinoImplementation(ArduinoImplementation.CreateInstanceForAnotherGenericParameter, CompareByParameterNames = true)]
        internal static object CreateInstanceForAnotherGenericParameter(Type type, Type genericParameter)
        {
            throw new NotImplementedException();
        }

        internal Type GetRuntimeType()
        {
            return m_type;
        }

        [ArduinoImplementation(ArduinoImplementation.MiniRuntimeTypeHandleGetCorElementType)]
        internal static CorElementType GetCorElementType(Type type)
        {
            throw new NotImplementedException();
        }

        // What's this supposed to do?
        internal Type Instantiate(Type[]? inst)
        {
            // defensive copy to be sure array is not mutated from the outside during processing
            ////IntPtr[]? instHandles = CopyRuntimeTypeHandles(inst, out int instCount);

            ////fixed (IntPtr* pInst = instHandles)
            ////{
            ////    RuntimeType? type = null;
            ////    RuntimeTypeHandle nativeHandle = GetNativeHandle();
            ////    Instantiate(new QCallTypeHandle(ref nativeHandle), pInst, instCount, ObjectHandleOnStack.Create(ref type));
            ////    GC.KeepAlive(inst);
            ////    return type!;
            ////}
            if (inst == null)
            {
                return null!;
            }
            return inst[0];
        }

        internal static bool HasInstantiation(Type type)
        {
            throw new NotImplementedException();
        }

        internal static bool IsGenericTypeDefinition(Type type)
        {
            throw new NotImplementedException();
        }

        public void GetObjectData(SerializationInfo info, StreamingContext context)
        {
            throw new PlatformNotSupportedException();
        }
    }
}
