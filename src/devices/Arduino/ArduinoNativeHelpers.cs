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
        [ArduinoImplementation(NativeMethod.ArduinoNativeHelpersGetMicroseconds)]
        public static UInt32 GetMicroseconds()
        {
            throw new PlatformNotSupportedException("This method works on the Arduino only");
        }

        [ArduinoImplementation(NativeMethod.ArduinoNativeHelpersSleepMicroseconds)]
        public static void SleepMicroseconds(UInt32 micros)
        {
            throw new PlatformNotSupportedException("This method works on the Arduino only");
        }

        /// <summary>
        /// This method serves as stub for the main startup code. The body will be dynamically generated (from the static ctors and the actual main method)
        /// </summary>
        internal static void MainStub()
        {
            throw new NotImplementedException();
        }
    }
}
