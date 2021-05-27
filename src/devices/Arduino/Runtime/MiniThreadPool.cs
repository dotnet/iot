using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Iot.Device.Arduino.Runtime
{
    [ArduinoReplacement(typeof(ThreadPool), IncludingPrivates = true)]
    internal sealed class MiniThreadPool
    {
        [ArduinoImplementation]
        public static bool GetEnableWorkerTracking()
        {
            return false;
        }
    }
}
