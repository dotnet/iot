// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using System;
using System.Device.Gpio;
using System.Device.I2c;
using System.IO;
using System.Threading;
using Iot.Device.Ccs811;
using Iot.Device.Ft4222;
using UnitsNet;

namespace Ccs811sample
{
    /// <summary>
    /// Sample class for CCS811
    /// </summary>
    public class Css811SensorSample
    {
        private static void Main(string[] args)
        {
            Ccs811Sensor ccs811;

            Console.WriteLine("Hello CCS811!");
            // Simple menu to select native I2C/GPIO or thru FT4222, with GPIO pins and specific features to test
            Console.WriteLine("Select which platform I2C/GPIO you want to use:");
            Console.WriteLine(" 1. Native I2C/GPIO");
            Console.WriteLine(" 2. FT4222");
            var platformChoice = Console.ReadKey();
            Console.WriteLine();
            Console.WriteLine("Which I2C address do you want to use? Use the first one if Address pin is to ground, use the second one if to VCC.");
            Console.WriteLine($" 1. First 0x{Ccs811Sensor.I2cFirstAddress:X2}");
            Console.WriteLine($" 2. Second 0x{Ccs811Sensor.I2cSecondAddress:X2}");
            var addressChoice = Console.ReadKey();
            Console.WriteLine();
            Console.WriteLine("Do you want to the interrupt pin and events?");
            Console.WriteLine("(Y)es,(N)o");
            var pinChoice = Console.ReadKey();
            Console.WriteLine();
            int pinInterrupt = CheckAndAskPinNumber(pinChoice.KeyChar);
            Console.WriteLine($"Do you want to use the Wake pin?");
            Console.WriteLine("(Y)es,(N)o");
            pinChoice = Console.ReadKey();
            Console.WriteLine();
            int pinWake = CheckAndAskPinNumber(pinChoice.KeyChar);
            Console.WriteLine($"Do you want to use the Reset pin?");
            Console.WriteLine("(Y)es,(N)o");
            pinChoice = Console.ReadKey();
            Console.WriteLine();
            int pinReset = CheckAndAskPinNumber(pinChoice.KeyChar);

            if (platformChoice.KeyChar == '1')
            {
                Console.WriteLine("Creating an instance of a CCS811 using the platform drivers.");
                ccs811 = new Ccs811Sensor(I2cDevice.Create(new I2cConnectionSettings(3, addressChoice.KeyChar == '1' ? Ccs811Sensor.I2cFirstAddress : Ccs811Sensor.I2cSecondAddress)), pinWake: pinWake, pinInterruption: pinInterrupt, pinReset: pinReset);
            }
            else if (platformChoice.KeyChar == '2')
            {
                Console.WriteLine("Creating an instance of a CCS811 using FT4222 drivers.");
                var ftdiI2C = new Ft4222I2c(new I2cConnectionSettings(0, addressChoice.KeyChar == '1' ? Ccs811Sensor.I2cFirstAddress : Ccs811Sensor.I2cSecondAddress));
                var gpioController = new GpioController(PinNumberingScheme.Board, new Ft4222Gpio());
                ccs811 = new Ccs811Sensor(ftdiI2C, gpioController, pinWake, pinInterrupt, pinReset, false);
            }
            else
            {
                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine("Error: Invalid platform choice.");
                Console.ResetColor();
                return;
            }

            Console.WriteLine("Choose your operation mode:");
            int i = 0;
            OperationMode operationMode = OperationMode.ConstantPower1Second;
            foreach (var mode in Enum.GetValues(typeof(OperationMode)))
            {
                Console.WriteLine($" {i++}. {mode}");
            }

            var modeChoice = Console.ReadKey();
            try
            {
                // Converting the char to decimal, removing '0' = 0x30
                int selectedMode = Convert.ToInt32(modeChoice.KeyChar) - 0x30;
                if ((selectedMode >= 0) && (selectedMode <= 4))
                {
                    operationMode = (OperationMode)Convert.ToInt32(selectedMode);
                }
                else
                {
                    Console.ForegroundColor = ConsoleColor.Red;
                    Console.WriteLine($"Error selecting mode, default {operationMode} will be selected");
                }
            }
            catch (StackOverflowException)
            {
                Console.ForegroundColor = ConsoleColor.Yellow;
                Console.WriteLine($"Error selecting mode, default {operationMode} will be selected");
            }

            Console.ResetColor();
            DisplayBasicInformatio(ccs811);
            Console.WriteLine($"Current operating mode: {ccs811.OperationMode}, changing for {operationMode}");
            ccs811.OperationMode = operationMode;
            Console.WriteLine($"Current operating mode: {ccs811.OperationMode}");
            Console.ForegroundColor = ConsoleColor.Yellow;
            Console.WriteLine($"Warning: the sensor needs to run for 48h in operation mode {OperationMode.ConstantPower1Second} before getting accurate results. " +
                $"Also, every time you'll start the sensor, it will need approximately 20 minutes to get accurate results as well");
            Console.ResetColor();

            if (pinInterrupt >= 0)
            {
                Console.WriteLine("Interruption mode selected.");
                Console.WriteLine("Do you want to enable Threshold interruption?");
                Console.WriteLine("(Y)es,(N)o");
                var threshChoice = Console.ReadKey();
                Console.WriteLine();
                if ((threshChoice.KeyChar == 'Y') || (threshChoice.KeyChar == 'y'))
                {
                    TestThresholdAndInterrupt(ccs811);
                }
                else
                {
                    Console.Write(", once a measurement will be available, it will be displayed. Press any key to exit the program.");
                    ccs811.MeasurementReady += Ccs811MeasurementReady;
                    while (!Console.KeyAvailable)
                    {
                        Thread.Sleep(10);
                    }
                }
            }
            else
            {
                Console.WriteLine("What to you want to test?");
                Console.WriteLine(" 1. Read and display gas information for 10 times.");
                Console.WriteLine(" 2. Read and display gas information for 1000 times.");
                Console.WriteLine(" 3. Read and display detailed gas information for 10 times.");
                Console.WriteLine(" 4. Read and display detailed gas information for 1000 times.");
                Console.WriteLine(" 5. Read, load and change back the baseline.");
                Console.WriteLine(" 6. Test temperature and humidity changes.");
                Console.WriteLine(" 7. Read and log gas information 10000 times.");
                var operationChoice = Console.ReadKey();
                Console.WriteLine();
                switch (operationChoice.KeyChar)
                {
                    case '1':
                        ReadAnDisplay(ccs811);
                        break;
                    case '2':
                        ReadAnDisplay(ccs811, 1000);
                        break;
                    case '3':
                        ReadAndDisplayDetails(ccs811);
                        break;
                    case '4':
                        ReadAndDisplayDetails(ccs811, 1000);
                        break;
                    case '5':
                        TestBaseline(ccs811);
                        break;
                    case '6':
                        TestTemperatureHumidityAdjustment(ccs811);
                        break;
                    case '7':
                        Console.WriteLine("Result file will be log.csv. The file is flushed on the disk every 100 results.");
                        ReadAndLog(ccs811, 10000);
                        break;
                    default:
                        Console.ForegroundColor = ConsoleColor.Red;
                        Console.WriteLine("Error: Sorry, didn't get your choice, program will now exit.");
                        Console.ResetColor();
                        break;
                }
            }

            Console.WriteLine($"Current operating mode: {ccs811.OperationMode}, changing for {OperationMode.Idle}");
            ccs811.OperationMode = OperationMode.Idle;
            Console.WriteLine($"Current operating mode: {ccs811.OperationMode}");
            // Dispose the CCS811 sensor
            ccs811.Dispose();
        }

