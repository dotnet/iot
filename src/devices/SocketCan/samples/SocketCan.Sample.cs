// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;

namespace Iot.Device.SocketCan.Samples
{
    class Program
    {
        const uint Id = 0x1A; // arbitrary id

        static Dictionary<string, Action> s_samples = new Dictionary<string, Action>
        {
            { "send", SendExample },
            { "receive", ReceiveAllExample },
            { "receive-on-specific-address", ReceiveOnAddressExample },
        };

        static void Main(string[] args)
        {
            if (args.Length == 0 || !s_samples.ContainsKey(args[0]))
            {
                Console.WriteLine("Usage: SocketCan.Sample <sample-name>");
                Console.WriteLine("Available samples:");

                foreach (var kv in s_samples)
                {
                    Console.WriteLine($"- {kv.Key}");
                }

                return;
            }

            s_samples[args[0]]();
        }

        private static void SendExample()
        {
            Console.WriteLine($"Sending to id = {Id}");

            using (CanRaw can = new CanRaw())
            {
                byte[][] buffers = new byte[][]
                {
                    new byte[8] { 1, 2, 3, 40, 50, 60, 70, 80 },
                    new byte[7] { 1, 2, 3, 40, 50, 60, 70 },
                    new byte[0] { },
                    new byte[1] { 254 },
                };

                CanFrame frame = new CanFrame();
                frame.StandardId = Id;

                if (!frame.IsValid)
                {
                    // This is more form of the test rather than actual part of the sample
                    throw new Exception("Frame is invalid");
                }

                while (true)
                {
                    foreach (byte[] buffer in buffers)
                    {
                        frame.Data = buffer;
                        can.WriteFrame(ref frame);
                        string dataAsHex = string.Join("", frame.Data.ToArray().Select((x) => x.ToString("X2")));
                        Console.WriteLine($"Sending: {dataAsHex}");
                        Thread.Sleep(1000);
                    }
                }
            }
        }

        private static void ReceiveAllExample()
        {
            Console.WriteLine("Listening for any id");

            using (CanRaw can = new CanRaw())
            {
                CanFrame frame = new CanFrame();
                while (true)
                {
                    can.ReadFrame(ref frame);

                    if (frame.IsValid)
                    {
                        string type = frame.ExtendedFrameFormat ? "EFF" : "SFF";
                        string dataAsHex = string.Join("", frame.Data.ToArray().Select((x) => x.ToString("X2")));
                        Console.WriteLine($"Id: {frame.Id} [{type}]: {dataAsHex}");
                    }
                    else
                    {
                        Console.WriteLine($"Invalid frame received!");
                    }
                }
            }
        }

        private static void ReceiveOnAddressExample()
        {
            Console.WriteLine($"Listening for id = {Id}");

            using (CanRaw can = new CanRaw())
            {
                can.Filter(false, Id);
                CanFrame frame = new CanFrame();
                while (true)
                {
                    can.ReadFrame(ref frame);

                    if (frame.IsValid)
                    {
                        string type = frame.ExtendedFrameFormat ? "EFF" : "SFF";
                        string dataAsHex = string.Join("", frame.Data.ToArray().Select((x) => x.ToString("X2")));
                        Console.WriteLine($"Id: {frame.Id} [{type}]: {dataAsHex}");
                    }
                    else
                    {
                        Console.WriteLine($"Invalid frame received!");
                    }
                }
            }
        }
    }
}
