// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System;
using System.Globalization;
using System.Linq;
using System.Threading;
using Iot.Device.Ft200X;
using Iot.Device.Ft200X.Samples;
using Iot.Device.FtCommon;

if (args.Length == 0)
{
    var devices = Ft200XDevice.GetDevices();
    foreach (FtDevice device in devices)
    {
        Console.WriteLine($"{device.SerialNumber}: {device.Description} ({device.Type})");
    }

    if (devices.Count == 0)
    {
        Console.Error.WriteLine("No FT200X found. Check D2XX installation, USB access, and the USB description.");
        return 1;
    }

    Console.WriteLine("FT200X is I2C slave-only. An external I2C master must initiate transfers.");
    Console.WriteLine("Use --status SERIAL, --read SERIAL COUNT, --write SERIAL HEX, --sensor SERIAL, or --get-address SERIAL.");
    Console.WriteLine("Explicit persistent programming: --program-address SERIAL HEX --confirm");
    return 0;
}

if ((args.Length != 2 || (args[0] != "--status" && args[0] != "--sensor" && args[0] != "--get-address")) &&
    (args.Length != 3 || (args[0] != "--read" && args[0] != "--write")) &&
    (args.Length != 4 || args[0] != "--program-address" || args[3] != "--confirm"))
{
    Console.Error.WriteLine("Usage: --status SERIAL | --read SERIAL COUNT | --write SERIAL HEX | --sensor SERIAL | --get-address SERIAL | --program-address SERIAL HEX --confirm");
    return 1;
}

byte[] payload = Array.Empty<byte>();
int readCount = 0;
int address = 0;
if (args[0] == "--read")
{
    if (!int.TryParse(args[2], NumberStyles.None, CultureInfo.InvariantCulture, out readCount) || readCount < 1 || readCount > 65536)
    {
        Console.Error.WriteLine("COUNT must be between 1 and 65536 bytes.");
        return 1;
    }
}
else if (args[0] == "--write")
{
    if (args[2].Length == 0 || args[2].Length % 2 != 0 || !args[2].All(Uri.IsHexDigit))
    {
        Console.Error.WriteLine("HEX must contain a nonempty, even number of hexadecimal digits, for example 01020304.");
        return 1;
    }

    payload = Convert.FromHexString(args[2]);
}
else if (args[0] == "--program-address")
{
    string text = args[2].StartsWith("0x", StringComparison.OrdinalIgnoreCase) ? args[2].Substring(2) : args[2];
    if (!int.TryParse(text, NumberStyles.AllowHexSpecifier, CultureInfo.InvariantCulture, out address) || address < 0x08 || address > 0x77)
    {
        Console.Error.WriteLine("HEX must be a nonreserved 7-bit address from 0x08 to 0x77, for example 0x22.");
        return 1;
    }

    Console.WriteLine("WARNING: This persistently programs MTP memory (2,000-cycle endurance).");
    Console.WriteLine("Verify the physical FT200X, back up its configuration, and stop the external I2C master first.");
}

// Explicit serial selection also supports FT200X devices with customized descriptions.
FtDevice? selected = FtCommon.GetDevices().SingleOrDefault(device => device.Type == FtDeviceType.FtXSeries && device.SerialNumber == args[1]);
if (selected is null)
{
    Console.Error.WriteLine("No FT-X device with the specified serial number was found. Verify that your adapter is an FT200X.");
    return 1;
}

using var adapter = new Ft200XDevice(selected);
if (args[0] == "--program-address")
{
    bool changed = adapter.ProgramI2cAddress(address);
    Console.WriteLine(changed
        ? $"Programmed and verified address 0x{address:X2}; other configuration fields and USB strings verified unchanged. Connection closed. Disconnect/reconnect USB, then start a new sample instance."
        : $"Stored address is already 0x{address:X2}. No MTP write performed. This does not verify the currently active address.");
}
else if (args[0] == "--get-address")
{
    Console.WriteLine($"Stored I2C address: 0x{adapter.ProgrammedI2cAddress:X2}. This reads MTP, not the active I2C address.");
}
else if (args[0] == "--sensor")
{
    RunSensor(adapter);
}
else if (args[0] == "--write")
{
    adapter.Write(payload);
    Console.WriteLine($"Queued {payload.Length} bytes for the external I2C master. Delivery on I2C is not confirmed.");
}
else if (args[0] == "--read")
{
    byte[] buffer = new byte[readCount];
    int count = adapter.Read(buffer);
    Console.WriteLine($"Received {count} bytes: {Convert.ToHexString(buffer.AsSpan(0, count))}");
    if (count != readCount)
    {
        Console.Error.WriteLine("The read timeout elapsed before the requested byte count was received.");
        return 1;
    }
}
else
{
    Console.WriteLine($"Opened FT200X. USB receive queue: {adapter.BytesAvailable} bytes.");
}

return 0;

static void RunSensor(Ft200XDevice adapter)
{
    Console.WriteLine("Temperature sensor demo: 23.50 C, ID register 0x00, temperature 0x01, configuration 0x02.");
    Console.WriteLine("Configure the FT200X's 7-bit address to 0x22 using --program-address SERIAL 0x22 --confirm or FT_PROG, then reconnect USB.");
    Console.WriteLine("This sample does not set or verify the chip's programmed address.");
    Console.WriteLine("Master: write [command, register, value MSB, value LSB], wait, then read a 4-byte reply.");
    Console.WriteLine("Commands: 0 = read, 1 = write. Reply: [status, register, value MSB, value LSB].");
    Console.WriteLine("Do not use WriteRead/repeated START. Allow USB processing time; see README for master code.");
    Console.WriteLine("Send one request at a time and consume the entire reply before the next request. Ctrl+C stops.");

    var runner = new FifoSensorRunner(
        adapter, new TemperatureSensor(), response => Console.WriteLine($"Reply: {Convert.ToHexString(response.Span)}"));
    using var cancellation = new CancellationTokenSource();
    ConsoleCancelEventHandler cancelHandler = (_, e) =>
    {
        e.Cancel = true;
        cancellation.Cancel();
    };
    Console.CancelKeyPress += cancelHandler;
    try
    {
        runner.Run(cancellation.Token);
    }
    finally
    {
        Console.CancelKeyPress -= cancelHandler;
    }
}
