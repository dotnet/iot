// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System.IO;
using System.Text;
using System.Threading;

namespace System.Device.Pwm.Channels
{
    /// <summary>
    /// Represents a PWM channel running on Unix.
    /// </summary>
    internal class UnixPwmChannel : PwmChannel
    {
        protected readonly int _chip;
        protected readonly int _channel;

        private readonly string _chipPath;
        private readonly string _channelPath;

        private int _frequency;
        private double _dutyCycle;
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
            _channelPath = $"{_chipPath}/{ChannelName}";

            Validate();
            Open();

            // avoid opening the file for operations changing relatively frequently
            var dutyCycleFile = new FileStream($"{_channelPath}/duty_cycle", FileMode.Open, FileAccess.ReadWrite);
            _dutyCycleWriter = new StreamWriter(dutyCycleFile);
            _frequencyWriter = new StreamWriter(new FileStream($"{_channelPath}/period", FileMode.Open, FileAccess.ReadWrite));

            int currentDutyCycleNs = GetCurrentDutyCycleNs(dutyCycleFile);
            SetFrequency(frequency, dutyCycle, currentDutyCycleNs);
        }

        /// <summary>The sysfs name of the PWM channel</summary>
        /// <remarks>May be overriden to allow for non-standard sysfs naming.</remarks>
        protected virtual string ChannelName => $"pwm{_channel}";

        private static int GetCurrentDutyCycleNs(FileStream dutyCycleFile)
        {
            using (var sr = new StreamReader(dutyCycleFile, Encoding.UTF8, detectEncodingFromByteOrderMarks: false, bufferSize: 10, leaveOpen: true))
            {
                int currentDutyCycleNs;
                int.TryParse(sr.ReadLine(), out currentDutyCycleNs);
                return currentDutyCycleNs;
            }
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
                SetFrequency(value, _dutyCycle);
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

        private void SetFrequency(int frequency, double newDutyCycle, int? dutyCycleOnTimeNs = null)
        {
            if (frequency < 0)
            {
                throw new ArgumentOutOfRangeException(nameof(frequency), frequency, "Value must not be negative.");
            }

            int periodInNanoseconds = GetPeriodInNanoseconds(frequency);

            int dutyCycleNs = dutyCycleOnTimeNs ?? GetDutyCycleOnTimeNs(GetPeriodInNanoseconds(_frequency), _dutyCycle);

            if (dutyCycleNs > periodInNanoseconds)
            {
                // Internally duty cycle is represented as `on time` and frequency as `period`
                // When changing to certain values of frequency current `on time` might be higher
                // than period which would cause driver to cause error ("invalid argument").
                // We cannot set duty cycle first as well because we have similar problem.
                // Also after rebooting both period and duty cycle are zeros which requires
                // period to be always set first.
                // What we do to fix this is following:
                // - at program start we read current value of duty cycle time in ns and this is the real value
                // - any time later we use cached value and assume cached value is correct
                //   - external changes to the file will cause issues with this assumption
                // - we prefer setting frequency first
                // - the only time this doesn't work is when we are required to use temporary value
                //
                // Now additionally there is chicken and the egg problem: we cannot use _dutyCycle value at startup
                // At startup we are required that this value is passed in explicitly.
                DutyCycle = 0;
            }

            _frequencyWriter.BaseStream.SetLength(0);
            _frequencyWriter.Write(periodInNanoseconds);
            _frequencyWriter.Flush();
            _frequency = frequency;

            DutyCycle = newDutyCycle;
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
            int dutyCycleInNanoseconds = GetDutyCycleOnTimeNs(GetPeriodInNanoseconds(_frequency), dutyCycle);
            _dutyCycleWriter.BaseStream.SetLength(0);
            _dutyCycleWriter.Write(dutyCycleInNanoseconds);
            _dutyCycleWriter.Flush();
            _dutyCycle = dutyCycle;
        }

        private static int GetDutyCycleOnTimeNs(int pwmPeriodNs, double dutyCycle)
        {
            return (int)(pwmPeriodNs * dutyCycle);
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

            SysFsHelpers.EnsureDirectoryExistsAndHasReadWriteAccess(_channelPath);
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
            _dutyCycleWriter = null!;
            _frequencyWriter?.Dispose();
            _frequencyWriter = null!;
            Close();
            base.Dispose(disposing);
        }
    }
}
