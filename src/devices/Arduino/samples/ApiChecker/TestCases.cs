// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System;
using System.Collections.Generic;
using System.Device.Gpio;
using System.Device.I2c;
using System.Device.Spi;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Iot.Device.Adc;
using Iot.Device.Bmxx80;
using Iot.Device.Bmxx80.PowerMode;
using Iot.Device.Board;
using UnitsNet;

namespace Iot.Device.Arduino.Sample
{
    /// <summary>
    /// Contains a set of test cases for testing Arduino/Firmata boards
    /// </summary>
    public class TestCases
    {
        private readonly ArduinoBoard _board;
        private int _ledPin = 13;
        private int _buttonPin = 2;
        private int _analogInputChannel = 1;
        private int _lowestI2cAddress = 0x3;
        private int _highestI2cAddress = 0x77;

        private TestCases(ArduinoBoard board)
        {
            _board = board;
        }

        /// <summary>
        /// Shows an interactive selection of test cases
        /// </summary>
        /// <param name="board">The board to operate on</param>
        public static void Run(ArduinoBoard board)
        {
            var t = new TestCases(board);
            bool loop = false;
            do
            {
                try
                {
                    loop = t.Menu();
                }
                catch (Exception x) when (!(x is NullReferenceException))
                {
                    Console.WriteLine($"There was an error processing the command: {x.Message}");
                }
            }
            while (loop);
        }

        private static int GetAnalogPin(ArduinoBoard board, int analogChannel)
        {
            int analogPin;
            foreach (var pin in board.SupportedPinConfigurations)
            {
                if (pin.AnalogPinNumber == analogChannel)
                {
                    analogPin = pin.Pin;
                    Console.WriteLine($"Using pin for A{analogChannel}: {analogPin}");
                    return analogPin;
                }
            }

            return -1;
        }

        private static void TestI2cBmp280(ArduinoBoard board)
        {
            using var device = board.CreateI2cDevice(new I2cConnectionSettings(0, Bmp280.DefaultI2cAddress));
            using var bmp = new Bmp280(device);
            bmp.StandbyTime = StandbyTime.Ms250;
            bmp.SetPowerMode(Bmx280PowerMode.Normal);
            Console.WriteLine("Device open");
            while (!Console.KeyAvailable)
            {
                bmp.TryReadTemperature(out var temperature);
                bmp.TryReadPressure(out var pressure);
                Console.Write($"\rTemperature: {temperature.DegreesCelsius:F2}°C. Pressure {pressure.Hectopascals:F1} hPa                  ");
                Thread.Sleep(100);
            }

            Console.ReadKey();
            Console.WriteLine();
        }

        private static void TestI2cBme680(ArduinoBoard board)
        {
            var device = board.CreateI2cDevice(new I2cConnectionSettings(0, Bme680.SecondaryI2cAddress));

            var bme = new Bme680(device, Temperature.FromDegreesCelsius(20));
            bme.Reset();
            Console.WriteLine("Device open");
            while (!Console.KeyAvailable)
            {
                bme.SetPowerMode(Bme680PowerMode.Forced);
                if (bme.TryReadTemperature(out var temperature) && bme.TryReadPressure(out var pressure))
                {
                    Console.Write($"\rTemperature: {temperature.DegreesCelsius:F2}°C. Pressure {pressure.Hectopascals:F1} hPa                  ");
                }
                else
                {
                    Console.WriteLine("Read error");
                }

                Thread.Sleep(500);
            }

            bme.Dispose();
            device.Dispose();
            Console.ReadKey();
            Console.WriteLine();
        }

        private static void MyCallback(object sender, PinValueChangedEventArgs pinValueChangedEventArgs)
        {
            Console.WriteLine($"Event on GPIO {pinValueChangedEventArgs.PinNumber}, event type: {pinValueChangedEventArgs.ChangeType}");
        }

        private static void TestSpi(ArduinoBoard board)
        {
            const double vssValue = 5; // Set this to the supply voltage of the arduino. Most boards have 5V, some newer ones run at 3.3V.
            SpiConnectionSettings settings = new SpiConnectionSettings(0, 10);
            using (var spi = board.CreateSpiDevice(settings))
            using (Mcp3008 mcp = new Mcp3008(spi))
            {
                Console.WriteLine("SPI Device open");
                while (!Console.KeyAvailable)
                {
                    double vdd = mcp.Read(5);
                    double vss = mcp.Read(6);
                    double middle = mcp.Read(7);
                    Console.WriteLine($"Raw values: VSS {vss} VDD {vdd} Average {middle}");
                    vdd = vssValue * vdd / 1024;
                    vss = vssValue * vss / 1024;
                    middle = vssValue * middle / 1024;
                    Console.WriteLine($"Converted values: VSS {vss:F2}V, VDD {vdd:F2}V, Average {middle:F2}V");
                    Thread.Sleep(200);
                }
            }

            Console.ReadKey();
        }

