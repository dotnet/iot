using Iot.Device.Arduino;

namespace ArduinoCsCompiler.Runtime
{
    [ArduinoReplacement("System.CLRConfig", null, true, IncludingPrivates = true)]
    internal static class MiniCLRConfig
    {
        [ArduinoImplementation]
        public static bool GetBoolValueWithFallbacks(string switchName, string environmentName, bool defaultValue)
        {
            return defaultValue;
        }
    }
}
