using System.Threading;
using Iot.Device.Arduino;

namespace ArduinoCsCompiler.Runtime
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
