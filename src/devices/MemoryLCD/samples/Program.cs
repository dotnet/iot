using System;
using System.Device.Gpio;
using System.Device.Spi;
using System.Drawing;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Iot.Device.MemoryLcd;

namespace MemoryLcd.Samples
{
    internal class Program
    {
        private static void Main(string[] args)
        {
            // *** LSxxxB7DHxx's chip select is high-level votage enabled ***
            // You can use default ChipSelectLineActiveState value and a NOT gate to inverse this signal
            // If you use gpio pins to control SCS, ChipSelectLineActiveState is optional
            SpiDevice spi = SpiDevice.Create(new SpiConnectionSettings(0, 0)
            {
                ChipSelectLineActiveState = 1, // optional
                ClockFrequency = 2_000_000,
                DataFlow = DataFlow.MsbFirst,
                Mode = 0
            });

            // You can fix DISP and EXTCOMIN in your circuit and use SPI's CE line to economize the GPIO pins
            // DISP -- HIGH
            // EXTCOMIN -- LOW
            // EXTMODE -- LOW
            // LSxxxB7DHxx mlcd = new LS027B7DH01(spi);
            GpioController gpio = new GpioController(PinNumberingScheme.Logical);
            LSxxxB7DHxx mlcd = new LS027B7DH01(spi, gpio, 25, 24, 23);

            Random random = new Random();

            Console.WriteLine("Clear. Press any key to continue...");
            Console.ReadKey();

            mlcd.AllClear();

            Console.WriteLine("DataUpdate1Line. Press any key to continue...");
            Console.ReadKey();

            byte[] lineBuffer = new byte[mlcd.BytesPerLine];
            for (int i = 0; i < mlcd.PixelHeight; i++)
            {
                random.NextBytes(lineBuffer);
                mlcd.DataUpdate1Line((byte)i, lineBuffer);
            }

            Console.WriteLine("Clear(inverse). Press any key to continue...");
            Console.ReadKey();

            mlcd.AllClear(true);

            Console.WriteLine("DataUpdateMultipleLines. Press any key to continue...");
            Console.ReadKey();

            byte[] lineNumberBuffer = Enumerable.Range(0, mlcd.PixelHeight).Select(m => (byte)m).ToArray();
            byte[] frameBuffer = new byte[mlcd.BytesPerLine * mlcd.PixelHeight];

            // LS027B7DH01 needs split bytes into 4 spans on RaspberryPi
            int frameSplit = 4; // mlcd is LS027B7DH01 ? 4 : 1;
            int linesToSend = mlcd.PixelHeight / frameSplit;
            int bytesToSend = frameBuffer.Length / frameSplit;

            random.NextBytes(frameBuffer);
            for (int fs = 0; fs < frameSplit; fs++)
            {
                Span<byte> lineNumbers = lineNumberBuffer.AsSpan(linesToSend * fs, linesToSend);
                Span<byte> bytes = frameBuffer.AsSpan(bytesToSend * fs, bytesToSend);

                mlcd.DataUpdateMultipleLines(lineNumbers, bytes);
            }

            Console.WriteLine("Show image. Press any key to continue...");
            Console.ReadKey();

            Bitmap image = new Bitmap(mlcd.PixelWidth, mlcd.PixelHeight);
            Graphics graphics = Graphics.FromImage(image);
            graphics.Clear(Color.White);

            graphics.DrawImage(Image.FromFile("./images/dotnet-bot.bmp"), 4, 4);
            graphics.DrawImage(Image.FromFile("./images/github-dotnet-iot-black.bmp"), 136, 4);
            graphics.DrawImage(Image.FromFile("./images/shapes.bmp"), 268, 4);

            Brush clearScreenBrush = new SolidBrush(Color.White);
            Brush textBrush = new SolidBrush(Color.Black);
            Font textFont = new Font("Sans Serif", 12, FontStyle.Regular);

            int fpsCpunter = 0;
            int fps = 0;
            Task.Run(() =>
            {
                while (true)
                {
                    fps = fpsCpunter;
                    fpsCpunter = 0;
                    Thread.Sleep(1000 - DateTime.Now.Millisecond);
                }
            });

            while (true)
            {
                graphics.FillRectangle(clearScreenBrush, 4, 40, 392, 196);

                DateTime time = DateTime.Now;
                string message = string.Join(Environment.NewLine, new[]
                {
                    $"Memory LCD model: {mlcd.GetType().Name}",
                    string.Empty,
                    $"Pixels: {mlcd.PixelWidth} x {mlcd.PixelHeight}",
                    string.Empty,
                    $"Current time: {time}",
                    string.Empty,
                    $"Current time ticks: {time.Ticks}",
                    string.Empty,
                    $"FPS: {fps}",
                });
                graphics.DrawString(message, textFont, textBrush, 4, 40);

                mlcd.ShowImage(image, 4);

                fpsCpunter++;
                Thread.Sleep(0);
            }
        }
    }
}
