using System;
using System.Collections.Generic;
using System.Text;

namespace Iot.Device.Board
{
    /// <summary>
    /// Alternate pin modes, for pins that support them
    /// </summary>
    public enum AlternatePinMode
    {
        // Note: Do not change the enum values without reviewing the impact
        Unknown = -2,
        NotSupported = -1,
        Gpio = 0,

        // the names are Raspi specific, but this is considered a generic representation of alternate modes (if any)
        Alt0 = 1,
        Alt1 = 2,
        Alt2 = 3,
        Alt3 = 4,
        Alt4 = 5,
        Alt5 = 6,
        Alt6 = 7,
        Alt7 = 8,
        Alt8 = 9,
        Alt9 = 10,
    }
}
