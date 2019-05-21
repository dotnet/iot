// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using System.Collections.Generic;
using Windows.Security.ExchangeActiveSyncProvisioning;

namespace System.Device.Pwm.Drivers
{
    /// <summary>
    /// A PWM driver for Windows 10 IoT.
    /// </summary>
    public class Windows10PwmDriver : PwmDriver
    {
        private readonly Dictionary<int, Windows10PwmDriverChip> _chipMap = new Dictionary<int, Windows10PwmDriverChip>();
        private readonly bool _useDefaultChip;

        public Windows10PwmDriver()
        {
            // On Hummingboard, fallback to check for generic PWM controller (friendly name is not currently used).
            var deviceInfo = new EasClientDeviceInformation();
            if (deviceInfo.SystemProductName.IndexOf("Hummingboard", StringComparison.OrdinalIgnoreCase) >= 0)
            {
                _useDefaultChip = true;
            }
        }

        /// <summary>
        /// Opens a channel in order for it to be ready to use.
        /// </summary>
        /// <param name="pwmChip">The PWM chip.</param>
        /// <param name="pwmChannel">The PWM channel.</param>
        protected internal override void OpenChannel(int pwmChip, int pwmChannel)
        {
            if (!_chipMap.TryGetValue(pwmChip, out Windows10PwmDriverChip chip))
            {
                chip = new Windows10PwmDriverChip(pwmChip, _useDefaultChip);
                _chipMap[pwmChip] = chip;
            }

            chip.OpenChannel(pwmChannel);
        }

        /// <summary>
        /// Closes an open channel.
        /// </summary>
        /// <param name="pwmChip">The PWM chip.</param>
        /// <param name="pwmChannel">The PWM channel.</param>
        protected internal override void CloseChannel(int pwmChip, int pwmChannel)
        {
            // This assumes that PwmController has ensured that the chip is already open.
            Windows10PwmDriverChip chip = _chipMap[pwmChip];

            if (chip.CloseChannel(pwmChannel))
            {
                _chipMap.Remove(pwmChannel);
            }
        }

        /// <summary>
        /// Changes the duty cycle for an open channel.
        /// </summary>
        /// <param name="pwmChip">The PWM chip.</param>
        /// <param name="pwmChannel">The PWM channel.</param>
        /// <param name="dutyCyclePercentage">The duty cycle percentage to change.</param>
        protected internal override void ChangeDutyCycle(int pwmChip, int pwmChannel, double dutyCyclePercentage)
        {
            LookupOpenChip(pwmChip).ChangeDutyCycle(pwmChannel, dutyCyclePercentage);
        }

        /// <summary>
        /// Starts writing to an open channel.
        /// </summary>
        /// <param name="pwmChip">The PWM chip.</param>
        /// <param name="pwmChannel">The PWM channel.</param>
        /// <param name="frequencyInHertz">The frequency in hertz.</param>
        /// <param name="dutyCyclePercentage"></param>
        protected internal override void StartWriting(int pwmChip, int pwmChannel, double frequencyInHertz, double dutyCyclePercentage)
        {
            LookupOpenChip(pwmChip).Start(pwmChannel, frequencyInHertz, dutyCyclePercentage);
        }

        /// <summary>
        /// Stops writing to an open channel.
        /// </summary>
        /// <param name="pwmChip">The PWM chip.</param>
        /// <param name="pwmChannel">The PWM channel.</param>
        protected internal override void StopWriting(int pwmChip, int pwmChannel)
        {
            LookupOpenChip(pwmChip).Stop(pwmChannel);
        }

        protected override void Dispose(bool disposing)
        {
            foreach (Windows10PwmDriverChip chip in _chipMap.Values)
            {
                chip.Dispose();
            }
            _chipMap.Clear();

            base.Dispose(disposing);
        }

        private Windows10PwmDriverChip LookupOpenChip(int pwmChip)
        {
            // This assumes that PwmController has ensured that the chip is already open.
            return _chipMap[pwmChip];
        }
    }
}
