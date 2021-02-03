using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Iot.Device.Arduino
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
