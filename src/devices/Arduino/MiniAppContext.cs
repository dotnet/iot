using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Iot.Device.Arduino
{
    [ArduinoReplacement(typeof(AppContext), true, false, IncludingPrivates = true)]
    internal class MiniAppContext
    {
    }
}
