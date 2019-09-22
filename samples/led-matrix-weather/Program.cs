// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Iot.Device.LEDMatrix;
using Iot.Device.Graphics;
using System.Drawing;
using System.Numerics;
using System.Diagnostics;
using System.Net;
using System.Net.Sockets;
using System.Net.NetworkInformation;

namespace LedMatrixWeather
{
    partial class Program
    {
        static Action<RGBLedMatrix> s_scenario = WeatherDemo;
        static Stopwatch s_showLocalIp = null;
        static string[] s_ips;
        static bool s_networkAvailable = false;
        static Weather s_client;
        static OpenWeatherResponse s_weatherResponse;
        static TimeZoneInfo s_timeZonePst = TimeZoneInfo.FindSystemTimeZoneById("America/Los_Angeles");

        static void Main(string[] args)
        {
            if (args.Length != 1)
            {
                Console.WriteLine("Usage: led-matrix-weather <openweathermap.org API key>");
                Console.WriteLine();
                Console.WriteLine("If wrong key is used or there is no network connection");
                Console.WriteLine("example data will be displayed.");
                Console.WriteLine();
                Console.WriteLine("For the first 30 seconds of execution");
                Console.WriteLine("IP addresses will be displayed instead of data.");
                return;
            }

            s_client = new Weather(args[0]);

            UpdateWeather();
            Task.Run(WeatherUpdater);
            Task.Run(IpsGetter);

            PinMapping mapping = PinMapping.MatrixBonnetMapping32;
            RGBLedMatrix matrix = new RGBLedMatrix(mapping, 64, 64, 2, 2);

            Task drawing = Task.Run(() =>
            {
                matrix.StartRendering();

                while (true)
                {
                    Action<RGBLedMatrix> scenario = s_scenario;

                    if (scenario == null)
                        break;

                    Stopwatch sw = Stopwatch.StartNew();
                    scenario(matrix);

                    if (s_scenario != null && sw.ElapsedMilliseconds < 100)
                    {
                        Debug.WriteLine("Scenario execution finished in less than 100ms. This is likely due to bug.");
                    }
                }
            });

            try
            {
                if (!Console.IsOutputRedirected)
                {
                    while (s_scenario != null)
                    {
                        switch (Console.ReadKey(intercept: true).Key)
                        {
                            case ConsoleKey.Q:
                            {
                                s_scenario = null;
                                break;
                            }
                        }
                    }
                }

                drawing.Wait();
            }
            finally
            {
                matrix.Dispose();
            }
        }

        static void WeatherUpdater()
        {
            while (true)
            {
                UpdateWeather();
                Thread.Sleep(5000);
            }
        }

        static void IpsGetter()
        {
            try
            {
                s_ips = GetLocalNetworkIPAddresses().ToArray();
            }
            catch
            {
                // Choke all errors.
                // This is function is used for diagnostics
                // we don't want it to interrupt the process in any way
                s_ips = new string[1] { "Cannot get IPs" };
            }

            s_showLocalIp = Stopwatch.StartNew();
        }

        static void UpdateWeather()
        {
            try
            {
                s_weatherResponse = s_client.GetWeatherFromOpenWeather();
                s_networkAvailable = true;
            }
            catch (Exception e)
            {
                s_networkAvailable = false;
                Console.WriteLine($"UpdateWeather error: {e}");

                // So that we don't crash and cam display something reasonable
                s_weatherResponse = OpenWeatherResponse.GetExampleResponse();
            }
        }

        static IEnumerable<string> GetLocalNetworkIPAddresses()
        {
            var networks = NetworkInterface
                .GetAllNetworkInterfaces()
                .Where((network) => network.OperationalStatus == OperationalStatus.Up);

            foreach (var network in networks)
            {
                var properties = network.GetIPProperties();

                if (properties.GatewayAddresses.Count == 0)
                    continue;

                foreach (var address in properties.UnicastAddresses)
                {
                    if (address.Address.AddressFamily != AddressFamily.InterNetwork)
                        continue;

                    if (IPAddress.IsLoopback(address.Address))
                        continue;

                    yield return address.Address.ToString();
                }
            }
        }

