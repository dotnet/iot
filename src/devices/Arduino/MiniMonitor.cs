using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Iot.Device.Arduino
{
    [ArduinoReplacement(typeof(Monitor))]
    internal class MiniMonitor
    {
        [ArduinoImplementation(ArduinoImplementation.MonitorEnter1)]
        public static void Enter(Object o)
        {
        }

        [ArduinoImplementation(ArduinoImplementation.MonitorEnter2)]
        public static void Enter(Object o, ref bool lockTaken)
        {
        }

        [ArduinoImplementation(ArduinoImplementation.MonitorExit)]
        public static void Exit(Object o)
        {
        }
    }
}
