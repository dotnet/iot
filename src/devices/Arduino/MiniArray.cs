using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Iot.Device.Arduino
{
    [ArduinoReplacement(typeof(System.Array), true)]
    internal class MiniArray : ICloneable
    {
        [ArduinoImplementation(ArduinoImplementation.ArrayCopy5)]
        public static void Copy(
            Array sourceArray,
            int sourceIndex,
            Array destinationArray,
            int destinationIndex,
            int length)
        {
            throw new NotImplementedException();
        }

        [ArduinoImplementation(ArduinoImplementation.ArrayCopy3)]
        public static void Copy(System.Array sourceArray, System.Array desitnationArray, Int32 length)
        {
            throw new NotImplementedException();
        }

        [ArduinoImplementation(ArduinoImplementation.ArrayClone)]
        public object Clone()
        {
            throw new NotImplementedException();
        }
    }
}
