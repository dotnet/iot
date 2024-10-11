// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System;
using System.Runtime.CompilerServices;
using Iot.Device.Arduino;

namespace ArduinoCsCompiler.Runtime
{
    [ArduinoReplacement(typeof(BitConverter), true)]
    internal class MiniBitConverter
    {
        // This must be set to the endianness of the target platform, but currently all Microcontrollers that are supported seem to use little endian
        public static readonly bool IsLittleEndian = true;

        [ArduinoImplementation("BitConverterSingleToInt32Bits")]
        public static int SingleToInt32Bits(float value)
        {
            throw new NotImplementedException();
        }

        [ArduinoImplementation("BitConverterDoubleToInt64Bits")]
        public static Int64 DoubleToInt64Bits(double value)
        {
            throw new NotImplementedException();
        }

        [ArduinoImplementation("BitConverterDoubleToUInt64Bits")]
        public static Int64 DoubleToUInt64Bits(double value)
        {
            throw new NotImplementedException();
        }

        [ArduinoImplementation("BitConverterSingleToUInt32Bits")]
        public static UInt32 SingleToUInt32Bits(float value)
        {
            throw new NotImplementedException();
        }

        [ArduinoImplementation("BitConverterInt64BitsToDouble")]
        public static double Int64BitsToDouble(Int64 value)
        {
            throw new NotImplementedException();
        }

        [ArduinoImplementation("UInt64BitsToDouble")]
        public static double UInt64BitsToDouble(UInt64 value)
        {
            throw new NotImplementedException();
        }

        [ArduinoImplementation("BitConverterInt32BitsToSingle")]
        public static float Int32BitsToSingle(Int32 value)
        {
            throw new NotImplementedException();
        }

        [ArduinoImplementation("BitConverterHalfToInt16Bits")]
        public static Int16 HalfToInt16Bits(Half value)
        {
            throw new NotImplementedException();
        }

        [ArduinoImplementation("BitConverterInt16BitsToHalf")]
        public static Half Int16BitsToHalf(Int16 value)
        {
            throw new NotImplementedException();
        }

        [ArduinoImplementation("BitConverterHalfToUInt16Bits")]
        public static UInt16 HalfToUInt16Bits(Half value)
        {
            throw new NotImplementedException();
        }

        [ArduinoImplementation("BitConverterUInt16BitsToHalf")]
        public static Half UInt16BitsToHalf(UInt16 value)
        {
            throw new NotImplementedException();
        }

        [ArduinoImplementation("BitCountUint32BitsToSingle")]
        public static float UInt32BitsToSingle(uint value)
        {
            throw new NotImplementedException();
        }

        public static short ToInt16(ReadOnlySpan<byte> value)
        {
            if (value.Length < sizeof(short))
            {
                throw new ArgumentOutOfRangeException(nameof(value));
            }

            return (short)(value[0] | value[1] << 8);
        }

        public static Int32 ToInt32(System.Byte[] value, System.Int32 startIndex)
        {
            return (value[startIndex] | (value[1 + startIndex] << 8) | (value[2 + startIndex] << 16) | (value[3 + startIndex] << 24));
        }

        public static Int16 ToInt16(System.Byte[] value, System.Int32 startIndex)
        {
            return (Int16)(value[startIndex] | (value[1 + startIndex] << 8));
        }

        public static bool TryWriteBytes(Span<byte> destination, uint value)
        {
            if (destination.Length < sizeof(uint))
            {
                return false;
            }

            destination[0] = (byte)value;
            destination[1] = (byte)(value >> 8);
            destination[2] = (byte)(value >> 16);
            destination[3] = (byte)(value >> 24);
            return true;
        }
    }
}
