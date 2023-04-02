// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System;
using System.Collections.Generic;
using System.Device;
using System.Device.Pwm;
using System.Linq;
using System.Text;

namespace Iot.Device.Arduino
{
    internal class ArduinoPwmChannel : PwmChannel
    {
        private readonly ArduinoBoard _board;
        // The digital pin used
        private readonly int _pin;
        private double _dutyCycle;
        private int _frequency;
        private bool _enabled;

        /// <summary>
        /// Create a PWM Channel on the Arduino. Depending on the board, it has 4-8 pins that support PWM.
        /// This expects to take the normal pin number (i.e. D6, D9) as input
        /// </summary>
        /// <param name="board">Reference to Board</param>
        /// <param name="chip">Always needs to be 0</param>
        /// <param name="channel">See above. Valid values depend on the board.</param>
        /// <param name="frequency">This parameter is ignored and exists only for compatibility</param>
        /// <param name="dutyCyclePercentage">PWM duty cycle (0 - 1)</param>
        public ArduinoPwmChannel(ArduinoBoard board,
            int chip,
            int channel,
            int frequency = 400,
            double dutyCyclePercentage = 0.5)
        {
            Chip = chip;
            if (chip != 0)
            {
                throw new NotSupportedException("No such chip: {0}");
            }

            Channel = channel;
            _pin = channel;
            SupportedPinConfiguration? caps = board.SupportedPinConfigurations.FirstOrDefault(x => x.Pin == _pin);
            if (caps == null || !caps.PinModes.Contains(SupportedMode.Pwm))
            {
                throw new NotSupportedException($"Pin {_pin} does not support PWM");
            }

            _frequency = frequency;
            _dutyCycle = dutyCyclePercentage;
            _board = board;
            _enabled = false;
        }

        public int Chip
        {
            get;
        }

        public int Channel
        {
            get;
        }

        /// <summary>
        /// Setting the frequency is not supported on the Arduino.
        /// Therefore, this property has no effect.
        /// </summary>
        public override int Frequency
        {
            get => _frequency;
            set
            {
                if (value < 0)
                {
                    throw new ArgumentOutOfRangeException(nameof(value), value, "Value must not be negative.");
                }

                _frequency = value;
                Update();
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

                _dutyCycle = value;
                Update();
            }
        }

        /// <inheritdoc/>
        public override void Start()
        {
            _enabled = true;
            _board.Firmata.SetPinMode(_pin, SupportedMode.Pwm);
            Update();
        }

        /// <inheritdoc/>
        public override void Stop()
        {
            _enabled = false;
            Update();
            _board.Firmata.SetPinMode(_pin, SupportedMode.DigitalInput);
        }

        private void Update()
        {
            _board.Firmata.SetPwmChannel(_pin, _enabled ? _dutyCycle : 0);
        }

        protected override void Dispose(bool disposing)
        {
            Stop();
            base.Dispose(disposing);
        }

        public override ComponentInformation QueryComponentInformation()
        {
            return new ComponentInformation(this, "Arduino PWM Channel");
        }
    }
}
