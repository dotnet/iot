using System;
using System.Diagnostics;
using Iot.Device.Arduino;

namespace ArduinoCsCompiler
{
    [ArduinoReplacement(typeof(Debug), true, IncludingPrivates = true)]
    internal class MiniDebug
    {
        [ArduinoImplementation("DebugWriteLine")]
        public static void WriteLine(string? message)
        {
            throw new NotImplementedException();
        }
    }
}
