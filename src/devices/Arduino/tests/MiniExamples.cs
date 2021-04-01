using System;
using System.Collections.Generic;
using System.Device.Gpio;
using System.Device.I2c;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Iot.Device.Arduino.Tests;
using Iot.Device.CharacterLcd;
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
                MaxMemoryUsage = 256 * 1024,
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
                using GpioController gpioController = new GpioController(PinNumberingScheme.Logical, new ArduinoNativeGpioDriver());
                gpioController.OpenPin(redLed, PinMode.Output);
                gpioController.Write(redLed, PinValue.High);
                Thread.Sleep(1000);
                using I2cDevice i2cDevice = new ArduinoNativeI2cDevice(new I2cConnectionSettings(1, 0x27));
                using LcdInterface lcdInterface = LcdInterface.CreateI2c(i2cDevice, false);
                using Hd44780 hd44780 = new Lcd2004(lcdInterface);
                hd44780.UnderlineCursorVisible = false;
                hd44780.BacklightOn = true;
                hd44780.DisplayOn = true;
                hd44780.Clear();
                hd44780.Write("Hello World!");
                gpioController.Write(redLed, PinValue.Low);
                for (int i = 0; i < 60; i++)
                {
                    hd44780.SetCursorPosition(0, 1);
                    var time = DateTime.Now;
                    hd44780.Write(time.ToString("dddd"));
                    hd44780.SetCursorPosition(0, 2);
                    hd44780.Write(time.ToString("dd. MMMM yyyy"));
                    hd44780.SetCursorPosition(0, 3);
                    hd44780.Write(time.ToLongTimeString());
                    Thread.Sleep(800);
                    gpioController.Write(redLed, PinValue.High);
                    Thread.Sleep(100);
                    gpioController.Write(redLed, PinValue.Low);
                }

                return 1;
            }
        }
    }
}
