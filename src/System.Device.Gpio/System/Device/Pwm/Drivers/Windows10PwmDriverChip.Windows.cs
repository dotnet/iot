// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using System.Collections.Generic;
using Windows.Devices.Enumeration;
using WinPwm = Windows.Devices.Pwm;

namespace System.Device.Pwm.Drivers
{
    public class Windows10PwmDriverChip : IDisposable
    {
        private WinPwm.PwmController _winController;
        private readonly Dictionary<int, Windows10PwmDriverChannel> _channelMap = new Dictionary<int, Windows10PwmDriverChannel>();

        public Windows10PwmDriverChip(int chipIndex, bool useDefaultChip)
        {
            // Open the Windows PWM controller for the specified PWM chip
            string controllerFriendlyName = $"PWM{chipIndex}";
            string deviceSelector = useDefaultChip ? WinPwm.PwmController.GetDeviceSelector() : WinPwm.PwmController.GetDeviceSelector(controllerFriendlyName);

            DeviceInformationCollection deviceInformationCollection = DeviceInformation.FindAllAsync(deviceSelector).WaitForCompletion();
            if (deviceInformationCollection.Count == 0)
            {
                throw new ArgumentException($"No PWM device exists for PWM chip at index {chipIndex}", $"{nameof(chipIndex)}");
            }

            string deviceId = deviceInformationCollection[0].Id;
            _winController = WinPwm.PwmController.FromIdAsync(deviceId).WaitForCompletion();
        }

        public void OpenChannel(int channelIndex)
        {
            if (!_channelMap.TryGetValue(channelIndex, out Windows10PwmDriverChannel channel))
            {
                channel = new Windows10PwmDriverChannel(_winController, channelIndex);
                _channelMap.Add(channelIndex, channel);
            }
        }

        /// <summary>
        /// Closes the channel.
        /// </summary>
        /// <param name="channelIndex">The PWM channel to close.</param>
        /// <returns><see langword="true" /> if the chip has no more open channels open upon exiting; <see langword="false" /> otherwise.</returns>
        public bool CloseChannel(int channelIndex)
        {
            // This assumes that PwmController has ensured that the channel is already open
            Windows10PwmDriverChannel channel = _channelMap[channelIndex];

            channel.Dispose();
            _channelMap.Remove(channelIndex);

            return _channelMap.Count == 0;
        }

        public void ChangeDutyCycle(int channelIndex, double dutyCyclePercentage)
        {
            // This assumes that PwmController has ensured that the channel is already open
            Windows10PwmDriverChannel channel = _channelMap[channelIndex];

            channel.ChangeDutyCycle(dutyCyclePercentage);
        }

        public void Start(int channelIndex, double frequencyInHertz, double dutyCyclePercentage)
        {
            // This assumes that PwmController has ensured that the channel is already open
            Windows10PwmDriverChannel channel = _channelMap[channelIndex];

            _winController.SetDesiredFrequency(frequencyInHertz);
            channel.Start(dutyCyclePercentage);
        }

        public void Stop(int channelIndex)
        {
            // This assumes that PwmController has ensured that the channel is already open
            Windows10PwmDriverChannel channel = _channelMap[channelIndex];
            channel.Stop();
        }

        public void Dispose()
        {
            _winController = null;

            foreach (Windows10PwmDriverChannel channel in _channelMap.Values)
            {
                channel.Dispose();
            }
            _channelMap.Clear();
        }
    }
}
