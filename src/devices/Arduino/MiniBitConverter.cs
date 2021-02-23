using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Iot.Device.Arduino
{
    [ArduinoReplacement(typeof(BitConverter), true)]
    internal class MiniBitConverter
    {
        // This must be set to the endianess of the target platform, but currently all Arduinos seem to use little endian
        public static readonly bool IsLittleEndian = true;

        [ArduinoImplementation(NativeMethod.BitConverterSingleToInt32Bits)]
        public static int SingleToInt32Bits(float value)
        {
            throw new NotImplementedException();
        }

        [ArduinoImplementation(NativeMethod.BitConverterDoubleToInt64Bits)]
        public static Int64 DoubleToInt64Bits(double value)
        {
            throw new NotImplementedException();
        }

        [ArduinoImplementation(NativeMethod.BitConverterInt64BitsToDouble)]
        public static double Int64BitsToDouble(Int64 value)
        {
            throw new NotImplementedException();
        }

        [ArduinoImplementation(NativeMethod.BitConverterInt32BitsToSingle)]
        public static float Int32BitsToSingle(Int32 value)
        {
            throw new NotImplementedException();
        }

#if NET5_0
        [ArduinoImplementation(NativeMethod.BitConverterHalfToInt16Bits)]
        public static Int16 HalfToInt16Bits(Half value)
        {
            throw new NotImplementedException();
        }
#endif
    }
}