        private static void TestDht(ArduinoBoard board)
        {
            Console.WriteLine("Reading DHT11. Any key to quit.");
            DhtSensor? handler = board.GetCommandHandler<DhtSensor>();
            if (handler == null)
            {
                Console.WriteLine("DHT Command handler not available.");
                return;
            }

            while (!Console.KeyAvailable)
            {
                // Read from DHT11 at pin 3
                if (handler.TryReadDht(3, 11, out var temperature, out var humidity))
                {
                    Console.WriteLine($"Temperature: {temperature}, Humidity {humidity}");
                }
                else
                {
                    Console.WriteLine("Unable to read DHT11");
                }

                Thread.Sleep(2500);
            }

            Console.ReadKey();
        }

        private void ScanDeviceAddressesOnI2cBus(ArduinoBoard board)
        {
            var bus = board.CreateOrGetI2cBus(0);
            Console.WriteLine();
            // Due to internal caching, this takes much longer the first time.
            var availableDevices = bus.PerformBusScan(new ProgressPrinter(), _lowestI2cAddress, _highestI2cAddress);
            Console.WriteLine();
            string result = availableDevices.ToUserReadableTable();
            Console.WriteLine(result);
        }

        internal bool Menu()
        {
            Console.WriteLine("Hello I2C and GPIO on Arduino!");
            Console.WriteLine("Select the test you want to run:");
            Console.WriteLine(" 1 Run I2C tests with a BMP280");
            Console.WriteLine($" 2 Run GPIO tests with a simple led blinking on GPIO{_ledPin} port");
            Console.WriteLine($" 3 Run polling button test on GPIO{_buttonPin}");
            Console.WriteLine($" 4 Run event wait test event on GPIO{_buttonPin} on Falling and Rising");
            Console.WriteLine($" 5 Run callback event test on GPIO{_buttonPin}");
            Console.WriteLine($" 6 Run PWM test with a LED dimming on GPIO{_ledPin} port");
            Console.WriteLine($" 7 Blink the LED according to the input on A{_analogInputChannel}");
            Console.WriteLine(" 8 Read analog channel as fast as possible");
            Console.WriteLine(" 9 Run SPI tests with an MCP3008 (experimental)");
            Console.WriteLine(" 0 Detect all devices on the I2C bus");
            Console.WriteLine(" H Read DHT11 Humidity sensor on GPIO 3 (experimental)");
            Console.WriteLine(" A Color fade an RGB led on 3 PWM channels");
            Console.WriteLine(" B Run I2C tests with a BME680");
            Console.WriteLine(" F Measure frequency on a GPIO Pin (experimental)");
            Console.WriteLine($" S Send board to sleep (wake up with interrupt on {_buttonPin})");
            Console.WriteLine();
            Console.WriteLine(" C Configure pins for tests");
            Console.WriteLine(" I Get board information");
            Console.WriteLine(" X Exit");
            var key = Console.ReadKey();
            Console.WriteLine();

            try
            {
                switch (key.KeyChar)
                {
                    case '1':
                        TestI2cBmp280(_board);
                        break;
                    case '2':
                        TestGpio();
                        break;
                    case '3':
                        TestInput();
                        break;
                    case '4':
                        TestEventsDirectWait();
                        break;
                    case '5':
                        TestEventsCallback();
                        break;
                    case '6':
                        TestPwm();
                        break;
                    case '7':
                        TestAnalogIn();
                        break;
                    case '8':
                        TestAnalogCallback(_board);
                        break;
                    case '9':
                        TestSpi(_board);
                        break;
                    case '0':
                        ScanDeviceAddressesOnI2cBus(_board);
                        break;
                    case 'h':
                    case 'H':
                        TestDht(_board);
                        break;
                    case 'b':
                    case 'B':
                        TestI2cBme680(_board);
                        break;
                    case 'f':
                    case 'F':
                        TestFrequency(_board);
                        break;
                    case 'a':
                    case 'A':
                        {
                            var test = new RgbLedTest(_board);
                            test.DoTest();
                        }

                        break;
                    case 'c':
                    case 'C':
                        ConfigurePins();
                        break;
                    case 'i':
                    case 'I':
                        BoardInformation();
                        break;
                    case 's':
                    case 'S':
                        SendBoardToSleep();
                        break;
                    case 'x':
                    case 'X':
                        return false;
                }
            }
            catch (IOException x)
            {
                Console.WriteLine($"The command failed with the following error: {x.Message}");
            }

            return true;
        }

