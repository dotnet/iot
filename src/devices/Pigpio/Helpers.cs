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
                case PinMode.InputPullUp: return PigpiodIf.PI_INPUT;
                case PinMode.InputPullDown: return PigpiodIf.PI_INPUT;
                case PinMode.Output: return PigpiodIf.PI_OUTPUT;
                default: throw new ArgumentException($"PinMode value of '{mode}' is not valid");
            }
        }

        public static uint AsPullUpDown(this PinMode mode)
        {
            switch (mode)
            {
                case PinMode.InputPullUp: return PigpiodIf.PI_PUD_UP;
                case PinMode.InputPullDown: return PigpiodIf.PI_PUD_DOWN;
                default: return PigpiodIf.PI_PUD_OFF;
            }
        }


        public static uint AsValue(this PinValue value)
        {
            return (uint)(int)value;
        }
    }
}
