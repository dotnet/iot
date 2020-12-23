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

        [ArduinoImplementation(ArduinoImplementation.BitConverterSingleToInt32Bits)]
        public static int SingleToInt32Bits(float value)
        {
            throw new NotImplementedException();
        }

        public static Int64 DoubleToInt64Bits(double value)
        {
            throw new NotImplementedException();
        }
    }
}
