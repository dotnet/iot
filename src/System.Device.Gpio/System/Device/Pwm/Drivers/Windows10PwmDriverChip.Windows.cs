// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using System.Collections.Generic;
using WinPwm = Windows.Devices.Pwm;

namespace System.Device.Pwm.Drivers
{
    public class Windows10PwmDriverChip : IDisposable
    {
        private int _referenceCount = 1;
        private WinPwm.PwmController _winController;
        private readonly Dictionary<int, Windows10PwmDriverChannel> _channelMap = new Dictionary<int, Windows10PwmDriverChannel>();

        public Windows10PwmDriverChip(WinPwm.PwmController winController)
        {
            _winController = winController;
        }

        public void OpenChannel(int channelIndex)
        {
            if (!_channelMap.TryGetValue(channelIndex, out Windows10PwmDriverChannel channel))
            {
                WinPwm.PwmPin winPin = _winController.OpenPin(channelIndex);
                if (winPin == null)
                {
                    throw new ArgumentOutOfRangeException($"The PWM chip is unable to open a channel at index {channelIndex}.", nameof(channelIndex));
                }

                channel = new Windows10PwmDriverChannel(winPin);
                _channelMap.Add(channelIndex, channel);
            }
            else
            {
                channel.OpenChannel();
            }
        }

        /// <summary>
        /// Closes the channel.
        /// </summary>
        /// <param name="channelIndex">The PWM channel to close.</param>
        /// <returns><see langword="true" /> if the chip has no more open channels open upon exiting; <see langword="false" /> otherwise.</returns>
        /// <exception cref="NotImplementedException"></exception>
        /// TODO Edit XML Comment Template for CloseChannel
        public bool CloseChannel(int channelIndex)
        {
            // This assumes that PwmController has ensured that the channel is already open
            Windows10PwmDriverChannel channel = _channelMap[channelIndex];

            if (channel.CloseChannel())
            {
                if (--_referenceCount == 0)
                {
                    _channelMap.Remove(channelIndex);
                    if (_channelMap.Count == 0)
                    {
                        this.Dispose();
                        return true;
                    }
                }
            }

            return false;
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
            _referenceCount = 0;
            _winController = null;

            foreach (Windows10PwmDriverChannel channel in _channelMap.Values)
            {
                channel.CloseChannel();
            }
            _channelMap.Clear();
        }
    }
}
