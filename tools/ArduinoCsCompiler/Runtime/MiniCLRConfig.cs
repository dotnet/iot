using Iot.Device.Arduino;

namespace ArduinoCsCompiler.Runtime
{
    [ArduinoReplacement("System.CLRConfig", null, true, IncludingPrivates = true)]
    internal static class MiniCLRConfig
    {
        [ArduinoImplementation(NativeMethod.None)]
        public static bool GetBoolValueWithFallbacks(string switchName, string environmentName, bool defaultValue)
        {
            return defaultValue;
        }
    }
}
