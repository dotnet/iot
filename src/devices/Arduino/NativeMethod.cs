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
        EnvironmentTickCount64,
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

        MonitorEnter,
        MonitorWait,
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
        TypeGetBaseType,

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
        UnsafeSkipInit,

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
        MathExp,
        MathAbs,

        DebugWriteLine,

        ThreadGetCurrentThreadNative,

        Interop_Kernel32CreateFile,
        Interop_Kernel32SetLastError,
        Interop_Kernel32GetLastError,
        Interop_Kernel32SetFilePointerEx,
        Interop_Kernel32CloseHandle,
        Interop_Kernel32SetEndOfFile,
        Interop_Kernel32WriteFile,
        Interop_Kernel32WriteFileOverlapped,
        Interop_Kernel32CancelIoEx,
        Interop_Kernel32ReadFile,
        Interop_Kernel32ReadFileOverlapped,
        Interop_Kernel32FlushFileBuffers,
        Interop_Kernel32GetFileInformationByHandleEx,
        Interop_Kernel32QueryUnbiasedInterruptTime,
    }
}
