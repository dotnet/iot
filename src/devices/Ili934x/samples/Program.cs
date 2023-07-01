// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System;
using System.Device.Gpio;
using System.Device.I2c;
using System.Device.Spi;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Threading;
using System.Threading.Tasks;
using Iot.Device.Arduino;
using Iot.Device.Axp192;
using Iot.Device.Common;
using Iot.Device.Ft4222;
using Iot.Device.Graphics;
using Iot.Device.Graphics.SkiaSharpAdapter;
using Iot.Device.Ili934x;
using UnitsNet;

namespace Iot.Device.Ili934x.Samples
{
    internal class Program
    {
        private static LowLevelX11Window? _window = null;

        public static int Main(string[] args)
        {
            bool isFt4222 = false;
            bool isArduino = false;
            IPAddress address = IPAddress.None;
            SkiaSharpAdapter.Register();
            string nmeaSourceAddress = "localhost";

            if (args.Length < 2)
            {
                Console.WriteLine("Are you using Ft4222? Type 'yes' and press ENTER if so, anything else will be treated as no.");
                isFt4222 = Console.ReadLine() == "yes";
                isArduino = true;

                if (!isFt4222)
                {
                    Console.WriteLine("Are you using an Arduino/Firmata? Type 'yes' and press ENTER if so.");
                    isArduino = Console.ReadLine() == "yes";
                }
            }
            else
            {
                if (args[0] == "Ft4222")
                {
                    isFt4222 = true;
                }
                else if (args[0] == "INET" && args.Length >= 2)
                {
                    isArduino = true;
                    IPAddress[] addr = Array.Empty<IPAddress>();
                    try
                    {
                        addr = Dns.GetHostAddresses(args[1]);
                    }
                    catch (SocketException)
                    {
                        // Ignore, will be handled below
                    }

                    if (addr.Any())
                    {
                        address = addr.First();
                    }
                    else
                    {
                        Console.WriteLine($"Could not resolve host: {args[1]}");
                        return 1;
                    }
                }

                if (args.Any(x => x.Equals("--debug", StringComparison.OrdinalIgnoreCase)))
                {
                    Console.WriteLine("Waiting for debugger...");
                    while (!Debugger.IsAttached)
                    {
                        Thread.Sleep(100);
                    }
                }
            }

            var idx = Array.IndexOf(args, "--nmeaserver");
            if (idx >= 0 && args.Length > idx)
            {
                nmeaSourceAddress = args[idx + 1];
            }

            //// TestWindowing();

            int pinDC = isFt4222 ? 1 : 23;
            int pinReset = isFt4222 ? 0 : 24;
            int pinLed = isFt4222 ? 2 : -1;

            if (isArduino)
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

            if (isFt4222)
            {
                gpio = GetGpioControllerFromFt4222();
                displaySPI = GetSpiFromFt4222();
            }
            else if (isArduino)
            {
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
            else
            {
                gpio = new GpioController();
                displaySPI = GetSpiFromDefault();
            }

            Ili9342 display = new Ili9342(displaySPI, pinDC, pinReset, backlightPin: pinLed, gpioController: gpio, spiBufferSize: spiBufferSize, shouldDispose: false);

            if (board != null)
            {
                touch = new Chsc6440(board.CreateI2cDevice(new I2cConnectionSettings(0, Chsc6440.DefaultI2cAddress)), new Size(display.ScreenWidth, display.ScreenHeight), 39, board.CreateGpioController(), false);
                touch.UpdateInterval = TimeSpan.FromMilliseconds(100);
                touch.EnableEvents();
            }

            IInputDeviceSimulator touchSimulator;

            using ScreenCapture screenCapture = new ScreenCapture();
            if (Environment.OSVersion.Platform == PlatformID.Win32NT)
            {
                touchSimulator = new WindowsTouchSimulator();
            }
            else
            {
                touchSimulator = new MouseClickSimulatorUInput(screenCapture.ScreenSize().Width, screenCapture.ScreenSize().Height);
            }

            using RemoteControl ctrol = new RemoteControl(touch, display, powerControl, touchSimulator, screenCapture, nmeaSourceAddress);
            ctrol.DisplayFeatures();

            display.ClearScreen(true);
            if (powerControl != null)
            {
                powerControl.SetLcdVoltage(ElectricPotential.Zero);
                powerControl.Sleep(true);
            }

            touch?.Dispose();

            display.Dispose();

            powerControl?.Dispose();
            board?.Dispose();

            _window?.Dispose();

            return 0;
        }

        private static GpioController GetGpioControllerFromFt4222()
        {
            return new GpioController(PinNumberingScheme.Logical, new Ft4222Gpio());
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

        private static void TestWindowing()
        {
            if (Environment.OSVersion.Platform == PlatformID.Unix)
            {
                _window = new LowLevelX11Window();
                _window.CreateWindow(10, 20, 200, 100);
                _window.StartMessageLoop();
            }
        }
    }
}
