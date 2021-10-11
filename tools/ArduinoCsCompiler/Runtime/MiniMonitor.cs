using System;
using System.Threading;
using Iot.Device.Arduino;

namespace ArduinoCsCompiler.Runtime
{
    /// <summary>
    /// These locking primitives are no-ops as long as we're not supporting threads.
    /// </summary>
    [ArduinoReplacement(typeof(Monitor), true)]
    internal class MiniMonitor
    {
        [ArduinoImplementation(NativeMethod.MonitorEnter)]
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

        public static void PulseAll(Object o)
        {
            // No op
        }

        public static void Pulse(Object o)
        {
            // No op
        }

        [ArduinoImplementation(NativeMethod.MonitorWait)]
        public static bool Wait(Object obj, Int32 millisecondsTimeout)
        {
            throw new NotImplementedException();
        }
    }
}