        static IEnumerable<string> NetConf2019Feed()
        {
            if (s_showLocalIp != null && s_ips != null && s_ips.Length > 0 && s_showLocalIp.ElapsedMilliseconds < 30000)
            {
                foreach (var ip in s_ips)
                {
                    yield return ip;
                }
            }
            else
            {
                yield return ".NET IoT";

                if (!s_networkAvailable)
                {
                    yield return "Network not available. Displaying example data.";
                }

                yield return s_weatherResponse.Weather[0].Description;
                yield return $"{s_weatherResponse.Main.Temp:0}\u00B0F";

                float pressureHPa = s_weatherResponse.Main.Pressure;
                yield return $"{pressureHPa:0}hPa";
                yield return $"Humidity: {s_weatherResponse.Main.Humidity:0}%";
                yield return $"Wind: {s_weatherResponse.Wind.Speed:1}MPH {s_weatherResponse.Wind.Deg}\u00B0";
            }
        }

        static void WeatherDemo(RGBLedMatrix matrix)
        {
            BdfFont font = BdfFont.Load(@"fonts/10x20.bdf");
            BdfFont font1 = BdfFont.Load(@"fonts/8x13B.bdf");
            matrix.Fill(0, 0, 0);
            Thread.Sleep(100);

            int textLeft = 0;

            const int feedUpdateMs = 500;
            const int drawIntervalMs = 25;
            const int iterations = feedUpdateMs / drawIntervalMs;

            byte Col(float x)
            {
                return (byte)Math.Clamp(x * 255, 0, 255);
            }

            string text = null;
            int fullTextWidth = 0;

            Stopwatch sw = Stopwatch.StartNew();
            while (s_scenario != null)
            {
                text = " * " + string.Join(" * ", NetConf2019Feed());
                fullTextWidth = text.Length * font.Width;

                for (int i = 0; i < iterations && s_scenario != null; i++, textLeft --)
                {
                    matrix.DrawText(textLeft, -2, text, font, 0, 0, 255, 0, 0, 0);

                    if (textLeft + fullTextWidth < matrix.Width)
                    {
                        matrix.DrawText(textLeft + fullTextWidth, -2, text, font, 0, 0, 255, 0, 0, 0);
                    }

                    if (textLeft + fullTextWidth <= 0)
                    {
                        textLeft += fullTextWidth;
                    }

                    DateTimeOffset localNow = DateTimeOffset.Now;
                    DateTimeOffset pstNow = TimeZoneInfo.ConvertTime(localNow, s_timeZonePst);
                    string d = pstNow.ToString("hh:mm:ss");
                    matrix.DrawText(0, font.Height + 1 - 2, d, font1, 0, 255, 0, 0, 0, 0);

                    int halfHeight = matrix.Height / 2;
                    int halfWidth = matrix.Width / 2;
                    float time = sw.ElapsedMilliseconds / 1000f;
                    for (int x = 0; x < matrix.Width; x++)
                    {
                        if (x < halfWidth)
                        {
                            for (int y = halfHeight; y < matrix.Height; y++)
                            {
                                Vector3 col3 = Clock(new Vector2((float)x / halfWidth, (float)(y - halfHeight) / halfHeight), pstNow);
                                Color color = Color.FromArgb(Col(col3.X), Col(col3.Y), Col(col3.Z));
                                matrix.SetPixel(x, y, color.R, color.G, color.B);
                            }
                        }
                        else
                        {
                            for (int y = halfHeight; y < matrix.Height; y++)
                            {
                                Vector2 uv = new Vector2((float)(x - halfWidth) / halfWidth, (float)(y - halfHeight) / halfHeight);
                                Vector3 col3 = OpenWeatherIcon(uv, s_weatherResponse.Weather[0].Icon, time);
                                Color color = Color.FromArgb(Col(col3.X), Col(col3.Y), Col(col3.Z));
                                matrix.SetPixel(x, y, color.R, color.G, color.B);
                            }
                        }
                    }

                    Thread.Sleep(drawIntervalMs);
                }
            }
        }
    }
}
