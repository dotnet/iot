using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Iot.Device.Arduino.Runtime
{
    [ArduinoReplacement("System.IO.FileSystem", "System.IO.FileSystem.dll", true, typeof(System.IO.File), IncludingPrivates = true)]
    internal static class MiniFileSystem
    {
        public static void CreateDirectory(string fullPath, byte[] securityDescriptor)
        {
            throw new NotImplementedException();
        }

        public static Boolean FileExists(System.String fullPath)
        {
            throw new NotImplementedException();
        }
    }
}