        private static int CheckAndAskPinNumber(char toCehck)
        {
            if ((toCehck == 'Y') || (toCehck == 'y'))
            {
                Console.WriteLine("Type the GPIO number with pin numbering scheme, you want to use:");
                var pinInterChoice = Console.ReadLine();
                try
                {
                    return Convert.ToInt32(pinInterChoice);
                }
                catch (OverflowException)
                {
                    Console.ForegroundColor = ConsoleColor.Red;
                    Console.WriteLine("Error: Can't convert your pin number.");
                    Console.ResetColor();
                }
            }

            return -1;
        }

        private static void Ccs811MeasurementReady(object sender, MeasurementArgs args)
        {
            Console.WriteLine($"Measurement Event: Success: {args.MeasurementSuccess}, eCO2: {args.EquivalentCO2.PartsPerMillion} ppm, " +
                $"eTVOC: {args.EquivalentTotalVolatileOrganicCompound.PartsPerBillion} ppb, Current: {args.RawCurrentSelected.Microamperes} µA, " +
                $"ADC: {args.RawAdcReading} = {args.RawAdcReading * 1.65 / 1023} V.");
        }

        private static void DisplayBasicInformatio(Ccs811Sensor ccs811)
        {
            Console.WriteLine($"Hardware identification: 0x{ccs811.HardwareIdentification:X2}, must be 0x81");
            Console.WriteLine($"Hardware version: 0x{ccs811.HardwareVersion:X2}, must be 0x1X where any X is valid");
            Console.WriteLine($"Application version: {ccs811.ApplicationVersion}");
            Console.WriteLine($"Boot loader version: {ccs811.BootloaderVersion}");
        }

