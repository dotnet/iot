// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using System;
using System.Device.Gpio;
using System.Threading;
using System.Threading.Tasks;

namespace Iot.Device.Hx711.Samples
{
    class Program
    {
        private static async void ReadValues(Hx711 hx711, CancellationToken cancellationToken)
        {
            while (!cancellationToken.IsCancellationRequested)
            {
                var value = hx711.ReadValue();

                Console.WriteLine($"Read '{value}'");

                await Task.Delay(TimeSpan.FromSeconds(1), cancellationToken);
            }
        }

        static async Task Main(string[] args)
        {
            Console.WriteLine("Starting");

            using (GpioController controller = new GpioController())
            {
                using (Hx711 hx711 = new Hx711(controller, 5, 6))
                {
                    hx711.Enable();

                    using (CancellationTokenSource cts = new CancellationTokenSource())
                    {
                        Console.WriteLine("Running. Hit <Enter> to stop.");

                        var task = Task.Run(() => ReadValues(hx711, cts.Token));

                        Console.ReadLine();
                        cts.Cancel();

                        Console.WriteLine("Stopping...");
                        await task;
                    }

                    hx711.Disable();

                    Console.WriteLine("Stopped.");
                }
            }
        }
    }
}
