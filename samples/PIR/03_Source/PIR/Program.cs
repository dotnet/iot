using System;
using System.Devices.Gpio;
using System.Diagnostics;
using System.Threading;

namespace PIR
{
    class Program
    {
        static HCSR501 sensor;
        static GpioPin led;

        static void Main(string[] args)
        {
            // get the GPIO controller
            // 获取 GPIO 控制器
            GpioController controller = new GpioController(PinNumberingScheme.Gpio);
            // open PIN 27 for led
            // 为 led 打开引脚 27
            led = controller.OpenPin(27, PinMode.Output);

            // initialize PIR sensor
            // 初始化传感器
            sensor = new HCSR501(17);
            sensor.Initialize();

            // loop
            // 循环
            while (true)
            {
                if (sensor.Read() == true)
                {
                    // turn the led on when the sensor detected infrared heat
                    // 当传感器检测到热量时打开 led
                    led.Write(PinValue.High);
                    Console.WriteLine("Detected! Turn the LED on.");
                }
                else
                {
                    // turn the led off when the sensor undetected infrared heat
                    // 当传感器未检测到热量时关闭 led
                    led.Write(PinValue.Low);
                    Console.WriteLine("Undetected! Turn the LED off.");
                }

                // wait for a second
                // 等待 1s
                Thread.Sleep(1000);
            }
        }
    }
}
