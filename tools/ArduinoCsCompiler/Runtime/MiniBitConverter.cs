using System;
using Iot.Device.Arduino;

namespace ArduinoCsCompiler.Runtime
{
    [ArduinoReplacement(typeof(BitConverter), true)]
    internal class MiniBitConverter
    {
        // This must be set to the endianess of the target platform, but currently all Microcontrollers that are supported seem to use little endian
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

        [ArduinoImplementation("BitConverterInt32BitsToSingle")]
        public static float Int32BitsToSingle(Int32 value)
        {
            throw new NotImplementedException();
        }

#if NET5_0_OR_GREATER
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
#endif
    }
}
