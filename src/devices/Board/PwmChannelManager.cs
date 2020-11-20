using System;
using System.Device.Pwm;

namespace Iot.Device.Board
{
    public class PwmChannelManager : PwmChannel
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
    }
}
