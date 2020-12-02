using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

#pragma warning disable CS1591
namespace Iot.Device.Arduino
{
    /// <summary>
    /// Contains helper functions usable when running on the Arduino.
    /// None of these functions work when called on the host PC directly.
    /// </summary>
    public class ArduinoNativeHelpers
    {
        [ArduinoImplementation(ArduinoImplementation.GetMicroseconds)]
        public static UInt32 GetMicroseconds()
        {
            throw new PlatformNotSupportedException("This method works on the Arduino only");
        }

        [ArduinoImplementation(ArduinoImplementation.SleepMicroseconds)]
        public static void SleepMicroseconds(UInt32 micros)
        {
            throw new PlatformNotSupportedException("This method works on the Arduino only");
        }
    }
}
