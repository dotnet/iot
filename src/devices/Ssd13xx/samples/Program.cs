// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System;
using System.Threading.Tasks;
using System.Device.I2c;
using System.Drawing;
using System.Device.Spi;
using System.Threading;
using System.IO;
using System.Linq;
using System.Device.Gpio;
using Iot.Device.Arduino;
using Iot.Device.Graphics;
using Iot.Device.Graphics.SkiaSharpAdapter;
using Iot.Device.Ssd13xx;
using Iot.Device.Board;
using Ssd13xx.Samples.Simulations;

namespace Ssd13xx.Samples
{
    /// <summary>
    /// Demo entry point
    /// </summary>
    public class Program
    {
        /// <summary>
        /// Entry point
        /// </summary>
        public static async Task Main(string[] args)
        {
            Console.WriteLine("Hello Ssd13xx Sample!");
            SkiaSharpAdapter.Register();

            Program program = new Program();
            if (args.Any(x => x == "--sim"))
            {
                await program.RunSimulationsAsync(args);
            }
            else
            {
                program.Run(args);
            }
        }

        private void Run(string[] args)
        {
            try
            {
                Board? board = null;
                Iot.Device.Ssd13xx.Ssd13xx device;

                I2cDevice? i2cDevice = null;
                SpiDevice? spiDevice = null;

                if (args.Any(x => x == "--arduino"))
                {
                    board = new ArduinoBoard("COM4", 115200);
                    I2cConnectionSettings connectionSettings = new(0, 0x3C);
                    i2cDevice = board.CreateI2cDevice(connectionSettings);
                }
                else if (args.Any(x => x == "--i2c"))
                {
                    I2cConnectionSettings connectionSettings = new(1, 0x3C);
                    i2cDevice = I2cDevice.Create(connectionSettings);
                }

                if (args.Any(x => x == "--1327"))
                {
                    device = new Ssd1327(i2cDevice!);
                }
                else if (args.Any(x => x == "--1309") && args.Any(x => x == "--spi") && args.Any(x => x == "--raspi"))
                {
                    board = new RaspberryPiBoard();

                    var spiSettings = new SpiConnectionSettings(0)
                    {
                        ClockFrequency = 8_000_000,
                    };

                    var gpio = board.CreateGpioController();

                    // SoftwareSpi is substantially slower but useful for testing:
                    // spiDevice = new SoftwareSpi(clk: 11, sdi: -1, sdo: 10, cs: -1, settings: spiSettings, gpioController: gpio);
                    spiDevice = board.CreateSpiDevice(spiSettings);
                    device = new Ssd1309(spiDevice, gpio, csGpioPin: 8, dcGpioPin: 25, rstGpioPin: 27, width: 128, height: 64);

                }
                else
                {
                    device = new Ssd1306(i2cDevice!, 128, 32);
                }

                LogConfiguration(i2cDevice, spiDevice, board, device);

                DisplayClock(device);
                device.ClearScreen();

                DisplayImages(device);
                device.ClearScreen();

                device.Dispose();
                board?.Dispose();
            }
            catch (Exception e)
            {
                Console.WriteLine(e.ToString());
            }
        }

        /// <summary>
        /// Run simulations asynchronously
        /// </summary>
        /// <param name="args">Command line arguments</param>
        private async Task RunSimulationsAsync(string[] args)
        {
            try
            {
                Board? board = null;
                Ssd1309 device;

                I2cDevice? i2cDevice = null;
                SpiDevice? spiDevice = null;

                // TODO: Implement better argument parsing
                if (args.Any(x => x == "--1309") && args.Any(x => x == "--spi") && args.Any(x => x == "--raspi"))
                {
                    board = new RaspberryPiBoard();

                    var spiSettings = new SpiConnectionSettings(0)
                    {
                        ClockFrequency = 8_000_000,
                    };

                    var gpio = board.CreateGpioController();
                    spiDevice = board.CreateSpiDevice(spiSettings);
                    // SoftwareSpi is substantially slower but useful for testing
                    // spiDevice = new SoftwareSpi(clk: 11, sdi: -1, sdo: 10, cs: -1, settings: spiSettings, gpioController: gpio);
                    device = new Ssd1309(spiDevice, gpio, csGpioPin: 8, dcGpioPin: 25, rstGpioPin: 27, width: 128, height: 64);
                }
                else
                {
                    throw new NotSupportedException("Could not find --ssd1309, --spi, or --raspi arguments. Only SSD1309 devices on RaspberryPi boards are supported by this simulation sample.");
                }

                LogConfiguration(i2cDevice, spiDevice, board, device);

                await SimulateFallingSandAsync(device, args);
                device.ClearScreen();

                device?.Dispose();
                board?.Dispose();
                spiDevice?.Dispose();
            }
            catch (Exception e)
            {
                Console.WriteLine(e.ToString());
                if (e.InnerException != null)
                {
                    Console.WriteLine(e.InnerException.ToString());
                }
            }
        }

