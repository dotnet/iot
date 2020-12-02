using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Iot.Device.Arduino
{
    [ArduinoReplacement(typeof(System.Environment), true)]
    internal static class MiniEnvironment
    {
        public static int TickCount
        {
            [ArduinoImplementation(ArduinoImplementation.EnvironmentTickCount)]
            get
            {
                throw new NotImplementedException();
            }
        }
    }
}
