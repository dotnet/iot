// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using System;
using System.Collections.Generic;
using System.Device.Gpio;
using System.Device.Gpio.Drivers;
using System.Device.I2c;
using System.Device.Spi;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.IO.Ports;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Reflection;
using System.Text;
using System.Threading;
using Iot.Device.Adc;
using Iot.Device.Arduino;
using Iot.Device.Bmxx80;
using Iot.Device.Bmxx80.PowerMode;
using UnitsNet;

namespace Arduino.Samples
{
    /// <summary>
    /// Test application for Arduino/Firmata protocol
    /// </summary>
    internal class Program
    {
        /// <summary>
        /// Main entry point
        /// </summary>
        /// <param name="args">Unused</param>
        public static void Main(string[] args)
        {
            string portName = "INET";
            if (args.Length > 1)
            {
                portName = args[1];
            }

            if (portName == "INET")
            {
                ConnectToSocket();
                return;
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

                ConnectWithStream(port.BaseStream);
            }
        }

        private static void ConnectWithStream(Stream stream)
        {
            ArduinoBoard board = new ArduinoBoard(stream);
            try
            {
                board.LogMessages += BoardOnLogMessages;
                board.Initialize();
                Console.WriteLine(
                    $"Connection successful. Firmware version: {board.FirmwareVersion}, Builder: {board.FirmwareName}");
                while (Menu(board))
                {
                }
            }
            catch (TimeoutException x)
            {
                Console.WriteLine($"No answer from board: {x.Message} ");
            }
            finally
            {
                stream.Close();
                board?.Dispose();
            }
        }

