#pragma warning disable CS1591
namespace ArduinoCsCompiler
{
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
        Byte = 19,
        Int32 = 20,
        Uint32 = 21,
        Int64 = 22,
        Uint64 = 23,
        LargestKnownTypeToken = 40,
        // If more of these are required, check the ctor of ExecutionSet to make sure enough entries have been reserved
        IEnumerableOfT = ExecutionSet.GenericTokenStep,
        SpanOfT = ExecutionSet.GenericTokenStep * 2,
    }
}