        private static void TestBaseline(Ccs811Sensor ccs811)
        {
            var baseline = ccs811.BaselineAlgorithmCalculation;
            Console.WriteLine($"Baseline calculation value: {baseline}, changing baseline");
            // Please refer to documentation, baseline is not a human readable number
            ccs811.BaselineAlgorithmCalculation = 50300;
            Console.WriteLine($"Baseline calculation value: {ccs811.BaselineAlgorithmCalculation}, changing baseline for the previous one");
            ccs811.BaselineAlgorithmCalculation = baseline;
            Console.WriteLine($"Baseline calculation value: {ccs811.BaselineAlgorithmCalculation}");
        }

        private static void TestTemperatureHumidityAdjustment(Ccs811Sensor ccs811)
        {
            Console.WriteLine("Drastically change the temperature and humidity to see the impact on the calculation " +
                "In real life, we'll get normal data and won't change them that often. " +
                "The system does not react the best way when shake like this");
            // First use with the default ones, no changes should appear
            Temperature temp = Temperature.FromDegreesCelsius(25);
            Ratio hum = Ratio.FromPercent(50);
            Console.WriteLine($"Changing temperature and humidity reference to {temp.DegreesCelsius:0.00} C, {hum.Percent:0.0} %, baseline for calculation: {ccs811.BaselineAlgorithmCalculation}");
            ccs811.SetEnvironmentData(temp, hum);
            ReadAndDisplayDetails(ccs811, 100);
            // Changing with very different temperature
            temp = Temperature.FromDegreesCelsius(70);
            hum = Ratio.FromPercent(53.8);
            Console.WriteLine($"Changing temperature and humidity reference to {temp.DegreesCelsius:0.00} C, {hum.Percent:0.0} %, baseline for calculation: {ccs811.BaselineAlgorithmCalculation}");
            ccs811.SetEnvironmentData(temp, hum);
            ReadAndDisplayDetails(ccs811, 100);

            temp = Temperature.FromDegreesCelsius(-25);
            hum = Ratio.FromPercent(0.5);
            Console.WriteLine($"Changing temperature and humidity reference to {temp.DegreesCelsius:0.00} C, {hum.Percent:0.0} %, baseline for calculation: {ccs811.BaselineAlgorithmCalculation}");
            ccs811.SetEnvironmentData(temp, hum);
            ReadAndDisplayDetails(ccs811, 100);
            // Back to normal which still can lead to different results than initially
            // This is due to the baseline
            temp = Temperature.FromDegreesCelsius(25);
            hum = Ratio.FromPercent(50);
            Console.WriteLine($"Changing temperature and humidity reference to {temp.DegreesCelsius:0.00} C, {hum.Percent:0.0} %, baseline for calculation: {ccs811.BaselineAlgorithmCalculation}");
            ccs811.SetEnvironmentData(temp, hum);
            ReadAndDisplayDetails(ccs811, 100);
        }

