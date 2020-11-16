using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Iot.Device.Arduino
{
    [ArduinoReplacement(typeof(System.Array))]
    internal class MiniArray
    {
        [ArduinoImplementation(ArduinoImplementation.ArrayCopy)]
        public static void Copy(
            Array sourceArray,
            int sourceIndex,
            Array destinationArray,
            int destinationIndex,
            int length,
            bool reliable)
        {
            throw new InvalidOperationException("Call to internal implementation");
        }
    }
}
