// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using System;
using System.Threading;
using System.Device.I2c;
using System.Device.Gpio;
using Iot.Device.GrovePiDevice.Models;
using Iot.Device.GrovePiDevice;
using Iot.Device.GrovePiDevice.Sensors;

namespace GrovePisample
{
    /// <summary>
    /// Test class for the GrovePi
    /// </summary>
    public class Program
    {
        private static GrovePi _grovePi;

        /// <summary>
        /// Main entry point
        /// </summary>
        /// <param name="args">unused</param>
        public static void Main(string[] args)
        {
            Console.WriteLine("Hello GrovePi!");
            PinValue relay = PinValue.Low;
            I2cConnectionSettings i2CConnectionSettings = new I2cConnectionSettings(1, GrovePi.DefaultI2cAddress);
            _grovePi = new GrovePi(I2cDevice.Create(i2CConnectionSettings));
            Console.WriteLine($"Manufacturer :{_grovePi.GrovePiInfo.Manufacturer}");
            Console.WriteLine($"Board: {_grovePi.GrovePiInfo.Board}");
            Console.WriteLine($"Firmware version: {_grovePi.GrovePiInfo.SoftwareVersion}");
            // Specific example to show how to read directly a pin without a high level class
            _grovePi.PinMode(GrovePort.AnalogPin0, PinMode.Input);
            _grovePi.PinMode(GrovePort.DigitalPin2, PinMode.Output);
            _grovePi.PinMode(GrovePort.DigitalPin3, PinMode.Output);
            _grovePi.PinMode(GrovePort.DigitalPin4, PinMode.Input);
            // 2 high level classes
            UltrasonicSensor ultrasonic = new UltrasonicSensor(_grovePi, GrovePort.DigitalPin6);
            DhtSensor dhtSensor = new DhtSensor(_grovePi, GrovePort.DigitalPin7, DhtType.Dht11);
            int poten = 0;
            while (!Console.KeyAvailable)
            {
                Console.Clear();
                poten = _grovePi.AnalogRead(GrovePort.AnalogPin0);
                Console.WriteLine($"Potentiometer: {poten}");
                relay = (relay == PinValue.Low) ? PinValue.High : PinValue.Low;
                _grovePi.DigitalWrite(GrovePort.DigitalPin2, relay);
                Console.WriteLine($"Relay: {relay}");
                _grovePi.AnalogWrite(GrovePort.DigitalPin3, (byte)(poten * 100 / 1023));
                Console.WriteLine($"Button: {_grovePi.DigitalRead(GrovePort.DigitalPin4)}");
                Console.WriteLine($"Ultrasonic: {ultrasonic}");
                dhtSensor.Read();
                Console.WriteLine($"{dhtSensor.DhtType}: {dhtSensor}");
                Thread.Sleep(2000);
            }
        }
    }
}
