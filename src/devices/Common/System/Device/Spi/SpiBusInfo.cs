// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System.IO;
using System.Runtime.InteropServices;

namespace System.Device.Spi
{
    /// <summary>
    /// Base class for SPI bus information.
    /// </summary>
    public static class SpiBusInfo
    {
        private static int s_bufferSize = GetBufferSize();

        /// <summary>
        /// Buffer size assigned to the SPI bus or -1 if the buffer size is not available.
        /// </summary>
        public static int BufferSize => s_bufferSize;

        private static int GetBufferSize()
        {
            if (RuntimeInformation.IsOSPlatform(OSPlatform.Linux))
            {
                const string parameterPath = "/sys/module/spidev/parameters/bufsiz";
                if (File.Exists(parameterPath) && int.TryParse(File.ReadAllText(parameterPath), out int buferSize))
                {
                    return buferSize;
                }
                else
                {
                    return -1;
                }
            }
            else
            {
                throw new PlatformNotSupportedException("This value is only supported on Linux operating systems");
            }
        }
    }
}
