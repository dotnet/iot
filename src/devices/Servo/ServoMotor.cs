using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Device.Gpio;
using System.Device.Pwm;
using System.Device.Pwm.Drivers;

namespace Iot.Device.Servo
{
    public class ServoMotor : IDisposable
    {
        private int servoPin;
        private int pwmChannel;
        private double PulseFrequency = 20.0;

        double currentPulseWidth = 0;

        private float angle;
        /// <summary>
        /// Servo motor definition.
        /// </summary>
        private ServoMotorDefinition definition;
        /// <summary>
        /// The duration per angle's degree.
        /// </summary>
        private double rangePerDegree;
        private PwmController pwmController;

        public ServoMotor(int pinNumber, int pwmChannel, ServoMotorDefinition definition)
        {                       
            this.definition = definition;
            PulseFrequency = definition.Period / 1000.0;

            UpdateRange();

            servoPin = pinNumber;
            this.pwmChannel = pwmChannel;
            // try hardware PWM first
            try{
                pwmController = new PwmController();
                pwmController.OpenChannel(pinNumber, pwmChannel);
                Console.WriteLine("Using hardware PWM");

            } catch
            {
                pwmController = new PwmController(new SoftPwm(true));
                pwmController.OpenChannel(pinNumber, pwmChannel);
                Console.WriteLine("Using software PWM");
            }

            pwmController.StartWriting(servoPin, pwmChannel, 1000 / PulseFrequency,  (1 - (PulseFrequency - currentPulseWidth)/PulseFrequency) * 100);         

        }

        /// <summary>
        /// Rotate the servomotor with a specific pulse in microseconds
        /// </summary>
        /// <param name="duration">Pulse duration in microseconds</param>
        public void SetPulse(uint duration)
        {
            currentPulseWidth = duration / 1000.0;
            pwmController.ChangeDutyCycle(servoPin, pwmChannel, (1 - (PulseFrequency - currentPulseWidth)/PulseFrequency) * 100);
        }
        /// <summary>
        /// Rotate the servomotor with a specific pulse in microseconds
        /// </summary>
        /// <param name="period">Period duration in microseconds</param>
        /// <param name="duration">Pulse duration in microseconds</param>
        /// <remarks>Changing the period will override the period definition for the motor. It is not recommended to do so.</remarks>
        public void SetPulse(uint period, uint duration)
        {
            definition.Period = period;
            PulseFrequency = period / 1000.0;
            currentPulseWidth = duration / 1000.0;
            pwmController.ChangeDutyCycle(servoPin, pwmChannel, (1 - (PulseFrequency - currentPulseWidth)/PulseFrequency) * 100);
        }
        /// <summary>
        /// Is it moving? Return true if the servomotor is currently moving.
        /// </summary>
        public bool IsMoving
        {
            get
            {
                if (currentPulseWidth != 0)
                    return true;
                else
                    return false;
            }
        }

        /// <summary>
        /// Current angle.
        /// </summary>
        /// <remarks>Motor won't move until Angle is set explicitly.</remarks>
        public float Angle
        {
            get
            {
                return angle;
            }
            set
            {
                float newAngle;
            if (value > definition.MaximumAngle)
                newAngle = definition.MaximumAngle;
            else if (value < 0)
                newAngle = 0;
            else
                newAngle = value;
            if (newAngle != Angle)
            {
                angle = newAngle;
                Rotate();
            }
            }
        }


        /// <summary>
        /// Rotate the motor to current <see cref="Angle"/>.
        /// </summary>
        private void Rotate()
        {
            double duration = (definition.MinimumDuration + rangePerDegree * Angle) / 1000.0;
            currentPulseWidth = duration;
            pwmController.ChangeDutyCycle(servoPin, pwmChannel, (1 - (PulseFrequency - currentPulseWidth)/PulseFrequency) * 100);
        }

        /// <summary>
        /// Updates the temporary variables when definition changes.
        /// </summary>
        private void UpdateRange()
        {
            if (definition.MaximumAngle != 0)
                rangePerDegree = (definition.MaximumDuration - definition.MinimumDuration) / definition.MaximumAngle;
        }

        public void Dispose()
        {
            pwmController.StopWriting(servoPin, pwmChannel);   
            pwmController.CloseChannel(servoPin, pwmChannel);
        }

        /// <summary>
        /// Minimum duration expressed microseconds that servo supports.
        /// </summary>
        public uint MinimumDuration
        {
            get { return definition.MinimumDuration; }
            set
            {
                if (definition.MinimumDuration != value)
                {
                    definition.MinimumDuration = value;
                    UpdateRange();
                }
            }
        }

        /// <summary>
        /// Maximum duration expressed microseconds that servo supports.
        /// </summary>
        public uint MaximumDuration
        {
            get { return definition.MaximumDuration; }
            set
            {
                if (definition.MaximumDuration != value)
                {
                    definition.MaximumDuration = value;
                    UpdateRange();
                }
            }
        }

        /// <summary>
        /// Period length expressed microseconds.
        /// </summary>
        public uint Period
        {
            get { return definition.Period; }
            set
            {
                definition.Period = value;
                if (!IsMoving)
                    Rotate();
            }
        }


    }
}
