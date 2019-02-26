using System;
using System.Device.Spi;
using System.Device.Spi.Drivers;
using System.Threading.Tasks;

namespace Iot.Device.Mfrc522
{
    class Program
    {
        static async Task Main(string[] args)
        {
            Console.WriteLine("Hello Mfrc522!");

            // This sample demonstrates how to read a RFID tag 
            // using the Mfrc522Controller

            var connection = new SpiConnectionSettings(0, 0);
            connection.ClockFrequency = 500000;

            var spi = new UnixSpiDevice(connection);
            using(var mfrc522Controller = new Mfrc522Controller(spi)) 
            {
                while(true) 
                {
                    var (status, _) =  mfrc522Controller.Request(RequestMode.RequestIdle);

                    if(status != Status.OK)
                        continue;

                    var (status2, data) =  mfrc522Controller.AntiCollision();
                    Console.WriteLine(string.Join(", ", data));

                    await Task.Delay(500);
                }
            }
        }
    }
}
