// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using System;
using System.Device.Gpio;
using System.Device.Spi;
using System.Device.Spi.Drivers;
using System.Threading;
using Iot.Device.Mcp25xxx;

namespace Iot.Device.Mcp25xxx.Samples
{
    class Program
    {
        static void Main(string[] args)
        {
            Console.WriteLine("Hello Mcp25xxx Sample!");

            using (Mcp25625 mcp25625 = GetMcp25625Device())
            {
                // TODO: Add samples.
            }
        }

        private static Mcp25625 GetMcp25625Device()
        {
            var spiConnectionSettings = new SpiConnectionSettings(0, 0);
            var spiDevice = new UnixSpiDevice(spiConnectionSettings);
            return new Mcp25625(spiDevice);
        }
    }
}
