// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using Iot.Device.Ssd1306.Command;
using System;
using System.Collections.Generic;
using System.Device.I2c;
using System.Device.I2c.Drivers;
using System.Linq;
using System.Net;
using System.Net.NetworkInformation;
using System.Net.Sockets;
using System.Threading;
using System.IO;
using SixLabors.Fonts;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.PixelFormats;
using SixLabors.ImageSharp.Processing;

namespace Iot.Device.Ssd1306.Samples
{
    static class Program
    {
        static void Main(string[] args)
        {
            Console.WriteLine("Hello Ssd1306 Sample!");

            using (Ssd1306 ssd1306 = GetSsd1306WithI2c())
            {
                InitializeSsd1306(ssd1306);
                ClearScreen(ssd1306);
                //SendMessage(ssd1306, "Hello .NET IoT!!!");
                //DisplayIpAddress(ssd1306);
                DisplayImages(ssd1306);
                DisplayClock(ssd1306);
                ClearScreen(ssd1306);
            }
        }

        private static Ssd1306 GetSsd1306WithI2c()
        {
            Console.WriteLine("Using I2C protocol");

            var connectionSettings = new I2cConnectionSettings(1, 0x3C);
            var i2cDevice = new UnixI2cDevice(connectionSettings);
            var ssd1306 = new Ssd1306(i2cDevice);
            return ssd1306;
        }

        // Display size 128x32.
        private static void InitializeSsd1306(Ssd1306 ssd1306)
        {
            ssd1306.SendCommand(new SetDisplayOff());
            ssd1306.SendCommand(new SetDisplayClockDivideRatioOscillatorFrequency(0x00, 0x08));
            ssd1306.SendCommand(new SetMultiplexRatio(0x1F));
            ssd1306.SendCommand(new SetDisplayOffset(0x00));
            ssd1306.SendCommand(new SetDisplayStartLine(0x00));
            ssd1306.SendCommand(new SetChargePump(true));
            ssd1306.SendCommand(new SetMemoryAddressingMode(SetMemoryAddressingMode.AddressingMode.Horizontal));
            ssd1306.SendCommand(new SetSegmentReMap(true));
            ssd1306.SendCommand(new SetComOutputScanDirection(false));
            ssd1306.SendCommand(new SetComPinsHardwareConfiguration(false, false));
            ssd1306.SendCommand(new SetContrastControlForBank0(0x8F));
            ssd1306.SendCommand(new SetPreChargePeriod(0x01, 0x0F));
            ssd1306.SendCommand(new SetVcomhDeselectLevel(SetVcomhDeselectLevel.DeselectLevel.Vcc1_00));
            ssd1306.SendCommand(new EntireDisplayOn(false));
            ssd1306.SendCommand(new SetNormalDisplay());
            ssd1306.SendCommand(new SetDisplayOn());
            ssd1306.SendCommand(new SetColumnAddress());
            ssd1306.SendCommand(new SetPageAddress(PageAddress.Page1, PageAddress.Page3));
        }

        private static void ClearScreen(Ssd1306 ssd1306)
        {
            ssd1306.SendCommand(new SetColumnAddress());
            ssd1306.SendCommand(new SetPageAddress(PageAddress.Page0, PageAddress.Page3));

            for (int cnt = 0; cnt < 32; cnt++)
            {
                byte[] data = new byte[16];
                ssd1306.SendData(data);
            }
        }

        private static void SendMessage(Ssd1306 ssd1306, string message)
        {
            ssd1306.SendCommand(new SetColumnAddress());
            ssd1306.SendCommand(new SetPageAddress(PageAddress.Page0, PageAddress.Page3));

            foreach (char character in message)
            {
                ssd1306.SendData(BasicFont.GetCharacterBytes(character));
            }
        }

        private static void DisplayIpAddress(Ssd1306 ssd1306)
        {
            string ipAddress = GetIpAddress();

            if (ipAddress != null)
            {
                SendMessage(ssd1306, $"IP: {ipAddress}");
            }
            else
            {
                SendMessage(ssd1306, $"Error: IP Address Not Found");
            }
        }

        private static void DisplayImages(Ssd1306 ssd1306)
        {   
            Console.WriteLine("Display Images"); 
            foreach(var image_name in Directory.GetFiles("images", "*.bmp").OrderBy(f => f))
            {
                using (Image<Gray16> image = Image.Load<Gray16>(image_name))
                {
                    ssd1306.DisplayImage(image);
                    Thread.Sleep(1000);
                }
            }
        }

        private static void DisplayClock(Ssd1306 ssd1306)
        {
            Console.WriteLine("Display clock");
            var fontSize = 25;
            var font =  "DejaVu Sans";
            var fontsys = SystemFonts.CreateFont(font, fontSize, FontStyle.Italic);
            var y = 0;

            foreach(var i in Enumerable.Range(0,100))
            {   
                using (Image<Rgba32> image  = new Image<Rgba32>(128, 32))
                {
                    image.Mutate(ctx => ctx
                        .Fill(Rgba32.Black)
                        .DrawText(DateTime.Now.ToString("HH:mm:ss"), fontsys, Rgba32.White, new SixLabors.Primitives.PointF(0, y))
                    );

                    using(Image<Gray16> image_t = image.CloneAs<Gray16>())
                    {
                        ssd1306.DisplayImage(image_t);
                    }

                    y++;
                    if(y >= image.Height) y = 0;

                    Thread.Sleep(100);
                }       
            }
        }
        
        // Referencing https://stackoverflow.com/questions/6803073/get-local-ip-address   
        private static string GetIpAddress()
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
                            continue;

                        // Ignore loopback addresses (e.g., 127.0.0.1).
                        if (IPAddress.IsLoopback(address.Address))
                            continue;

                        return address.Address.ToString();
                    }
                }
            }

            return null;
        }

        // Port from https://github.com/adafruit/Adafruit_Python_SSD1306/blob/8819e2d203df49f2843059d981b7347d9881c82b/Adafruit_SSD1306/SSD1306.py#L184
        public static void DisplayImage(this Ssd1306 s, Image<Gray16> image)
        {
            Int16 width = 128;
            Int16 pages = 4; 
            List<byte> buffer = new List<byte>(); 

            for (int page = 0; page < pages; page++)
                {
                    for (int x = 0; x < width; x++)
                    {
                        int bits = 0;
                        for (byte bit = 0; bit < 8; bit++)
                        {                            
                            bits = bits << 1;
                            bits |= image[x, page * 8 + 7 - bit].PackedValue > 0 ? 1 : 0;                                                                                        
                        } 

                        buffer.Add((byte)bits);          
                    }
                }
            
            int chunk_size = 16;
            for(int i = 0; i < buffer.Count; i += chunk_size)
            {
                s.SendData(buffer.Skip(i).Take(chunk_size).ToArray());
            }
        }
        
    }
}
