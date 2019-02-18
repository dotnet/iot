using System;
using System.Threading;
using Iot.Device.GrovePiDevice.Models;
using Iot.Device.GrovePiDevice;
using Iot.Device.GrovePiDevice.Sensors;
using System.Device.I2c;
using System.Device.I2c.Drivers;

namespace GrovePisample
{
    class Program
    {
        static private GrovePi grovePi;
        static void Main(string[] args)
        {
            Console.WriteLine("Hello GrovePi!");
            PinLevel relay = PinLevel.Low;
            I2cConnectionSettings i2CConnectionSettings = new I2cConnectionSettings(1, GrovePi.GrovePiSefaultI2cAddress);
            grovePi = new GrovePi(new UnixI2cDevice(i2CConnectionSettings));
            Console.WriteLine($"Manufacturer :{grovePi.GrovePiInfo.Manufacturer}");
            Console.WriteLine($"Board: {grovePi.GrovePiInfo.Board}");
            Console.WriteLine($"Firmware version: {grovePi.GrovePiInfo.SoftwareVersion}");
            grovePi.PinMode(GrovePort.AnalogPin0, PinMode.Input);
            grovePi.PinMode(GrovePort.DigitalPin2, PinMode.Output);
            grovePi.PinMode(GrovePort.DigitalPin3, PinMode.Output);
            grovePi.PinMode(GrovePort.DigitalPin4, PinMode.Input);
            UltrasonicSensor ultrasonic = new UltrasonicSensor(grovePi, GrovePort.DigitalPin6);
            DhtSensor dhtSensor = new DhtSensor(grovePi, GrovePort.DigitalPin7, DhtType.Dht11);
            int poten = 0;
            while (!Console.KeyAvailable)
            {
                poten = grovePi.AnalogRead(GrovePort.AnalogPin0);
                Console.WriteLine($"Potentiometer: {poten}");
                relay = (relay == PinLevel.Low) ? PinLevel.High : PinLevel.Low;
                grovePi.DigitalWrite(GrovePort.DigitalPin2, relay);
                Console.WriteLine($"Relay: {relay}");
                grovePi.AnalogWrite(GrovePort.DigitalPin3, (byte)(poten * 100 / 1023));
                Console.WriteLine($"Button: {grovePi.DigitalRead(GrovePort.DigitalPin4)}");
                Console.WriteLine($"Ultrasonic: {ultrasonic}");
                dhtSensor.ReadSensor();
                Console.WriteLine($"{dhtSensor.DhtType}: {dhtSensor}");
                Thread.Sleep(2000);
                Console.CursorTop -= 5;                
            }

            Console.CursorTop += 5;
        }
    }
}
