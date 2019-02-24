// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using System;
using System.Device.Spi;
using System.Device.Spi.Drivers;
using System.Text;
using System.Threading;

namespace Iot.Device.Nrf24l01.Samples
{
    class Program
    {
        static void Main(string[] args)
        {
            // SPI0 CS0
            SpiConnectionSettings senderSettings = new SpiConnectionSettings(0, 0)
            {
                ClockFrequency = Nrf24l01.SpiClockFrequency,
                Mode = Nrf24l01.SpiMode
            };
            // SPI1 CS0
            SpiConnectionSettings receiverSettings = new SpiConnectionSettings(1, 2)
            {
                ClockFrequency = Nrf24l01.SpiClockFrequency,
                Mode = Nrf24l01.SpiMode
            };
            UnixSpiDevice senderDevice = new UnixSpiDevice(senderSettings);
            UnixSpiDevice receiverDevice = new UnixSpiDevice(receiverSettings);

            // SPI Device, CE Pin, IRQ Pin, Receive Packet Size
            using (Nrf24l01 sender = new Nrf24l01(senderDevice, 23, 24, 20))
            {
                using (Nrf24l01 receiver = new Nrf24l01(receiverDevice, 5, 6, 20))
                {
                    // Set sender send address, receiver pipe0 address (Optional)
                    byte[] receiverAddress = Encoding.UTF8.GetBytes("NRF24");
                    sender.Address = receiverAddress;
                    receiver.Pipe0.Address = receiverAddress;

                    // Binding DataReceived event
                    receiver.DataReceived += Receiver_ReceivedData;

                    // Loop
                    while (true)
                    {
                        sender.Send(Encoding.UTF8.GetBytes("Hello! .NET Core IoT"));

                        Thread.Sleep(2000);
                    }
                }
            }
        }

        private static void Receiver_ReceivedData(object sender, DataReceivedEventArgs e)
        {
            var raw = e.Data;
            var res = Encoding.UTF8.GetString(raw);

            Console.Write("Received Raw Data: ");
            foreach (var item in raw)
            {
                Console.Write($"{item} ");
            }
            Console.WriteLine();

            Console.WriteLine($"Massage: {res}");
            Console.WriteLine();
        }
    }
}
