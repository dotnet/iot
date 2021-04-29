#pragma warning disable CS1591

namespace Iot.Device.Arduino
{
    public enum NativeMethod
    {
        None = 0,
        HardwareLevelAccessSetPinMode,
        HardwareLevelAccessWritePin,
        HardwareLevelAccessReadPin,
        HardwareLevelAccessGetPinMode,
        HardwareLevelAccessIsPinModeSupported,
        HardwareLevelAccessGetPinCount,

        EnvironmentTickCount,
        EnvironmentProcessorCount,
        EnvironmentFailFast1,
        EnvironmentFailFast2,

        ArduinoNativeHelpersSleepMicroseconds,
        ArduinoNativeHelpersGetMicroseconds,
        ObjectEquals,
        ObjectGetHashCode,
        ObjectReferenceEquals,
        ObjectToString,
        ObjectGetType,
        ObjectMemberwiseClone,

        MonitorEnter1,
        MonitorEnter2,
        MonitorExit,
        StringEquals,
        StringToString,
        StringGetHashCode,
        StringSetElem,
        StringGetElem,
        StringGetPinnableReference,
        StringGetRawStringData,
        StringEqualsStatic,
        StringFastAllocateString,
        StringUnEqualsStatic,
        StringImplicitConversion,
        StringEqualsStringComparison,
        StringInternalAllocateString,
        StringCtorSpan,
        StringCompareTo,
        StringCtorCharCount,

        RuntimeHelpersInitializeArray,
        RuntimeHelpersRunClassConstructor,
        RuntimeHelpersIsReferenceOrContainsReferencesCore,

        TypeGetTypeFromHandle,
        TypeEquals,
        TypeIsAssignableTo,
        TypeIsEnum,
        TypeTypeHandle,
        TypeIsValueType,
        TypeIsSubclassOf,
        TypeIsAssignableFrom,
        TypeCtor,
        TypeMakeGenericType,
        TypeGetHashCode,
        TypeGetGenericTypeDefinition,
        TypeGetGenericArguments,
        TypeCreateInstanceForAnotherGenericParameter,
        TypeIsArray,
        TypeGetElementType,
        TypeContainsGenericParameters,
        TypeName,

        ValueTypeGetHashCode,
        ValueTypeEquals,
        ValueTypeToString,

        BitConverterSingleToInt32Bits,
        BitConverterDoubleToInt64Bits,
        BitConverterHalfToInt16Bits,
        BitConverterInt64BitsToDouble,
        BitConverterInt32BitsToSingle,
        BitConverterInt16BitsToHalf,

        BitOperationsLog2SoftwareFallback,
        BitOperationsTrailingZeroCount,

        EnumGetHashCode,
        EnumToUInt64,
        EnumInternalBoxEnum,
        EnumInternalGetValues,

        UnsafeNullRef,
        UnsafeAs2,
        UnsafeAddByteOffset,
        UnsafeSizeOfType,
        UnsafeAsPointer,
        UnsafeByteOffset,
        UnsafeAreSame,

        UnsafeIsAddressGreaterThan,
        UnsafeIsAddressLessThan,

        BufferMemmove,
        BufferZeroMemory,

        RuntimeHelpersGetHashCode,
        RuntimeHelpersIsBitwiseEquatable,
        RuntimeHelpersGetMethodTable,
        RuntimeHelpersGetRawArrayData,

        RuntimeHelpersGetMultiDimensionalArrayBounds,
        RuntimeHelpersGetMultiDimensionalArrayRank,

        RuntimeTypeHandleValue,
        RuntimeTypeHandleGetCorElementType,

        RuntimeHelpersEnumEquals,
        Interop_GlobalizationGetCalendarInfo,
        InteropGetRandomBytes,

        ArduinoNativeI2cDeviceReadByte,
        ArduinoNativeI2cDeviceReadSpan,
        ArduinoNativeI2cDeviceWriteByte,
        ArduinoNativeI2cDeviceWriteSpan,
        ArduinoNativeI2cDeviceWriteRead,
        ArduinoNativeI2cDeviceInit,

        ByReferenceCtor,
        ByReferenceValue,

        InteropQueryPerformanceFrequency,
        InteropQueryPerformanceCounter,

        InterlockedCompareExchange_Object,
        InterlockedExchangeAdd,

        DelegateInternalEqualTypes,

        DateTimeUtcNow,

        MemoryMarshalGetArrayDataReference,

        ArrayCopyCore,
        ArrayClear,
        ArrayInternalCreate,
        ArraySetValue1,
        ArrayGetValue1,
        ArrayGetLength,

        ActivatorCreateInstance,

        GcCollect,
        GcGetTotalMemory,
        GcGetTotalAllocatedBytes,
        GcTotalAvailableMemoryBytes,

        MathCeiling,
        MathFloor,
        MathPow,
        MathLog,
        MathLog2,
        MathLog10,
        MathSin,
        MathCos,
        MathTan,
        MathSqrt,

        DebugWriteLine
    }
}
