// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Iot.Device.BrickPi3;
using Iot.Device.BrickPi3.Models;

namespace BrickPiHardwareTest
{
    /// <summary>
    /// Test program for BrickPi3
    /// </summary>
    public partial class Program
    {
        private const string MotorTest = "-motor";
        private const string VehiculeTest = "-vehicle";
        private const string MultiSensorTest = "-multi";
        private const string NoBasicTest = "-nobrick";
        private const string ColorTest = "-color";
        private const string TouchTest = "-touch";
        private const string NXTLightTest = "-nxtlight";
        private const string NXTUSTest = "-nxtus";
        private const string NXTColorTest = "-nxtcolor";
        private const string IRSensorTest = "-irsensor";

        private static Brick _brick = new Brick();

        /// <summary>
        /// Main entry point
        /// </summary>
        /// <param name="args">Module to test</param>
        public static void Main(string[] args)
        {
            Console.WriteLine("Hello BrickPi3!");

            if (args.Length == 0)
            {
                Console.WriteLine(@"You can use preset hardware tests. Usage:");
                Console.WriteLine(@"./BrickPiHardwareTest -arg1 - arg2");
                Console.WriteLine(@"where -arg1, arg2, etc are one of the following:");
                Console.WriteLine($"{NoBasicTest}: don't run the basic BrickPi tests.");
                Console.WriteLine($"{MotorTest}: run basic motor tests, motors need to be on port A and D.");
                Console.WriteLine($"{VehiculeTest}: run a vehicle test, motors need to be on port A and D.");
                Console.WriteLine($"{MultiSensorTest}: run a multi sensor test");
                Console.WriteLine(@"   EV3TouchSensor on port 1");
                Console.WriteLine(@"   NXTTouchSensor on port 2");
                Console.WriteLine(@"   NXTColorSensor on port 3");
                Console.WriteLine(@"   NXTSoundSensor on port 4");
                Console.WriteLine(@"   Press the EV3TouchSensor sensor to finish");
                Console.WriteLine($"{ColorTest}: run an EV3 color test");
                Console.WriteLine(@"   EV3TouchSensor on port 1");
                Console.WriteLine(@"   EV3ColorSensor on port 2");
                Console.WriteLine($"{TouchTest}: run touch sensor test");
                Console.WriteLine(@"   EV3TouchSensor on port 1");
                Console.WriteLine($"{NXTLightTest}: run the NXT light sensor tests");
                Console.WriteLine(@"   NXTLightSensor on port 4");
                Console.WriteLine($"{NXTUSTest}: run NXT Ultrasonic test on port 4");
                Console.WriteLine($"{NXTColorTest}: run NXT Color sensor test");
                Console.WriteLine(@"   EV3TouchSensor on port 1");
                Console.WriteLine(@"   NXTColorSensor on port 4");
                Console.WriteLine($"{IRSensorTest}: run EV3 IR sensor test on port 4");
            }

            try
            {
                // uncomment any of the test to run it
                if (!(args.Contains(NoBasicTest)))
                {
                    TestBrickDetails();
                }

                // TestSensors();
                if (args.Contains(MotorTest))
                {
                    // Tests directly using the brick low level driver
                    TestRunMotors();
                    TestMotorEncoder();
                    TestMotorDPS();
                    TestMotorPosition();
                    // Test using the high level classes
                    //
                    // TestMotorTacho();
                    // Test3Motors();
                    TestMotorEvents();
                }

                if (args.Contains(VehiculeTest))
                {
                    TestVehicle();
                }

                // Test using high level classes for sensors
                if (args.Contains(MultiSensorTest))
                {
                    TestMultipleSensorsTouchCSSoud();
                }

                if (args.Contains(ColorTest))
                {
                    TestEV3Color();
                }

                if (args.Contains(ColorTest))
                {
                    TestTouch();
                }

                if (args.Contains(IRSensorTest))
                {
                    TestIRSensor();
                }

                if (args.Contains(NXTUSTest))
                {
                    TestNXTUS();
                }

                if (args.Contains(NXTLightTest))
                {
                    TestNXTLight();
                }

                if (args.Contains(NXTColorTest))
                {
                    TestNXTCS();
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Exception: {ex.Message}");
            }
        }

        private static void TestMotorPosition()
        {
            // Test motor position
            _brick.OffsetMotorEncoder((byte)MotorPort.PortD, _brick.GetMotorEncoder((byte)MotorPort.PortD));
            _brick.OffsetMotorEncoder((byte)MotorPort.PortA, _brick.GetMotorEncoder((byte)MotorPort.PortA));
            _brick.SetMotorPositionKD((byte)MotorPort.PortA);
            _brick.SetMotorPositionKP((byte)MotorPort.PortA);
            // Float motor D
            _brick.SetMotorPower((byte)MotorPort.PortD, (byte)MotorSpeed.Float);
            // set some limits
            _brick.SetMotorLimits((byte)MotorPort.PortA, 50, 200);
            _brick.SetSensorType((byte)SensorPort.Port1, SensorType.EV3Touch);
            Console.WriteLine("Read Motor A and D positions. Press EV3 Touch sensor on port 1 to stop.");
            // run until we press the button on port2
            while (_brick.GetSensor((byte)SensorPort.Port1)[0] == 0)
            {
                var target = _brick.GetMotorEncoder((byte)MotorPort.PortD);
                _brick.SetMotorPosition((byte)MotorPort.PortA, target);
                var status = _brick.GetMotorStatus((byte)MotorPort.PortA);
                Console.WriteLine($"Motor A Target Degrees Per Second: {target}; Motor A speed: {status.Speed}; DPS: {status.Dps}; Encoder: {status.Encoder}; Flags: {status.Flags}");
                Thread.Sleep(20);
            }
        }

        private static void TestMotorDPS()
        {
            // Test Motor Degree Per Second (DPS)
            _brick.OffsetMotorEncoder((byte)MotorPort.PortD, _brick.GetMotorEncoder((byte)MotorPort.PortD));
            _brick.OffsetMotorEncoder((byte)MotorPort.PortA, _brick.GetMotorEncoder((byte)MotorPort.PortA));
            // Float motor D
            _brick.SetMotorPower((byte)MotorPort.PortD, (byte)MotorSpeed.Float);
            _brick.SetSensorType((byte)SensorPort.Port1, SensorType.EV3Touch);
            Console.WriteLine("Control Motor A speed with Motor D encoder. Turn Motor D to control speed of Motor A");
            Console.WriteLine("Press EV3 Touch sensor on port 1 to stop the test");
            // run until we press the button on port 1
            while (_brick.GetSensor((byte)SensorPort.Port1)[0] == 0)
            {
                var target = _brick.GetMotorEncoder((byte)MotorPort.PortD);
                _brick.SetMotorDps((byte)MotorPort.PortA, target);
                var status = _brick.GetMotorStatus((byte)MotorPort.PortA);
                Console.WriteLine($"Motor A Target Degrees Per Second: {target}; Motor A speed: {status.Speed}; DPS: {status.Dps}; Encoder: {status.Encoder}; Flags: {status.Flags}");
                Thread.Sleep(20);
            }
        }

        private static void TestMotorEncoder()
        {
            // Test Motor encoders
            //
            // Reset first the position
            Console.WriteLine("Read encoder of Motor D 100 times. Reset position to 0 to start");
            _brick.OffsetMotorEncoder((byte)MotorPort.PortD, _brick.GetMotorEncoder((byte)MotorPort.PortD));
            for (int i = 0; i < 100; i++)
            {
                var encodermotor = _brick.GetMotorEncoder((byte)MotorPort.PortD);
                Console.WriteLine($"Encoder: {encodermotor}");
                Thread.Sleep(200);
            }
        }

        private static void TestBrickDetails()
        {
            // Get the details about the brick
            var brickinfo = _brick.BrickPi3Info;
            Console.WriteLine($"Manufacturer: {brickinfo.Manufacturer}");
            Console.WriteLine($"Board: {brickinfo.Board}");
            Console.WriteLine($"Hardware version: {brickinfo.HardwareVersion}");
            var hdv = brickinfo.GetHardwareVersion();
            for (int i = 0; i < hdv.Length; i++)
            {
                Console.WriteLine($"Hardware version {i}: {hdv[i]}");
            }

            Console.WriteLine($"Software version: {brickinfo.SoftwareVersion}");
            var swv = brickinfo.GetSoftwareVersion();
            for (int i = 0; i < swv.Length; i++)
            {
                Console.WriteLine($"Software version {i}: {swv[i]}");
            }

            Console.WriteLine($"Id: {brickinfo.Id}");
            // Testing Led
            Console.WriteLine("Testing Led, PWM on Led");
            for (int i = 0; i < 10; i++)
            {
                _brick.SetLed((byte)(i * 10));
                Task.Delay(500).Wait();
            }

            for (int i = 0; i < 10; i++)
            {
                _brick.SetLed((byte)(100 - i * 10));
                Task.Delay(500).Wait();
            }

            _brick.SetLed(255);
            // Get the voltage details
            var voltage = _brick.BrickPi3Voltage;
            Console.WriteLine($"3.3V: {voltage.Voltage3V3}");
            Console.WriteLine($"5V: {voltage.Voltage5V}");
            Console.WriteLine($"9V: {voltage.Voltage9V}");
            Console.WriteLine($"Battery voltage: {voltage.VoltageBattery}");
        }

        private static void TestSensors()
        {
            // Setting a sencor and reading values
            Console.WriteLine($"{SensorType.EV3UltrasonicCentimeter.ToString()}");
            _brick.SetSensorType((byte)SensorPort.Port3, SensorType.EV3UltrasonicCentimeter);
            for (int i = 0; i < 100; i++)
            {
                Console.WriteLine($"Iteration {i}");
                try
                {
                    var sensordata = _brick.GetSensor((byte)SensorPort.Port3);
                    for (int j = 0; j < sensordata.Length; j++)
                    {
                        Console.WriteLine($"Sensor value {j}: {sensordata[j]}");
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Exception: {ex.Message}");
                }

                Task.Delay(200).Wait();
            }

            Console.WriteLine($"{SensorType.EV3Touch.ToString()}");
            _brick.SetSensorType((byte)SensorPort.Port4, SensorType.EV3Touch);
            for (int i = 0; i < 100; i++)
            {
                Console.WriteLine($"Iteration {i}");
                try
                {
                    var sensordata = _brick.GetSensor((byte)SensorPort.Port4);
                    for (int j = 0; j < sensordata.Length; j++)
                    {
                        Console.WriteLine($"Sensor value {j}: {sensordata[j]}");
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Exception: {ex.Message}");
                }

                Task.Delay(200).Wait();
            }

            Console.WriteLine($"{SensorType.NXTTouch.ToString()}");
            _brick.SetSensorType((byte)SensorPort.Port1, SensorType.NXTTouch);
            for (int i = 0; i < 100; i++)
            {
                Console.WriteLine($"Iteration {i}");
                try
                {
                    var sensordata = _brick.GetSensor((byte)SensorPort.Port1);
                    for (int j = 0; j < sensordata.Length; j++)
                    {
                        Console.WriteLine($"Sensor value {j}: {sensordata[j]}");
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Exception: {ex.Message}");
                }

                Task.Delay(200).Wait();
            }

            Console.WriteLine($"{SensorType.EV3ColorColor.ToString()}");
            _brick.SetSensorType((byte)SensorPort.Port2, SensorType.EV3ColorColor);
            for (int i = 0; i < 100; i++)
            {
                Console.WriteLine($"Iteration {i}");
                try
                {
                    var sensordata = _brick.GetSensor((byte)SensorPort.Port2);
                    for (int j = 0; j < sensordata.Length; j++)
                    {
                        Console.WriteLine($"Sensor value {j}: {sensordata[j]}");
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Exception: {ex.Message}");
                }

                Task.Delay(200).Wait();

            }
        }

        private static void TestRunMotors()
        {
            // Testing motors
            // Acceleration to full speed, float and decreasing speed to stop
            Console.WriteLine("Speed test on Motor D, increasing and decreasing speed from 0 to maximum");
            Console.WriteLine("Acceleration on Motor D");
            for (int i = 0; i < 10; i++)
            {
                _brick.SetMotorPower((byte)MotorPort.PortD, (byte)(i * 10));
                Task.Delay(1000).Wait();
            }

            _brick.SetMotorPower((byte)MotorPort.PortD, (byte)MotorSpeed.Float);
            Console.WriteLine("Waiting 1 second");
            Thread.Sleep(1000);
            Console.WriteLine("Deceleration on Motor D");
            for (int i = 0; i < 10; i++)
            {
                _brick.SetMotorPower((byte)MotorPort.PortD, (byte)(100 - i * 10));
                Task.Delay(1000).Wait();
            }

            _brick.SetMotorPower((byte)MotorPort.PortD, (byte)MotorSpeed.Float);
            Console.WriteLine("End of test on Motor D");
        }
    }
}
