// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System.Globalization;
using Iot.Device.Arduino;

namespace ArduinoCsCompiler.Runtime
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
