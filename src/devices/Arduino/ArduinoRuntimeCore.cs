using System;
using System.Collections.Generic;
using System.Text;

#pragma warning disable CS1591
namespace Iot.Device.Arduino
{
    /// <summary>
    /// Helper methods for the arduino runtime
    /// </summary>
    public static class ArduinoRuntimeCore
    {
        public static void Sleep(IArduinoHardwareLevelAccess hardwareLevelAccess, int delayMs)
        {
            if (delayMs <= 0)
            {
                return;
            }

            int ticks = hardwareLevelAccess.GetTickCount();
            int endTicks = ticks + delayMs;
            if (ticks > endTicks)
            {
                // There will be a wraparound
                int previous = ticks;
                // wait until the tick count wraps around
                while (previous < ticks)
                {
                    previous = ticks;
                    ticks = hardwareLevelAccess.GetTickCount();
                }
            }

            while (endTicks > ticks)
            {
                // Busy waiting is ok here - the microcontroller has no sleep state
                ticks = hardwareLevelAccess.GetTickCount();
            }
        }
    }
}
