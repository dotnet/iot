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
            [ArduinoImplementation(NativeMethod.EnvironmentTickCount)]
            get
            {
                throw new NotImplementedException();
            }
        }

        public static int ProcessorCount
        {
            [ArduinoImplementation(NativeMethod.EnvironmentProcessorCount)]
            get
            {
                return 1;
            }
        }

        public static bool IsSingleProcessor
        {
            get
            {
                return ProcessorCount == 1;
            }
        }

        public static string SystemDirectory
        {
            get
            {
                return "/"; // At the moment, we do not have a file system at all
            }
        }

        public static string NewLine
        {
            get
            {
                return "\n"; // We'll have our "Arduino-OS" look like an unix style system
            }
        }

        public static OperatingSystem OSVersion
        {
            get
            {
                // This does not have a "anything else" option...
                return new OperatingSystem(PlatformID.Unix, new Version(1, 0));
            }
        }

        [ArduinoImplementation(NativeMethod.EnvironmentFailFast1)]
        public static void FailFast(string message)
        {
            throw new NotImplementedException();
        }

        [ArduinoImplementation(NativeMethod.EnvironmentFailFast2)]
        public static void FailFast(string message, Exception exception)
        {
            throw new NotImplementedException();
        }

        public static string? GetEnvironmentVariable(string variable)
        {
            return null;
        }

        public static string ExpandEnvironmentVariables(string input)
        {
            return input;
        }
    }
}
