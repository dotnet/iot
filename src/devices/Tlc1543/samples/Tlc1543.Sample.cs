// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using System;
using System.Collections.Generic;
using System.Device.Gpio;
using System.Device.I2c;
using System.Device.Spi;
using System.Threading;

namespace Iot.Device.Tlc1543.Samples
{
    /// <summary>
    /// Samples for Tlc1543
    /// </summary>
    public class Program
    {
        private static List<Tlc1543.Channel> _channelList = new List<Tlc1543.Channel>
        {
            Tlc1543.Channel.A0,
            Tlc1543.Channel.A1,
            Tlc1543.Channel.A2,
            Tlc1543.Channel.A3,
            Tlc1543.Channel.A4
        };
        private static Tlc1543 _adc;

        /// <summary>
        /// Main entry point
        /// </summary>
        public static void Main()
        {
            _adc = new Tlc1543(24, 5, 23, 25);
            _adc.ChargeChannel = Tlc1543.Channel.SelfTest0;
            List<int> values = _adc.ReadChannels(_channelList);
            foreach (var value in values)
            {
                Console.WriteLine(value);
            }
        }
    }
}
