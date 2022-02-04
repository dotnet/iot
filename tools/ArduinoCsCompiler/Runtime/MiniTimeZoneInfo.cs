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
            return GetLocalTimeZoneInternal();
        }

        [ArduinoImplementation]
        public static TimeSpan GetDateTimeNowUtcOffsetFromUtc(DateTime time, out bool isAmbiguousLocalDst)
        {
            isAmbiguousLocalDst = false;
            return GetDateTimeNowUtcOffsetFromUtcInternal();
        }

        [ArduinoCompileTimeConstant]
        private static TimeSpan GetDateTimeNowUtcOffsetFromUtcInternal()
        {
            // TODO: This probably needs to be a bit more complicated, to allow DST changes
            return TimeZoneInfo.Local.GetUtcOffset(DateTime.Now);
        }

        [ArduinoCompileTimeConstant]
        private static TimeZoneInfo GetLocalTimeZoneInternal()
        {
            return TimeZoneInfo.Utc;
        }

        [ArduinoImplementation]
        public static string GetUtcStandardDisplayName()
        {
            var utcTz = TimeZoneInfo.Utc;
            return utcTz.StandardName;
        }
    }
}
