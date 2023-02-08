// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System;
using System.Device.Gpio;
using System.Device.Gpio.Drivers;
using System.Device.I2c;
using System.Device.Spi;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.IO.Ports;
using System.Linq;
using System.Text;
using System.Threading;
using Iot.Device.Adc;
using Iot.Device.Arduino;
using Iot.Device.Arduino.Sample;
using Iot.Device.Bmxx80;
using Iot.Device.Bmxx80.PowerMode;
using Iot.Device.Button;
using Iot.Device.Common;
using Iot.Device.HardwareMonitor;
using UnitsNet;

namespace Arduino.Samples
{
    internal class Program
    {
        /// <summary>
        /// Main entry point
        /// </summary>
        /// <param name="args">The first argument gives the Port name. Default "COM4"</param>
        public static void Main(string[] args)
        {
            string portName = "COM4";
            if (args.Length > 0)
            {
                portName = args[0];
            }

            using (var port = new SerialPort(portName, 115200))
            {
                Console.WriteLine($"Connecting to Arduino on {portName}");
                try
                {
                    port.Open();
                }
                catch (UnauthorizedAccessException x)
                {
                    Console.WriteLine($"Could not open COM port: {x.Message} Possible reason: Arduino IDE connected or serial console open");
                    return;
                }

                ArduinoBoard board = new ArduinoBoard(port.BaseStream);
                try
                {
                    Console.WriteLine($"Firmware version: {board.FirmwareVersion}, Builder: {board.FirmwareName}");
                    DisplayModes(board);
                }
                catch (TimeoutException x)
                {
                    Console.WriteLine($"No answer from board: {x.Message} ");
                }
                finally
                {
                    port.Close();
                    board?.Dispose();
                }
            }
        }

        private static void BoardOnLogMessages(string message, Exception? exception)
        {
            Console.WriteLine("Log message: " + message);
            if (exception != null)
            {
                Console.WriteLine(exception);
            }
        }

