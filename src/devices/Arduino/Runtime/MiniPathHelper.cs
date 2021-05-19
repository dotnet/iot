using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace Iot.Device.Arduino.Runtime
{
    [ArduinoReplacement("System.IO.PathHelper", "System.Private.CoreLib.dll", false)]
    internal static class MiniPathHelper
    {
    }
}