        private void TestAnalogCallback(ArduinoBoard board)
        {
            int analogPin = GetAnalogPin(board, _analogInputChannel);
            var analogController = board.CreateAnalogController(0);
            // Add this line for 3.3V boards, to get correct voltages.
            // analogController.VoltageReference = ElectricPotential.FromVolts(3.3);
            board.SetAnalogPinSamplingInterval(TimeSpan.FromMilliseconds(10));
            var pin = analogController.OpenPin(analogPin);
            pin.EnableAnalogValueChangedEvent(null, 0);

            pin.ValueChanged += (sender, args) =>
            {
                if (args.PinNumber == analogPin)
                {
                    Console.WriteLine($"New voltage: {args.Value}.");
                }
            };

            Console.WriteLine("Waiting for changes on the analog input");
            while (!Console.KeyAvailable)
            {
                // Nothing to do
                Thread.Sleep(100);
            }

            Console.ReadKey();
            pin.DisableAnalogValueChangedEvent();
            pin.Dispose();
            analogController.Dispose();
        }

        private void ConfigurePins()
        {
            Console.WriteLine();
            Console.Write("Which pin to use for the LED? ");
            var input = Console.ReadLine();
            if (int.TryParse(input, NumberStyles.Integer, CultureInfo.CurrentCulture, out int led))
            {
                _ledPin = led;
            }
            else
            {
                Console.WriteLine("You did not enter a valid number");
            }

            Console.Write("Which pin to use for the button? ");
            input = Console.ReadLine();
            if (int.TryParse(input, NumberStyles.Integer, CultureInfo.CurrentCulture, out int button))
            {
                _buttonPin = button;
            }
            else
            {
                Console.WriteLine("You did not enter a valid number");
            }

            Console.Write("Which analog channel to use as input? ");
            input = Console.ReadLine();
            if (int.TryParse(input, NumberStyles.Integer, CultureInfo.CurrentCulture, out int inputChannel))
            {
                _analogInputChannel = inputChannel;
            }
            else
            {
                Console.WriteLine("You did not enter a valid number");
            }

            int analogPin = GetAnalogPin(_board, _analogInputChannel);
            if (analogPin < 0)
            {
                Console.WriteLine($"Warn: Analog channel A{_analogInputChannel} does not exist");
            }

            Console.WriteLine($"Led-Pin: {_ledPin}. Button-Pin: {_buttonPin}. Analog input channel A{_analogInputChannel} (pin {analogPin})");

            Console.WriteLine();
            Console.Write("Lowest Address for I2C bus scan? (Default: 0x03) ");
            input = Console.ReadLine()!;
            if (input.Length < 3 || !int.TryParse(input.Substring(2), NumberStyles.HexNumber, CultureInfo.CurrentCulture, out _lowestI2cAddress))
            {
                _lowestI2cAddress = 3;
            }

            Console.Write("Highest Address for I2C bus scan? (Default: 0x77) ");
            input = Console.ReadLine()!;
            if (input.Length < 3 || !int.TryParse(input.Substring(2), NumberStyles.HexNumber, CultureInfo.CurrentCulture, out _highestI2cAddress))
            {
                _highestI2cAddress = 0x77;
            }
        }

        private void TestFrequency(ArduinoBoard board)
        {
            Console.Write("Which pin number to use? ");
            string? input = Console.ReadLine();
            if (input == null)
            {
                return;
            }

            if (!int.TryParse(input, out int pin))
            {
                return;
            }

            FrequencySensor? sensor = board.GetCommandHandler<FrequencySensor>();
            if (sensor == null)
            {
                Console.WriteLine("Frequency handling software module missing");
                return;
            }

            try
            {
                sensor.EnableFrequencyReporting(pin, FrequencyMode.Rising, 500);

                while (!Console.KeyAvailable)
                {
                    var f = sensor.GetMeasuredFrequency();
                    Console.Write($"\rFrequency at GPIO{pin}: {f}                       ");
                    Thread.Sleep(100);
                }
            }
            finally
            {
                sensor.DisableFrequencyReporting(pin);
            }

            Console.ReadKey(true);
            Console.WriteLine();
        }

