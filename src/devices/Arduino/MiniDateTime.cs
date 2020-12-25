using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Iot.Device.Arduino
{
    [ArduinoReplacement(typeof(DateTime), false, false, IncludingPrivates = true)]
    internal struct MiniDateTime
    {
        [ArduinoImplementation(ArduinoImplementation.None)]
        public static bool SystemSupportsLeapSeconds()
        {
            return false;
        }
    }
}
