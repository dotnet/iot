// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System;
using System.Device.Gpio;
using System.Device.I2c;
using System.Device.Spi;
using System.Diagnostics;
using System.Drawing;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Threading;
using System.Threading.Tasks;
using CommandLine;
using Iot.Device.Arduino;
using Iot.Device.Axp192;
using Iot.Device.Common;
using Iot.Device.Ft4222;
using Iot.Device.Graphics;
using Iot.Device.Graphics.SkiaSharpAdapter;
using Iot.Device.Gui;
using Iot.Device.Ili934x;
using Iot.Device.M5Stack;
using UnitsNet;

namespace Iot.Device.Ili934x.Samples
{
    internal class Program
    {
        public static int Main(string[] args)
        {
            SkiaSharpAdapter.Register();

            var parser = new Parser(x =>
            {
                x.AutoHelp = true;
                x.AutoVersion = true;
                x.CaseInsensitiveEnumValues = true;
                x.ParsingCulture = CultureInfo.InvariantCulture;
                x.CaseSensitive = false;
                x.HelpWriter = Console.Out;
            });

            var parsed = parser.ParseArguments<Arguments>(args);

            if (parsed.Errors.Any())
            {
                Console.WriteLine("Error in command line");
                return 1;
            }

            Arguments parsedArguments = parsed.Value;

            if (parsedArguments.Debug)
            {
                Console.WriteLine("Waiting for debugger...");
                while (!Debugger.IsAttached)
                {
                    Thread.Sleep(100);
                }
            }
            

            int pinDC = parsedArguments.IsFt4222 ? 1 : 23;
            int pinReset = parsedArguments.IsFt4222 ? 0 : 24;
            int pinLed = parsedArguments.IsFt4222 ? 2 : -1;

            if (!parsedArguments.IsFt4222)
            {
                // Pin mappings for the display in an M5Core2/M5Though
                pinDC = 15;
                pinReset = -1;
                pinLed = -1;
            }

            LogDispatcher.LoggerFactory = new SimpleConsoleLoggerFactory();
            SpiDevice displaySPI;
            ArduinoBoard? board = null;
            GpioController gpio;
            int spiBufferSize = 4096;
            M5ToughPowerControl? powerControl = null;
            Chsc6440? touch = null;

            if (parsedArguments.IsFt4222)
            {
                gpio = GetGpioControllerFromFt4222();
                displaySPI = GetSpiFromFt4222();
            }
            else
            {
                IPAddress[] addr = Array.Empty<IPAddress>();
                try
                {
                    addr = Dns.GetHostAddresses(parsedArguments.M5Address);
                }
                catch (SocketException)
                {
                    // Ignore, will be handled below
                }

                IPAddress address;
                if (addr.Any())
                {
                    address = addr.First();
                }
                else
                {
                    Console.WriteLine($"Could not resolve host: {parsedArguments.M5Address}");
                    return 1;
                }

                if (!ArduinoBoard.TryConnectToNetworkedBoard(address, 27016, out board))
                {
                    throw new IOException("Couldn't connect to board");
                }

                gpio = board.CreateGpioController();
                displaySPI = board.CreateSpiDevice(new SpiConnectionSettings(0, 5)
                {
                    ClockFrequency = 50_000_000
                });
                spiBufferSize = 25;
                if (board.GetSystemVariable(SystemVariable.MaxSysexSize, out int maxSize))
                {
                    int maxPayloadSizePerMsg = Encoder7Bit.Num8BitOutBytes(maxSize - 6);
                    spiBufferSize = maxPayloadSizePerMsg;
                }

                powerControl = new M5ToughPowerControl(board);
                powerControl.EnableSpeaker = false; // With my current firmware, it's used instead of the status led. Noisy!
                powerControl.Sleep(false);
            }

            Ili9342 display = new Ili9342(displaySPI, pinDC, pinReset, backlightPin: pinLed, gpioController: gpio, spiBufferSize: spiBufferSize, shouldDispose: false);

            if (board != null)
            {
                touch = new Chsc6440(board.CreateI2cDevice(new I2cConnectionSettings(0, Chsc6440.DefaultI2cAddress)), 
                    new Size(display.ScreenWidth, display.ScreenHeight), parsedArguments.FlipScreen, 39, board.CreateGpioController(), false);
                touch.UpdateInterval = TimeSpan.FromMilliseconds(100);
                touch.EnableEvents();
            }

            IPointingDevice touchSimulator;

            using ScreenCapture screenCapture = new ScreenCapture();
            var size = screenCapture.ScreenSize();
            touchSimulator = VirtualPointingDevice.CreateAbsolute(size.Width, size.Height);

            using RemoteControl ctrol = new RemoteControl(touch, display, powerControl, touchSimulator, screenCapture, parsedArguments);
            ctrol.Run();

            display.ClearScreen(true);
            if (powerControl != null)
            {
                powerControl.SetLcdVoltage(ElectricPotential.Zero);
                if (!parsedArguments.NoSleep)
                {
                    powerControl.Sleep(true);
                }
            }

            touch?.Dispose();

            display.Dispose();

            powerControl?.Dispose();
            board?.Dispose();

            return 0;
        }

        private static GpioController GetGpioControllerFromFt4222()
        {
            return new GpioController(new Ft4222Gpio());
        }

        private static SpiDevice GetSpiFromFt4222()
        {
            return new Ft4222Spi(new SpiConnectionSettings(0, 1)
            {
                ClockFrequency = Ili9341.DefaultSpiClockFrequency, Mode = Ili9341.DefaultSpiMode
            });
        }

        private static SpiDevice GetSpiFromDefault()
        {
            return SpiDevice.Create(new SpiConnectionSettings(0, 0)
            {
                ClockFrequency = Ili9341.DefaultSpiClockFrequency, Mode = Ili9341.DefaultSpiMode
            });
        }
    }
}
