using System;

namespace Iot.Device.Arduino.Runtime
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

        [ArduinoImplementation(NativeMethod.None, CompareByParameterNames = true)]
        public static unsafe bool ValidateSystemTime(byte* time, bool localTime)
        {
            return true;
        }
    }
}
