// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

#pragma warning disable CS1591
namespace ArduinoCsCompiler
{
    /// <summary>
    /// A set of tokens which are always assigned to these classes (or methods), because they need to be identifiable in the firmware, i.e. the token assigned
    /// to "System.Type" is always 2. See GetOrAddClassToken for where this is used.
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
        Thread = 13,
        Byte = 19,
        Int32 = 20,
        Uint32 = 21,
        Int64 = 22,
        Uint64 = 23,
        Exception = 30,
        NullReferenceException = 31,
        MissingMethodException = 32,
        DivideByZeroException = 33,
        IndexOutOfRangeException = 34,
        ArrayTypeMismatchException = 35,
        InvalidOperationException = 36,
        ClassNotFoundException = 37,
        InvalidCastException = 38,
        NotSupportedException = 39,
        FieldAccessException = 40,
        OverflowException = 41,
        IoException = 42,
        ArithmeticException = 43,
        ThreadStartCallback = 50, // The token of Thread.StartCallback(). This is the startup method for new threads.
        AppDomainTimerCallback = 51, // Used to fire the timer in MiniTimerQueue
        LargestKnownTypeToken = 52,
        // If more of these are required, check the ctor of ExecutionSet to make sure enough entries have been reserved
        IEnumerableOfT = ExecutionSet.GenericTokenStep,
        SpanOfT = ExecutionSet.GenericTokenStep * 2,
    }
}
