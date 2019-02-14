// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using System;
using System.Device.Spi;
using System.Device.Spi.Drivers;
using System.Threading;
using Iot.Device.Mfrc522;

namespace Iot.Device.Samples
{
    class Program
    {
        static void Main(string[] args)
        {
            Console.WriteLine("Hello Mfrc522!");

            // This sample demonstrates how to read a RFID tag 
            // using the Mfrc522Controller

            var connection = new SpiConnectionSettings(0, 0);
            connection.ClockFrequency = 500000;

            var spi = new UnixSpiDevice(connection);
            var mfrc522Controller = new Mfrc522Controller(spi);

            while(true) 
            {
                var (status, _) =  mfrc522Controller.Request(Mfrc522Controller.PICC_REQIDL);

                if(status != Mfrc522Controller.MI_OK)
                    continue;

                var (status2, data) =  mfrc522Controller.Anticoll();
                Console.WriteLine(string.Join(", ", data));
            }
        }
    }
}
