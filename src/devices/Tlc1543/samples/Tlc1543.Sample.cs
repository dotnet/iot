// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using System;
using System.Collections.Generic;
using System.Device.Spi;
using Iot.Device.Spi;

namespace Iot.Device.Tlc1543.Samples
{
    /// <summary>
    /// Samples for Tlc1543
    /// </summary>
    public class Program
    {
        /// <summary>
        /// Main entry point
        /// </summary>
        public static void Main()
        {
            SoftwareSpi spi = new SoftwareSpi(
                clk: 25,
                sdi: 23,
                sdo: 24,
                cs: 5,
                settings: new SpiConnectionSettings(-1) { DataBitLength = Tlc1543.SpiDataBitLength });

            Tlc1543 adc = new Tlc1543(spi);
            Channel[] channels = new Channel[]
            {
                Channel.A0,
                Channel.A1,
                Channel.A2,
                Channel.A3,
                Channel.A4
            };

            foreach (Channel channel in channels)
            {
                Console.WriteLine($"Channel {channel}: {adc.ReadChannel(channel)}");
            }

            // or a bit faster
            // we ignore the first reading
            adc.ReadPreviousAndChargeChannel(channels[0]);
            for (int i = 0; i < channels.Length; i++)
            {
                // For last reading we need to pass something so let's pass test channel
                Channel nextChannel = i < channels.Length - 1 ? channels[i + 1] : Channel.SelfTestHalf;
                int previous = adc.ReadPreviousAndChargeChannel(nextChannel);
                Console.WriteLine($"Channel {channels[i]}: {previous}");
            }

            // now continuously read from one channel
            Channel ch = Channel.A0;
            int numberOfReadings = 10;
            adc.ReadPreviousAndChargeChannel(ch);

            for (int i = 0; i < numberOfReadings; i++)
            {
                Console.WriteLine($"Channel {ch}: {adc.ReadPreviousAndChargeChannel(ch)}");
            }
        }
    }
}
