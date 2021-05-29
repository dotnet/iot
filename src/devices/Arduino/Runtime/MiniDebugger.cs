using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Iot.Device.Arduino.Runtime
{
    [ArduinoReplacement(typeof(System.Diagnostics.Debugger), true)]
    internal class MiniDebugger
    {
        public static bool IsAttached
        {
            get
            {
                return false;
            }
        }

        public static void NotifyOfCrossThreadDependency()
        {
        }
    }
}
