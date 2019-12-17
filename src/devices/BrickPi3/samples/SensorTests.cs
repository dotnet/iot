// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.
using System;
using System.Diagnostics;
using System.Threading;
using System.Threading.Tasks;
using Iot.Device.BrickPi3.Models;
using Iot.Device.BrickPi3.Sensors;

namespace BrickPiHardwareTest
{
    public partial class Program
    {
        private static void TestMultipleSensorsTouchCSSoud()
        {
            NXTTouchSensor touch = new NXTTouchSensor(_brick, SensorPort.Port2);
            EV3TouchSensor ev3Touch = new EV3TouchSensor(_brick, SensorPort.Port1, 20);
            NXTSoundSensor sound = new NXTSoundSensor(_brick, SensorPort.Port4);
            NXTColorSensor nxtlight = new NXTColorSensor(_brick, SensorPort.Port3);
            RGBColor rgb;
            while (!((touch.IsPressed()) && ev3Touch.IsPressed()))
            {
                Console.WriteLine($"NXT Touch, Raw: {touch.ReadRaw()}, ReadASString: {touch.ReadAsString()}, IsPressed: {touch.IsPressed()}, NumberNodes: {touch.NumberOfModes()}, SensorName: {touch.GetSensorName()}");
                Console.WriteLine($"EV3 Touch, Raw: {ev3Touch.ReadRaw()}, ReadASString: {ev3Touch.ReadAsString()}, IsPressed: {ev3Touch.IsPressed()}, NumberNodes: {ev3Touch.NumberOfModes()}, SensorName: {ev3Touch.GetSensorName()}");
                Console.WriteLine($"NXT Sound, Raw: {sound.ReadRaw()}, ReadASString: {sound.ReadAsString()}, NumberNodes: {sound.NumberOfModes()}, SensorName: {sound.GetSensorName()}");
                Console.WriteLine($"NXT Color Sensor, Raw: {nxtlight.ReadRaw()}, ReadASString: {nxtlight.ReadAsString()}, NumberNodes: {nxtlight.NumberOfModes()}, SensorName: {nxtlight.GetSensorName()}");
                rgb = nxtlight.ReadRGBColor();
                Console.WriteLine($"Color: {nxtlight.ReadColor()}, Red: {rgb.Red}, Green: {rgb.Green}, Blue: {rgb.Blue}");
                Thread.Sleep(300);
            }
        }

        private static void TestEV3Color()
        {
            Console.WriteLine("EV3 sensor color test mode");
            EV3ColorSensor nxtlight = new EV3ColorSensor(_brick, SensorPort.Port2, ColorSensorMode.Green);
            EV3TouchSensor touch = new EV3TouchSensor(_brick, SensorPort.Port1);
            RGBColor rgb;
            Thread.Sleep(5000);
            for (int i = 0; i < nxtlight.NumberOfModes(); i++)
            {
                int count = 0;
                while ((count < 100) && !touch.IsPressed())
                {
                    Console.WriteLine($"EV3 Color Sensor, Raw: {nxtlight.ReadRaw()}, ReadASString: {nxtlight.ReadAsString()}");
                    rgb = nxtlight.ReadRGBColor();
                    Console.WriteLine($"Color: {nxtlight.ReadColor()}, Red: {rgb.Red}, Green: {rgb.Green}, Blue: {rgb.Blue}");
                    Thread.Sleep(1000);
                    count++;
                }

                nxtlight.SelectNextMode();
                Thread.Sleep(5000);
            }
        }

        private static void TestIRSensor()
        {
            Console.WriteLine("Run test on EV3 IR sensor on port 4. Run test for the Remote, Proximity and Seek modes.");
            EV3InfraredSensor ultra = new EV3InfraredSensor(_brick, SensorPort.Port4, IRMode.Remote);
            int count = 0;
            while (count < 100)
            {
                Console.WriteLine($"EV3 ultra, Remote: {ultra.Value}, ReadAsString: {ultra.ReadAsString()}, NumberNodes: {ultra.Mode}, SensorName: {ultra.GetSensorName()}");
                Thread.Sleep(300);
                count++;
            }

            ultra.Mode = IRMode.Proximity;
            count = 0;
            while (count < 10)
            {
                Console.WriteLine($"EV3 ultra, Remote: {ultra.Value}, ReadAsString: {ultra.ReadAsString()}, NumberNodes: {ultra.Mode}, SensorName: {ultra.GetSensorName()}");
                Thread.Sleep(300);
                count++;
            }

            ultra.Mode = IRMode.Seek;
            count = 0;
            while (count < 10)
            {
                Console.WriteLine($"EV3 ultra, Remote: {ultra.Value}, ReadAsString: {ultra.ReadAsString()}, NumberNodes: {ultra.Mode}, SensorName: {ultra.GetSensorName()}");
                Thread.Sleep(300);
                count++;
            }
        }

