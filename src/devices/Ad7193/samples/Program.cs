using System;
using System.Device.Spi;
using System.Diagnostics;
using System.Threading;
using Ad7193.Metadata;
using Iot.Device.Ad7193;

namespace AD7193Sample
{
    internal class Program
    {
        private static Iot.Device.Ad7193.Ad7193 ad7193;
        private static DateTime firstDataRead;
        private static double lastChecked = 0;
        private static int lastCount = 0;
        private static int samplesTaken = 0;

        private static NetCoreServer.UdpServer udpServer;

        public static void Main(string[] args)
        {
            WaitForDebugger();

            // set SPI bus ID: 0
            // AD7193 CS Pin: 1
            SpiConnectionSettings settings = new SpiConnectionSettings(0, 1)
            {
                ClockFrequency = ((ISpiDeviceMetadata)Iot.Device.Ad7193.Ad7193.GetDeviceMetadata()).MaximumSpiFrequency,
                Mode = SpiMode.Mode3
            };
            SpiDevice ad7193SpiDevice = SpiDevice.Create(settings);

            ad7193 = new Iot.Device.Ad7193.Ad7193(ad7193SpiDevice);
            ad7193.OnValueReceived += Ad7193_AdcValueReceived;

            Console.WriteLine($"-- Resetting and calibrating AD7193.");
            ad7193.Reset();
            ad7193.PGAGain = Gain.X1;
            ad7193.Averaging = AveragingMode.Off;
            ad7193.InputMode = AnalogInputMode.EightPseudoDifferentialAnalogInputs;
            ad7193.AppendStatusRegisterToData = true;
            ad7193.JitterCorrection = true;
            ad7193.Filter = 0;

            Console.WriteLine($"AD7193 before calibration: offset={ad7193.Offset.ToString("X8")}, full-scale={ad7193.FullScale.ToString("X8")}");
            ad7193.Calibrate();
            Console.WriteLine($"AD7193  after calibration: offset={ad7193.Offset.ToString("X8")}, full-scale={ad7193.FullScale.ToString("X8")}");

            Console.WriteLine();
            Console.WriteLine();
            Console.WriteLine("Starting a UDP Server for GNURadio at 192.168.1.36:8843...");
            StartUdpServer("192.168.1.36", 8843);

            Console.WriteLine();
            Console.WriteLine();
            Console.WriteLine("Starting 100 single conversions on CH0...");
            ad7193.ActiveChannels = Channel.CH00;

            for (int i = 0; i < 100; i++)
            {
                ad7193.StartSingleConversion();
                ad7193.WaitForADC();
                ad7193.ReadADCValue();
                Thread.Sleep(25);
            }

            Thread.Sleep(1000);

            Console.WriteLine();
            Console.WriteLine();
            Console.WriteLine("Starting continuous conversion on CH0 and CH1...");
            ad7193.ActiveChannels = Channel.CH00 | Channel.CH01;
            ad7193.StartContinuousConversion();

            int loopcounter = 0;
            while (true)
            {
                loopcounter++;
                if (ad7193.HasErrors || (loopcounter % 50 == 0))
                {
                    Console.WriteLine();
                    Console.WriteLine($"AD7193 status: {ad7193.RegisterToString(Register.Status)}");
                    Console.WriteLine($"AD7193 mode: {ad7193.RegisterToString(Register.Mode)}");
                    Console.WriteLine($"AD7193 config: {ad7193.RegisterToString(Register.Configuration)}");
                    Console.WriteLine();
                    Thread.Sleep(1500);
                }

                Thread.Sleep(250);
            }
        }

        private static void Ad7193_AdcValueReceived(object sender, Iot.Device.Ad7193.AdcValueReceivedEventArgs e)
        {
            if (firstDataRead == DateTime.MinValue)
            {
                firstDataRead = DateTime.UtcNow;
            }

            double secondsElapsed = (DateTime.UtcNow - firstDataRead).TotalSeconds;

            samplesTaken++;

            // show the results in every 0.25 seconds
            if (secondsElapsed - lastChecked > 0.25)
            {
                double sps = (double)(samplesTaken - lastCount) / (secondsElapsed - lastChecked);

                lastCount = samplesTaken;
                lastChecked = secondsElapsed;

                Iot.Device.Ad7193.AdcValue adcValue = e.AdcValue;

                Console.WriteLine($"Channel {adcValue.Channel.ToString().PadLeft(2)}: {adcValue.Voltage.ToString("0.0000").PadLeft(11)} V | {adcValue.Raw.ToString("N0").PadLeft(13)} | {sps.ToString("N1").PadLeft(9)} SPS");
            }

            if ((udpServer != null) && (udpServer.IsStarted))
            {
                udpServer.Send(BitConverter.GetBytes((float)e.AdcValue.Voltage));
            }
        }

        private static void WaitForDebugger()
        {
            int i = 0;
            Console.WriteLine("Waiting for the debugger to attach for 30 seconds... ");
            Console.WriteLine("(To attach the debugger in Visual Studio, press Ctrl+Alt+P, select SSH, set the IP address of the Raspberry Pi, enter your credentials, select the process, and click Attach. Press Shift+Alt+P next time.) ");
            while (true)
            {
                Console.WriteLine(++i + " ");
                if (Debugger.IsAttached)
                {
                    break;
                }

                Thread.Sleep(1000);

                if (i > 30)
                {
                    break;
                }
            }

            Console.WriteLine();
        }

        private static void StartUdpServer(string address, int port)
        {
            udpServer = new NetCoreServer.UdpServer(address, port);
            udpServer.Start();
        }
    }
}
