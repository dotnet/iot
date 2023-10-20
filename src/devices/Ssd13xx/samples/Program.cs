// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System;
using System.Collections.Generic;
using System.Device.I2c;
using System.Drawing;
using System.Linq;
using System.Net;
using System.Net.NetworkInformation;
using System.Net.Sockets;
using System.Threading;
using System.IO;
using Iot.Device.Arduino;
using Iot.Device.Graphics;
using Iot.Device.Graphics.SkiaSharpAdapter;
using Iot.Device.Ssd13xx;
using Iot.Device.Ssd13xx.Commands;
using Iot.Device.Ssd13xx.Samples;
using Ssd1306Cmnds = Iot.Device.Ssd13xx.Commands.Ssd1306Commands;
using Ssd1327Cmnds = Iot.Device.Ssd13xx.Commands.Ssd1327Commands;

/// <summary>
/// Demo entry point
/// </summary>
public class Program
{
    /// <summary>
    /// Entry point
    /// </summary>
    public static int Main(string[] args)
    {
        Console.WriteLine("Hello Ssd1306 Sample!");
        SkiaSharpAdapter.Register();

        Program program = new Program();
        program.Run(args);

        return 0;
    }

    private void Run(string[] args)
    {
        ArduinoBoard? board = null;
        Ssd13xx device;
        Console.WriteLine("Using direct I2C protocol");

        I2cDevice? i2cDevice = null;

        if (args.Any(x => x == "--arduino"))
        {
            board = new ArduinoBoard("COM4", 115200);
            I2cConnectionSettings connectionSettings = new(0, 0x3C);
            i2cDevice = board.CreateI2cDevice(connectionSettings);
        }
        else
        {
            I2cConnectionSettings connectionSettings = new(1, 0x3C);
            i2cDevice = I2cDevice.Create(connectionSettings);
        }

        if (args.Any(x => x == "--1327"))
        {
            var device1 = new Ssd1327(i2cDevice);
            // Todo: Move these to implementation
            InitializeSsd1327(device1);
            ClearScreenSsd1327(device1);
            device = device1;
        }
        else
        {
            var device2 = new Ssd1306(i2cDevice);
            InitializeSsd1306(device2);
            ClearScreenSsd1306(device2);
            device = device2;
            DisplayImages(device2);
            DisplayClock(device2);
            ClearScreenSsd1306(device2);
        }

        // SendMessage(device, "Hello .NET IoT!!!");
        device.Dispose();
        board?.Dispose();
    }

    // Display size 128x32.
    private void InitializeSsd1306(Ssd1306 device)
    {
        device.SendCommand(new SetDisplayOff());
        device.SendCommand(new Ssd1306Cmnds.SetDisplayClockDivideRatioOscillatorFrequency(0x00, 0x08));
        device.SendCommand(new SetMultiplexRatio(0x1F));
        device.SendCommand(new Ssd1306Cmnds.SetDisplayOffset(0x00));
        device.SendCommand(new Ssd1306Cmnds.SetDisplayStartLine(0x00));
        device.SendCommand(new Ssd1306Cmnds.SetChargePump(true));
        device.SendCommand(
            new Ssd1306Cmnds.SetMemoryAddressingMode(Ssd1306Cmnds.SetMemoryAddressingMode.AddressingMode
                .Horizontal));
        device.SendCommand(new Ssd1306Cmnds.SetSegmentReMap(true));
        device.SendCommand(new Ssd1306Cmnds.SetComOutputScanDirection(false));
        device.SendCommand(new Ssd1306Cmnds.SetComPinsHardwareConfiguration(false, false));
        device.SendCommand(new SetContrastControlForBank0(0x8F));
        device.SendCommand(new Ssd1306Cmnds.SetPreChargePeriod(0x01, 0x0F));
        device.SendCommand(
            new Ssd1306Cmnds.SetVcomhDeselectLevel(Ssd1306Cmnds.SetVcomhDeselectLevel.DeselectLevel.Vcc1_00));
        device.SendCommand(new Ssd1306Cmnds.EntireDisplayOn(false));
        device.SendCommand(new Ssd1306Cmnds.SetNormalDisplay());
        device.SendCommand(new SetDisplayOn());
        device.SendCommand(new Ssd1306Cmnds.SetColumnAddress());
        device.SendCommand(new Ssd1306Cmnds.SetPageAddress(Ssd1306Cmnds.PageAddress.Page1,
            Ssd1306Cmnds.PageAddress.Page3));
    }

    // Display size 96x96.
    private void InitializeSsd1327(Ssd1327 device)
    {
        device.SendCommand(new Ssd1327Cmnds.SetUnlockDriver(true));
        device.SendCommand(new SetDisplayOff());
        device.SendCommand(new SetMultiplexRatio(0x5F));
        device.SendCommand(new Ssd1327Cmnds.SetDisplayStartLine());
        device.SendCommand(new Ssd1327Cmnds.SetDisplayOffset(0x5F));
        device.SendCommand(new Ssd1327Cmnds.SetReMap());
        device.SendCommand(new Ssd1327Cmnds.SetInternalVddRegulator(true));
        device.SendCommand(new SetContrastControlForBank0(0x53));
        device.SendCommand(new Ssd1327Cmnds.SetPhaseLength(0X51));
        device.SendCommand(new Ssd1327Cmnds.SetDisplayClockDivideRatioOscillatorFrequency(0x01, 0x00));
        device.SendCommand(new Ssd1327Cmnds.SelectDefaultLinearGrayScaleTable());
        device.SendCommand(new Ssd1327Cmnds.SetPreChargeVoltage(0x08));
        device.SendCommand(new Ssd1327Cmnds.SetComDeselectVoltageLevel(0X07));
        device.SendCommand(new Ssd1327Cmnds.SetSecondPreChargePeriod(0x01));
        device.SendCommand(new Ssd1327Cmnds.SetSecondPreChargeVsl(true));
        device.SendCommand(new Ssd1327Cmnds.SetNormalDisplay());
        device.SendCommand(new DeactivateScroll());
        device.SendCommand(new SetDisplayOn());
        device.SendCommand(new Ssd1327Cmnds.SetRowAddress());
        device.SendCommand(new Ssd1327Cmnds.SetColumnAddress());
    }

