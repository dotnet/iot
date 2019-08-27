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
        private const int DefaultPwmFrequency = 50;

        /// <summary>
        /// <see cref="GpioController"/> related with operations on pins
        /// </summary>
        protected GpioController Controller;

        /// <summary>
        /// Constructs generic <see cref="DCMotor"/> instance
        /// </summary>
        /// <param name="controller"><see cref="GpioController"/> related with operations on pins</param>
        protected DCMotor(GpioController controller)
        {
            Controller = controller;
        }

        /// <summary>
        /// Gets or sets the speed of the motor. Range is -1..1 or 0..1 for 1-pin connection.
        /// 1 means maximum speed, 0 means no movement and -1 means movement in opposite direction.
        /// </summary>
        public abstract double Speed { get; set; }

        /// <summary>
        /// Disposes the <see cref="DCMotor"/> class
        /// </summary>
        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        /// <summary>
        /// Releases the resources used by the <see cref="DCMotor"/> instance.
        /// </summary>
        /// <param name="disposing">true to release both managed and unmanaged resources; false to release only unmanaged resources.</param>
        protected virtual void Dispose(bool disposing)
        {
            if (disposing)
            {
                Controller?.Dispose();
                Controller = null;
            }
        }

        /// <summary>
        /// Creates <see cref="DCMotor"/> instance which allows to control speed in one direction.
        /// </summary>
        /// <param name="speedControlChannel"><see cref="PwmChannel"/> used to control the speed of the motor</param>
        /// <returns>DCMotor instance</returns>
        /// <remarks>
        /// PWM pin <paramref name="speedControlChannel"/> can be connected to either enable pin of the H-bridge.
        /// or directly to the input related with the motor (if H-bridge allows inputs to change frequently).
        /// Connecting motor directly to GPIO pin is not recommended and may damage your board.
        /// </remarks>
        public static DCMotor Create(PwmChannel speedControlChannel)
        {
            if (speedControlChannel == null)
                throw new ArgumentNullException(nameof(speedControlChannel));

            return new DCMotor2PinNoEnable(speedControlChannel, -1, null);
        }

        /// <summary>
        /// Creates <see cref="DCMotor"/> instance which allows to control speed in one direction.
        /// </summary>
        /// <param name="speedControlPin">Pin used to control the speed of the motor with software PWM (frequency will default to 50Hz)</param>
        /// <param name="controller"><see cref="GpioController"/> related to the <paramref name="speedControlPin"/></param>
        /// <returns><see cref="DCMotor"/> instance</returns>
        /// <remarks>
        /// <paramref name="speedControlPin"/> can be connected to either enable pin of the H-bridge.
        /// or directly to the input related with the motor (if H-bridge allows inputs to change frequently).
        /// Connecting motor directly to GPIO pin is not recommended and may damage your board.
        /// </remarks>
        public static DCMotor Create(int speedControlPin, GpioController controller = null)
        {
            if (speedControlPin == -1)
                throw new ArgumentOutOfRangeException(nameof(speedControlPin));

            controller = controller ?? new GpioController();
            return new DCMotor2PinNoEnable(
                new SoftwarePwmChannel(speedControlPin, DefaultPwmFrequency, 0.0, controller: controller),
                -1,
                controller);
        }

        /// <summary>
        /// Creates <see cref="DCMotor"/> instance which allows to control speed in both directions.
        /// </summary>
        /// <param name="speedControlChannel"><see cref="PwmChannel"/> used to control the speed of the motor</param>
        /// <param name="directionPin">Pin used to control the direction of the motor</param>
        /// <param name="controller"><see cref="GpioController"/> related to the <paramref name="directionPin"/></param>
        /// <returns><see cref="DCMotor"/> instance</returns>
        /// <remarks>
        /// <paramref name="speedControlChannel"/> can be connected to either enable pin of the H-bridge.
        /// or directly to the input related with the motor (if H-bridge allows inputs to change frequently).
        /// <paramref name="directionPin"/> should be connected to H-bridge input corresponding to one of the motor inputs.
        /// Connecting motor directly to GPIO pin is not recommended and may damage your board.
        /// </remarks>
        public static DCMotor Create(PwmChannel speedControlChannel, int directionPin, GpioController controller = null)
        {
            if (speedControlChannel == null)
                throw new ArgumentNullException(nameof(speedControlChannel));

            if (directionPin == -1)
                throw new ArgumentOutOfRangeException(nameof(directionPin));

            return new DCMotor2PinNoEnable(speedControlChannel, directionPin, controller);
        }
        
        /// <summary>
        /// Creates <see cref="DCMotor"/> instance which allows to control speed in both directions.
        /// </summary>
        /// <param name="speedControlPin">Pin used to control the speed of the motor with software PWM (frequency will default to 50Hz)</param>
        /// <param name="directionPin">Pin used to control the direction of the motor</param>
        /// <param name="controller">GPIO controller related to <paramref name="speedControlPin"/> and <paramref name="directionPin"/></param>
        /// <returns><see cref="DCMotor"/> instance</returns>
        /// <remarks>
        /// PWM pin <paramref name="speedControlPin"/> can be connected to either enable pin of the H-bridge.
        /// or directly to the input related with the motor (if H-bridge allows inputs to change frequently).
        /// <paramref name="directionPin"/> should be connected to H-bridge input corresponding to one of the motor inputs.
        /// Connecting motor directly to GPIO pin is not recommended and may damage your board.
        /// </remarks>
        public static DCMotor Create(int speedControlPin, int directionPin, GpioController controller = null)
        {
            if (speedControlPin == -1)
                throw new ArgumentOutOfRangeException(nameof(speedControlPin));

            if (directionPin == -1)
                throw new ArgumentOutOfRangeException(nameof(directionPin));

            controller = controller ?? new GpioController();
            return new DCMotor2PinNoEnable(
                new SoftwarePwmChannel(speedControlPin, DefaultPwmFrequency, 0.0, controller: controller),
                directionPin,
                controller);
        }

        /// <summary>
        /// Creates <see cref="DCMotor"/> instance which allows to control speed in both directions.
        /// </summary>
        /// <param name="speedControlChannel"><see cref="PwmChannel"/> used to control the speed of the motor</param>
        /// <param name="directionPin">Pin used to control the direction of the motor</param>
        /// <param name="otherDirectionPin">Pin used to control the direction of the motor</param>
        /// <param name="controller"><see cref="GpioController"/> related to <paramref name="directionPin"/> and <paramref name="otherDirectionPin"/></param>
        /// <returns><see cref="DCMotor"/> instance</returns>
        /// <remarks>
        /// When speed is non-zero the value of <paramref name="otherDirectionPin"/> will always be opposite to that of <paramref name="directionPin"/>.
        /// <paramref name="speedControlChannel"/> should be connected to enable pin of the H-bridge.
        /// <paramref name="directionPin"/> should be connected to H-bridge input corresponding to one of the motor inputs.
        /// <paramref name="otherDirectionPin"/> should be connected to H-bridge input corresponding to the remaining motor input.
        /// Connecting motor directly to GPIO pin is not recommended and may damage your board.
        /// </remarks>
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
        /// Creates <see cref="DCMotor"/> instance which allows to control speed in both directions.
        /// </summary>
        /// <param name="speedControlPin">Pin used to control the speed of the motor with software PWM (frequency will default to 50Hz)</param>
        /// <param name="directionPin">Pin used to control the direction of the motor</param>
        /// <param name="otherDirectionPin">Pin used to control the direction of the motor</param>
        /// <param name="controller"><see cref="GpioController"/> related to <paramref name="speedControlPin"/>, <paramref name="directionPin"/> and <paramref name="otherDirectionPin"/></param>
        /// <returns><see cref="DCMotor"/> instance</returns>
        /// <remarks>
        /// When speed is non-zero the value of <paramref name="otherDirectionPin"/> will always be opposite to that of <paramref name="directionPin"/>
        /// PWM pin <paramref name="speedControlPin"/> should be connected to enable pin of the H-bridge.
        /// <paramref name="directionPin"/> should be connected to H-bridge input corresponding to one of the motor inputs.
        /// <paramref name="otherDirectionPin"/> should be connected to H-bridge input corresponding to the remaining motor input.
        /// Connecting motor directly to GPIO pin is not recommended and may damage your board.
        /// </remarks>
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
                new SoftwarePwmChannel(speedControlPin, DefaultPwmFrequency, 0.0, controller: controller),
                directionPin,
                otherDirectionPin,
                controller);
        }
    }
}
