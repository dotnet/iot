using System.Runtime.InteropServices;

namespace Iot.Device.Arduino.Runtime
{
    [ArduinoReplacement(typeof(Marshal), true)]
    internal class MiniMarshal
    {
        private static int _lastError;

        public static void SetLastWin32Error(int error)
        {
            _lastError = error;
        }

        public static int GetLastWin32Error()
        {
            return _lastError;
        }

        public static bool IsPinnable(object obj)
        {
            return false;
        }
    }
}
