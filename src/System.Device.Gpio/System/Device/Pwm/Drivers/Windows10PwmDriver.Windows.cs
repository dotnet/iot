// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using System.Collections.Generic;
using Windows.Security.ExchangeActiveSyncProvisioning;

namespace System.Device.Pwm.Drivers
{
    internal class Windows10PwmDriver : PwmDriver
    {
        private readonly Dictionary<int, Windows10PwmDriverChip> _chipMap = new Dictionary<int, Windows10PwmDriverChip>();
        private bool useDefaultChip;

        public Windows10PwmDriver()
        {
            // On Hummingboard, fallback to check for generic PWM controller (friendlyname is not currently used)
            var deviceInfo = new EasClientDeviceInformation();
            if (deviceInfo.SystemProductName.IndexOf("Hummingboard", StringComparison.OrdinalIgnoreCase) >= 0)
            {
                useDefaultChip = true;
            }
        }

        protected internal override void OpenChannel(int chipIndex, int channelIndex)
        {
            if (!_chipMap.TryGetValue(chipIndex, out Windows10PwmDriverChip chip))
            {
                chip = new Windows10PwmDriverChip(chipIndex, useDefaultChip);
                _chipMap[chipIndex] = chip;
            }

            chip.OpenChannel(channelIndex);
        }

        protected internal override void CloseChannel(int chipIndex, int channelIndex)
        {
            // This assumes that PwmController has ensured that the chip is already open
            Windows10PwmDriverChip chip = _chipMap[chipIndex];

            if (chip.CloseChannel(channelIndex))
            {
                _chipMap.Remove(channelIndex);
            }
        }

        protected internal override void ChangeDutyCycle(int chipIndex, int channelIndex, double dutyCyclePercentage)
        {
            this.LookupOpenChip(chipIndex).ChangeDutyCycle(channelIndex, dutyCyclePercentage);
        }

        protected internal override void StartWriting(int chipIndex, int channelIndex, double frequencyInHertz, double dutyCyclePercentage)
        {
            this.LookupOpenChip(chipIndex).Start(channelIndex, frequencyInHertz, dutyCyclePercentage);
        }

        protected internal override void StopWriting(int chipIndex, int channelIndex)
        {
            this.LookupOpenChip(chipIndex).Stop(channelIndex);
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

        private Windows10PwmDriverChip LookupOpenChip(int chipIndex)
        {
            // This assumes that PwmController has ensured that the chip is already open
            return _chipMap[chipIndex];
        }

    }
}
