using System;
using System.Collections.Generic;
using System.Text;

namespace Iot.Device.GoPiGo3.Models
{
    /// <summary>
    /// The grove port used for analogic and/or digital read/write
    /// </summary>
    [Flags]
    public enum GrovePort
    {
        Grove1Pin1 = 0x01,
        Grove1Pin2 = 0x02,
        Grove2Pin1 = 0x04,
        Grove2Pin2 = 0x08,
        Grove1 = Grove1Pin1 + Grove1Pin2,
        Grove2 = Grove2Pin1 + Grove2Pin2,
        Both = Grove1 + Grove2
    }
}
