// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using System;
using System.Threading;

namespace Iot.Device.Bh1750.Samples
{
    public class Program
    {
        static void Main(string[] args)
        {
            Bh1750 lightSensor = new Bh1750();
            ManualResetEvent mre = new ManualResetEvent(false);

            Console.CancelKeyPress += (o, e) => { mre.Set(); };

            while (mre.WaitOne())
            {
                Console.WriteLine($"Light level detected: {lightSensor.ReadLight()} lux");
                Thread.Sleep(TimeSpan.FromSeconds(1));
            }
        }
    }
}
