// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System;
using System.Device.Gpio;
using System.Device.Pwm;
using System.Device.Pwm.Drivers;

namespace Iot.Device.DCMotor
{
    /// <summary>
    /// Direct current (DC) motor
    /// </summary>
    public abstract class DCMotor : IDisposable
    {
        private const int DefaultPwmFrequency = 50;
        private bool _shouldDispose;

        /// <summary>
        /// Constructs generic <see cref="DCMotor"/> instance
        /// </summary>
        /// <param name="controller"><see cref="GpioController"/> related with operations on pins</param>
        /// <param name="shouldDispose">True to dispose the Gpio Controller</param>
        protected DCMotor(GpioController? controller, bool shouldDispose)
        {
            _shouldDispose = shouldDispose;
            Controller = controller ?? new GpioController();
        }

        /// <summary>
        /// Gets or sets the speed of the motor. Range is -1..1 or 0..1 for 1-pin connection.
        /// 1 means maximum speed, 0 means no movement and -1 means movement in opposite direction.
        /// </summary>
        public abstract double Speed { get; set; }

        /// <summary>
        /// <see cref="GpioController"/> related with operations on pins
        /// </summary>
        protected GpioController Controller
        {
            get;
            set;
        }

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
                if (_shouldDispose)
                {
                    Controller?.Dispose();
                    Controller = null!;
                }
            }
        }

        /// <summary>
        /// Creates <see cref="DCMotor"/> instance using only one pin which allows to control speed in one direction.
        /// </summary>
        /// <param name="speedControlChannel"><see cref="PwmChannel"/> used to control the speed of the motor</param>
        /// <returns>DCMotor instance</returns>
        /// <remarks>
        /// PWM pin <paramref name="speedControlChannel"/> can be connected to either enable pin of the H-bridge.
        /// or directly to the one of two inputs related with the motor direction (if H-bridge allows inputs to change frequently).
        /// Connecting motor directly to GPIO pin is not recommended and may damage your board.
        /// </remarks>
        public static DCMotor Create(PwmChannel speedControlChannel)
        {
            if (speedControlChannel is null)
            {
                throw new ArgumentNullException(nameof(speedControlChannel));
            }

            return new DCMotor2PinNoEnable(speedControlChannel, -1, null, true);
        }

        /// <summary>
        /// Creates <see cref="DCMotor"/> instance using only one pin which allows to control speed in one direction.
        /// </summary>
        /// <param name="speedControlPin">Pin used to control the speed of the motor with software PWM (frequency will default to 50Hz)</param>
        /// <param name="controller"><see cref="GpioController"/> related to the <paramref name="speedControlPin"/></param>
        /// <param name="shouldDispose">True to dispose the Gpio Controller</param>
        /// <returns><see cref="DCMotor"/> instance</returns>
        /// <remarks>
        /// <paramref name="speedControlPin"/> can be connected to either enable pin of the H-bridge.
        /// or directly to the on of two inputs related with the motor direction (if H-bridge allows inputs to change frequently).
        /// Connecting motor directly to GPIO pin is not recommended and may damage your board.
        /// </remarks>
        public static DCMotor Create(int speedControlPin, GpioController? controller = null, bool shouldDispose = true)
        {
            if (speedControlPin == -1)
            {
                throw new ArgumentOutOfRangeException(nameof(speedControlPin));
            }

            controller = controller ?? new GpioController();
            return new DCMotor2PinNoEnable(
                new SoftwarePwmChannel(speedControlPin, DefaultPwmFrequency, 0.0, controller: controller),
                -1,
                controller,
                shouldDispose);
        }

        /// <summary>
        /// Creates <see cref="DCMotor"/> instance using two pins which allows to control speed in both directions.
        /// </summary>
        /// <param name="speedControlChannel"><see cref="PwmChannel"/> used to control the speed of the motor</param>
        /// <param name="directionPin">Pin used to control the direction of the motor</param>
        /// <param name="controller"><see cref="GpioController"/> related to the <paramref name="directionPin"/></param>
        /// <param name="shouldDispose">True to dispose the Gpio Controller</param>
        /// <param name="singleBiDirectionPin">True if a controller with one direction input is used,
        /// false if a controller with two direction inputs is used</param>
        /// <returns><see cref="DCMotor"/> instance</returns>
        /// <remarks>
        /// <paramref name="speedControlChannel"/> should be connected to the one of two inputs
        /// related with the motor direction (if H-bridge allows inputs to change frequently),
        /// or to PWM input if a controller with one direction input is used.
        /// <paramref name="directionPin"/> should be connected to H-bridge input corresponding to one of the motor inputs.
        /// or to direction input if a controller with one direction input is used.
        /// Connecting motor directly to GPIO pin is not recommended and may damage your board.
        /// </remarks>
        public static DCMotor Create(PwmChannel speedControlChannel, int directionPin, GpioController? controller = null, bool shouldDispose = true, bool singleBiDirectionPin = false)
        {
            if (speedControlChannel == null)
            {
                throw new ArgumentNullException(nameof(speedControlChannel));
            }

            if (directionPin == -1)
            {
                throw new ArgumentOutOfRangeException(nameof(directionPin));
            }

            if (singleBiDirectionPin)
            {
                return new DCMotor2PinWithBiDirectionalPin(speedControlChannel, directionPin, controller, shouldDispose);
            }
            else
            {
                return new DCMotor2PinNoEnable(speedControlChannel, directionPin, controller, shouldDispose);
            }
        }

        /// <summary>
        /// Creates <see cref="DCMotor"/> instance using two pins which allows to control speed in both directions.
        /// </summary>
        /// <param name="speedControlPin">Pin used to control the speed of the motor with software PWM (frequency will default to 50Hz)</param>
        /// <param name="directionPin">Pin used to control the direction of the motor</param>
        /// <param name="controller">GPIO controller related to <paramref name="speedControlPin"/> and <paramref name="directionPin"/></param>
        /// <param name="shouldDispose">True to dispose the Gpio Controller</param>
        /// <param name="singleBiDirectionPin">True if a controller with one direction input is used,
        /// false if a controller with two direction inputs is used</param>
        /// <returns><see cref="DCMotor"/> instance</returns>
        /// <remarks>
        /// <paramref name="speedControlPin"/> should be connected to the one of two inputs
        /// related with the motor direction (if H-bridge allows inputs to change frequently),
        /// or to PWM input if a controller with one direction input is used.
        /// <paramref name="directionPin"/> should be connected to H-bridge input corresponding to one of the motor inputs.
        /// or to direction input if a controller with one direction input is used.
        /// Connecting motor directly to GPIO pin is not recommended and may damage your board.
        /// </remarks>
        public static DCMotor Create(int speedControlPin, int directionPin, GpioController? controller = null, bool shouldDispose = true, bool singleBiDirectionPin = false)
        {
            if (speedControlPin == -1)
            {
                throw new ArgumentOutOfRangeException(nameof(speedControlPin));
            }

            if (directionPin == -1)
            {
                throw new ArgumentOutOfRangeException(nameof(directionPin));
            }

            controller = controller ?? new GpioController();

            if (singleBiDirectionPin)
            {
                return new DCMotor2PinWithBiDirectionalPin(
                    new SoftwarePwmChannel(speedControlPin, DefaultPwmFrequency, 0.0, controller: controller),
                    directionPin,
                    controller,
                    shouldDispose);
            }
            else
            {
                return new DCMotor2PinNoEnable(
                    new SoftwarePwmChannel(speedControlPin, DefaultPwmFrequency, 0.0, controller: controller),
                    directionPin,
                    controller,
                    shouldDispose);
            }
        }

        /// <summary>
        /// Creates <see cref="DCMotor"/> instance using three pins which allows to control speed in both directions.
        /// </summary>
        /// <param name="speedControlChannel"><see cref="PwmChannel"/> used to control the speed of the motor</param>
        /// <param name="directionPin">First pin used to control the direction of the motor</param>
        /// <param name="otherDirectionPin">Second pin used to control the direction of the motor</param>
        /// <param name="controller"><see cref="GpioController"/> related to <paramref name="directionPin"/> and <paramref name="otherDirectionPin"/></param>
        /// <param name="shouldDispose">True to dispose the Gpio Controller</param>
        /// <returns><see cref="DCMotor"/> instance</returns>
        /// <remarks>
        /// When speed is non-zero the value of <paramref name="otherDirectionPin"/> will always be opposite to that of <paramref name="directionPin"/>.
        /// <paramref name="speedControlChannel"/> should be connected to enable pin of the H-bridge.
        /// <paramref name="directionPin"/> should be connected to H-bridge input corresponding to one of the motor inputs.
        /// <paramref name="otherDirectionPin"/> should be connected to H-bridge input corresponding to the remaining motor input.
        /// Connecting motor directly to GPIO pin is not recommended and may damage your board.
        /// </remarks>
        public static DCMotor Create(PwmChannel speedControlChannel, int directionPin, int otherDirectionPin, GpioController? controller = null, bool shouldDispose = true)
        {
            if (speedControlChannel == null)
            {
                throw new ArgumentNullException(nameof(speedControlChannel));
            }

            if (directionPin == -1)
            {
                throw new ArgumentOutOfRangeException(nameof(directionPin));
            }

            if (otherDirectionPin == -1)
            {
                throw new ArgumentOutOfRangeException(nameof(otherDirectionPin));
            }

            return new DCMotor3Pin(
                speedControlChannel,
                directionPin,
                otherDirectionPin,
                controller,
                shouldDispose);
        }

        /// <summary>
        /// Creates <see cref="DCMotor"/> instance using three pins which allows to control speed in both directions.
        /// </summary>
        /// <param name="speedControlPin">Pin used to control the speed of the motor with software PWM (frequency will default to 50Hz)</param>
        /// <param name="directionPin">First pin used to control the direction of the motor</param>
        /// <param name="otherDirectionPin">Second pin used to control the direction of the motor</param>
        /// <param name="controller"><see cref="GpioController"/> related to <paramref name="speedControlPin"/>, <paramref name="directionPin"/> and <paramref name="otherDirectionPin"/></param>
        /// <param name="shouldDispose">True to dispose the Gpio Controller</param>
        /// <returns><see cref="DCMotor"/> instance</returns>
        /// <remarks>
        /// When speed is non-zero the value of <paramref name="otherDirectionPin"/> will always be opposite to that of <paramref name="directionPin"/>
        /// PWM pin <paramref name="speedControlPin"/> should be connected to enable pin of the H-bridge.
        /// <paramref name="directionPin"/> should be connected to H-bridge input corresponding to one of the motor inputs.
        /// <paramref name="otherDirectionPin"/> should be connected to H-bridge input corresponding to the remaining motor input.
        /// Connecting motor directly to GPIO pin is not recommended and may damage your board.
        /// </remarks>
        public static DCMotor Create(int speedControlPin, int directionPin, int otherDirectionPin, GpioController? controller = null, bool shouldDispose = true)
        {
            if (speedControlPin == -1)
            {
                throw new ArgumentOutOfRangeException(nameof(speedControlPin));
            }

            if (directionPin == -1)
            {
                throw new ArgumentOutOfRangeException(nameof(directionPin));
            }

            if (otherDirectionPin == -1)
            {
                throw new ArgumentOutOfRangeException(nameof(otherDirectionPin));
            }

            controller = controller ?? new GpioController();
            return new DCMotor3Pin(
                new SoftwarePwmChannel(speedControlPin, DefaultPwmFrequency, 0.0, controller: controller),
                directionPin,
                otherDirectionPin,
                controller,
                shouldDispose);
        }
    }
}
