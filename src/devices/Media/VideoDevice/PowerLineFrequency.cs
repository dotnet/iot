using System;
using System.Collections.Generic;
using System.Text;

namespace Iot.Device.Media
{
    /// <summary>
    /// The power line frequency of a video device.
    /// </summary>
    public enum PowerLineFrequency : int
    {
        Disabled = 0,
        Frequency50Hz = 1,
        Frequency60Hz = 2,
        Auto = 3,
    }
}
