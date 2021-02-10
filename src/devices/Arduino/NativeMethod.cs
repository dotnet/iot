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
        StringCtor0,
        StringLength,
        StringIndexer,
        StringFormat2,
        StringFormat2b,
        StringFormat3,
        StringEquals,
        StringToString,
        StringGetHashCode,
        StringConcat2,
        StringConcat3,
        StringConcat4,
        StringCtor2,
        StringSetElem,
        StringGetElem,
        StringGetPinnableReference,
        StringGetRawStringData,
        StringEqualsStatic,
        StringFastAllocateString,
        StringMiniStringConversion,
        StringUnEqualsStatic,

        StringEqualsStringComparison,
        StringInternalAllocateString,
        StringCtor1,
        StringCompareTo,

        ArrayCopy3,
        ArrayCopy5,
        ArrayClone,
        ArrayClear,

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
        BitOperationsLog2SoftwareFallback,
        BitOperationsTrailingZeroCount,
        EnumGetHashCode,
        EumToUInt64,
        UnsafeNullRef,
        UnsafeAs2,
        UnsafeAddByteOffset,
        UnsafeSizeOfType,
        UnsafeAsPointer,
        ArrayResize,
        UnsafeByteOffset,
        UnsafeAreSame,

        UnsafeIsAddressGreaterThan,
        UnsafeIsAddressLessThan,

        BufferMemmove,
        BufferMemmoveRefArgs,
        RuntimeHelpersGetHashCode,
        RuntimeHelpersIsBitwiseEquatable,
        RuntimeHelpersGetMethodTable,
        RuntimeHelpersGetRawArrayData,
        RuntimeTypeHandleValue,
        RuntimeTypeHandleGetCorElementType,

        RuntimeHelpersEnumEquals,
        Interop_GlobalizationGetCalendarInfo,
        InteropGetRandomBytes,

        MiniBuffer_BulkMoveWithWriteBarrier,
        MiniBuffer_ZeroMemory,

        ArduinoNativeI2cDeviceReadByte,
        ArduinoNativeI2cDeviceWriteByte,
        ArduinoNativeI2cDeviceInit,

        ByReferenceCtor,
        ByReferenceValue,

        InteropQueryPerformanceFrequency,
        InteropQueryPerformanceCounter,

        InterlockedCompareExchange_Object,
        InterlockedExchangeAdd,

        DelegateInternalEqualTypes
    }
}
