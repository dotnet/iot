using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Iot.Device.Arduino
{
    [ArduinoReplacement(typeof(Debug), true, IncludingPrivates = true)]
    internal class MiniDebug
    {
        [ArduinoImplementation(NativeMethod.DebugWriteLine)]
        public static void WriteLine(string? message)
        {
            throw new NotImplementedException();
        }
    }
}
