using System;

namespace Iot.Device.Arduino.Runtime
{
    [ArduinoReplacement(typeof(AppContext), true, IncludingPrivates = true)]
    internal class MiniAppContext
    {
        public static bool TryGetSwitch(string switchName, out bool isEnabled)
        {
            if (switchName == "System.Globalization.Invariant")
            {
                isEnabled = true;
                return true;
            }

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
