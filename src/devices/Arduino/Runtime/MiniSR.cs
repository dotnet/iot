namespace Iot.Device.Arduino.Runtime
{
    [ArduinoReplacement("System.SR", IncludingPrivates = true)]
    internal class MiniSR
    {
        [ArduinoImplementation(NativeMethod.None)]
        public static string GetResourceString(string resourceKey, string? defaultString)
        {
            if (ReferenceEquals(defaultString, null))
            {
                return resourceKey;
            }

            return defaultString;
        }
    }
}
