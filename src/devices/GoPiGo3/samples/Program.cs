// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System;
using System.Threading;
using System.Drawing;
using System.Device.Spi;
using Iot.Device.GoPiGo3.Models;
using Iot.Device.GoPiGo3;

namespace GoPiGo3.Samples
{
    /// <summary>
    /// Test program
    /// </summary>
    public partial class Program
    {
        private static GoPiGo _goPiGo3;

        /// <summary>
        /// Test program entry point
        /// </summary>
        /// <param name="args">Unused</param>
        public static void Main(string[] args)
        {
            Console.WriteLine("Hello GoPiGo3!");
            // Default on the Raspberry is Bus ID = 0 and Chip Set Select Line = 1 for GoPiGo3
            var settings = new SpiConnectionSettings(0, 1)
            {
                // 500K is the SPI communication with GoPiGo
                ClockFrequency = 500000,
                // see http://tightdev.net/SpiDev_Doc.pdf
                Mode = SpiMode.Mode0,
                DataBitLength = 8
            };
            _goPiGo3 = new GoPiGo(SpiDevice.Create(settings));
            Console.WriteLine("Choose a test by entering the number and press enter:");
            Console.WriteLine("  1. Basic GoPiGo3 info and embedded led test");
            Console.WriteLine("  2. Control left motor from motor right position");
            Console.WriteLine("  3. Read encoder of right motor");
            Console.WriteLine("  4. Test both servo motors");
            Console.WriteLine("  5. Test Ultrasonic sensor on Grove1");
            Console.WriteLine("  6. Test buzzer on Grove1");
            Console.WriteLine("  7. Change buzzer tone on Grove1 with a potentiometer on Grove2");
            Console.WriteLine("  8. Test sound sensor on Grove1");
            Console.WriteLine("  9. Test a relay on Grove1");
            Console.WriteLine(" 10. Test a button on Grove1");
            Console.WriteLine(" 11. Control a led light on Grove2 from a light sensor on Grove1");
            Console.WriteLine(" 12. Test MotorLeft speed based on encoder");
            Console.WriteLine(" 13. Test driving the vehicle");
            var readCar = Console.ReadLine();
            switch (readCar)
            {
                case "1":
                    TestGoPiGoDetails();
                    break;
                case "2":
                    TestMotorPosition();
                    break;
                case "3":
                    TestMotorEncoder();
                    break;
                case "4":
                    TestServo();
                    break;
                case "5":
                    TestUltrasound();
                    break;
                case "6":
                    TestBuzzer();
                    break;
                case "7":
                    TestPotentiometer();
                    break;
                case "8":
                    TestSound();
                    break;
                case "9":
                    TestRelay();
                    break;
                case "10":
                    TestButton();
                    break;
                case "11":
                    TestLedPwmLightSensor();
                    break;
                case "12":
                    TestMotorTacho();
                    break;
                case "13":
                    Testvehicle();
                    break;
                default:
                    break;
            }
        }

        private static void TestServo()
        {
            Console.WriteLine("Move both servo from position 800 µs to 1600µs. Press enter to stop the test.");
            while (!Console.KeyAvailable)
            {
                _goPiGo3.SetServo(ServoPort.Servo1, 800);
                _goPiGo3.SetServo(ServoPort.Servo2, 800);
                Thread.Sleep(700);
                _goPiGo3.SetServo(ServoPort.Both, 1600);
                Thread.Sleep(700);
            }
        }