    private void ClearScreenSsd1306(Ssd1306 device)
    {
        device.SendCommand(new Ssd1306Cmnds.SetColumnAddress());
        device.SendCommand(new Ssd1306Cmnds.SetPageAddress(Ssd1306Cmnds.PageAddress.Page0,
            Ssd1306Cmnds.PageAddress.Page3));

        for (int cnt = 0; cnt < 32; cnt++)
        {
            byte[] data = new byte[16];
            device.SendData(data);
        }
    }

    private void ClearScreenSsd1327(Ssd1327 device)
    {
        device.ClearDisplay();
    }

    private void SendMessageSsd1306(Ssd1306 device, string message)
    {
        device.SendCommand(new Ssd1306Cmnds.SetColumnAddress());
        device.SendCommand(new Ssd1306Cmnds.SetPageAddress(Ssd1306Cmnds.PageAddress.Page0,
            Ssd1306Cmnds.PageAddress.Page3));

        foreach (char character in message)
        {
            device.SendData(BasicFont.GetCharacterBytes(character));
        }
    }

    private void SendMessageSsd1327(Ssd1327 device, string message)
    {
        device.SetRowAddress(0x00, 0x07);

        foreach (char character in message)
        {
            byte[] charBitMap = BasicFont.GetCharacterBytes(character);
            List<byte> data = new List<byte>();
            for (var i = 0; i < charBitMap.Length; i = i + 2)
            {
                for (var j = 0; j < 8; j++)
                {
                    byte cdata = 0x00;
                    int bit1 = (byte)((charBitMap[i] >> j) & 0x01);
                    cdata |= (bit1 == 1) ? (byte)0xF0 : (byte)0x00;
                    var secondBitIndex = i + 1;
                    if (secondBitIndex < charBitMap.Length)
                    {
                        int bit2 = (byte)((charBitMap[i + 1] >> j) & 0x01);
                        cdata |= (bit2 == 1) ? (byte)0x0F : (byte)0x00;
                    }

                    data.Add(cdata);
                }
            }

            device.SendData(data.ToArray());
        }
    }

    private string DisplayIpAddress()
    {
        string? ipAddress = GetIpAddress();

        if (ipAddress is null)
        {
            return $"IP:{ipAddress}";
        }
        else
        {
            return $"Error: IP Address Not Found";
        }
    }

    private void DisplayImages(Ssd1306 ssd1306)
    {
        Console.WriteLine("Display Images");
        foreach (var image_name in Directory.GetFiles("images", "*.bmp").OrderBy(f => f))
        {
            using BitmapImage image = BitmapImage.CreateFromFile(image_name);
            ssd1306.DisplayImage(image);
            Thread.Sleep(1000);
        }
    }

    private void DisplayClock(Ssd1306 ssd1306)
    {
        Console.WriteLine("Display clock");
        var fontSize = 25;
        var font = "DejaVu Sans";
        var y = 0;

        foreach (var i in Enumerable.Range(0, 100))
        {
            using (var image = BitmapImage.CreateBitmap(128, 32, PixelFormat.Format32bppArgb))
            {
                image.Clear(Color.Black);
                var g = image.GetDrawingApi();
                g.DrawText(DateTime.Now.ToString("HH:mm:ss"), font, fontSize, Color.White, new Point(0, y));
                ssd1306.DisplayImage(image);

                y++;
                if (y >= image.Height)
                {
                    y = 0;
                }

                Thread.Sleep(100);
            }
        }
    }

    // Referencing https://stackoverflow.com/questions/6803073/get-local-ip-address
    private string? GetIpAddress()
    {
        // Get a list of all network interfaces (usually one per network card, dialup, and VPN connection).
        NetworkInterface[] networkInterfaces = NetworkInterface.GetAllNetworkInterfaces();

        foreach (NetworkInterface network in networkInterfaces)
        {
            // Read the IP configuration for each network
            IPInterfaceProperties properties = network.GetIPProperties();

            if (network.NetworkInterfaceType == NetworkInterfaceType.Ethernet &&
                network.OperationalStatus == OperationalStatus.Up &&
                !network.Description.ToLower().Contains("virtual") &&
                !network.Description.ToLower().Contains("pseudo"))
            {
                // Each network interface may have multiple IP addresses.
                foreach (IPAddressInformation address in properties.UnicastAddresses)
                {
                    // We're only interested in IPv4 addresses for now.
                    if (address.Address.AddressFamily != AddressFamily.InterNetwork)
                    {
                        continue;
                    }

                    // Ignore loopback addresses (e.g., 127.0.0.1).
                    if (IPAddress.IsLoopback(address.Address))
                    {
                        continue;
                    }

                    return address.Address.ToString();
                }
            }
        }

        return null;
    }
}
