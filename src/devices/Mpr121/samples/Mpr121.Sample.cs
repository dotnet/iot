// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using System;
using System.ComponentModel;
using System.Device.I2c;
using System.Device.I2c.Drivers;


namespace Iot.Device.Mpr121.Samples
{
    class Program
    {
        static void Main(string[] args)
        {
            var i2cDevice = new UnixI2cDevice(new I2cConnectionSettings(busId: 1, deviceAddress: 0x5A));

            // Initialize controller with default configuration and auto-refresh the channel statuses every 100 ms.
            var mpr121 = new Mpr121(i2cDevice, 100);

            Console.Clear();
            Console.CursorVisible = false;

            PrintChannelsTable();
            Console.WriteLine("Press Enter to exit.");

            // Subscribe to channel statuses updates.
            mpr121.PropertyChanged += (object sender, PropertyChangedEventArgs e) => {
                if (e.PropertyName == nameof(Mpr121.ChannelStatuses))
                {
                    foreach (var channel in mpr121.ChannelStatuses.Keys)
                    {
                        Console.SetCursorPosition(10, (int)channel * 2 + 1);
                        Console.Write(mpr121.ChannelStatuses[channel] ? "#" : " ");
                    }
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
            Console.WriteLine("-------------");

            foreach (var channel in Enum.GetValues(typeof(Channels)))
            {
                Console.WriteLine("| " + Enum.GetName(typeof(Channels), channel) + ((int)channel < (int)Channels.CH_10 ? " " : "")  + " |   |");
                Console.WriteLine("-------------");
            }
        }
    }
}
