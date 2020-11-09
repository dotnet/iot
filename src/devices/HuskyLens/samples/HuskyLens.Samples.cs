// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System;
using System.Threading;
using System.Threading.Tasks;
using System.IO.Ports;

namespace Iot.Device.HuskyLens.Samples
{
    internal class Program
    {
        public static void Main(string[] args)
        {
            Console.WriteLine(string.Join(",", SerialPort.GetPortNames()));

            var sp = new SerialPort("COM5", 9600, Parity.None, 8, StopBits.One);

            Console.WriteLine("Opening Serial Port");

            sp.Open();

            Console.WriteLine($"IsOpen: {sp.IsOpen}");

            var device = new HuskyLens(new SerialPortConnection(sp));
            Console.WriteLine("Pinging HuskyLens");
            if (device.Ping())
            {
                Console.WriteLine("HuskyLens Pinged successfully");
            }
            else
            {
                Console.WriteLine("Yeah, it sucks, but we failed");
            }

            Console.WriteLine("Switching to face recognition");
            device.SetAlgorithm(Algorithm.FaceRecognition);

            Console.WriteLine("Press enter");
            Console.ReadLine();
            Console.WriteLine("Switching to object tracking");
            device.SetAlgorithm(Algorithm.ObjectTracking);

            Console.WriteLine("Press enter");
            Console.ReadLine();

            var cancellationSource = new CancellationTokenSource();

            Task.Run(() =>
            {
                Console.WriteLine("Continuously fetching objects, press Enter to exit");
                while (!cancellationSource.IsCancellationRequested)
                {
                    foreach (var o in device.GetAllObjects())
                    {
                        Console.WriteLine($"{o.ToString()}");
                    }
                }
            },
            cancellationSource.Token);

            Console.ReadLine();
            cancellationSource.Cancel(false);
            sp.Close();
        }
    }
}
