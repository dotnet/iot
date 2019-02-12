using System;
using System.Collections.Generic;
using System.Text;

namespace Iot.Device.GoPiGo3.Models
{
    /// <summary>
    /// The grove port used for analogic and/or digital read/write
    /// </summary>
    [Flags]
    public enum GroovePort
    {
        Groove1Pin1 = 0x01,
        Groove1Pin2 = 0x02,
        Groove2Pin1 = 0x04,
        Groove2Pin2 = 0x08,
        Groove1 = Groove1Pin1 + Groove1Pin2,
        Groove2 = Groove2Pin1 + Groove2Pin2,
        Both = Groove1 + Groove2
    }
}
