using System;
using System.Collections.Generic;
using System.Device.Gpio;
using System.Device.I2c;
using System.Diagnostics;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Iot.Device.Arduino.Tests;
using Iot.Device.Bmxx80;
using Iot.Device.Bmxx80.PowerMode;
using Iot.Device.CharacterLcd;
using Iot.Device.Graphics;
using UnitsNet;
using Xunit;

namespace Iot.Device.Arduino.Tests
{
    /// <summary>
    /// This class contains some larger examples for the Arduino compiler
    /// </summary>
    public class MiniExamples : ArduinoTestBase, IClassFixture<FirmataTestFixture>
    {
        public MiniExamples(FirmataTestFixture fixture)
            : base(fixture)
        {
            Compiler.ClearAllData(true, false);
        }

        [Fact]
        public void DisplayHelloWorld()
        {
            ExecuteComplexProgramSuccess<Func<int>>(UseI2cDisplay.Run, false);
        }

        [Fact]
        public void DisplayTheClock()
        {
            CompilerSettings s = new CompilerSettings()
            {
                CreateKernelForFlashing = false,
                LaunchProgramFromFlash = true,
                UseFlashForProgram = true,
                AutoRestartProgram = true,
                MaxMemoryUsage = 350 * 1024,
            };

            ExecuteComplexProgramSuccess<Func<int>>(UseI2cDisplay.RunClock, false, s);
        }

        public class UseI2cDisplay
        {
            public static int Run()
            {
                using I2cDevice i2cDevice = new ArduinoNativeI2cDevice(new I2cConnectionSettings(1, 0x27));
                using LcdInterface lcdInterface = LcdInterface.CreateI2c(i2cDevice, false);
                using Hd44780 hd44780 = new Lcd2004(lcdInterface);
                hd44780.UnderlineCursorVisible = false;
                hd44780.BacklightOn = true;
                hd44780.DisplayOn = true;
                hd44780.Clear();
                hd44780.Write("Hello World!");
                return 1;
            }

            public static int RunClock()
            {
                const int redLed = 6;
                const int button = 2;
                using GpioController gpioController = new GpioController(PinNumberingScheme.Logical, new ArduinoNativeGpioDriver());
                gpioController.OpenPin(redLed, PinMode.Output);
                gpioController.Write(redLed, PinValue.High);
                gpioController.OpenPin(button, PinMode.Input);
                // This Sleep is just because the display sometimes needs a bit of time to properly initialize -
                // otherwise it just doesn't properly accept commands
                Thread.Sleep(1000);
                using I2cDevice i2cDevice = new ArduinoNativeI2cDevice(new I2cConnectionSettings(1, 0x27));
                using LcdInterface lcdInterface = LcdInterface.CreateI2c(i2cDevice, false);
                using Hd44780 hd44780 = new Lcd2004(lcdInterface);
                hd44780.UnderlineCursorVisible = false;
                hd44780.BacklightOn = true;
                hd44780.DisplayOn = true;
                hd44780.Clear();
                hd44780.Write("Startup!");
                Thread.Sleep(500);
                LcdConsole console = new LcdConsole(hd44780, "A00", false);
                LcdCharacterEncoding encoding = LcdConsole.CreateEncoding(CultureInfo.CreateSpecificCulture("de-CH"), "A00", '?', 8);
                console.LoadEncoding(encoding);
                I2cDevice bme680Device = new ArduinoNativeI2cDevice(new I2cConnectionSettings(0, Bme680.DefaultI2cAddress));
                using Bme680 bme680 = new Bme680(bme680Device, Temperature.FromDegreesCelsius(20));
                bme680.Reset();
                bme680.GasConversionIsEnabled = false;
                bme680.HeaterIsEnabled = false;
                console.Clear();
                console.LineFeedMode = LineWrapMode.Truncate;
                gpioController.Write(redLed, PinValue.Low);
                while (gpioController.Read(button) == PinValue.Low)
                {
                    bme680.SetPowerMode(Bme680PowerMode.Forced);
                    if (bme680.TryReadTemperature(out Temperature temp) && bme680.TryReadPressure(out Pressure pressure))
                    {
                        string temperatureLine = temp.DegreesCelsius.ToString("F2") + " °C " + pressure.Hectopascals.ToString("F1") + " hPa";
                        Debug.WriteLine(temperatureLine);
                        console.ReplaceLine(0, temperatureLine);
                    }

                    if (bme680.TryReadHumidity(out RelativeHumidity humidity))
                    {
                        string humidityLine = humidity.Percent.ToString("F1") + "% RH";
                        console.ReplaceLine(1, humidityLine);
                    }

                    var time = DateTime.Now;
                    console.ReplaceLine(2, time.ToString("ddd dd. MMMM yyyy"));
                    console.SetCursorPosition(0, 3);
                    console.ReplaceLine(3, time.ToLongTimeString());
                    gpioController.Write(redLed, PinValue.High);
                    Thread.Sleep(100);
                    gpioController.Write(redLed, PinValue.Low);
                }

                return 1;
            }
        }
    }
}
