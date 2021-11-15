// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Iot.Device.SocketCan;

CanId id = new CanId()
{
    Standard = 0x1A // arbitrary id
};

Dictionary<string, Func<string, ValueTask>> samples = new()
{
    { "send", SendExample },
    { "receive", ReceiveAllExample },
    { "receive-on-specific-address", ReceiveOnAddressExample },
};

if (args.Length == 0 || !samples.ContainsKey(args[0]))
{
    Console.WriteLine("Usage: SocketCan.Sample <sample-name>");
    Console.WriteLine("Available samples:");

    foreach (var kv in samples)
    {
        Console.WriteLine($"- {kv.Key}");
    }

    return;
}

await samples[args[0]](args.Length > 1 ? args[1] : "can0");

async ValueTask SendExample(string interfaceName)
{
    Console.WriteLine($"Sending to id = 0x{id.Value:X2}");

    using CanRaw can = new CanRaw(interfaceName);
    byte[][] buffers = new byte[][]
    {
        new byte[8] { 1, 2, 3, 40, 50, 60, 70, 80 },
        new byte[7] { 1, 2, 3, 40, 50, 60, 70 },
        Array.Empty<byte>(),
        new byte[1] { 254 },
    };

    if (!id.IsValid)
    {
        // This is more form of the self-test rather than actual part of the sample
        throw new Exception("Id is invalid");
    }

    while (true)
    {
        foreach (byte[] buffer in buffers)
        {
            // can.WriteFrame(buffer, id); // old, synchronous implementation - kept for backward compatibility
            await can.WriteFrameAsync(buffer, id);
            string dataAsHex = string.Join(string.Empty, buffer.Select((x) => x.ToString("X2")));
            Console.WriteLine($"Sending: {dataAsHex}");
            Thread.Sleep(1000);
        }
    }
}

async ValueTask ReceiveAllExample(string interfaceName)
{
    Console.WriteLine("Listening for any id");

    using CanRaw can = new(interfaceName);

    var buffer = new byte[8];

    var counter = 0;
    while (true)
    {
        (int frameLength, CanId receivedId) = await can.ReadFrameAsync(buffer);

        string type = receivedId.ExtendedFrameFormat ? "EFF" : "SFF";
        string dataAsHex = string.Join(string.Empty, buffer.Take(frameLength).Select((x) => x.ToString("X2")));
        Console.WriteLine($"{++counter}: Id: 0x{receivedId.Value:X2} [{type}]: {dataAsHex}");
    }
}

async ValueTask ReceiveOnAddressExample(string interfaceName)
{
    Console.WriteLine($"Listening for id = 0x{id.Value:X2}");

    using CanRaw can = new(interfaceName);
    byte[] buffer = new byte[8];
    can.Filter(id);

    while (true)
    {
        var frame = await can.ReadFrameAsync(buffer);

        string type = id.ExtendedFrameFormat ? "EFF" : "SFF";
        string dataAsHex = string.Join(string.Empty, buffer.Select((x) => x.ToString("X2")));
        Console.WriteLine($"Id: 0x{id.Value:X2} [{type}]: {dataAsHex}");
    }
}
