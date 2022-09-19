// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System;
using Iot.Device.Arduino;

namespace ArduinoCsCompiler.Runtime
{
    [ArduinoReplacement("System.IO.FileSystem", "System.IO.FileSystem.dll", true, typeof(System.IO.File), IncludingPrivates = true)]
    internal static class MiniFileSystem
    {
        [ArduinoImplementation("FileSystemCreateDirectory")]
        public static void CreateDirectory(string fullPath, byte[] securityDescriptor)
        {
            throw new NotImplementedException();
        }

        [ArduinoImplementation("FileSystemFileExists")]
        public static bool FileExists(string fullPath)
        {
            throw new NotImplementedException();
        }

        [ArduinoImplementation("FileSystemDirectoryExists")]
        public static bool DirectoryExists(string fullPath)
        {
            throw new NotImplementedException();
        }

        [ArduinoImplementation("FileSystemDeleteFile")]
        public static void DeleteFile(string fullPath)
        {
            throw new NotImplementedException();
        }
    }
}