        private static void TestThresholdAndInterrupt(Ccs811Sensor ccs811)
        {
            if (!ccs811.InterruptEnable)
            {
                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine("Error: interrupt needs to be activated to run this test");
                Console.ResetColor();
                return;
            }

            ccs811.MeasurementReady += Ccs811MeasurementReady;
            // Setting up a range where we will see something in a normal environment
            VolumeConcentration low = VolumeConcentration.FromPartsPerMillion(400);
            VolumeConcentration high = VolumeConcentration.FromPartsPerMillion(600);
            Console.WriteLine($"Setting up {low.PartsPerMillion}-{high.PartsPerMillion} range, in clear environment, that should raise interrupts. Wait 3 minutes and change mode. Blow on the sensor and wait a bit.");
            Console.ForegroundColor = ConsoleColor.Yellow;
            Console.WriteLine("Warning: only the first measurement to cross the threshold is raised.");
            Console.ResetColor();
            ccs811.SetThreshold(low, high);
            DateTime dt = DateTime.Now.AddMinutes(3);
            while (dt > DateTime.Now)
            {
                Thread.Sleep(10);
            }

            low = VolumeConcentration.FromPartsPerMillion(15000);
            high = VolumeConcentration.FromPartsPerMillion(20000);
            Console.WriteLine($"Changing threshold for {low.PartsPerMillion}-{high.PartsPerMillion}, a non reachable range in clear environment. No measurement should appear in next 3 minutes");
            dt = DateTime.Now.AddMinutes(3);
            ccs811.SetThreshold(low, high);
            while (dt > DateTime.Now)
            {
                Thread.Sleep(10);
            }
        }

        private static void ReadAndLog(Ccs811Sensor ccs811, int count = 10)
        {
            string toWrite = "Time;Success;eCO2 (ppm);eTVOC (ppb);Current (µA);ADC;ADC (V);Baseline";

            using (var fl = new StreamWriter("log.csv"))
            {
                fl.WriteLine(toWrite);
                for (int i = 0; i < count; i++)
                {
                    while (!ccs811.IsDataReady)
                    {
                        Thread.Sleep(10);
                    }

                    var error = ccs811.TryReadGasData(out VolumeConcentration eCO2, out VolumeConcentration eTVOC, out ElectricCurrent curr, out int adc);
                    toWrite = $"{DateTime.Now};{error};{eCO2.PartsPerMillion};{eTVOC.PartsPerBillion};{curr.Microamperes};{adc};{adc * 1.65 / 1023};{ccs811.BaselineAlgorithmCalculation}";
                    fl.WriteLine(toWrite);
                    Console.WriteLine(toWrite);
                    if (i % 100 == 0)
                    {
                        fl.Flush();
                    }
                }
            }
        }

        private static void ReadAnDisplay(Ccs811Sensor ccs811, int count = 10)
        {
            for (int i = 0; i < count; i++)
            {
                while (!ccs811.IsDataReady)
                {
                    Thread.Sleep(10);
                }

                var error = ccs811.TryReadGasData(out VolumeConcentration eCO2, out VolumeConcentration eTVOC);
                Console.WriteLine($"Success: {error}, eCO2: {eCO2.PartsPerMillion} ppm, eTVOC: {eTVOC.PartsPerBillion} ppb");
            }
        }

        private static void ReadAndDisplayDetails(Ccs811Sensor ccs811, int count = 10)
        {
            for (int i = 0; i < count; i++)
            {
                while (!ccs811.IsDataReady)
                {
                    Thread.Sleep(10);
                }

                var error = ccs811.TryReadGasData(out VolumeConcentration eCO2, out VolumeConcentration eTVOC, out ElectricCurrent curr, out int adc);
                Console.WriteLine($"Success: {error}, eCO2: {eCO2.PartsPerMillion} ppm, eTVOC: {eTVOC.PartsPerBillion} ppb, Current: {curr.Microamperes} µA, ADC: {adc} = {adc * 1.65 / 1023} V.");
            }
        }
    }
}