        private void TestPwm()
        {
            int pin = _ledPin;
            using (var pwm = _board.CreatePwmChannel(0, pin, 100, 0))
            {
                Console.WriteLine("Now dimming LED. Press any key to exit");
                pwm.Start();
                while (!Console.KeyAvailable)
                {
                    for (double fadeValue = 0; fadeValue <= 1.0; fadeValue += 0.05)
                    {
                        // sets the value (range from 0 to 255):
                        pwm.DutyCycle = fadeValue;
                        // wait for 80 milliseconds to see the dimming effect
                        Thread.Sleep(80);
                    }

                    // fade out from max to min in increments of 5 points:
                    for (double fadeValue = 1.0; fadeValue >= 0; fadeValue -= 0.05)
                    {
                        // sets the value (range from 0 to 255):
                        pwm.DutyCycle = fadeValue;
                        // wait for 80 milliseconds to see the dimming effect
                        Thread.Sleep(80);
                    }

                }

                Console.ReadKey();
            }
        }

        private sealed class ProgressPrinter : IProgress<float>
        {
            public void Report(float value)
            {
                Console.Write($"\rPlease wait. {value:F0}% done.    ");
            }
        }

        private void TestGpio()
        {
            int gpio = _ledPin;
            var gpioController = _board.CreateGpioController();

            // Opening GPIO2
            gpioController.OpenPin(gpio);
            gpioController.SetPinMode(gpio, PinMode.Output);

            Console.WriteLine($"Blinking GPIO{_ledPin}");
            while (!Console.KeyAvailable)
            {
                gpioController.Write(gpio, PinValue.High);
                Thread.Sleep(500);
                gpioController.Write(gpio, PinValue.Low);
                Thread.Sleep(500);
            }

            Console.ReadKey();
            gpioController.Dispose();
        }

        private void TestAnalogIn()
        {
            // Use Pin 6
            int gpio = _ledPin;
            int analogPin = GetAnalogPin(_board, _analogInputChannel);
            var gpioController = _board.CreateGpioController();
            var analogController = _board.CreateAnalogController(0);

            var pin = analogController.OpenPin(analogPin);
            gpioController.OpenPin(gpio);
            gpioController.SetPinMode(gpio, PinMode.Output);

            Console.WriteLine("Blinking GPIO6, based on analog input.");
            while (!Console.KeyAvailable)
            {
                ElectricPotential voltage = pin.ReadVoltage();
                gpioController.Write(gpio, PinValue.High);
                Thread.Sleep((int)(voltage * 100).Volts);
                voltage = pin.ReadVoltage();
                gpioController.Write(gpio, PinValue.Low);
                Thread.Sleep((int)(voltage * 100).Volts);
            }

            pin.Dispose();
            Console.ReadKey();
            analogController.Dispose();
            gpioController.Dispose();
        }

        private void TestInput()
        {
            int gpio = _buttonPin;
            var gpioController = _board.CreateGpioController();

            // Opening GPIO2
            gpioController.OpenPin(gpio);
            gpioController.SetPinMode(gpio, PinMode.Input);

            if (gpioController.GetPinMode(gpio) != PinMode.Input)
            {
                throw new InvalidOperationException("Couldn't set pin mode");
            }

            Console.WriteLine($"Polling input pin {_buttonPin}");
            var lastState = gpioController.Read(gpio);
            while (!Console.KeyAvailable)
            {
                var newState = gpioController.Read(gpio);
                if (newState != lastState)
                {
                    if (newState == PinValue.High)
                    {
                        Console.WriteLine("Button pressed");
                    }
                    else
                    {
                        Console.WriteLine("Button released");
                    }
                }

                lastState = newState;
                Thread.Sleep(10);
            }

            Console.ReadKey();
            gpioController.Dispose();
        }

        private void TestEventsDirectWait()
        {
            int gpio2 = _buttonPin;
            var gpioController = _board.CreateGpioController();

            // Opening GPIO2
            gpioController.OpenPin(gpio2);
            gpioController.SetPinMode(gpio2, PinMode.Input);

            Console.WriteLine("Waiting for both falling and rising events");
            while (!Console.KeyAvailable)
            {
                var res = gpioController.WaitForEvent(gpio2, PinEventTypes.Falling | PinEventTypes.Rising, new TimeSpan(0, 0, 0, 0, 50));
                if ((!res.TimedOut) && (res.EventTypes != PinEventTypes.None))
                {
                    Console.WriteLine($"Event on GPIO {gpio2}, event type: {res.EventTypes}");
                }
            }

            Console.ReadKey();
            Console.WriteLine("Waiting for only rising events");
            while (!Console.KeyAvailable)
            {
                var res = gpioController.WaitForEvent(gpio2, PinEventTypes.Rising, new TimeSpan(0, 0, 0, 0, 50));
                if ((!res.TimedOut) && (res.EventTypes != PinEventTypes.None))
                {
                    MyCallback(gpioController, new PinValueChangedEventArgs(res.EventTypes, gpio2));
                }
            }

            gpioController.Dispose();
        }

