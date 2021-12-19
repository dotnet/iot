using System;
using Iot.Device.Arduino;

namespace ArduinoCsCompiler.Runtime
{
    [ArduinoReplacement(typeof(System.TimeZoneInfo), false, IncludingPrivates = true)]
    internal class MiniTimeZoneInfo
    {
        [ArduinoImplementation(CompareByParameterNames = true)]
        public static TimeZoneInfo GetLocalTimeZone(int cachedData)
        {
            // Don't access the argument here, it has a different type
            return TimeZoneInfo.Utc;
        }

        [ArduinoImplementation]
        public static TimeSpan GetDateTimeNowUtcOffsetFromUtc(DateTime time, out bool isAmbiguousLocalDst)
        {
            isAmbiguousLocalDst = false;
            return TimeSpan.Zero;
        }

        [ArduinoImplementation]
        public static string GetUtcStandardDisplayName()
        {
            return "Coordinated Universal Time";
        }
    }
}
