// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using System.Collections.Generic;
using System.Linq;

namespace System.Device.Pwm
{
    public sealed partial class PwmController : IDisposable
    {
        private PwmDriver _driver;
        private HashSet<ValueTuple<int, int>> _openChannels;

        public PwmController(PwmDriver driver)
        {
            _driver = driver;
            _openChannels = new HashSet<ValueTuple<int, int>>();
        }

        public void OpenChannel(int pwmChip, int pwmChannel)
        {
            if (_openChannels.Contains((pwmChip, pwmChannel)))
            {
                throw new InvalidOperationException("The selected pwm channel is already open.");
            }

            _driver.OpenChannel(pwmChip, pwmChannel);
            _openChannels.Add((pwmChip, pwmChannel));
        }

        public void CloseChannel(int pwmChip, int pwmChannel)
        {
            if (!_openChannels.Contains((pwmChip, pwmChannel)))
            {
                throw new InvalidOperationException("Can not close a pwm channel that is not yet opened.");
            }

            _driver.CloseChannel(pwmChip, pwmChannel);
            _openChannels.Remove((pwmChip, pwmChannel));
        }

        public void ChangeDutyCycle(int pwmChip, int pwmChannel, double dutyCycle)
        {
            if (!_openChannels.Contains((pwmChip, pwmChannel)))
            {
                throw new InvalidOperationException("Can not change dutycycle to a pwm channel that is not yet opened.");
            }
            _driver.ChangeDutyCycle(pwmChip, pwmChannel, dutyCycle);
        }

        public void StartWriting(int pwmChip, int pwmChannel, double frequency, double dutyCycle)
        {
            if (!_openChannels.Contains((pwmChip, pwmChannel)))
            {
                throw new InvalidOperationException("Can not start writing to a pwm channel that is not yet opened.");
            }
            _driver.StartWriting(pwmChip, pwmChannel, frequency, dutyCycle);
        }

        public void StopWriting(int pwmChip, int pwmChannel)
        {
            if (!_openChannels.Contains((pwmChip, pwmChannel)))
            {
                throw new InvalidOperationException("Can not stop writing to a pwm channel that is not yet opened.");
            }
            _driver.StopWriting(pwmChip, pwmChannel);
        }

        public void Dispose()
        {
            Dispose(true);
        }

        private void Dispose(bool disposing)
        {
            while (_openChannels.Count > 0)
            {
                ValueTuple<int, int> channel = _openChannels.FirstOrDefault();
                _driver.CloseChannel(channel.Item1, channel.Item2);
                _openChannels.Remove(channel);
            }
            _driver.Dispose();
        }
    }
}
