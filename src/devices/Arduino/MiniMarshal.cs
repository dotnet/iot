using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace Iot.Device.Arduino
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
