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
        private double _dutyCycle;
        private readonly string _chipPath;
        private readonly string _channelPath;
        private StreamWriter _dutyCycleWriter;
        private StreamWriter _frequencyWriter;

        /// <summary>
        /// Initializes a new instance of the <see cref="UnixPwmChannel"/> class.
        /// </summary>
        /// <param name="chip">The PWM chip number.</param>
        /// <param name="channel">The PWM channel number.</param>
        /// <param name="frequency">The frequency in hertz.</param>
        /// <param name="dutyCycle">The duty cycle represented as a value between 0.0 and 1.0.</param>
        public UnixPwmChannel(
            int chip,
            int channel,
            int frequency = 400,
            double dutyCycle = 0.5)
        {
            _chip = chip;
            _channel = channel;
            _chipPath = $"/sys/class/pwm/pwmchip{_chip}";
            _channelPath = $"{_chipPath}/pwm{_channel}";
            Validate();
            Open();

            // avoid opening the file for operations changing relatively frequently
            _dutyCycleWriter = new StreamWriter(new FileStream($"{_channelPath}/duty_cycle", FileMode.Open, FileAccess.ReadWrite));
            _frequencyWriter = new StreamWriter(new FileStream($"{_channelPath}/period", FileMode.Open, FileAccess.ReadWrite));

            SetFrequency(frequency);
            DutyCycle = dutyCycle;
        }

        /// <inheritdoc/>
        public override int Frequency
        {
            get
            {
                return _frequency;
            }
            set
            {
                SetFrequency(value);
            }
        }

        /// <inheritdoc/>
        public override double DutyCycle
        {
            get
            {
                return _dutyCycle;
            }
            set
            {
                SetDutyCycle(value);
            }
        }

        /// <summary>
        /// Gets the frequency period in nanoseconds.
        /// </summary>
        /// <param name="frequency">The frequency in hertz.</param>
        /// <returns>The frequency period in nanoseconds.</returns>
        private static int GetPeriodInNanoseconds(int frequency)
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
            if (frequency < 0)
            {
                throw new ArgumentOutOfRangeException(nameof(frequency), frequency, "Value must not be negative.");
            }

            int periodInNanoseconds = GetPeriodInNanoseconds(frequency);
            _frequencyWriter.BaseStream.SetLength(0);
            _frequencyWriter.Write(periodInNanoseconds);
            _frequencyWriter.Flush();
            _frequency = frequency;
        }

        /// <summary>
        /// Sets the duty cycle for the channel.
        /// </summary>
        /// <param name="dutyCycle">The duty cycle to set represented as a value between 0.0 and 1.0.</param>
        private void SetDutyCycle(double dutyCycle)
        {
            if (dutyCycle < 0 || dutyCycle > 1)
            {
                throw new ArgumentOutOfRangeException(nameof(dutyCycle), dutyCycle, "Value must be between 0.0 and 1.0.");
            }

            // In Linux, the period needs to be a whole number and can't have decimal point.
            int dutyCycleInNanoseconds = (int)(GetPeriodInNanoseconds(_frequency) * dutyCycle);
            _dutyCycleWriter.BaseStream.SetLength(0);
            _dutyCycleWriter.Write(dutyCycleInNanoseconds);
            _dutyCycleWriter.Flush();
            _dutyCycle = dutyCycle;
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

        /// <inheritdoc/>
        public override void Start()
        {
            string enablePath = $"{_channelPath}/enable";
            File.WriteAllText(enablePath, "1");
        }

        /// <inheritdoc/>
        public override void Stop()
        {
            string enablePath = $"{_channelPath}/enable";
            File.WriteAllText(enablePath, "0");
        }

        protected override void Dispose(bool disposing)
        {
            _dutyCycleWriter?.Dispose();
            _dutyCycleWriter = null;
            _frequencyWriter?.Dispose();
            _frequencyWriter = null;
            Close();
            base.Dispose(disposing);
        }
    }
}
