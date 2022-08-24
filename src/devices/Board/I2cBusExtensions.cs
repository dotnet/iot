// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System;
using System.Collections.Generic;
using System.Device.I2c;
using System.IO;
using System.Text;

namespace Iot.Device.Board
{
    /// <summary>
    /// Contains extension methods that operate on the I2c bus
    /// </summary>
    public static class I2cBusExtensions
    {
        /// <summary>
        /// Performs a scan on the I2C bus, returning the addresses for all connected devices.
        /// </summary>
        /// <param name="bus">The bus to scan</param>
        /// <param name="lowest">The lowest address to scan. Default 0x03</param>
        /// <param name="highest">The highest address to scan. Default 0x77</param>
        /// <returns>A list of bus addresses that are in use, an empty list if no device was found</returns>
        /// <remarks>This method should never throw an exception. Bus scanning may interfere with normal device operation,
        /// so this should not be done while devices are being used.</remarks>
        public static List<int> PerformBusScan(this I2cBus bus, int lowest = 0x3, int highest = 0x77)
        {
            List<int> ret = new List<int>();
            for (int addr = lowest; addr <= highest; addr++)
            {
                try
                {
                    using (var device = bus.CreateDevice(addr))
                    {
                        device.ReadByte(); // Success means that this does not throw an exception
                        ret.Add(addr);
                    }
                }
                catch (Exception x) when (x is IOException || x is UnauthorizedAccessException)
                {
                    // Ignore and continue
                }
            }

            return ret;
        }
    }
}
