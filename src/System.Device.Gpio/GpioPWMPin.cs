using System;
using System.Collections.Generic;
using System.Devices.Gpio;
using System.Text;

namespace System.Device.Gpio
{
    public enum PWMMode
    {
        Balanced,
        Serial
    }

    public class GpioPWMPin : IDisposable
    {
        internal GpioPWMPin(GpioController controller, int chip, int channel, PWMMode pwmMode, int period, int dutyCycle)
        {
            Controller = controller;
            Chip = chip;
            Channel = channel;
            PWMMode = pwmMode;
            Period = period;
            DutyCycle = dutyCycle;
        }

        public void Dispose()
        {
            Controller.ClosePWMPin(this);
        }

        public GpioController Controller { get; }

        public int Chip { get; }
        public int Channel { get; }
        public PWMMode PWMMode { get; set; }
        public int Period { get; set; }
        public int DutyCycle { get; set; }

        public void PWMWrite() => Controller.Driver.PWMWrite(Chip, Channel, PWMMode, Period, DutyCycle);
    }
}
