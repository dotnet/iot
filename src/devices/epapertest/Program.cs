using System;
using System.Device.Spi;
using System.Device.Spi.Drivers;
using System.Drawing;
using Iot.Device.Il03xx;

namespace epapertest
{
    class Program
    {
        static void Main(string[] args)
        {
            var spiConnectionSettings = new SpiConnectionSettings(0, 0)
            {
                ClockFrequency = 1000000,
                Mode = SpiMode.Mode0
            };
            SpiDevice spi = new UnixSpiDevice(spiConnectionSettings);
            Il0376 display = new Il0376(spi, busyPin: 24, resetPin: 17, dataCommandPin: 25, resolution: new Size(200, 200));

            Console.WriteLine("Turning on...");
            Console.ReadLine();
            display.PowerOn();

            Console.WriteLine("Clearing...");
            Console.ReadLine();
            display.Clear();

            Console.WriteLine("Clearing...");
            Console.ReadLine();
            display.Clear();

            Console.WriteLine("Drawing...");
            Console.ReadLine();

            Bitmap bitmap = new Bitmap(200, 200);

            using (Graphics g = Graphics.FromImage(bitmap))
            {
                g.Clear(Color.White);
                g.FillEllipse(Brushes.Black, 30, 30, 20, 25);
                g.FillEllipse(Brushes.Red, 150, 150, 40, 30);
                g.DrawString("Hello world", new Font(FontFamily.GenericMonospace, 8.0f), Brushes.Black, 30.0f, 100.0f);
            }
            display.SendBitmap(bitmap);

            Console.WriteLine("Image...");
            Console.ReadLine();
            Bitmap image = new Bitmap("dotnetbot3.png");
            display.SendBitmap(image);

            Console.WriteLine("Turning off...");
            Console.ReadLine();
            display.PowerOff();


        }
    }
}
