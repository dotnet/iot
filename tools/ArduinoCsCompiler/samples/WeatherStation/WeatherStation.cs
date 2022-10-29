// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System;
using System.Collections.Generic;
using System.Device.Gpio;
using System.Device.I2c;
using System.Globalization;
using System.IO;
using System.IO.Ports;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Iot.Device.Arduino;
using Iot.Device.Bmxx80;
using Iot.Device.Bmxx80.PowerMode;
using Iot.Device.CharacterLcd;
using Iot.Device.Common;
using Iot.Device.Graphics;
using UnitsNet;

namespace WeatherStation
{
    internal class WeatherStation
    {
        private const int RedLed = 16;
        private const int Button = 17;

        private const int StationAltitude = 650;

        private readonly ArduinoBoard _board;
        private Bme680? _bme680;

        private WeatherStation(ArduinoBoard board)
        {
            _board = board;
        }

        public static int Main(string[] args)
        {
            ArduinoBoard? board;
            string portName;
            Console.WriteLine("Connecting...");
            if (args.Length > 0)
            {
                portName = args[0];
            }
            else
            {
                portName = string.Empty;
            }

            if (portName == "INET")
            {
                IPAddress address = IPAddress.Loopback;
                if (args.Length > 1)
                {
                    address = IPAddress.Parse(args[1]);
                }

                if (!ArduinoBoard.TryConnectToNetworkedBoard(address, 27016, out board))
                {
                    Console.WriteLine($"Unable to connect to board at address {address}");
                    return 1;
                }
            }
            else
            {
                string[] boards = SerialPort.GetPortNames();
                if (portName != string.Empty)
                {
                    boards = new string[]
                    {
                        portName
                    };
                }

                if (!ArduinoBoard.TryFindBoard(boards, new List<int>() { 115200 }, out board))
                {
                    Console.WriteLine($"Unable to connect to board at any of {String.Join(", ", boards)}");
                    return 1;
                }
            }

            try
            {
                Console.WriteLine("Successfully connected");
                WeatherStation ws = new(board);
                return ws.Run();
            }
            finally
            {
                board.Dispose();
            }
        }

        private void InitBme()
        {
            I2cDevice bme680Device = _board.CreateI2cDevice(new I2cConnectionSettings(0, Bme680.SecondaryI2cAddress));
            if (_bme680 != null)
            {
                _bme680.Dispose();
                _bme680 = null;
            }

            _bme680 = new Bme680(bme680Device, Temperature.FromDegreesCelsius(20));
            _bme680.Reset();
            _bme680.GasConversionIsEnabled = false;
            _bme680.HeaterIsEnabled = false;
        }

        public int Run()
        {
            GpioController gpioController = _board.CreateGpioController();
            gpioController.OpenPin(RedLed, PinMode.Output);
            gpioController.Write(RedLed, PinValue.High);
            gpioController.OpenPin(Button, PinMode.Input);
            // This Sleep is just because the display sometimes needs a bit of time to properly initialize -
            // otherwise it just doesn't properly accept commands
            Thread.Sleep(1000);
            using I2cDevice i2cDevice = _board.CreateI2cDevice(new I2cConnectionSettings(0, 0x27));
            using LcdInterface lcdInterface = LcdInterface.CreateI2c(i2cDevice, false);
            using Hd44780 hd44780 = new Lcd2004(lcdInterface);
            hd44780.UnderlineCursorVisible = false;
            hd44780.BacklightOn = true;
            hd44780.DisplayOn = true;
            hd44780.Clear();
            hd44780.Write("Startup!");
            Thread.Sleep(500);
            Length stationAltitude = Length.FromMeters(StationAltitude);
            LcdConsole console = new LcdConsole(hd44780, "A00", false);
            LcdCharacterEncoding encoding = LcdConsole.CreateEncoding(CultureInfo.CreateSpecificCulture("de-CH"), "A00", '?', 8);
            console.LoadEncoding(encoding);
            console.LineFeedMode = LineWrapMode.Truncate;
            console.ReplaceLine(0, "Startup!");
            console.ReplaceLine(1, "Initializing BME680...");
            InitBme();
            gpioController.Write(RedLed, PinValue.Low);
            while (gpioController.Read(Button) == PinValue.Low)
            {
                try
                {
                    _bme680!.SetPowerMode(Bme680PowerMode.Forced);
                    var time = DateTime.Now;
                    if (_bme680.TryReadTemperature(out Temperature temp) && _bme680.TryReadPressure(out Pressure pressure) && _bme680.TryReadHumidity(out RelativeHumidity humidity))
                    {
                        Pressure correctedPressure = WeatherHelper.CalculateBarometricPressure(pressure, temp, stationAltitude);
                        Temperature dewPoint = WeatherHelper.CalculateDewPoint(temp, humidity);

                        string temperatureLine = temp.DegreesCelsius.ToString("F2") + " °C " + correctedPressure.Hectopascals.ToString("F1") + " hPa";
                        string humidityLine = humidity.Percent.ToString("F1") + "% RH, DP: " + dewPoint.DegreesCelsius.ToString("F1") + " °C";

                        console.ReplaceLine(0, temperatureLine);
                        console.ReplaceLine(1, humidityLine);
                        Console.WriteLine(temperatureLine);
                        Console.WriteLine(humidityLine);
                    }

                    string line = time.ToLongDateString();
                    console.ReplaceLine(2, line);
                    console.SetCursorPosition(0, 3);
                    Console.WriteLine(line);
                    line = time.ToLongTimeString();
                    console.ReplaceLine(3, line);
                    Console.WriteLine(line);
                }
                catch (TimeoutException x)
                {
                    console.ReplaceLine(0, $"ERR: {x.Message}");
                    Console.WriteLine($"TimeoutException: {x.Message}");
                }
                catch (IOException y)
                {
                    console.ReplaceLine(0, $"ERR: {y.Message}");
                    Console.WriteLine($"IOException: {y.Message}");
                    InitBme(); // reinit, something is fishy
                }

                gpioController.Write(RedLed, PinValue.High);
                Thread.Sleep(100);
                gpioController.Write(RedLed, PinValue.Low);
            }

            return 0;
        }
    }
}
