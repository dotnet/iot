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

            if (options.ListPorts)
            {
                Console.WriteLine("COM ports available:");
                Console.WriteLine(string.Join(", ", SerialPort.GetPortNames()));
            }

            using (var port = new SerialPort(options.PortName, options.BaudRate))
            {
                Console.WriteLine($"Connecting to Arduino on {options.PortName}");
                try
                {
                    port.Open();
                    port.BaseStream.ReadTimeout = 60000;
                }
                catch (Exception x) when (x is UnauthorizedAccessException || x is FileNotFoundException)
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
            const int MaxMode = 13;
            Length stationAltitude = Length.FromMeters(options.Altitude);
            int mode = 0;
            var gpioController = board.CreateGpioController();

            GpioButton button = new GpioButton(ButtonPin, TimeSpan.FromDays(1), TimeSpan.FromSeconds(2),
                false, true, gpioController, false, TimeSpan.FromMilliseconds(200));
            button.IsHoldingEnabled = true;

            CharacterDisplay disp = new CharacterDisplay(board);
            Console.WriteLine("Display output test");
            Console.WriteLine("The button on GPIO 2 changes modes");
            Console.WriteLine("Press x to exit");
            disp.Output.ScrollUpDelay = TimeSpan.FromMilliseconds(500);
            disp.Output.ReplaceLine(0, "Initializing...");

            AutoResetEvent buttonClicked = new AutoResetEvent(false);

            Pressure? qnhValue = null;

            bool buttonWasHolding = false;

            void ChangeMode(object? sender, EventArgs pinValueChangedEventArgs)
            {
                mode++;
                if (mode > MaxMode)
                {
                    // Don't change back to 0
                    mode = 1;
                }

                Console.WriteLine($"Mode changed to {mode}");
                buttonClicked.Set();
            }

            void ButtonHolding(object? sender, ButtonHoldingEventArgs e)
            {
                if (e.HoldingState == ButtonHoldingState.Completed)
                {
                    buttonWasHolding = true;
                }
            }

            button.Press += ChangeMode;
            button.Holding += ButtonHolding;

            string modeName = string.Empty;
            string modeData = string.Empty;
            string previousModeName = string.Empty;
            int firstCharInText = 0;
            TimeSpan sleeptime = TimeSpan.FromMilliseconds(400);

            // While accessing the display, we should not try to do any other sensor operation,
            // as it may cause the display to display garbage (probably because we handle some bits while
            // the display select line is set)
            Object displayLock = new object();
            var sensors = new SensorHandling(board, displayLock);
            SensorValues? values;

            while (true)
            {
                if (Console.KeyAvailable && Console.ReadKey(true).KeyChar == 'x')
                {
                    break;
                }

                switch (mode)
                {
                    case 0:
                        modeName = "Display ready";
                        modeData = "Button for mode";
                        // Just text
                        break;
                    case 1:
                    {
                        modeName = "Time";
                        modeData = DateTime.Now.ToLongTimeString();
                        sleeptime = TimeSpan.FromMilliseconds(200);
                        break;
                    }

                    case 2:
                    {
                        modeName = "Date";
                        modeData = DateTime.Now.ToShortDateString();
                        break;
                    }

                    case 3:
                        modeName = "Temperature / Reduced Pressure V1";
                        values = sensors.GetSensor(SensorHandling.BmpSensor);
                        if (values.Temperature.HasValue && values.Pressure.HasValue)
                        {
                            Pressure p3 = WeatherHelper.CalculateBarometricPressure(values.Pressure.Value, values.Temperature.Value, stationAltitude);
                            modeData = string.Format(CultureInfo.CurrentCulture, "{0:s1} {1:s1}", values.Temperature.Value, p3);
                            if (buttonWasHolding)
                            {
                                qnhValue = p3;
                                modeName = "QNH Value stored";
                                buttonWasHolding = false;
                            }
                        }
                        else
                        {
                            modeData = "N/A";
                        }

                        break;

                    case 4:
                        modeName = "Raw Pressure";
                        values = sensors.GetSensor(SensorHandling.BmpSensor);
                        if (values.Pressure.HasValue)
                        {
                            var pressure = values.Pressure.Value.ToUnit(PressureUnit.Hectopascal);
                            modeData = $"{pressure.Hectopascals:F2} hPa";
                        }
                        else
                        {
                            modeData = "N/A";
                        }

                        break;

                    case 5:
                        modeName = "Reduced Pressure V2";
                        values = sensors.GetSensor(SensorHandling.BmpSensor);
                        if (values.Temperature.HasValue && values.Pressure.HasValue)
                        {
                            Pressure p3 = WeatherHelper.CalculateSeaLevelPressure(values.Pressure.Value, stationAltitude, values.Temperature.Value);
                            p3 = p3.ToUnit(PressureUnit.Hectopascal);
                            modeData = string.Format(CultureInfo.CurrentCulture, "{0:s1}", p3);
                            if (buttonWasHolding)
                            {
                                qnhValue = p3;
                                modeName = "QNH Value stored";
                                buttonWasHolding = false;
                            }
                        }
                        else
                        {
                            modeData = "N/A";
                        }

                        break;

                    case 6:
                        modeName = "Temperature / Humidity";
                        values = sensors.GetSensor(SensorHandling.DhtSensor);
                        if (values.Temperature.HasValue && values.Humidity.HasValue)
                        {
                            modeData = string.Format(CultureInfo.CurrentCulture, "{0:s1} {1:s0}", values.Temperature.Value,
                                values.Humidity.Value);
                        }
                        else
                        {
                            modeData = "N/A";
                        }

                        break;

                    case 7:
                        modeName = "Dew point";
                        values = sensors.GetSensor(SensorHandling.DhtSensor);
                        if (values.Temperature.HasValue && values.Humidity.HasValue)
                        {
                            Temperature dewPoint = WeatherHelper.CalculateDewPoint(values.Temperature.Value, values.Humidity.Value);
                            modeData = dewPoint.ToString("s1", CultureInfo.CurrentCulture);
                        }
                        else
                        {
                            modeData = "N/A";
                        }

                        break;
                    case 8:
                        modeName = "CPU Temperature";
                        values = sensors.GetSensor(SensorHandling.Cpu);
                        if (values.Temperature.HasValue)
                        {
                            modeData = values.Temperature.Value.ToString("s1", CultureInfo.CurrentCulture);
                        }
                        else
                        {
                            modeData = "N/A";
                        }

                        break;
                    case 9:
                        modeName = "GPU Temperature";
                        values = sensors.GetSensor(SensorHandling.Gpu);
                        if (values.Temperature.HasValue)
                        {
                            modeData = values.Temperature.Value.ToString("s1", CultureInfo.CurrentCulture);
                        }
                        else
                        {
                            modeData = "N/A";
                        }

                        break;
                    case 10:
                        modeName = "CPU Load";
                        values = sensors.GetSensor(SensorHandling.Cpu);
                        if (values.Load.HasValue)
                        {
                            modeData = values.Load.Value.ToString("s1", CultureInfo.CurrentCulture);
                        }
                        else
                        {
                            modeData = "N/A";
                        }

                        break;

                    case 11:
                        modeName = "Total power dissipation";
                        values = sensors.GetSensor(SensorHandling.Cpu);

                        if (values.Power.HasValue)
                        {
                            modeData = values.Power.Value.ToString("s1", CultureInfo.CurrentCulture);
                        }
                        else
                        {
                            modeData = "N/A";
                        }

                        break;

                    case 12:
                        modeName = "Energy consumed";
                        values = sensors.GetSensor(SensorHandling.Cpu);

                        if (values.Energy.HasValue)
                        {
                            modeData = values.Energy.Value.ToString("s1", CultureInfo.CurrentCulture);
                        }
                        else
                        {
                            modeData = "N/A";
                        }

                        break;

                    case 13:
                        modeName = "Altitude from QNH";
                        values = sensors.GetSensor(SensorHandling.BmpSensor);
                        if (qnhValue.HasValue && values.Pressure.HasValue && values.Temperature.HasValue)
                        {
                            Length altitude = WeatherHelper.CalculateAltitude(values.Pressure.Value, qnhValue.Value, 
                                values.Temperature.Value);
                            modeData = $"{altitude.Meters:F2} m";
                        }
                        else
                        {
                            modeData = "N/A";
                        }

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

                    lock (displayLock)
                    {
                        disp.Output.ReplaceLine(0, modeName.Substring(firstCharInText));
                    }
                }

                if (modeName != previousModeName)
                {
                    lock (displayLock)
                    {
                        disp.Output.ReplaceLine(0, modeName);
                    }

                    previousModeName = modeName;
                    firstCharInText = 0;
                }

                lock (displayLock)
                {
                    disp.Output.ReplaceLine(1, modeData);
                }

                buttonClicked.WaitOne(sleeptime);
            }

            sensors.Dispose();
            button.Dispose();
            disp.Output.Clear();
            disp.Dispose();
            gpioController.Dispose();
        }
    }
}
