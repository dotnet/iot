namespace Iot.Device.Arduino.Runtime
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
