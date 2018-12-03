// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace System.Device.Pwm.Drivers
{
    public class UnixPwmDriver : PwmDriver
    {
        private const string PwmPath = "/sys/class/pwm";
        /// <summary>
        /// Collection that holds the exported channels and the period in nanoseconds.
        /// </summary>
        private Dictionary<(int, int), int> _exportedChannels = new Dictionary<(int, int), int>();

        protected internal override void ChangeDutyCycle(int pwmChip, int pwmChannel, double dutyCyclePercentage)
        {
            if (_exportedChannels[(pwmChip, pwmChannel)] == 1)
            {
                throw new InvalidOperationException("Can not change the duty cycle if Pwm has not started.");
            }
            int dutyCycleInNanoSeconds = (int)(_exportedChannels[(pwmChip, pwmChannel)] * dutyCyclePercentage / 100.0);
            string dutyCyclePath = Path.Combine(PwmPath, $"pwmchip{pwmChip}", $"pwm{pwmChannel}", "duty_cycle");
            File.WriteAllText(dutyCyclePath, Convert.ToString(dutyCycleInNanoSeconds));
        }

        protected internal override void CloseChannel(int pwmChip, int pwmChannel)
        {
            ValidatePWMChannel(pwmChip, pwmChannel);
            string channelPath = Path.Combine(PwmPath, $"pwmchip{pwmChip}", $"pwm{pwmChannel}");
            if (Directory.Exists(channelPath))
            {
                File.WriteAllText(Path.Combine(PwmPath, $"pwmchip{pwmChip}", "unexport"), Convert.ToString(pwmChannel));
                _exportedChannels.Remove((pwmChip, pwmChannel));
            }
        }

        protected internal override void OpenChannel(int pwmChip, int pwmChannel)
        {
            ValidatePWMChannel(pwmChip, pwmChannel);
            string channelPath = Path.Combine(PwmPath, $"pwmchip{pwmChip}", $"pwm{pwmChannel}");
            if (!Directory.Exists(channelPath))
            {
                File.WriteAllText(Path.Combine(PwmPath, $"pwmchip{pwmChip}", "export"), Convert.ToString(pwmChannel));
                _exportedChannels.Add((pwmChip, pwmChannel), -1);
            }
        }

        protected internal override void StartWriting(int pwmChip, int pwmChannel, double frequencyInHertz, double dutyCyclePercentage)
        {
            // In Linux the period needs to be a whole number, and can't have decimal point.
            int periodInNanoSeconds = (int)((1.0 / frequencyInHertz) * 1_000_000_000);
            // In Linux the dutyCycle needs to be a whole number, and can't have decimal point.
            int dutyCycleInNanoSeconds = (int)(periodInNanoSeconds * dutyCyclePercentage / 100.0);

            string periodPath = Path.Combine(PwmPath, $"pwmchip{pwmChip}", $"pwm{pwmChannel}", "period");
            File.WriteAllText(periodPath, Convert.ToString(periodInNanoSeconds));
            _exportedChannels[(pwmChip, pwmChannel)] = periodInNanoSeconds;

            string dutyCyclePath = Path.Combine(PwmPath, $"pwmchip{pwmChip}", $"pwm{pwmChannel}", "duty_cycle");
            File.WriteAllText(dutyCyclePath, Convert.ToString(dutyCycleInNanoSeconds));

            string enablePath = Path.Combine(PwmPath, $"pwmchip{pwmChip}", $"pwm{pwmChannel}", "enable");
            File.WriteAllText(enablePath, "1"); // Enable PWM
        }

        protected internal override void StopWriting(int pwmChip, int pwmChannel)
        {
            string enablePath = Path.Combine(PwmPath, $"pwmchip{pwmChip}", $"pwm{pwmChannel}", "enable");
            File.WriteAllText(enablePath, "0"); // Disable PWM
        }

        private void ValidatePWMChannel(int pwmChip, int pwmChannel)
        {
            string chipPath = Path.Combine(PwmPath, $"pwmchip{pwmChip}");
            if (!Directory.Exists(chipPath))
            {
                throw new ArgumentException($"The chip number {pwmChip} is invalid or is not enabled.");
            }
            string supportedChannels = File.ReadAllText(Path.Combine(chipPath, "npwm"));
            int numSupportedChannels;
            if (int.TryParse(supportedChannels, out numSupportedChannels))
            {
                if (pwmChip < 0 || pwmChip >= numSupportedChannels)
                {
                    throw new ArgumentException($"The pwm chip {pwmChip} does not support the channel {pwmChannel}");
                }
            }
            else
            {
                throw new IOException($"Unable to parse the number of supported channels at {Path.Combine(chipPath, "npwm")}");
            }
        }

        protected override void Dispose(bool disposing)
        {
            while (_exportedChannels.Count > 0)
            {
                (int, int) channel = _exportedChannels.FirstOrDefault().Key;
                CloseChannel(channel.Item1, channel.Item2);
            }
            base.Dispose(disposing);
        }
    }
}