        private static void TestNXTUS()
        {
            Console.WriteLine("Running NXT Ultrasonic sensor test on port 4. Uses all the modes and read 50 times.");
            NXTUltraSonicSensor ultra = new NXTUltraSonicSensor(_brick, SensorPort.Port4);
            for (int i = 0; i < ultra.NumberOfModes(); i++)
            {
                int count = 0;
                while (count < 50)
                {
                    Console.WriteLine($"NXT Ultrasound, Distance: {ultra.ReadDistance()}, ReadAsString: {ultra.ReadAsString()}, Selected mode: {ultra.SelectedMode()}");
                    Thread.Sleep(2000);
                    count++;
                }

                ultra.SelectNextMode();
            }
        }

        private static void TestTouch()
        {
            Console.WriteLine("Running 100 reads on EV3 touch sensor on port 1.");
            EV3TouchSensor touch = new EV3TouchSensor(_brick, SensorPort.Port1);
            // Alternative to test NXT touch sensor
            // NXTTouchSensor touch = new NXTTouchSensor(brick, BrickPortSensor.PORT_S2);
            int count = 0;
            while (count < 100)
            {
                Console.WriteLine($"NXT Touch, IsPRessed: {touch.IsPressed()}, ReadAsString: {touch.ReadAsString()}, Selected mode: {touch.SelectedMode()}");
                Task.Delay(300).Wait();
            }
        }

        private static void TestNXTLight()
        {
            Console.WriteLine("Run NXT Light sensor test on port 4. Uses all the modes and read 100 times.");
            NXTLightSensor nxtlight = new NXTLightSensor(_brick, SensorPort.Port4);
            int count = 0;
            while (count < 100)
            {
                Console.WriteLine($"NXT Color Sensor, Raw: {nxtlight.ReadRaw()}, ReadASString: {nxtlight.ReadAsString()}, NumberNodes: {nxtlight.NumberOfModes()}, SensorName: {nxtlight.GetSensorName()}");
                Console.WriteLine($"Color: {nxtlight.ReadRaw()}");
                Thread.Sleep(300);
                count++;
            }

            count = 0;
            nxtlight.SelectNextMode();
            while (count < 100)
            {
                Console.WriteLine($"NXT Color Sensor, Raw: {nxtlight.ReadRaw()}, ReadASString: {nxtlight.ReadAsString()}, NumberNodes: {nxtlight.NumberOfModes()}, SensorName: {nxtlight.GetSensorName()}");
                Console.WriteLine($"Color: {nxtlight.ReadRaw()}");
                Thread.Sleep(300);
                count++;
            }
        }

        private static void TestNXTCS()
        {
            Console.WriteLine("Run NXT Color sensor test on port 4. Press the EV3 touch sensor on port 1 to stop the test.");
            NXTColorSensor nxtlight = new NXTColorSensor(_brick, SensorPort.Port4);
            EV3TouchSensor touch = new EV3TouchSensor(_brick, SensorPort.Port1);
            RGBColor rgb;
            while (!touch.IsPressed())
            {
                Console.WriteLine($"NXT Color Sensor, Raw: {nxtlight.ReadRaw()}, ReadASString: {nxtlight.ReadAsString()}, NumberNodes: {nxtlight.SelectedMode()}");
                rgb = nxtlight.ReadRGBColor();
                Console.WriteLine($"Color: {nxtlight.ReadColor()}, Red: {rgb.Red}, Green: {rgb.Green}, Blue: {rgb.Blue}");
                Thread.Sleep(300);
            }
        }
    }
}
