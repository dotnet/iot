using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Iot.Device.Arduino
{
    [ArduinoReplacement(typeof(BitConverter))]
    internal class MiniBitConverter
    {
        [ArduinoImplementation(ArduinoImplementation.BitConverterSingleToInt32Bits)]
        public static int SingleToInt32Bits(float value)
        {
            throw new NotImplementedException();
        }
    }
}
