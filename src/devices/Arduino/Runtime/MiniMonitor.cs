using System;
using System.Threading;

namespace Iot.Device.Arduino.Runtime
{
    [ArduinoReplacement(typeof(Monitor))]
    internal class MiniMonitor
    {
        [ArduinoImplementation(NativeMethod.MonitorEnter1)]
        public static void Enter(Object o)
        {
        }

        [ArduinoImplementation(NativeMethod.MonitorEnter2)]
        public static void Enter(Object o, ref bool lockTaken)
        {
        }

        [ArduinoImplementation(NativeMethod.MonitorExit)]
        public static void Exit(Object o)
        {
        }
    }
}
