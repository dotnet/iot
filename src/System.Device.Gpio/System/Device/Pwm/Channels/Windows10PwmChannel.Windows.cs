// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using Windows.Devices.Enumeration;
using Windows.Security.ExchangeActiveSyncProvisioning;
using WinPwm = Windows.Devices.Pwm;

namespace System.Device.Pwm.Channels
{
    /// <summary>
    /// Represents a PWM channel running on Windows 10 IoT.
    /// </summary>
    internal partial class Windows10PwmChannel : PwmChannel
    {
        private WinPwm.PwmController _winController;
        private WinPwm.PwmPin _winPin;
        private int _frequency;
        private double _dutyCycle;

        /// <summary>
        /// Initializes a new instance of the <see cref="Windows10PwmChannel"/> class.
        /// </summary>
        /// <param name="chip">The PWM chip number.</param>
        /// <param name="channel">The PWM channel number.</param>
        /// <param name="frequency">The frequency in hertz.</param>
        /// <param name="dutyCycle">The duty cycle percentage represented as a value between 0.0 and 1.0.</param>
        public Windows10PwmChannel(
            int chip,
            int channel,
            int frequency = 400,
            double dutyCycle = 0.5)
        {
            // When running on Hummingboard we require to use the default chip.
            var deviceInfo = new EasClientDeviceInformation();
            bool useDefaultChip = false;
            if (deviceInfo.SystemProductName.IndexOf("Hummingboard", StringComparison.OrdinalIgnoreCase) >= 0)
            {
                useDefaultChip = true;
            }

            // Open the Windows PWM controller for the specified PWM chip.
            string deviceSelector = useDefaultChip ? WinPwm.PwmController.GetDeviceSelector() : WinPwm.PwmController.GetDeviceSelector($"PWM{chip}");

            DeviceInformationCollection deviceInformationCollection = DeviceInformation.FindAllAsync(deviceSelector).WaitForCompletion();
            if (deviceInformationCollection.Count == 0)
            {
                throw new ArgumentException($"No PWM device exists for PWM chip at index {chip}.", nameof(chip));
            }

            string deviceId = deviceInformationCollection[0].Id;
            _winController = WinPwm.PwmController.FromIdAsync(deviceId).WaitForCompletion();

            _winPin = _winController.OpenPin(channel);
            if (_winPin == null)
            {
                throw new ArgumentOutOfRangeException($"The PWM chip is unable to open a channel at index {channel}.", nameof(channel));
            }

            Frequency = frequency;
            DutyCycle = dutyCycle;
        }

        /// <inheritdoc/>
        public override int Frequency
        {
            get => _frequency;
            set
            {
                if (value < 0)
                {
                    throw new ArgumentOutOfRangeException(nameof(value), value, "Value must not be negative.");
                }
                _winController.SetDesiredFrequency(value);
                _frequency = value;
            }
        }

        /// <inheritdoc/>
        public override double DutyCycle
        {
            get => _dutyCycle;
            set
            {
                if (value < 0.0 || value > 1.0)
                {
                    throw new ArgumentOutOfRangeException(nameof(value), value, "Value must be between 0.0 and 1.0.");
                }
                _winPin.SetActiveDutyCyclePercentage(value);
                _dutyCycle = value;
            }
        }

        /// <inheritdoc/>
        public override void Start()
        {
            _winPin.Start();
            // This extra call is required to generate PWM output - remove when the underlying issue is fixed. See issue #109
            DutyCycle = _dutyCycle;
        }

        /// <inheritdoc/>
        public override void Stop()
        {
            _winPin.Stop();
        }

        protected override void Dispose(bool disposing)
        {
            Stop();
            _winPin?.Dispose();
            _winPin = null;
            _winController = null;
            base.Dispose(disposing);
        }
    }
}
