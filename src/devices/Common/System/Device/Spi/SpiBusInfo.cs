// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

namespace System.Device.Spi
{
    public static class SpiBusInfo
    {
        private static int s_bufferSize = GetBufferSize();

        // we would need to decide if this should throw when size can't be found or just return -1 or other placeholder
        public static int BufferSize => s_bufferSize;

        private static int GetBufferSize()
        {
            // non-throwing version of int.Parse(File.ReadAllText("/sys/module/spidev/parameters/bufsiz"))
            // return -1 on error
        }
    }
}
