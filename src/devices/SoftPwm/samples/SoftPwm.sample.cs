using System;
using System.Device.Pwm;
using System.Device.Pwm.Drivers;
using System.Diagnostics;
using System.Device.Gpio;
using System.Threading;
class Program
{

    static void Main(string[] args)
    {
        Console.WriteLine("Hello PWM!");

        var PwmController = new PwmController(new SoftPwm());
        PwmController.OpenChannel(17, 0);
        PwmController.StartWriting(17,0, 200, 0);
        //PWM(17, 1000, 50, true);
        //pwm.Start();
        
        while(true)
        {
            for(int i = 0; i< 100; i++)
            {
                PwmController.ChangeDutyCycle(17, 0, i);       
                Thread.Sleep(100);
            }
        }
        
    }
}