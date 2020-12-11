using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Iot.Device.Arduino
{
    [ArduinoReplacement(typeof(System.Environment), true)]
    internal static class MiniEnvironment
    {
        public static int TickCount
        {
            [ArduinoImplementation(ArduinoImplementation.EnvironmentTickCount)]
            get
            {
                throw new NotImplementedException();
            }
        }

        public static int ProcessorCount
        {
            get
            {
                return 1;
            }
        }

        [ArduinoImplementation(ArduinoImplementation.FailFast1)]
        public static void FailFast(string message)
        {
            throw new NotImplementedException();
        }

        [ArduinoImplementation(ArduinoImplementation.FailFast2)]
        public static void FailFast(string message, Exception exception)
        {
            throw new NotImplementedException();
        }

        public static string? GetEnvironmentVariable(string variable)
        {
            return null;
        }
    }
}
