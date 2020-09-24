// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using System;
using System.Collections.Generic;

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
            Tlc1543 adc = new Tlc1543(24, 5, 23, 25);
            List<Channel> channelList = new List<Channel>
            {
                Channel.A0,
                Channel.A1,
                Channel.A2,
                Channel.A3,
                Channel.A4
            };
            adc.ChargeChannel = Channel.SelfTest512;

            int lineAverage = 0;
            int onLine = 0;
            List<int> values = adc.ReadChannels(channelList);

            for (int i = 0; i < values.Count; i++)
            {
                if (values[i] < 300)
                {
                    lineAverage += (i - 2);
                    onLine++;
                }
            }

            double linePosition = ((double)lineAverage / (double)onLine);
            Console.WriteLine($"Line position: {linePosition}");
        }
    }
}
