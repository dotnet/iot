using Iot.Device.DCMotor;
using System;
using System.Device.Pwm;

namespace MotorHat
{
    internal class DCMotor3Pwm : DCMotor
    {
        private PwmChannel pwmPin;
        private PwmChannel in1Pin;
        private PwmChannel in2Pin;
        private double speed = 0;

        public DCMotor3Pwm(PwmChannel pwm, PwmChannel in1, PwmChannel in2) : base(null)
        {
            this.pwmPin = pwm;
            this.pwmPin.DutyCycle = speed;

            this.in1Pin = in1;
            this.in1Pin.DutyCycle = 1;

            this.in2Pin = in2;
            this.in2Pin.DutyCycle = 1;
        }

        public override double Speed
        {
            get
            {
                // Just return the last speed received
                return this.speed;
            }
            set
            {
                // Make sure the speed is between -1 and 1
                speed = Math.Clamp(value, -1, 1);

                // The motor Direction is handled configuring in1 and in2 based on speed sign
                if (speed > 0)
                {
                    // Motor moving forward...
                    in1Pin.Start();
                    in2Pin.Stop();
                    pwmPin.Start();
                }
                else if (speed < 0)
                {
                    // Motor moving backwards...
                    in1Pin.Stop();
                    in2Pin.Start();
                    pwmPin.Start();
                }
                else
                {
                    // Motor stopped...
                    in1Pin.Stop();
                    in2Pin.Stop();
                    pwmPin.Stop();
                }

                // Here we set the DutyCycle based on the absolute speed value speed
                // (PWM pin only supports positive values)
                pwmPin.DutyCycle = Math.Abs(speed);
            }
        }

        protected override void Dispose(bool disposing)
        {
            pwmPin.Stop();
            in1Pin.Stop();
            in2Pin.Stop();
        }
    }
}
