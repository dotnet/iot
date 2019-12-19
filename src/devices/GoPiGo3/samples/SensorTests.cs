// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using System;
using System.Threading;
using Iot.Device.GoPiGo3.Models;
using Iot.Device.GoPiGo3.Sensors;

namespace GoPiGo3.Samples
{
    public partial class Program
    {
        private static void TestUltrasound()
        {
            UltraSonicSensor ultraSonic = new UltraSonicSensor(_goPiGo3, GrovePort.Grove1);
            Console.WriteLine($"Test {ultraSonic.SensorName} on port {ultraSonic.Port}. Gives the distance. Press enter to stop the test.");
            AddLines();
            while (!Console.KeyAvailable)
            {
                Console.CursorLeft = 0;
                Console.Write($"Value: {ultraSonic.Value}, ValueAsString: {ultraSonic.ToString()}");
                Thread.Sleep(100);
                CleanALine();
            }
        }

        private static void TestBuzzer()
        {
            int[] notes = new int[] { 261, 293, 329, 349, 392, 440, 493, 523 };
            Buzzer buzzer = new Buzzer(_goPiGo3, GrovePort.Grove1);
            Console.WriteLine($"Play notes on {buzzer.SensorName} on port {buzzer.Port} in loops, changing duty cycle 50 (default), 80 and 10. Press a enter to stop the test.");
            buzzer.Start();
            Console.WriteLine($"Playing with duty cycle = {buzzer.Duty}");
            int i = 0;
            while (!Console.KeyAvailable)
            {
                buzzer.Value = notes[i++];
                if (i == notes.Length)
                {
                    i = 0;
                }

                Thread.Sleep(1000);
            }

            var readkey = Console.ReadLine();
            buzzer.Duty = 80;
            Console.WriteLine($"Playing with duty cycle = {buzzer.Duty}");
            while (!Console.KeyAvailable)
            {
                buzzer.Value = notes[i++];
                if (i == notes.Length)
                {
                    i = 0;
                }

                Thread.Sleep(1000);
            }

            readkey = Console.ReadLine();
            buzzer.Duty = 10;
            Console.WriteLine($"Playing with duty cycle = {buzzer.Duty}");
            while (!Console.KeyAvailable)
            {
                buzzer.Value = notes[i++];
                if (i == notes.Length)
                {
                    i = 0;
                }

                Thread.Sleep(1000);
            }

            buzzer.Stop();
        }

        private static void TestPotentiometer()
        {
            Buzzer buzzer = new Buzzer(_goPiGo3, GrovePort.Grove1);
            PotentiometerSensor potentiometerSensor = new PotentiometerSensor(_goPiGo3, GrovePort.Grove2);
            Console.WriteLine($"Control the {buzzer.SensorName} on port {buzzer.Port} with the {potentiometerSensor.SensorName} on port {buzzer.Port}. Press enter to stop the test.");
            AddLines();
            buzzer.Value = potentiometerSensor.Value;
            buzzer.Start();
            while (!Console.KeyAvailable)
            {
                buzzer.Value = potentiometerSensor.Value;
                Thread.Sleep(100);
            }

            buzzer.Stop();
        }

        private static void TestSound()
        {
            SoundSensor soundSensor = new SoundSensor(_goPiGo3, GrovePort.Grove1);
            Console.WriteLine($"Test {soundSensor.SensorName} on port {soundSensor.Port}. Press a key to finish the test");
            AddLines();
            while (!Console.KeyAvailable)
            {
                Console.CursorLeft = 0;
                Console.Write($"{soundSensor.SensorName}: {soundSensor} which is {soundSensor.ValueAsPercent} %");
                Thread.Sleep(100);
                CleanALine();
            }
        }

        private static void TestRelay()
        {
            Relay relay = new Relay(_goPiGo3, GrovePort.Grove1);
            Console.WriteLine($"Test {relay.SensorName} on port {relay.Port}, will change the state of the relay from on to off, reverse the polarity, turn on and off, reverse the polarity and turn on an off again.");
            Console.WriteLine($"{relay.SensorName}, Value: {relay.Value}, state: {relay} IsInverted: {relay.IsInverted}");
            // 2 is different than 0, so it should swith it to "on"
            Thread.Sleep(2000);
            relay.Value = 2;
            Console.WriteLine($"{relay.SensorName}, Value: {relay.Value}, state: {relay} IsInverted: {relay.IsInverted}");
            Thread.Sleep(2000);
            relay.Value = 0;
            Console.WriteLine($"{relay.SensorName}, Value: {relay.Value}, state: {relay} IsInverted: {relay.IsInverted}");
            Thread.Sleep(2000);
            relay.IsInverted = true;
            relay.Value = 0;
            Console.WriteLine($"{relay.SensorName}, Value: {relay.Value}, state: {relay} IsInverted: {relay.IsInverted}");
            Thread.Sleep(2000);
            relay.Value = 2;
            Console.WriteLine($"{relay.SensorName}, Value: {relay.Value}, state: {relay} IsInverted: {relay.IsInverted}");
            Thread.Sleep(2000);
            relay.IsInverted = false;
            relay.On();
            Console.WriteLine($"{relay.SensorName}, Value: {relay.Value}, state: {relay} IsInverted: {relay.IsInverted}");
            Thread.Sleep(2000);
            relay.Off();
            Console.WriteLine($"{relay.SensorName}, Value: {relay.Value}, state: {relay} IsInverted: {relay.IsInverted}");
            Thread.Sleep(2000);
        }

        private static void TestButton()
        {
            Button button = new Button(_goPiGo3, GrovePort.Grove2);
            Console.WriteLine($"Test {button.SensorName} on port {button.Port}. Press enter to stop the test.");
            AddLines();
            while (!Console.KeyAvailable)
            {
                Console.CursorLeft = 0;
                Console.Write($"{button.SensorName} is {button.IsPressed} so it is {button}");
                Thread.Sleep(20);
                CleanALine();
            }
        }

        private static void TestLedPwmLightSensor()
        {
            LedPwm ledPwm = new LedPwm(_goPiGo3, GrovePort.Grove2);
            LightSensor lightSensor = new LightSensor(_goPiGo3, GrovePort.Grove1);
            Console.WriteLine($"Test {lightSensor.SensorName} on port {lightSensor.Port} controlling a {ledPwm.SensorName} on port {ledPwm.Port}. The intensity of the led will change proportionnaly to the intensity read by the sensor. Press enter to stop the test");
            AddLines();
            while (!Console.KeyAvailable)
            {
                Console.CursorLeft = 0;
                Console.Write($"Intensity: {lightSensor.ValueAsPercent} %");
                ledPwm.Duty = lightSensor.ValueAsPercent;
                Thread.Sleep(50);
                CleanALine();
            }

            ledPwm.Stop();
        }
    }
}
