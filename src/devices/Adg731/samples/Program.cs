using System;
using System.Device.Spi;
using System.Diagnostics;
using System.Threading;
using Iot.Device.Adg731;

namespace ADG731Sample
{
    internal class Program
    {
        private static Adg731 s_adg731;

        public static void Main()
        {
            WaitForDebugger();

            // Digilent Pmod HAT Adapter
            // JA - Bus 0, CS 0
            // JB - Bus 0, CS 1
            SpiConnectionSettings settings = new SpiConnectionSettings(0, 1)
            {
                ClockFrequency = ((ISpiDeviceMetadata)Adg731.GetDeviceMetadata()).MaximumSpiFrequency,
                Mode = SpiMode.Mode1
            };
            SpiDevice adg731SpiDevice = SpiDevice.Create(settings);

            Console.WriteLine($"Connecting to ADG731 using SPI {adg731SpiDevice.ConnectionSettings.Mode} at {adg731SpiDevice.ConnectionSettings.ClockFrequency / 1000.0:N1} kHz...");

            s_adg731 = new Adg731(adg731SpiDevice);
            s_adg731.IsEnabled = true;
            s_adg731.IsSelected = true;

            int loopcounter = 0;
            while (true)
            {
                s_adg731.ActiveChannel = loopcounter++;
                Console.WriteLine($"Channel changed to {s_adg731.ActiveChannel}");
                Thread.Sleep(500);
            }
        }

        private static void WaitForDebugger()
        {
            int i = 0;
            Console.WriteLine("Waiting for the debugger to attach for 30 seconds... ");
            Console.WriteLine("(To attach the debugger in Visual Studio, press Ctrl+Alt+P, select SSH, set the IP address of the Raspberry Pi, enter your credentials, select the process, and click Attach. Press Shift+Alt+P next time.) ");
            while (true)
            {
                Console.WriteLine(++i + " ");
                if (Debugger.IsAttached)
                {
                    break;
                }

                Thread.Sleep(1000);

                if (i > 30)
                {
                    break;
                }
            }

            Console.WriteLine();
        }
    }
}
