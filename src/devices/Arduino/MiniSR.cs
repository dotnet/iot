using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Iot.Device.Arduino
{
    [ArduinoReplacement("System.SR", IncludingPrivates = true)]
    internal class MiniSR
    {
        [ArduinoImplementation(ArduinoImplementation.None)]
        public static string GetResourceString(string resourceKey, string? defaultString)
        {
            if (ReferenceEquals(defaultString, null))
            {
                return resourceKey;
            }

            return defaultString;
        }
    }
}
