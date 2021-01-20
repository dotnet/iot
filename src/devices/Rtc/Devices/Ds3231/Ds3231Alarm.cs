// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

namespace Iot.Device.Rtc
{
    /// <summary>
    /// Available alarms on the DS3231
    /// </summary>
    public enum Ds3231Alarm
    {
        /// <summary>
        /// Indicates none of the alarms
        /// </summary>
        None,

        /// <summary>
        /// Indicates the first alarm
        /// </summary>
        AlarmOne,

        /// <summary>
        /// Indicates the second alarm
        /// </summary>
        AlarmTwo
    }
}
