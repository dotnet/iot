// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using Iot.Device.Ssd13xx.Commands;
using Ssd1306Cmnds = Iot.Device.Ssd13xx.Commands.Ssd1306Commands;
using Ssd1327Cmnds = Iot.Device.Ssd13xx.Commands.Ssd1327Commands;
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

namespace Iot.Device.Ssd13xx.Samples
{
    static class Program
    {
        static void Main(string[] args)
        {
            Console.WriteLine("Hello Ssd1306 Sample!");

#if SSD1327
            using (Ssd1327 device = GetSsd1327WithI2c())
            {
                Initialize(device);
                ClearScreen(device);
                //SendMessage(ssd1306, "Hello .NET IoT!!!");
                SendMessage(device, DisplayIpAddress());
            }
#else
            using (Ssd1306 device = GetSsd1306WithI2c())
            {
                Initialize(device);
                ClearScreen(device);
                //SendMessage(ssd1306, "Hello .NET IoT!!!");
                //SendMessage(device, DisplayIpAddress());
                DisplayImages(device);
                DisplayClock(device);
                ClearScreen(device);
            }
#endif
        }

        private static UnixI2cDevice GetI2CDevice(){
            Console.WriteLine("Using I2C protocol");

            var connectionSettings = new I2cConnectionSettings(1, 0x3C);
            return new UnixI2cDevice(connectionSettings);
        }

        private static Ssd1327 GetSsd1327WithI2c()
        {
            return new Ssd1327(GetI2CDevice());
        }

        private static Ssd1306 GetSsd1306WithI2c()
        {
            return new Ssd1306(GetI2CDevice());
        }
        
        // Display size 128x32.
        private static void Initialize(Ssd1306 device)
        {
            device.SendCommand(new SetDisplayOff());
            device.SendCommand(new Ssd1306Cmnds.SetDisplayClockDivideRatioOscillatorFrequency(0x00, 0x08));
            device.SendCommand(new SetMultiplexRatio(0x1F));
            device.SendCommand(new Ssd1306Cmnds.SetDisplayOffset(0x00));
            device.SendCommand(new Ssd1306Cmnds.SetDisplayStartLine(0x00));
            device.SendCommand(new Ssd1306Cmnds.SetChargePump(true));
            device.SendCommand(new Ssd1306Cmnds.SetMemoryAddressingMode(Ssd1306Cmnds.SetMemoryAddressingMode.AddressingMode.Horizontal));
            device.SendCommand(new Ssd1306Cmnds.SetSegmentReMap(true));
            device.SendCommand(new Ssd1306Cmnds.SetComOutputScanDirection(false));
            device.SendCommand(new Ssd1306Cmnds.SetComPinsHardwareConfiguration(false, false));
            device.SendCommand(new SetContrastControlForBank0(0x8F));
            device.SendCommand(new Ssd1306Cmnds.SetPreChargePeriod(0x01, 0x0F));
            device.SendCommand(new Ssd1306Cmnds.SetVcomhDeselectLevel(Ssd1306Cmnds.SetVcomhDeselectLevel.DeselectLevel.Vcc1_00));
            device.SendCommand(new Ssd1306Cmnds.EntireDisplayOn(false));
            device.SendCommand(new Ssd1306Cmnds.SetNormalDisplay());
            device.SendCommand(new SetDisplayOn());
            device.SendCommand(new Ssd1306Cmnds.SetColumnAddress());
            device.SendCommand(new Ssd1306Cmnds.SetPageAddress(Ssd1306Cmnds.PageAddress.Page1, Ssd1306Cmnds.PageAddress.Page3));
        }

        // Display size 96x96.
        private static void Initialize(Ssd1327 device) 
        {
            device.SendCommand(new Ssd1327Cmnds.SetUnlockDriver());
            device.SendCommand(new SetDisplayOff());
            device.SendCommand(new SetMultiplexRatio(0x5F));
            device.SendCommand(new Ssd1327Cmnds.SetDisplayStartLine());
            device.SendCommand(new Ssd1327Cmnds.SetDisplayOffset(0x5F));
            device.SendCommand(new Ssd1327Cmnds.SetReMap());
            device.SendCommand(new Ssd1327Cmnds.SetInternalVddRegulator());
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

        private static void ClearScreen(Ssd1306 device)
        {
            device.SendCommand(new Ssd1306Cmnds.SetColumnAddress());
            device.SendCommand(new Ssd1306Cmnds.SetPageAddress(Ssd1306Cmnds.PageAddress.Page0, Ssd1306Cmnds.PageAddress.Page3));

            for (int cnt = 0; cnt < 32; cnt++)
            {
                byte[] data = new byte[16];
                device.SendData(data);
            }
        }

        private static void ClearScreen(Ssd1327 device)
        {
            device.ClearDisplay();
        }

        private static void SendMessage(Ssd1306 device, string message)
        {
            device.SendCommand(new Ssd1306Cmnds.SetColumnAddress());
            device.SendCommand(new Ssd1306Cmnds.SetPageAddress(Ssd1306Cmnds.PageAddress.Page0, Ssd1306Cmnds.PageAddress.Page3));

            foreach (char character in message)
            {
                device.SendData(BasicFont.GetCharacterBytes(character));
            }
        }

        private static void SendMessage(Ssd1327 device, string message)
        {
            device.SetRowAddress(0x00, 0x07);

            foreach (char character in message)
            {
                byte[] charBitMap = BasicFont.GetCharacterBytes(character);
                List<byte> data = new List<byte>();
                for(var i = 0; i < charBitMap.Length; i = i + 2)
                {
                    for(var j = 0; j < 8; j++)
                    {
                        byte cdata = 0x00;
                        int bit1 = (byte)((charBitMap[i] >> j) & 0x01);
                        cdata |= (bit1 == 1) ? (byte)0xF0 : (byte)0x00;
                        var secondBitIndex = i + 1;
                        if (secondBitIndex < charBitMap.Length) {
                            int bit2 = (byte)((charBitMap[i + 1] >> j) & 0x01);
                            cdata |= (bit2 == 1) ? (byte)0x0F : (byte)0x00;
                        }

                        data.Add(cdata);
                    }
                }

                device.SendData(data.ToArray());
            }
        }

        private static string DisplayIpAddress()
        {
            string ipAddress = GetIpAddress();

            if (ipAddress != null)
            {
                return $"IP:{ipAddress}";
            }
            else
            {
                return $"Error: IP Address Not Found";
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
