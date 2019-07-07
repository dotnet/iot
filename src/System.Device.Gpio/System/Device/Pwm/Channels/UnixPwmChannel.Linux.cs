// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using System.IO;
using System.Threading;

namespace System.Device.Pwm.Channels
{
    /// <summary>
    /// Represents a PWM channel running on Unix.
    /// </summary>
    internal class UnixPwmChannel : PwmChannel
    {
        private readonly int _chip;
        private readonly int _channel;
        private int _frequency;
        private double _dutyCyclePercentage;
        private readonly string _chipPath;
        private readonly string _channelPath;

        /// <summary>
        /// Initializes a new instance of the <see cref="UnixPwmChannel"/> class.
        /// </summary>
        /// <param name="chip">The PWM chip number.</param>
        /// <param name="channel">The PWM channel number.</param>
        /// <param name="frequency">The frequency in hertz.</param>
        /// <param name="dutyCyclePercentage">The duty cycle percentage represented as a value between 0.0 and 1.0.</param>
        public UnixPwmChannel(
            int chip,
            int channel,
            int frequency,
            double dutyCyclePercentage = 0.5)
        {
            _chipPath = $"/sys/class/pwm/pwmchip{_chip}";
            _channelPath = $"{_chipPath}/pwm{_channel}";
            _chip = chip;
            _channel = channel;
            Validate();
            Open();
            //Thread.Sleep(100);  // TODO: Need a better solution to delay for available file.
            Console.WriteLine("Set Frequency");
            SetFrequency(frequency);
            DutyCyclePercentage = dutyCyclePercentage;
        }

        /// <summary>
        /// The frequency in hertz.
        /// </summary>
        public override int Frequency =>_frequency;

        /// <summary>
        /// The duty cycle percentage represented as a value between 0.0 and 1.0.
        /// </summary>
        public override double DutyCyclePercentage
        {
            get
            {
                return _dutyCyclePercentage;
            }
            set
            {
                SetDutyCyclePercentage(value);
            }
        }

        /// <summary>
        /// Gets the frequency period in nanoseconds.
        /// </summary>
        /// <param name="frequency">The frequency in hertz.</param>
        /// <returns>The frequency period in nanoseconds.</returns>
        private static int GetPeriodInNanoSeconds(int frequency)
        {
            // In Linux, the period needs to be a whole number and can't have a decimal point.
            return (int)((1.0 / frequency) * 1_000_000_000);
        }

        /// <summary>
        /// Sets the frequency for the channel.
        /// </summary>
        /// <param name="frequency">The frequency in hertz to set.</param>
        private void SetFrequency(int frequency)
        {
            int periodInNanoSeconds = GetPeriodInNanoSeconds(frequency);
            File.WriteAllText($"{_channelPath}/period", Convert.ToString(periodInNanoSeconds));
            _frequency = frequency;
        }

        /// <summary>
        /// Sets the duty cycle percentage for the channel.
        /// </summary>
        /// <param name="dutyCyclePercentage">The duty cycle percentage to set represented as a value between 0.0 and 1.0.</param>
        private void SetDutyCyclePercentage(double dutyCyclePercentage)
        {
            if (dutyCyclePercentage < 0 || dutyCyclePercentage > 1)
            {
                throw new ArgumentOutOfRangeException(nameof(dutyCyclePercentage), dutyCyclePercentage, "Value must be between 0.0 and 1.0.");
            }

            // In Linux, the period needs to be a whole number and can't have decimal point.
            int dutyCycleInNanoSeconds = (int)(GetPeriodInNanoSeconds(_frequency) * dutyCyclePercentage);
            File.WriteAllText($"{_channelPath}/duty_cycle", Convert.ToString(dutyCycleInNanoSeconds));
            _dutyCyclePercentage = dutyCyclePercentage;
        }

        /// <summary>
        /// Verifies the specified chip and channel are available.
        /// </summary>
        private void Validate()
        {
            if (!Directory.Exists(_chipPath))
            {
                throw new ArgumentException($"The chip number {_chip} is invalid or is not enabled.");
            }

            string npwmPath = $"{_chipPath}/npwm";

            if (int.TryParse(File.ReadAllText(npwmPath), out int numberOfSupportedChannels))
            {
                if (_channel < 0 || _channel >= numberOfSupportedChannels)
                {
                    throw new ArgumentException($"The PWM chip {_chip} does not support the channel {_channel}.");
                }
            }
            else
            {
                throw new IOException($"Unable to parse the number of supported channels at {npwmPath}.");
            }
        }

        /// <summary>
        /// Stops and closes the channel.
        /// </summary>
        private void Close()
        {
            if (Directory.Exists(_channelPath))
            {
                Stop();
                File.WriteAllText($"{_chipPath}/unexport", Convert.ToString(_channel));
            }
        }

        /// <summary>
        /// Opens the channel.
        /// </summary>
        private void Open()
        {
            if (!Directory.Exists(_channelPath))
            {
                File.WriteAllText($"{_chipPath}/export", Convert.ToString(_channel));
            }
        }

        /// <summary>
        /// Starts writing to the channel.
        /// </summary>
        public override void Start()
        {
            string enablePath = $"{_channelPath}/enable";
            File.WriteAllText(enablePath, "1");
        }

        /// <summary>
        /// Stops writing to the channel.
        /// </summary>
        public override void Stop()
        {
            string enablePath = $"{_channelPath}/enable";
            File.WriteAllText(enablePath, "0");
        }

        protected override void Dispose(bool disposing)
        {
            Close();
            base.Dispose(disposing);
        }
    }
}
