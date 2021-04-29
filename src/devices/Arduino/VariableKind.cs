using System;

#pragma warning disable CS1591
namespace Iot.Device.Arduino
{
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
}
