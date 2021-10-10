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
        [ArduinoImplementation(NativeMethod.FileSystemCreateDirectory)]
        public static void CreateDirectory(string fullPath, byte[] securityDescriptor)
        {
            throw new NotImplementedException();
        }

        [ArduinoImplementation(NativeMethod.FileSystemFileExists)]
        public static bool FileExists(string fullPath)
        {
            throw new NotImplementedException();
        }
    }
}