        private void DisplayImages(GraphicDisplay ssd1306)
        {
            Console.WriteLine("Display Images");
            foreach (var image_name in Directory.GetFiles("iot-sample/images", "*.bmp").OrderBy(f => f))
            {
                using BitmapImage image = BitmapImage.CreateFromFile(image_name);
                ssd1306.DrawBitmap(image);
                Thread.Sleep(1000);
            }
        }

        private void DisplayClock(GraphicDisplay ssd1306)
        {
            Console.WriteLine("Display clock");
            var fontSize = 25;
            var font = "DejaVu Sans";
            var y = 0;

            while (!Console.KeyAvailable)
            {
                using (var image = BitmapImage.CreateBitmap(128, 64, PixelFormat.Format32bppArgb))
                {
                    image.Clear(Color.Black);
                    var g = image.GetDrawingApi();
                    g.DrawText(DateTime.Now.ToString("HH:mm:ss"), font, fontSize, Color.White, new Point(0, y));
                    ssd1306.DrawBitmap(image);

                    y++;
                    if (y >= image.Height)
                    {
                        y = 0;
                    }

                    Thread.Sleep(100);
                }
            }

            Console.ReadKey(true);
        }

        /// <remarks>
        /// This simulation can run at ~45 FPS on a RaspberryPi Zero 2 W (Bookworm) without dropping frames
        /// with the bottleneck being the render time, not the display speed (at 8 Mhz).
        /// The simulation will run at the highest FPS possible while still processing every frame.
        /// Enable debug logging with --debug to see if frames are being skipped on your device.
        /// Usage: <c>./Ssd13xx.Samples --1309 --sim --spi --raspi --fps45 --debug</c>
        /// </remarks>
        private async Task SimulateFallingSandAsync(Ssd1309 ssd1309, string[] args)
        {
            var debug = args.Any(x => x == "--debug");

            var simulation = new FallingSandSimulation(ssd1309, fps: GetFps(args), debug: debug);
            await simulation.StartAsync(5000);
        }

        private void LogConfiguration(I2cDevice? i2cDevice, SpiDevice? spiDevice, Board? board, Iot.Device.Ssd13xx.Ssd13xx display)
        {
            Console.WriteLine($"Configuration:");
            Console.WriteLine($"SPI: {spiDevice != null}");
            Console.WriteLine($"I2C: {i2cDevice != null}");
            Console.WriteLine($"Board: {board?.GetType()}");
            Console.WriteLine($"Display: {display?.GetType()}");
        }

        private int GetFps(string[] args)
        {
            var fpsDefault = 1;
            var fpsPrefix = "--fps";

            if (args.Any(x => x.Substring(0, 5) == fpsPrefix))
            {
                var fpsParsed = int.TryParse(args.FirstOrDefault(x => x.Substring(0, fpsPrefix.Length) == fpsPrefix)?.Substring(fpsPrefix.Length), out var fps);
                if (!fpsParsed)
                {
                    throw new Exception($"FPS could not be parsed. Correct usage example: {fpsPrefix}30, {fpsPrefix}1, {fpsPrefix}101");
                }

                return fps;
            }

            return fpsDefault;
        }
    }

}
