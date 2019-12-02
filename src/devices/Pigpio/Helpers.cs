using System;
using System.Device.Gpio;

namespace Iot.Device.Pigpio
{
    internal static class Helpers
    {
        public static uint AsMode(this PinMode mode)
        {
            switch (mode)
            {
                case PinMode.Input: return PigpiodIf.PI_INPUT;
                case PinMode.Output: return PigpiodIf.PI_OUTPUT;
                default: throw new ArgumentException($"PinMode value of '{mode}' is not valid");
            }
        }


        public static uint AsValue(this PinValue value)
        {
            return (uint)(int)value;
        }
    }
}
