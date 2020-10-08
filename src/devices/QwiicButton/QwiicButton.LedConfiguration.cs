//// Licensed to the .NET Foundation under one or more agreements.
//// The .NET Foundation licenses this file to you under the MIT license.
//// See the LICENSE file in the project root for more information.

namespace Iot.Device.QwiicButton
{
    public sealed partial class QwiicButton
    {
        /// <summary>
        /// Turns the onboard LED on with the specified brightness.
        /// <param name="brightness">LED brightness value between 0 (off) and 255 (max). Defaults to max.</param>
        /// </summary>
        public void LedOn(byte brightness = 255)
        {
            LedConfig(brightness, 0, 0);
        }

        /// <summary>
        /// Turns the onboard LED off.
        /// </summary>
        public void LedOff()
        {
            LedConfig(0, 0, 0);
        }

        /// <summary>
        /// Configures the onboard LED with the given max brightness, granularity, cycle time, and off time.
        /// <param name="brightness">LED brightness value between 0 (off) and 255 (max). Default is 255.</param>
        /// <param name="cycleTime">Total pulse cycle time (in ms). Does not include off time.</param>
        /// <param name="offTime">Off time between pulses (in ms). Default is 500 ms.</param>
        /// <param name="granularity">Amount of steps it takes to get to the set brightness level (1 is fine for most applications).</param>
        /// </summary>
        public void LedConfig(byte brightness, ushort cycleTime, ushort offTime, byte granularity = 1)
        {
            _registerAccess.WriteSingleRegister(Register.LedBrightness, brightness);
            _registerAccess.WriteSingleRegister(Register.LedPulseGranularity, granularity);
            _registerAccess.WriteDoubleRegister(Register.LedPulseCycleTime, cycleTime);
            _registerAccess.WriteDoubleRegister(Register.LedPulseOffTime, offTime);
        }
    }
}
