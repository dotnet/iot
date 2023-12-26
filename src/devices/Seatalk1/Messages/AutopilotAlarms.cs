using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Iot.Device.Seatalk1.Messages
{
    [Flags]
    public enum AutopilotAlarms
    {
        None = 0,
        OffCourse = 4,
        WindShift = 8,
    }
}
