using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Iot.Device.Arduino
{
    [ArduinoReplacement(typeof(DateTime), false, IncludingPrivates = true)]
    internal struct MiniDateTime
    {
        [ArduinoImplementation(NativeMethod.None)]
        public static bool SystemSupportsLeapSeconds()
        {
            return false;
        }

        public static DateTime UtcNow
        {
            [ArduinoImplementation(NativeMethod.DateTimeUtcNow)]
            get
            {
                throw new NotImplementedException();
            }
        }
    }
}