        public static void DisplayModes(ArduinoBoard board)
        {
            const int ButtonPin = 2;
            const int MaxMode = 10;
            Length stationAltitude = Length.FromMeters(650);
            int mode = 0;
            var gpioController = board.CreateGpioController();

            GpioButton button = new GpioButton(ButtonPin, false, true, gpioController, false, TimeSpan.FromMilliseconds(200));
            CharacterDisplay disp = new CharacterDisplay(board);
            Console.WriteLine("Display output test");
            Console.WriteLine("The button on GPIO 2 changes modes");
            Console.WriteLine("Press x to exit");
            disp.Output.ScrollUpDelay = TimeSpan.FromMilliseconds(500);
            AutoResetEvent buttonClicked = new AutoResetEvent(false);

            void ChangeMode(object? sender, EventArgs pinValueChangedEventArgs)
            {
                mode++;
                if (mode > MaxMode)
                {
                    // Don't change back to 0
                    mode = 1;
                }

                buttonClicked.Set();
            }

            button.Press += ChangeMode;

            var device = board.CreateI2cDevice(new I2cConnectionSettings(0, Bmp280.DefaultI2cAddress));
            Bmp280? bmp;
            try
            {
                bmp = new Bmp280(device);
                bmp.StandbyTime = StandbyTime.Ms250;
                bmp.SetPowerMode(Bmx280PowerMode.Normal);
            }
            catch (IOException)
            {
                bmp = null;
                Console.WriteLine("BMP280 not available");
            }

            DhtSensor? dht = board.GetCommandHandler<DhtSensor>();
            if (dht == null)
            {
                // Note that this is a software error, hardware support is not tested here.
                Console.WriteLine("DHT Sensor module missing");
                return;
            }

            OpenHardwareMonitor hardwareMonitor = new OpenHardwareMonitor();
            hardwareMonitor.EnableDerivedSensors();
            TimeSpan sleeptime = TimeSpan.FromMilliseconds(500);
            string modeName = string.Empty;
            string previousModeName = string.Empty;
            int firstCharInText = 0;
            while (true)
            {
                if (Console.KeyAvailable && Console.ReadKey(true).KeyChar == 'x')
                {
                    break;
                }

                // Default
                sleeptime = TimeSpan.FromMilliseconds(500);

                switch (mode)
                {
                    case 0:
                        modeName = "Display ready";
                        disp.Output.ReplaceLine(1, "Button for mode");
                        // Just text
                        break;
                    case 1:
                    {
                        modeName = "Time";
                        disp.Output.ReplaceLine(1, DateTime.Now.ToLongTimeString());
                        sleeptime = TimeSpan.FromMilliseconds(200);
                        break;
                    }

                    case 2:
                    {
                        modeName = "Date";
                        disp.Output.ReplaceLine(1, DateTime.Now.ToShortDateString());
                        break;
                    }

                    case 3:
                        modeName = "Temperature / Barometric Pressure";
                        if (bmp != null && bmp.TryReadTemperature(out Temperature temp) && bmp.TryReadPressure(out Pressure p2))
                        {
                            Pressure p3 = WeatherHelper.CalculateBarometricPressure(p2, temp, stationAltitude);
                            disp.Output.ReplaceLine(1, string.Format(CultureInfo.CurrentCulture, "{0:s1} {1:s1}", temp, p3));
                        }
                        else
                        {
                            disp.Output.ReplaceLine(1, "N/A");
                        }

                        break;
                    case 4:
                        modeName = "Temperature / Humidity";
                        if (dht.TryReadDht(3, 11, out temp, out var humidity))
                        {
                            disp.Output.ReplaceLine(1, string.Format(CultureInfo.CurrentCulture, "{0:s1} {1:s0}", temp, humidity));
                        }
                        else
                        {
                            disp.Output.ReplaceLine(1, "N/A");
                        }

                        break;

                    case 5:
                        modeName = "Dew point";
                        if (bmp != null && bmp.TryReadPressure(out p2) && dht.TryReadDht(3, 11, out temp, out humidity))
                        {
                            Temperature dewPoint = WeatherHelper.CalculateDewPoint(temp, humidity);
                            disp.Output.ReplaceLine(1, dewPoint.ToString("s1", CultureInfo.CurrentCulture));
                        }
                        else
                        {
                            disp.Output.ReplaceLine(1, "N/A");
                        }

                        break;
                    case 6:
                        modeName = "CPU Temperature";
                        if (hardwareMonitor.TryGetAverageCpuTemperature(out temp))
                        {
                            disp.Output.ReplaceLine(1, temp.ToString("s1", CultureInfo.CurrentCulture));
                        }
                        else
                        {
                            disp.Output.ReplaceLine(1, "N/A");
                        }

                        break;
                    case 7:
                        modeName = "GPU Temperature";
                        if (hardwareMonitor.TryGetAverageGpuTemperature(out temp))
                        {
                            disp.Output.ReplaceLine(1, temp.ToString("s1", CultureInfo.CurrentCulture));
                        }
                        else
                        {
                            disp.Output.ReplaceLine(1, "N/A");
                        }

                        break;
                    case 8:
                        modeName = "CPU Load";
                        disp.Output.ReplaceLine(1, hardwareMonitor.GetCpuLoad().ToString("s1", CultureInfo.CurrentCulture));
                        break;

                    case 9:
                        modeName = "Total power dissipation";
                        var powerSources = hardwareMonitor.GetSensorList().Where(x => x.SensorType == SensorType.Power);
                        Power totalPower = Power.Zero;
                        foreach (var power in powerSources)
                        {
                            if (power.Name != "CPU Cores" && power.TryGetValue(out Power powerConsumption)) // included in CPU Package
                            {
                                totalPower = totalPower + powerConsumption;
                            }
                        }

                        disp.Output.ReplaceLine(1, totalPower.ToString("s1", CultureInfo.CurrentCulture));
                        break;

                    case 10:
                        modeName = "Energy consumed";
                        var energySources = hardwareMonitor.GetSensorList().Where(x => x.SensorType == SensorType.Energy);
                        Energy totalEnergy = Energy.FromWattHours(0); // Set up the desired output unit
                        foreach (var e in energySources)
                        {
                            if (!e.Name.StartsWith("CPU Cores") && e.TryGetValue(out Energy powerConsumption)) // included in CPU Package
                            {
                                totalEnergy = totalEnergy + powerConsumption;
                            }
                        }

                        disp.Output.ReplaceLine(1, totalEnergy.ToString("s1", CultureInfo.CurrentCulture));
                        break;
                }

                int displayWidth = disp.Output.Size.Width;
                if (modeName.Length > displayWidth)
                {
                    // Add one space at the end, makes it a bit easier to read
                    if (firstCharInText < modeName.Length - displayWidth + 1)
                    {
                        firstCharInText++;
                    }
                    else
                    {
                        firstCharInText = 0;
                    }

                    disp.Output.ReplaceLine(0, modeName.Substring(firstCharInText));
                }

                if (modeName != previousModeName)
                {
                    disp.Output.ReplaceLine(0, modeName);

                    previousModeName = modeName;
                    firstCharInText = 0;
                }

                buttonClicked.WaitOne(sleeptime);
            }

            hardwareMonitor.Dispose();
            button.Dispose();
            disp.Output.Clear();
            disp.Dispose();
            bmp?.Dispose();
            gpioController.Dispose();
        }
    }
}
