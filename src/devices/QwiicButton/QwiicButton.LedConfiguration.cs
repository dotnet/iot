//// Licensed to the .NET Foundation under one or more agreements.
//// The .NET Foundation licenses this file to you under the MIT license.
//// See the LICENSE file in the project root for more information.

namespace Iot.Device.QwiicButton
{
    public partial class QwiicButton
    {
        /// <summary>
        /// Turns the onboard LED on with the specified brightness.
        /// <param name="brightness">LED brightness value between 0 (off) and 255 (max). Defaults to max.</param>
        /// </summary>
        public bool LedOn(byte brightness = 255)
        {
            return LedConfig(brightness, 0, 0);
        }

        /// <summary>
        /// Turns the onboard LED off.
        /// </summary>
        public bool LedOff()
        {
            return LedConfig(0, 0, 0);
        }

        /// <summary>
        /// Configures the onboard LED with the given max brightness, granularity, cycle time, and off time.
        /// <param name="brightness">LED brightness value between 0 (off) and 255 (max). Default is 255.</param>
        /// <param name="cycleTime">Total pulse cycle time (in ms). Does not include off time.</param>
        /// <param name="offTime">Off time between pulses (in ms). Default is 500 ms.</param>
        /// <param name="granularity">Amount of steps it takes to get to the set brightness level (1 is fine for most applications).</param>
        /// </summary>
        public bool LedConfig(byte brightness, ushort cycleTime, ushort offTime, byte granularity = 1)
        {
            bool success = _i2cBus.WriteSingleRegister(Register.LED_BRIGHTNESS, brightness);
            success &= _i2cBus.WriteSingleRegister(Register.LED_PULSE_GRANULARITY, granularity);
            success &= _i2cBus.WriteDoubleRegister(Register.LED_PULSE_CYCLE_TIME, cycleTime);
            success &= _i2cBus.WriteDoubleRegister(Register.LED_PULSE_OFF_TIME, offTime);
            return success;
        }
    }
}
