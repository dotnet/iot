using System;
using System.Device.Gpio;
using System.Devices.Gpio;
using System.Threading;

namespace pwm_led
{
    class Program
    {
        static void Main(string[] args)
        {
            int pwmChannel = 0;
            int pwmChip = 0;
            int pwmPeriod = (int)((1f/800f)*100000000);
            int pwm80PercentFrequency = (int)(pwmPeriod*0.8f);
            int pwm40PercentFrequency = (int)(pwmPeriod*0.4f);

            using (GpioController controller = new GpioController())
            {
                GpioPWMPin pin = controller.OpenPWMPin(pwmChip, pwmChannel);
                pin.Period = pwmPeriod;
                for (int i = 0; i < 10; i++)
                {
                    pin.DutyCycle = pwm80PercentFrequency;
                    pin.PWMWrite();
                    Thread.Sleep(TimeSpan.FromSeconds(2));
                    pin.DutyCycle = pwm40PercentFrequency;
                    pin.PWMWrite();
                    Thread.Sleep(TimeSpan.FromSeconds(2));
                }
            }
        }
    }
}
