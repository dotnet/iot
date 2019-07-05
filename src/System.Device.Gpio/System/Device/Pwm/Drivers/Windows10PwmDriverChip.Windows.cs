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

        /// <summary>
        /// Initializes a new instance of the <see cref="Windows10PwmDriverChip"/> class.
        /// </summary>
        /// <param name="pwmChip">The PWM chip.</param>
        /// <param name="useDefaultChip">Use the default chip.</param>
        public Windows10PwmDriverChip(int pwmChip, bool useDefaultChip)
        {
            // Open the Windows PWM controller for the specified PWM chip.
            string controllerFriendlyName = $"PWM{pwmChip}";
            string deviceSelector = useDefaultChip ? WinPwm.PwmController.GetDeviceSelector() : WinPwm.PwmController.GetDeviceSelector(controllerFriendlyName);

            DeviceInformationCollection deviceInformationCollection = DeviceInformation.FindAllAsync(deviceSelector).WaitForCompletion();
            if (deviceInformationCollection.Count == 0)
            {
                throw new ArgumentException($"No PWM device exists for PWM chip at index {pwmChip}.", $"{nameof(pwmChip)}");
            }

            string deviceId = deviceInformationCollection[0].Id;
            _winController = WinPwm.PwmController.FromIdAsync(deviceId).WaitForCompletion();
        }

        /// <summary>
        /// Opens a channel in order for it to be ready to use.
        /// </summary>
        /// <param name="pwmChannel">The PWM channel.</param>
        public void OpenChannel(int pwmChannel)
        {
            if (!_channelMap.TryGetValue(pwmChannel, out _))
            {
                Windows10PwmDriverChannel channel = new Windows10PwmDriverChannel(_winController, pwmChannel);
                _channelMap.Add(pwmChannel, channel);
            }
        }

        /// <summary>
        /// Closes an open channel.
        /// </summary>
        /// <param name="pwmChannel">The PWM channel.</param>
        /// <returns><see langword="true" /> if the chip has no more open channels open upon exiting; <see langword="false" /> otherwise.</returns>
        public bool CloseChannel(int pwmChannel)
        {
            // This assumes that PwmController has ensured that the channel is already open.
            Windows10PwmDriverChannel channel = _channelMap[pwmChannel];

            channel.Dispose();
            _channelMap.Remove(pwmChannel);

            return _channelMap.Count == 0;
        }

        /// <summary>
        /// Changes the duty cycle for an open channel.
        /// </summary>
        /// <param name="pwmChannel">The PWM channel.</param>
        /// <param name="dutyCyclePercentage">The duty cycle percentage to change.</param>
        public void ChangeDutyCycle(int pwmChannel, double dutyCyclePercentage)
        {
            // This assumes that PwmController has ensured that the channel is already open.
            Windows10PwmDriverChannel channel = _channelMap[pwmChannel];

            channel.ChangeDutyCycle(dutyCyclePercentage);
        }

        /// <summary>
        /// Starts writing to an open channel.
        /// </summary>
        /// <param name="pwmChannel">The PWM channel.</param>
        /// <param name="frequencyInHertz"></param>
        /// <param name="dutyCyclePercentage"></param>
        public void Start(int pwmChannel, double frequencyInHertz, double dutyCyclePercentage)
        {
            // This assumes that PwmController has ensured that the channel is already open.
            Windows10PwmDriverChannel channel = _channelMap[pwmChannel];

            _winController.SetDesiredFrequency(frequencyInHertz);
            channel.Start(dutyCyclePercentage);
        }

        /// <summary>
        /// Stops writing to an open channel.
        /// </summary>
        /// <param name="pwmChannel">The PWM channel.</param>
        public void Stop(int pwmChannel)
        {
            // This assumes that PwmController has ensured that the channel is already open.
            Windows10PwmDriverChannel channel = _channelMap[pwmChannel];
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