        private static void ConnectToSocket()
        {
            Socket s = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
            s.Connect(IPAddress.Loopback, 27015);
            s.NoDelay = true;
            using (NetworkStream ns = new NetworkStream(s, true))
            {
                ConnectWithStream(ns);
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

        private static bool Menu(ArduinoBoard board)
        {
            Console.WriteLine("Hello I2C and GPIO on Arduino!");
            Console.WriteLine("Select the test you want to run:");
            Console.WriteLine(" 1 Run I2C tests with a BMP280");
            Console.WriteLine(" 2 Run GPIO tests with a simple led blinking on GPIO6 port");
            Console.WriteLine(" 3 Run polling button test on GPIO2");
            Console.WriteLine(" 4 Run event wait test event on GPIO2 on Falling and Rising");
            Console.WriteLine(" 5 Run callback event test on GPIO2");
            Console.WriteLine(" 6 Run PWM test with a simple led dimming on GPIO6 port");
            Console.WriteLine(" 7 Dim the LED according to the input on A1");
            Console.WriteLine(" 8 Read analog channel as fast as possible");
            Console.WriteLine(" 9 Run SPI tests with an MCP3008 (experimental)");
            Console.WriteLine(" 0 Detect all devices on the I2C bus");
            Console.WriteLine(" H Read DHT11 Humidity sensor on GPIO 3 (experimental)");
            Console.WriteLine(" C Test C# IL execution on device (very experimental)");
            Console.WriteLine(" X Exit");
            var key = Console.ReadKey();
            Console.WriteLine();

            switch (key.KeyChar)
            {
                case '1':
                    TestI2c(board);
                    break;
                case '2':
                    TestGpio(board);
                    break;
                case '3':
                    TestInput(board);
                    break;
                case '4':
                    TestEventsDirectWait(board);
                    break;
                case '5':
                    TestEventsCallback(board);
                    break;
                case '6':
                    TestPwm(board);
                    break;
                case '7':
                    TestAnalogIn(board);
                    break;
                case '8':
                    TestAnalogCallback(board);
                    break;
                case '9':
                    TestSpi(board);
                    break;
                case '0':
                    ScanDeviceAddressesOnI2cBus(board);
                    break;
                case 'h':
                case 'H':
                    TestDht(board);
                    break;
                case 'x':
                case 'X':
                    return false;
                case 'c':
                case 'C':
                    TestIlInterpreter(board);
                    break;
            }

            return true;
        }

        private static void TestPwm(ArduinoBoard board)
        {
            int pin = 6;
            using (var pwm = board.CreatePwmChannel(0, pin, 100, 0))
            {
                Console.WriteLine("Now dimming LED. Press any key to exit");
                while (!Console.KeyAvailable)
                {
                    pwm.Start();
                    for (double fadeValue = 0; fadeValue <= 1.0; fadeValue += 0.05)
                    {
                        // sets the value (range from 0 to 255):
                        pwm.DutyCycle = fadeValue;
                        // wait for 30 milliseconds to see the dimming effect
                        Thread.Sleep(30);
                    }

                    // fade out from max to min in increments of 5 points:
                    for (double fadeValue = 1.0; fadeValue >= 0; fadeValue -= 0.05)
                    {
                        // sets the value (range from 0 to 255):
                        pwm.DutyCycle = fadeValue;
                        // wait for 30 milliseconds to see the dimming effect
                        Thread.Sleep(30);
                    }

                }

                Console.ReadKey();
                pwm.Stop();
            }
        }

        private static void TestI2c(ArduinoBoard board)
        {
            var device = board.CreateI2cDevice(new I2cConnectionSettings(0, Bmp280.DefaultI2cAddress));

            var bmp = new Bmp280(device);
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

            bmp.Dispose();
            device.Dispose();
            Console.ReadKey();
            Console.WriteLine();
        }

        private static void ScanDeviceAddressesOnI2cBus(ArduinoBoard board)
        {
            StringBuilder stringBuilder = new StringBuilder();

            stringBuilder.Append("     0  1  2  3  4  5  6  7  8  9  a  b  c  d  e  f");
            stringBuilder.Append(Environment.NewLine);

            for (int startingRowAddress = 0; startingRowAddress < 128; startingRowAddress += 16)
            {
                stringBuilder.Append($"{startingRowAddress:x2}: ");  // Beginning of row.

                for (int rowAddress = 0; rowAddress < 16; rowAddress++)
                {
                    int deviceAddress = startingRowAddress + rowAddress;

                    // Skip the unwanted addresses.
                    if (deviceAddress < 0x3 || deviceAddress > 0x77)
                    {
                        stringBuilder.Append("   ");
                        continue;
                    }

                    var connectionSettings = new I2cConnectionSettings(0, deviceAddress);
                    using (var i2cDevice = board.CreateI2cDevice(connectionSettings))
                    {
                        try
                        {
                            i2cDevice.ReadByte();  // Only checking if device is present.
                            stringBuilder.Append($"{deviceAddress:x2} ");
                        }
                        catch
                        {
                            stringBuilder.Append("-- ");
                        }
                    }
                }

                stringBuilder.Append(Environment.NewLine);
            }

            Console.WriteLine(stringBuilder.ToString());
        }

        public static void TestGpio(ArduinoBoard board)
        {
            // Use Pin 6
            const int gpio = 6;
            var gpioController = board.CreateGpioController(PinNumberingScheme.Board);

            // Opening GPIO2
            gpioController.OpenPin(gpio);
            gpioController.SetPinMode(gpio, PinMode.Output);

            Console.WriteLine("Blinking GPIO6");
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

        public static void TestAnalogIn(ArduinoBoard board)
        {
            // Use Pin 6
            const int gpio = 6;
            const int analogPin = 15;
            var gpioController = board.CreateGpioController(PinNumberingScheme.Board);
            var analogController = board.CreateAnalogController(0);

            var pin = analogController.OpenPin(analogPin);
            gpioController.OpenPin(gpio);
            gpioController.SetPinMode(gpio, PinMode.Output);

            Console.WriteLine("Blinking GPIO6, based on analog input.");
            while (!Console.KeyAvailable)
            {
                double voltage = pin.ReadVoltage();
                gpioController.Write(gpio, PinValue.High);
                Thread.Sleep((int)voltage * 100);
                voltage = pin.ReadVoltage();
                gpioController.Write(gpio, PinValue.Low);
                Thread.Sleep((int)voltage * 100);
            }

            pin.Dispose();
            Console.ReadKey();
            analogController.Dispose();
            gpioController.Dispose();
        }

        public static void TestAnalogCallback(ArduinoBoard board)
        {
            int analogPin = GetAnalogPin1(board);
            var analogController = board.CreateAnalogController(0);

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

        private static int GetAnalogPin1(ArduinoBoard board)
        {
            int analogPin = 15;
            foreach (var pin in board.SupportedPinConfigurations)
            {
                if (pin.AnalogPinNumber == 1)
                {
                    analogPin = pin.Pin;
                    break;
                }
            }

            return analogPin;
        }

        public static void TestInput(ArduinoBoard board)
        {
            const int gpio = 2;
            var gpioController = board.CreateGpioController(PinNumberingScheme.Board);

            // Opening GPIO2
            gpioController.OpenPin(gpio);
            gpioController.SetPinMode(gpio, PinMode.Input);

            if (gpioController.GetPinMode(gpio) != PinMode.Input)
            {
                throw new InvalidOperationException("Couldn't set pin mode");
            }

            Console.WriteLine("Polling input pin 2");
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

        public static void TestEventsDirectWait(ArduinoBoard board)
        {
            const int Gpio2 = 2;
            var gpioController = board.CreateGpioController(PinNumberingScheme.Board);

            // Opening GPIO2
            gpioController.OpenPin(Gpio2);
            gpioController.SetPinMode(Gpio2, PinMode.Input);

            Console.WriteLine("Waiting for both falling and rising events");
            while (!Console.KeyAvailable)
            {
                var res = gpioController.WaitForEvent(Gpio2, PinEventTypes.Falling | PinEventTypes.Rising, new TimeSpan(0, 0, 0, 0, 50));
                if ((!res.TimedOut) && (res.EventTypes != PinEventTypes.None))
                {
                    Console.WriteLine($"Event on GPIO {Gpio2}, event type: {res.EventTypes}");
                }
            }

            Console.ReadKey();
            Console.WriteLine("Waiting for only rising events");
            while (!Console.KeyAvailable)
            {
                var res = gpioController.WaitForEvent(Gpio2, PinEventTypes.Rising, new TimeSpan(0, 0, 0, 0, 50));
                if ((!res.TimedOut) && (res.EventTypes != PinEventTypes.None))
                {
                    MyCallback(gpioController, new PinValueChangedEventArgs(res.EventTypes, Gpio2));
                }
            }

            gpioController.Dispose();
        }

        public static void TestEventsCallback(ArduinoBoard board)
        {
            const int Gpio2 = 2;
            var gpioController = board.CreateGpioController(PinNumberingScheme.Board);

            // Opening GPIO2
            gpioController.OpenPin(Gpio2);
            gpioController.SetPinMode(Gpio2, PinMode.Input);

            Console.WriteLine("Setting up events on GPIO2 for rising and falling");

            gpioController.RegisterCallbackForPinValueChangedEvent(Gpio2, PinEventTypes.Falling | PinEventTypes.Rising, MyCallback);
            Console.WriteLine("Event setup, press a key to remove the falling event");
            while (!Console.KeyAvailable)
            {
                // Nothing to do
                Thread.Sleep(100);
            }

            Console.ReadKey();
            gpioController.UnregisterCallbackForPinValueChangedEvent(Gpio2, MyCallback);
            gpioController.RegisterCallbackForPinValueChangedEvent(Gpio2, PinEventTypes.Rising, MyCallback);
            Console.WriteLine("Now only waiting for rising events, press a key to remove all events and quit");
            while (!Console.KeyAvailable)
            {
                // Nothing to do
                Thread.Sleep(100);
            }

            Console.ReadKey();
            gpioController.UnregisterCallbackForPinValueChangedEvent(Gpio2, MyCallback);
            gpioController.Dispose();
        }

        private static void MyCallback(object sender, PinValueChangedEventArgs pinValueChangedEventArgs)
        {
            Console.WriteLine($"Event on GPIO {pinValueChangedEventArgs.PinNumber}, event type: {pinValueChangedEventArgs.ChangeType}");
        }

        public static void TestSpi(ArduinoBoard board)
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

        public static void TestDht(ArduinoBoard board)
        {
            Console.WriteLine("Reading DHT11. Any key to quit.");

            while (!Console.KeyAvailable)
            {
                if (board.TryReadDht(3, 11, out var temperature, out var humidity))
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

        public static void TestIlInterpreter(ArduinoBoard board)
        {
            ArduinoCsCompiler compiler = new ArduinoCsCompiler(board);
            BasicCalculationTest(compiler);

            OperatorTest(compiler);

            //// AsyncExecutionTest(board, compiler);

            //// ReadDht11Test(compiler);

            LoadClassAndBlinkLed(compiler);

            compiler.Dispose();
        }

        private static void LoadClassAndBlinkLed(ArduinoCsCompiler compiler)
        {
            // These operations should be combined into one, to simplify usage (just provide the main entry point,
            // and derive everything required from there)
            compiler.LoadLowLevelInterface();
            compiler.LoadClass(typeof(ArduinoCompilerSampleMethods.SimpleLedBinding));
            // This should just return a reference to the now already loaded method
            var task = compiler.LoadCode<Action<int, int>>(ArduinoCompilerSampleMethods.SimpleLedBinding.RunBlink);

            HashSet<MethodBase> methods = new HashSet<MethodBase>();

            compiler.CollectDependencies(task.MethodInfo.MethodBase, methods);

            var list = methods.ToList();
            for (var index = 0; index < list.Count; index++)
            {
                var dep = list[index];
                // If we have a ctor in the call chain we need to ensure we have its class loaded.
                // This happens if the created object is only used in local variables but not as a class member
                // seen so far.
                if (dep.IsConstructor && dep.DeclaringType != null && !dep.DeclaringType.IsValueType)
                {
                    compiler.LoadClass(dep.DeclaringType);
                }
                else if (dep.DeclaringType != null && HasStaticFields(dep.DeclaringType))
                {
                    // Also load the class declaration if it contains static fields.
                    // TODO: We currently assume that no class is accessing static fields of another class.
                    compiler.LoadClass(dep.DeclaringType);
                }

                // Type is irrelevant here (should probably split this function into loading and preparing)
                compiler.LoadCode<Action>(dep);
            }

            task.InvokeAsync(6, 1000);

            task.WaitForResult();

            compiler.ClearAllData(true);
        }

        private static bool HasStaticFields(Type cls)
        {
            foreach (var fld in cls.GetFields())
            {
                if (fld.IsStatic)
                {
                    return true;
                }
            }

            return false;
        }

        private static void ReadDht11Test(ArduinoCsCompiler compiler)
        {
            object[] data;
            MethodState state;

            compiler.LoadLowLevelInterface();
            var dht = compiler.LoadCode(new Func<IArduinoHardwareLevelAccess, int, UInt32>(ArduinoCompilerSampleMethods.ReadDht11));
            dht.InvokeAsync(0, 3);

            CancellationTokenSource ts = new CancellationTokenSource(10000);
            bool result = dht.WaitForResult(ts.Token).Result;

            if (!result)
            {
                Console.WriteLine("Method execution timed out.");
            }
            else if (dht.GetMethodResults(out data, out state) && state == MethodState.Stopped)
            {
                UInt32 raw = (UInt32)data[0] >> 16;
                double temperature = raw * 0.1;
                raw = (UInt32)data[0] & 0xFFFF;
                double humidity = raw * 0.1;

                Console.WriteLine($"DHT 11 Temperature: {temperature}°C, Humidity {humidity}%");
            }
            else
            {
                Console.WriteLine($"Unable to read DHT temperature: {state}.");
            }

            dht.Terminate();
        }

        private static void AsyncExecutionTest(ArduinoBoard board, ArduinoCsCompiler compiler)
        {
            compiler.LoadLowLevelInterface();
            compiler.LoadCode(new Func<int, int, bool>(ArduinoCompilerSampleMethods.Smaller));
            var method3 = compiler.LoadCode(new Action<IArduinoHardwareLevelAccess, int, int>(ArduinoCompilerSampleMethods.Blink));
            method3.InvokeAsync(0, 10, 500);

            // While the above method executes (and blinks the led), we query the analog input
            var analogController = board.CreateAnalogController(0);
            int analogPin = 15;

            var pin = analogController.OpenPin(analogPin);

            while (method3.State == MethodState.Running)
            {
                double value = pin.ReadVoltage();
                Console.WriteLine($"Read analog value as {value:F2}");
                Thread.Sleep(100);
            }

            analogController.Close(pin);
            method3.WaitForResult();

            compiler.ClearAllData(true);

            // Start task again and terminate it immediately
            compiler.LoadLowLevelInterface();
            compiler.LoadCode(new Func<int, int, bool>(ArduinoCompilerSampleMethods.Smaller));
            method3 = compiler.LoadCode(new Action<IArduinoHardwareLevelAccess, int, int>(ArduinoCompilerSampleMethods.Blink));
            method3.InvokeAsync(0, 10, 500);
            method3.Terminate();
            if (method3.State != MethodState.Killed)
            {
                Console.WriteLine("Unable to terminate task");
            }

            method3.Dispose();
            compiler.ClearAllData(true);
        }

        // Note: Input values shall be 1 and 2.
        // It is only used to prevent the compiler from optimizing away anything
        private static bool PerformOperatorTest(int inputValue1, int inputValue2)
        {
            if (inputValue2 >= 20)
            {
                return false;
            }

            if (inputValue1 > inputValue2)
            {
                return false;
            }

            if (inputValue2 != 2)
            {
                return false;
            }

            if (inputValue1 <= 0)
            {
                return false;
            }

            return true;
        }

        private static void OperatorTest(ArduinoCsCompiler compiler)
        {
            var method1 = compiler.LoadCode<Func<int, int, bool>>(PerformOperatorTest);
            bool result = method1.Invoke(CancellationToken.None, 0, 1, 2);
            if (!result)
            {
                Console.WriteLine("Test failed");
            }

            compiler.ClearAllData(true);
        }

        private static void BasicCalculationTest(ArduinoCsCompiler compiler)
        {
            var method1 = compiler.LoadCode<Func<int, int, int>>(ArduinoCompilerSampleMethods.AddInts);
            method1.InvokeAsync(2, 3);
            int result;
            method1.WaitForResult();
            method1.GetMethodResults(out object[] data, out MethodState state);
            if (state != MethodState.Stopped)
            {
                Console.WriteLine("Method returned result but did not end?!?");
            }

            result = (int)data[0];
            Console.WriteLine($"2 + 3 = {result}");
            method1.InvokeAsync(255, 5);
            method1.WaitForResult();
            method1.GetMethodResults(out data, out state);
            result = (int)data[0];
            Console.WriteLine($"255 + 5 = {result}");

            var method2 = compiler.LoadCode(new Func<int, int, bool>(ArduinoCompilerSampleMethods.Equal));
            method2.InvokeAsync(2, 3);
            method2.WaitForResult();
            method2.GetMethodResults(out data, out state);
            bool trueOrFalse = (bool)data[0];
            Console.WriteLine($"Is 2 == 3? {trueOrFalse}");
            method2.InvokeAsync(257, 257);
            method2.WaitForResult();
            method2.GetMethodResults(out data, out state);
            trueOrFalse = (bool)data[0];
            Console.WriteLine($"Is 257 == 257? {trueOrFalse}");
        }
    }
}
