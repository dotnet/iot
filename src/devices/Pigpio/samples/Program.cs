using System;
using System.Device.Gpio;
using System.Net;
using System.Threading.Tasks;

namespace Pigpio.Sample
{
    class Program
    {
        static async Task Main(string[] args)
        {
            if (args.Length == 0)
            {
                Console.WriteLine("Please supply the IP Address of the Raspberry Pi to connect to");
            }
            else if (IPAddress.TryParse(args[0], out IPAddress ipAddress))
            {
                using (var driver = new Iot.Device.Pigpio.Driver(new IPEndPoint(ipAddress, 8888)))
                {
                    await driver.ConnectAsync();

                    // Line above doesn't current wait for connection so
                    // delay here to give the socket time to get connected
                    await Task.Delay(TimeSpan.FromSeconds(2));

                    using (GpioController controller = new GpioController(PinNumberingScheme.Logical, driver))
                    {
                        controller.OpenPin(4);
                        controller.SetPinMode(4, PinMode.Output);

                        var value = 0;
                        while (true)
                        {
                            value = (value + 1) % 2;

                            PinValue pinValue = (PinValue)value;

                            Console.WriteLine($"Writing PinValue of '{pinValue}' to pin '4'");

                            controller.Write(4, pinValue);

                            await Task.Delay(TimeSpan.FromSeconds(1));
                        }
                    }
                }
            }
            else
            {
                Console.WriteLine("The specified IP address is invalid.");
            }
        }
    }
}