        private static void TestGoPiGoDetails()
        {
            var goPiGoInfo = _goPiGo3.GoPiGo3Info;
            Console.WriteLine($"Manufacturer: {goPiGoInfo.Manufacturer}");
            Console.WriteLine($"Board: {goPiGoInfo.Board}");
            Console.WriteLine($"Hardware version: {goPiGoInfo.HardwareVersion}");
            Console.WriteLine($"Id: {goPiGoInfo.Id}");
            // Eyes led
            Console.WriteLine("Testing Led, changing colors for the leds on both eyes");
            for (int red = 0; red < 255; red += 10)
            {
                for (int green = 0; green < 255; green += 10)
                {
                    for (int blue = 0; blue < 255; blue += 10)
                    {
                        _goPiGo3.SetLed((byte)GoPiGo3Led.LedEyeLeft + (byte)GoPiGo3Led.LedEyeRight, Color.FromArgb(red, green, blue));
                    }
                }
            }

            // Led wifi
            Console.WriteLine("Changing wifi led to red");
            _goPiGo3.SetLed((byte)GoPiGo3Led.LedWifi, Color.Red);
            Thread.Sleep(2000);
            Console.WriteLine("Changing wifi led to blue");
            _goPiGo3.SetLed((byte)GoPiGo3Led.LedWifi, Color.Green);
            Thread.Sleep(2000);
            Console.WriteLine("Changing wifi led to green");
            _goPiGo3.SetLed((byte)GoPiGo3Led.LedWifi, Color.Blue);
            Thread.Sleep(2000);
            // Get the voltage details
            var voltage = _goPiGo3.GoPiGoVoltage;
            Console.WriteLine($"5V: {voltage.Voltage5V}");
            Console.WriteLine($"Battery voltage: {voltage.VoltageBattery}");
            _goPiGo3.SetLed((byte)GoPiGo3Led.LedEyeLeft + (byte)GoPiGo3Led.LedEyeRight + (byte)GoPiGo3Led.LedWifi, Color.Black);
        }

        private static void TestMotorPosition()
        {
            _goPiGo3.OffsetMotorEncoder(MotorPort.MotorRight, _goPiGo3.GetMotorEncoder(MotorPort.MotorRight));
            _goPiGo3.OffsetMotorEncoder(MotorPort.MotorLeft, _goPiGo3.GetMotorEncoder(MotorPort.MotorLeft));
            _goPiGo3.SetMotorPositionKD(MotorPort.MotorLeft);
            _goPiGo3.SetMotorPositionKP(MotorPort.MotorLeft);
            // Float motor Right
            _goPiGo3.SetMotorPower(MotorPort.MotorRight, (byte)MotorSpeed.Float);
            // set some limits
            _goPiGo3.SetMotorLimits(MotorPort.MotorLeft, 50, 200);
            Console.WriteLine("Read Motor Left and Right positions. Press enter stop the test.");
            AddLines();
            // run until we press enter
            while (!Console.KeyAvailable)
            {
                var target = _goPiGo3.GetMotorEncoder(MotorPort.MotorRight);
                _goPiGo3.SetMotorPosition(MotorPort.MotorLeft, target);
                var status = _goPiGo3.GetMotorStatus(MotorPort.MotorLeft);
                Console.WriteLine($"MotorLeft Target DPS: {target} Speed: {status.Speed} DPS: {status.Dps} Encoder: {status.Encoder} Flags: {status.Flags}");
                status = _goPiGo3.GetMotorStatus(MotorPort.MotorRight);
                Console.Write($"MotorRight Target DPS: {target} Speed: {status.Speed} DPS: {status.Dps} Encoder: {status.Encoder} Flags: {status.Flags}");
                Thread.Sleep(20);
                CleanALine();
                Console.CursorTop -= 1;
                CleanALine();
            }
        }

        private static void CleanALine()
        {
            Console.CursorLeft = 0;
            // Create a space string of size of the Window
            Console.Write(" ".PadLeft(Console.LargestWindowWidth));
            Console.CursorLeft = 0;
        }

        private static void AddLines()
        {
            Console.WriteLine();
            Console.WriteLine();
            Console.CursorTop -= 2;
        }

        private static void TestMotorEncoder()
        {
            // Reset first the position
            Console.WriteLine("Read encoder of Motor Right. Reset position to 0 to start. Press enter stop the test.");
            _goPiGo3.OffsetMotorEncoder(MotorPort.MotorRight, _goPiGo3.GetMotorEncoder(MotorPort.MotorRight));
            while (!Console.KeyAvailable)
            {
                var encodermotor = _goPiGo3.GetMotorEncoder(MotorPort.MotorRight);
                Console.Write($"Encoder: {encodermotor}");
                Console.CursorLeft = 0;
                Thread.Sleep(200);
            }
        }
    }
}
