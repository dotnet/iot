// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System;
using System.Device;
using System.Device.Pwm;

namespace Iot.Device.Pwm
{
    internal class Pca9685PwmChannel : PwmChannel
    {
        private Pca9685 _parent;
        private int _channel;
        private bool _running = true;
        private double _dutyCycle;

        public override int Frequency
        {
            get => (int)Math.Round(_parent.PwmFrequency);
            set => throw new InvalidOperationException("Frequency can only be changed globally for all channels in the Pca9685 instance.");
        }

        private double ActualDutyCycle
        {
            get => _parent.GetDutyCycle(_channel);
            set => _parent.SetDutyCycleInternal(_channel, value);
        }

        public override double DutyCycle
        {
            get => _running ? ActualDutyCycle : _dutyCycle;
            set
            {
                _dutyCycle = value;

                if (_running)
                {
                    ActualDutyCycle = value;
                }
            }
        }

        public Pca9685PwmChannel(Pca9685 parent, int channel)
        {
            _parent = parent;
            _channel = channel;
            _dutyCycle = ActualDutyCycle;
            _running = true;
        }

        public override void Start()
        {
            _running = true;
            ActualDutyCycle = _dutyCycle;
        }

        public override void Stop()
        {
            _running = false;
            ActualDutyCycle = 0.0;
        }

        protected override void Dispose(bool disposing)
        {
            _parent?.SetChannelAsDestroyed(_channel);
            _parent = null!;
        }

        /// <inheritdoc />
        public override ComponentInformation QueryComponentInformation()
        {
            return new ComponentInformation(this, "Pca9685 PWM Channel");
        }
    }
}
