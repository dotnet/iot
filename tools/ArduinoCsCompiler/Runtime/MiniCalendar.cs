using System.Globalization;

namespace Iot.Device.Arduino.Runtime
{
    [ArduinoReplacement(typeof(Calendar), IncludingPrivates = true)]
    internal class MiniCalendar
    {
        [ArduinoImplementation(CompareByParameterNames = true)]
        public static int GetSystemTwoDigitYearSetting(CalendarId CalID, int defaultYearValue)
        {
            return defaultYearValue;
        }
    }
}
