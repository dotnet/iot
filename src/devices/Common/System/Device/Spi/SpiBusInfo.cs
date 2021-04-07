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
            const string parameterPath = "/sys/module/spidev/parameters/bufsiz";
            if (RuntimeInformation.IsOSPlatform(OSPlatform.Linux) &&
                File.Exists(parameterPath) &&
                int.TryParse(File.ReadAllText(parameterPath), out int bufferSize))
            {
                return bufferSize;
            }

            return -1;
        }
    }
}
