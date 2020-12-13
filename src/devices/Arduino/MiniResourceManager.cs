using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Iot.Device.Arduino
{
    // The original implementation of this one is just too bloated. We do not need all that error message lookup stuff
    [ArduinoReplacement(typeof(System.Resources.ResourceManager), true)]
    internal class MiniResourceManager
    {
        public string GetString(string resourceName, CultureInfo culture)
        {
            return resourceName;
        }

        public MiniResourceManager(Type resourceSource)
        {
        }
    }
}
