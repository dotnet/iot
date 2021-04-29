using System;
using System.Threading;

namespace Iot.Device.Arduino.Runtime
{
    [ArduinoReplacement(typeof(Monitor), true)]
    internal class MiniMonitor
    {
        [ArduinoImplementation(NativeMethod.MonitorEnter1)]
        public static void Enter(Object o)
        {
        }

        [ArduinoImplementation]
        public static void Enter(Object o, ref bool lockTaken)
        {
            Enter(o);
            lockTaken = true;
        }

        [ArduinoImplementation(NativeMethod.MonitorExit)]
        public static void Exit(Object o)
        {
        }
    }
}
