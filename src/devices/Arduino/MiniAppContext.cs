using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Iot.Device.Arduino
{
    [ArduinoReplacement(typeof(AppContext), true, IncludingPrivates = true)]
    internal class MiniAppContext
    {
        public static bool TryGetSwitch(string switchName, out bool isEnabled)
        {
            isEnabled = false;
            return false;
        }

        public static object? GetData(string name)
        {
            if (name == null)
            {
                throw new ArgumentNullException(nameof(name));
            }

            return null;
        }
    }
}
