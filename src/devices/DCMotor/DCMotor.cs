// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using System;
using System.Device.Gpio;
using System.Device.Pwm;
using System.Device.Pwm.Drivers;

namespace Iot.Device.DCMotor
{
    public abstract class DCMotor : IDisposable
    {
        /// <summary>
        /// Controller related with operations on pins
        /// </summary>
        protected GpioController Controller;

        /// <summary>
        /// Constructs generic DCMotor instance
        /// </summary>
        /// <param name="controller"> Controller related with operations on pins</param>
        protected DCMotor(GpioController controller)
        {
            Controller = controller ?? new GpioController();
        }

        /// <summary>
        /// Gets or sets the speed of the motor. Range is -1..1 or 0..1 for 1-pin connection.
        /// 1 means maximum speed, 0 means no movement and -1 means movement in opposite direction.
        /// </summary>
        public abstract double Speed { get; set; }

        /// <summary>
        /// Disposes the DC motor class
        /// </summary>
        public virtual void Dispose()
        {
            Controller?.Dispose();
            Controller = null;
        }

        /// <summary>
        /// Creates DCMotor instance which allows to control speed in one direction.
        /// </summary>
        /// <param name="speedControlChannel">PWM channel used to control the speed of the motor</param>
        /// <returns>DCMotor instance</returns>
        public static DCMotor Create(PwmChannel speedControlChannel)
        {
            if (speedControlChannel == null)
                throw new ArgumentNullException(nameof(speedControlChannel));

            return new DCMotor2PinNoEnable(speedControlChannel, -1, null);
        }

        /// <summary>
        /// Creates DCMotor instance which allows to control speed in one direction.
        /// </summary>
        /// <param name="speedControlPin">Pin used to control the speed of the motor with software PWM</param>
        /// <param name="controller">GPIO controller related to the pin</param>
        /// <returns>DCMotor instance</returns>
        public static DCMotor Create(int speedControlPin, GpioController controller = null)
        {
            if (speedControlPin == -1)
                throw new ArgumentOutOfRangeException(nameof(speedControlPin));

            controller = controller ?? new GpioController();
            return new DCMotor2PinNoEnable(
                new SoftwarePwmChannel(speedControlPin, 50, 0.0, controller: controller),
                -1,
                controller);
        }

        /// <summary>
        /// Creates DCMotor instance which allows to control speed in both directions.
        /// </summary>
        /// <param name="speedControlChannel">PWM channel used to control the speed of the motor</param>
        /// <param name="directionPin">Pin used to control the direction of the motor</param>
        /// <param name="controller">GPIO controller related to the pin</param>
        /// <returns>DCMotor instance</returns>
        public static DCMotor Create(PwmChannel speedControlChannel, int directionPin, GpioController controller = null)
        {
            if (speedControlChannel == null)
                throw new ArgumentNullException(nameof(speedControlChannel));

            if (directionPin == -1)
                throw new ArgumentOutOfRangeException(nameof(directionPin));

            return new DCMotor2PinNoEnable(speedControlChannel, directionPin, controller);
        }
        
        /// <summary>
        /// Creates DCMotor instance which allows to control speed in both directions.
        /// </summary>
        /// <param name="speedControlPin">Pin used to control the speed of the motor with software PWM</param>
        /// <param name="directionPin">Pin used to control the direction of the motor</param>
        /// <param name="controller">GPIO controller related to the pins</param>
        /// <returns>DCMotor instance</returns>
        public static DCMotor Create(int speedControlPin, int directionPin, GpioController controller = null)
        {
            if (speedControlPin == -1)
                throw new ArgumentOutOfRangeException(nameof(speedControlPin));

            if (directionPin == -1)
                throw new ArgumentOutOfRangeException(nameof(directionPin));

            controller = controller ?? new GpioController();
            return new DCMotor2PinNoEnable(
                new SoftwarePwmChannel(speedControlPin, 50, 0.0, controller: controller),
                directionPin,
                controller);
        }

        /// <summary>
        /// Creates DCMotor instance which allows to control speed in both directions.
        /// </summary>
        /// <param name="speedControlChannel">PWM channel used to control the speed of the motor</param>
        /// <param name="directionPin">Pin used to control the direction of the motor</param>
        /// <param name="otherDirectionPin">Pin used to control the direction of the motor</param>
        /// <param name="controller">GPIO controller related to the pins</param>
        /// <returns>DCMotor instance</returns>
        /// <remarks>When speed is non-zero the value of otherDirectionPin will always be opposite to that of directionPin</remarks>
        public static DCMotor Create(PwmChannel speedControlChannel, int directionPin, int otherDirectionPin, GpioController controller = null)
        {
            if (speedControlChannel == null)
                throw new ArgumentNullException(nameof(speedControlChannel));

            if (directionPin == -1)
                throw new ArgumentOutOfRangeException(nameof(directionPin));

            if (otherDirectionPin == -1)
                throw new ArgumentOutOfRangeException(nameof(otherDirectionPin));

            return new DCMotor3Pin(
                speedControlChannel,
                directionPin,
                otherDirectionPin,
                controller);
        }

        /// <summary>
        /// Creates DCMotor instance which allows to control speed in both directions.
        /// </summary>
        /// <param name="speedControlPin">Pin used to control the speed of the motor with software PWM</param>
        /// <param name="directionPin">Pin used to control the direction of the motor</param>
        /// <param name="otherDirectionPin">Pin used to control the direction of the motor</param>
        /// <param name="controller">GPIO controller related to the pins</param>
        /// <returns>DCMotor instance</returns>
        /// <remarks>When speed is non-zero the value of <paramref name="otherDirectionPin"/> will always be opposite to that of <paramref name="directionPin"/></remarks>
        public static DCMotor Create(int speedControlPin, int directionPin, int otherDirectionPin, GpioController controller = null)
        {
            if (speedControlPin == -1)
                throw new ArgumentOutOfRangeException(nameof(speedControlPin));

            if (directionPin == -1)
                throw new ArgumentOutOfRangeException(nameof(directionPin));

            if (otherDirectionPin == -1)
                throw new ArgumentOutOfRangeException(nameof(otherDirectionPin));

            controller = controller ?? new GpioController();

            return new DCMotor3Pin(
                new SoftwarePwmChannel(speedControlPin, 50, 0.0, controller: controller),
                directionPin,
                otherDirectionPin,
                controller);
        }
    }
}
