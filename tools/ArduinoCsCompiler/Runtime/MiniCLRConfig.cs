using Iot.Device.Arduino;

namespace ArduinoCsCompiler.Runtime
{
    // This type appears to no longer exist in net6.0
    // [ArduinoReplacement("System.CLRConfig", "System.Private.Corelib.dll", true, IncludingPrivates = true)]
    internal static class MiniCLRConfig
    {
        [ArduinoImplementation]
        public static bool GetBoolValueWithFallbacks(string switchName, string environmentName, bool defaultValue)
        {
            return defaultValue;
        }
    }
}
