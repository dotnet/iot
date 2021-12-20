using Iot.Device.Arduino;

namespace ArduinoCsCompiler.Runtime
{
    [ArduinoReplacement("System.SR", IncludingPrivates = true)]
    internal class MiniSR
    {
        [ArduinoImplementation]
        public static string GetResourceString(string resourceKey, string? defaultString)
        {
            if (ReferenceEquals(defaultString, null))
            {
                return resourceKey;
            }

            return defaultString;
        }

        [ArduinoImplementation]
        public static string GetResourceString(string resourceKey)
        {
            return resourceKey;
        }
    }
}
