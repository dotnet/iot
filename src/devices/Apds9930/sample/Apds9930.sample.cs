using System;
using System.Device.I2c;
using System.Device.I2c.Drivers;
using System.Threading;

namespace Iot.Device.apds9930.Samples
{
    class Program
    {
        static void Main(string[] args)
        {
            Console.WriteLine("Hello apds9930!");

            //bus id on the raspberry pi 3
            const int busId = 1;

            var i2cSettings = new I2cConnectionSettings(busId, apds9930.DefaultI2cAddress);
            var i2cDevice = new UnixI2cDevice(i2cSettings);
            var i2apds9930 = new  apds9930(i2cDevice);
            
            i2apds9930.EnableProximitySensor();
            i2apds9930.EnableLightSensor();
           
            while (true){

                Console.WriteLine($"Prox : {i2apds9930.GetProximity()} Ambient Light : {i2apds9930.GetAmbientLight()} ch0: {i2apds9930.GetCh0Light()}");
                Thread.Sleep(100);
                Console.Clear();
            }
        }
    }
}