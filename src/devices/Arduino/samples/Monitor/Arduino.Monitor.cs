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
using CommandLine;
using Iot.Device.Adc;
using Iot.Device.Arduino;
using Iot.Device.Arduino.Sample;
using Iot.Device.Bmxx80;
using Iot.Device.Bmxx80.PowerMode;
using Iot.Device.Board;
using Iot.Device.Button;
using Iot.Device.Common;
using Iot.Device.HardwareMonitor;
using UnitsNet;
using UnitsNet.Units;

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
            var parser = new Parser(x =>
            {
                x.AutoHelp = true;
                x.AutoVersion = true;
                x.CaseInsensitiveEnumValues = true;
                x.ParsingCulture = CultureInfo.InvariantCulture;
                x.CaseSensitive = false;
                x.HelpWriter = Console.Out;
            });

            var parsed = parser.ParseArguments<CommandLineOptions>(args);
            if (parsed.Errors.Any())
            {
                // Errors are already printed by the parser, just exit.
                return;
            }

            CommandLineOptions options = parsed.Value;

            using (var port = new SerialPort(options.PortName, options.BaudRate))
            {
                Console.WriteLine($"Connecting to Arduino on {options.PortName}");
                try
                {
                    port.Open();
                    port.BaseStream.ReadTimeout = 60000;
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
                    DisplayModes(board, options);
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

        public static void DisplayModes(ArduinoBoard board, CommandLineOptions options)
        {
            const int ButtonPin = 2;
            const int MaxMode = 12;
            Length stationAltitude = Length.FromMeters(options.Altitude);
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

            Console.WriteLine("Scanning for I2C devices...");
            var bus = board.CreateOrGetI2cBus(board.GetDefaultI2cBusNumber());
            var scanned = bus.PerformBusScan(lowest: 0x71);

            int assumedBmp280Address = Bmp280.DefaultI2cAddress;
            if (scanned.Contains(Bmp280.DefaultI2cAddress))
            {
                assumedBmp280Address = Bmp280.DefaultI2cAddress;
            }
            else if (scanned.Contains(Bmp280.SecondaryI2cAddress))
            {
                assumedBmp280Address = Bmp280.SecondaryI2cAddress;
            }

            var device = board.CreateI2cDevice(new I2cConnectionSettings(0, assumedBmp280Address));
            Bmp280? bmp;
            try
            {
                bmp = new Bmp280(device);
                bmp.StandbyTime = StandbyTime.Ms250;
                bmp.SetPowerMode(Bmx280PowerMode.Normal);
                Console.WriteLine($"Found BMP280 at {assumedBmp280Address}");
            }
            catch (IOException)
            {
                bmp = null;
                Console.WriteLine($"BMP280 not available at detected address {assumedBmp280Address}");
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
            Temperature temp = Temperature.Zero;
            Pressure pressure = Pressure.Zero;
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
                        modeName = "Temperature / Reduced Pressure V1";
                        if (bmp != null && bmp.TryReadTemperature(out temp) && bmp.TryReadPressure(out pressure))
                        {
                            Pressure p3 = WeatherHelper.CalculateBarometricPressure(pressure, temp, stationAltitude);
                            disp.Output.ReplaceLine(1, string.Format(CultureInfo.CurrentCulture, "{0:s1} {1:s1}", temp, p3));
                        }
                        else
                        {
                            disp.Output.ReplaceLine(1, "N/A");
                        }

                        break;

                    case 4:
                        modeName = "Raw Pressure";
                        if (bmp != null && bmp.TryReadPressure(out pressure))
                        {
                            pressure = pressure.ToUnit(PressureUnit.Hectopascal);
                            disp.Output.ReplaceLine(1, string.Format(CultureInfo.CurrentCulture, "{0:s1}", pressure));
                        }
                        else
                        {
                            disp.Output.ReplaceLine(1, "N/A");
                        }

                        break;

                    case 5:
                        modeName = "Reduced Pressure V2";
                        if (bmp != null && bmp.TryReadTemperature(out temp) && bmp.TryReadPressure(out pressure))
                        {
                            Pressure p3 = WeatherHelper.CalculateSeaLevelPressure(pressure, stationAltitude, temp);
                            p3 = p3.ToUnit(PressureUnit.Hectopascal);
                            disp.Output.ReplaceLine(1, string.Format(CultureInfo.CurrentCulture, "{0:s1}", p3));
                        }
                        else
                        {
                            disp.Output.ReplaceLine(1, "N/A");
                        }

                        break;

                    case 6:
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

                    case 7:
                        modeName = "Dew point";
                        if (dht.TryReadDht(3, 11, out temp, out humidity))
                        {
                            Temperature dewPoint = WeatherHelper.CalculateDewPoint(temp, humidity);
                            disp.Output.ReplaceLine(1, dewPoint.ToString("s1", CultureInfo.CurrentCulture));
                        }
                        else
                        {
                            disp.Output.ReplaceLine(1, "N/A");
                        }

                        break;
                    case 8:
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
                    case 9:
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
                    case 10:
                        modeName = "CPU Load";
                        disp.Output.ReplaceLine(1, hardwareMonitor.GetCpuLoad().ToString("s1", CultureInfo.CurrentCulture));
                        break;

                    case 11:
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

                    case 12:
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