        private void TestEventsCallback()
        {
            int gpio2 = _buttonPin;
            var gpioController = _board.CreateGpioController();

            // Opening GPIO2
            gpioController.OpenPin(gpio2);
            gpioController.SetPinMode(gpio2, PinMode.Input);

            Console.WriteLine("Setting up events on GPIO2 for rising and falling");

            gpioController.RegisterCallbackForPinValueChangedEvent(gpio2, PinEventTypes.Falling | PinEventTypes.Rising, MyCallback);
            Console.WriteLine("Event setup, press a key to remove the falling event");
            while (!Console.KeyAvailable)
            {
                // Nothing to do
                Thread.Sleep(100);
            }

            Console.ReadKey();
            gpioController.UnregisterCallbackForPinValueChangedEvent(gpio2, MyCallback);
            gpioController.RegisterCallbackForPinValueChangedEvent(gpio2, PinEventTypes.Rising, MyCallback);
            Console.WriteLine("Now only waiting for rising events, press a key to remove all events and quit");
            while (!Console.KeyAvailable)
            {
                // Nothing to do
                Thread.Sleep(100);
            }

            Console.ReadKey();
            gpioController.UnregisterCallbackForPinValueChangedEvent(gpio2, MyCallback);
            gpioController.Dispose();
        }

        private void BoardInformation()
        {
            const int numberOfPings = 4;
            var results = _board.Ping(numberOfPings);
            int lostPackets = results.Count(x => x < TimeSpan.Zero);
            var validPackets = results.Where(x => x >= TimeSpan.Zero).ToList();
            double average = validPackets.Sum(x => x.TotalMilliseconds) / validPackets.Count();

            Console.WriteLine("Board information");
            Console.WriteLine($"Firmata Version:              {_board.FirmataVersion}");
            Console.WriteLine($"Firmware Version:             {_board.FirmwareVersion}");
            Console.WriteLine($"Firmware Name:                {_board.FirmwareName}");
            Console.WriteLine($"Round trip time:              {average:F1}ms ({validPackets.Count()} of {numberOfPings} received)");
            Console.WriteLine($" Pin capabilities: ");
            foreach (var pin in _board.SupportedPinConfigurations)
            {
                Console.Write($"    {pin.Pin}: ");
                Console.WriteLine(string.Join(", ", pin.PinModes.Select(x =>
                {
                    if (x == SupportedMode.AnalogInput)
                    {
                        return $"{x.Name} ({pin.AnalogInputResolutionBits} Bits Resolution, channel A{pin.AnalogPinNumber})";
                    }
                    else if (x == SupportedMode.Pwm)
                    {
                        return $"{x.Name} ({pin.PwmResolutionBits} Bits Resolution)";
                    }

                    return x.Name;
                })));
            }
        }

        private void SendBoardToSleep()
        {
            // Sends the board to sleep mode
            TimeSpan sleepDelay = TimeSpan.FromSeconds(5);
            if (!_board.SetSystemVariable(SystemVariable.SleepModeInterruptEnable, _buttonPin, 1))
            {
                return;
            }

            if (!_board.SetSystemVariable(SystemVariable.EnterSleepMode, _buttonPin, (int)sleepDelay.TotalSeconds))
            {
                return;
            }

            Console.WriteLine("Board is soon entering sleep. Connection might drop now.");
            Stopwatch sw = Stopwatch.StartNew();
            while (sw.Elapsed < sleepDelay + sleepDelay)
            {
                Thread.Sleep(50);
                var pings = _board.Ping(1);
                if (pings[0] < TimeSpan.Zero)
                {
                    break;
                }
            }

            sw.Restart();
            Console.WriteLine("Board is now asleep. Waiting for wakeup");
            while (true)
            {
                Thread.Sleep(50);
                var pings = _board.Ping(1);
                if (pings[0] >= TimeSpan.Zero)
                {
                    break;
                }
            }

            Console.WriteLine("Board is answering again");
            if (sw.Elapsed < TimeSpan.FromSeconds(2))
            {
                Console.WriteLine("That was to fast ... assuming a wrong interrupt was caught");
                SendBoardToSleep();
            }
        }
    }
}
