using Iot.Device.Arduino;

namespace ArduinoCsCompiler.Runtime
{
    [ArduinoReplacement(typeof(System.Exception), IncludingPrivates = true)]
    internal class MiniException
    {
        [ArduinoImplementation]
        public string CreateSourceName()
        {
            return "Source";
        }
    }
}
