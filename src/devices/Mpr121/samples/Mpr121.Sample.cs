// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using System;
using System.Device.I2c;
using System.Device.I2c.Devices;

namespace Iot.Device.Mpr121.Samples
{
    class Program
    {
        static void Main(string[] args)
        {
            var i2cDevice = I2cDevice.Create(new I2cConnectionSettings(busId: 1, deviceAddress: Mpr121.DefaultI2cAddress));

            // Initialize controller with default configuration and auto-refresh the channel statuses every 100 ms.
            var mpr121 = new Mpr121(device: i2cDevice, periodRefresh: 100);

            Console.Clear();
            Console.CursorVisible = false;

            PrintChannelsTable();
            Console.WriteLine("Press Enter to exit.");

            // Subscribe to channel statuses updates.
            mpr121.ChannelStatusesChanged += (object sender, ChannelStatusesChangedEventArgs e) =>
                {
                    var channelStatuses = e.ChannelStatuses;
                    foreach (var channel in channelStatuses.Keys)
                    {
                        Console.SetCursorPosition(14, (int)channel * 2 + 1);
                        Console.Write(channelStatuses[channel] ? "#" : " ");
                    }
                };

            using (mpr121)
            {
                Console.ReadLine();
                Console.Clear();
                Console.CursorVisible = true;
            }
        }

        private static void PrintChannelsTable()
        {
            Console.WriteLine("-----------------");

            foreach (var channel in Enum.GetValues(typeof(Channels)))
            {
                Console.WriteLine("| " + Enum.GetName(typeof(Channels), channel) + " |   |");
                Console.WriteLine("-----------------");
            }
        }
    }
}
