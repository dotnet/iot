// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System;
using System.Collections.Generic;
using System.Device;
using System.Device.Pwm;
using System.Globalization;

namespace Iot.Device.Board
{
    internal class PwmChannelManager : PwmChannel, IDeviceManager
    {
        private readonly Board _board;
        private readonly int _pin;
        private PwmChannel _pwm;

        public PwmChannelManager(Board board, int pin, int chip, int channel, int frequency, double dutyCyclePercentage, Func<int, int, int, double, PwmChannel> createOperation)
        {
            _board = board;
            _pin = pin;
            try
            {
                _board.ReservePin(pin, PinUsage.Pwm, this);
                _pwm = createOperation(chip, channel, frequency, dutyCyclePercentage);
            }
            catch (Exception)
            {
                _board.ReleasePin(pin, PinUsage.Pwm, this);
                throw;
            }
        }

        public override int Frequency
        {
            get => _pwm.Frequency;
            set => _pwm.Frequency = value;
        }

        public override double DutyCycle
        {
            get => _pwm.DutyCycle;
            set => _pwm.DutyCycle = value;
        }

        public override void Start()
        {
            _pwm.Start();
        }

        public override void Stop()
        {
            _pwm.Stop();
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                if (_pwm != null)
                {
                    _pwm.Dispose();
                    _board.ReleasePin(_pin, PinUsage.Pwm, this);
                }

                _pwm = null!;
            }

            base.Dispose(disposing);
        }

        public IReadOnlyCollection<int> GetActiveManagedPins()
        {
            return new List<int>()
            {
                _pin
            };
        }

        public override ComponentInformation QueryComponentInformation()
        {
            var self = new ComponentInformation(this, "PWM Channel manager");
            self.Properties["ManagedPins"] = _pin.ToString(CultureInfo.InvariantCulture);
            self.AddSubComponent(_pwm.QueryComponentInformation());
            return self;
        }
    }
}
