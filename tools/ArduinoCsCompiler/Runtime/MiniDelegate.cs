using System;
using Iot.Device.Arduino;

namespace ArduinoCsCompiler.Runtime
{
    [ArduinoReplacement(typeof(System.Delegate), IncludingPrivates = true)]
    internal class MiniDelegate
    {
        [ArduinoImplementation("DelegateInternalEqualTypes")]
        public static bool InternalEqualTypes(object a, object b)
        {
            throw new NotImplementedException();
        }
    }
}
