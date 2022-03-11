// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System;
using System.Collections.Generic;
using System.Device.Gpio;
using System.Device.I2c;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using ArduinoCsCompiler;
using ArduinoCsCompiler.Runtime;
using Iot.Device.Arduino.Tests;
using Iot.Device.Bmxx80;
using Iot.Device.Bmxx80.PowerMode;
using Iot.Device.CharacterLcd;
using Iot.Device.Common;
using Iot.Device.Graphics;
using UnitsNet;
using Xunit;

namespace Iot.Device.Arduino.Tests
{
    /// <summary>
    /// This class contains some larger examples for the Arduino compiler
    /// </summary>
    [Collection("SingleClientOnly")]
    [Trait("feature", "firmata")]
    [Trait("requires", "hardware")]
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
            CompilerSettings s = new CompilerSettings()
            {
                CreateKernelForFlashing = false,
                LaunchProgramFromFlash = true,
                UseFlashForProgram = true,
                AutoRestartProgram = true,
                MaxMemoryUsage = 350 * 1024,
            };

            ExecuteComplexProgramSuccess<Func<int>>(UseI2cDisplay.Run, false, s);
        }

        [Fact]
        public void FileSystemCheck()
        {
            // The File System access methods are currently not implemented for the simulator
            if (Board.FirmwareName.Contains("Simulator", StringComparison.InvariantCultureIgnoreCase))
            {
                return;
            }

            CompilerSettings s = new CompilerSettings()
            {
                CreateKernelForFlashing = false,
                LaunchProgramFromFlash = false,
                UseFlashForProgram = true,
                AutoRestartProgram = false,
                MaxMemoryUsage = 350 * 1024,
                ForceFlashWrite = true,
            };

            ExecuteComplexProgramSuccess<Func<int>>(UseI2cDisplay.FileSystemTest, false, s);
        }

        public class UseI2cDisplay
        {
            private const int StationAltitude = 650;
            public static int Run()
            {
                using var board = new ArduinoNativeBoard();
                using I2cDevice i2cDevice = board.CreateI2cDevice(new I2cConnectionSettings(0, 0x27));
                using LcdInterface lcdInterface = LcdInterface.CreateI2c(i2cDevice, false);
                using Hd44780 hd44780 = new Lcd2004(lcdInterface);
                hd44780.UnderlineCursorVisible = false;
                hd44780.BacklightOn = true;
                hd44780.DisplayOn = true;
                hd44780.Clear();
                hd44780.Write("Hello World!");
                return 1;
            }

            public static int FileSystemTest()
            {
                const string fileName = "Test.txt";
                string pathName = Path.GetTempPath();
                string fullName = Path.Combine(pathName, fileName);
                Directory.CreateDirectory(pathName);
                TextWriter tw = new StreamWriter(fullName);
                tw.WriteLine("This is text");
                tw.Close();
                MiniAssert.That(File.Exists(fullName));

                TextReader tr = new StreamReader(fullName);
                string? content = tr.ReadLine();
                if (string.IsNullOrEmpty(content))
                {
                    throw new MiniAssertionException("File was empty");
                }

                tr.Close();
                MiniAssert.AreEqual("This is text", content);
                return 1;
            }
        }
    }
}
