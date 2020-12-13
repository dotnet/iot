using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Iot.Device.Arduino
{
    [ArduinoReplacement(typeof(System.Enum))]
    internal class MiniEnum
    {
        [ArduinoImplementation(ArduinoImplementation.None)]
        public override string ToString()
        {
            return string.Empty;
        }

        [ArduinoImplementation(ArduinoImplementation.None)]
        public string ToString(string? format)
        {
            return string.Empty;
        }

        [ArduinoImplementation(ArduinoImplementation.None)]
        public string? ToString(string format, IFormatProvider provider)
        {
            return string.Empty;
        }
    }
}
