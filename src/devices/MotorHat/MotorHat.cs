// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System;
using System.Device.I2c;
using System.Collections.Generic;
using System.Device.Pwm;
using Iot.Device.Pwm;

namespace Iot.Device.MotorHat
{
    /// <summary>
    /// Raspberry Pi Motor Hat based on PCA9685 PWM controller
    /// </summary>
    public class MotorHat : IDisposable
    {
        /// <summary>
        /// Default I2C address of Motor Hat
        /// </summary>
        public const int I2cAddressBase = 0x60;

        /// <summary>
        /// Motor Hat is built on top of a PCa9685
        /// </summary>
        private Pca9685 _pca9685;

        /// <summary>
        /// Holds every channel that is being used by either a DCMotor, Stepper, ServoMotor, or PWM
        /// </summary>
        private List<PwmChannel> _channelsUsed = new List<PwmChannel>();

        /// <summary>
        /// Initializes a new instance of the <see cref="MotorHat"/> class with the specified I2C settings and PWM frequency.
        /// </summary>
        /// <param name="settings">The I2C settings of the MotorHat.</param>
        /// <param name="frequency">The frequency in Hz to set the PWM controller.</param>
        /// <remarks>
        /// The default i2c address is 0x60, but the HAT can be configured in hardware to any address from 0x60 to 0x7f.
        /// The PWM hardware used by this HAT is a PCA9685. It has a total possible frequency range of 24 to 1526 Hz.
        /// Setting the frequency above or below this range will cause PWM hardware to be set at its maximum or minimum setting.
        /// </remarks>
        public MotorHat(I2cConnectionSettings settings, double frequency = 1600)
        {
            I2cDevice device = I2cDevice.Create(settings);
            _pca9685 = new Pca9685(device);

            _pca9685.PwmFrequency = frequency;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="MotorHat"/> class with the default I2C address and PWM frequency.
        /// </summary>
        /// <param name="frequency">The frequency in Hz to set the PWM controller.</param>
        /// <param name="selectedAddress">The I2C settings of the MotorHat. 0 by default if jumpers were not changed
        /// Use the following code to set an address equivalent to the one configured in your device jumpers
        /// var selectedAddress = 0b000000;  // were bits represents jumpers A5 A4 A3 A2 A1 A0</param>
        /// <remarks>
        /// The <see cref="MotorHat"/> will be created with the default I2C address of 0x60 and PWM frequency of 1600 Hz.
        /// </remarks>
        public MotorHat(double frequency = 1600, int selectedAddress = 0b000000)
            : this(new I2cConnectionSettings(1, I2cAddressBase + selectedAddress), frequency)
        {
        }

        /// <summary>
        /// Creates a <see cref="DCMotor"/> object for the specified channel.
        /// </summary>
        /// <param name="motorNumber">A motor number from 1 to 4.</param>
        /// <remarks>
        /// The motorNumber parameter refers to the motor numbers M1, M2, M3 or M4 printed in the device.
        /// </remarks>
        public DCMotor.DCMotor CreateDCMotor(int motorNumber)
        {
            if (motorNumber < 1 || motorNumber > 4)
            {
                throw new ArgumentOutOfRangeException(nameof(motorNumber), $"Must be between 1 and 4, corresponding with M1, M2, M3 and M4. (Received: {motorNumber}");
            }

            // The PCA9685 PWM controller is used to control the inputs of two dual motor drivers.
            // These correspond to motor hat screw terminals M1, M2, M3 and M4.
            // Each motor driver circuit has one speed pin and two IN pins.
            // The PWM pin expects a PWM input signal. The two IN pins expect a logic 0 or 1 input signal.
            // The variables speed, in1 and in2 variables identify which PCA9685 PWM output pins will be used to drive this DCMotor.
            // The speed variable identifies which PCA9685 output pin is used to drive the PWM input on the motor driver.
            // And the in1 and in2 variables are used to specify which PCA9685 output pins are used to drive the xIN1 and xIN2 input pins of the motor driver.
            int speedPin, in1Pin, in2Pin;

            switch (motorNumber)
            {
                case 1:
                    speedPin = 8;
                    in2Pin = 9;
                    in1Pin = 10;
                    break;
                case 2:
                    speedPin = 13;
                    in2Pin = 12;
                    in1Pin = 11;
                    break;
                case 3:
                    speedPin = 2;
                    in2Pin = 3;
                    in1Pin = 4;
                    break;
                case 4:
                    speedPin = 7;
                    in2Pin = 6;
                    in1Pin = 5;
                    break;
                default:
                    throw new ArgumentException($"MotorHat Motor must be between 1 and 4 inclusive. Received: {motorNumber}");
            }

            var speedPwm = _pca9685.CreatePwmChannel(speedPin);
            var in1Pwm = _pca9685.CreatePwmChannel(in1Pin);
            var in2Pwm = _pca9685.CreatePwmChannel(in2Pin);

            _channelsUsed.Add(speedPwm);
            _channelsUsed.Add(in1Pwm);
            _channelsUsed.Add(in2Pwm);

            return new DCMotor3Pwm(speedPwm, in1Pwm, in2Pwm);
        }

        /// <summary>
        /// Creates a <see cref="PwmChannel"/> from one of the 4 available PWM channels in the MotorHat
        /// </summary>
        /// <param name="channelNumber">A valid PWM channel (0, 1, 14 or 15)</param>
        /// <remarks>
        /// The channelNumber refers to ont of the available PWM channel numbers available in the Motor Hat (0, 1, 14, 15) printed in the device.
        /// </remarks>
        public PwmChannel CreatePwmChannel(int channelNumber)
        {
            if (channelNumber != 0 && channelNumber != 1 && channelNumber != 14 && channelNumber != 15)
            {
                throw new ArgumentOutOfRangeException(nameof(channelNumber), $"Must be one of de available PWM channels (0, 1, 14 or 15). (received: {channelNumber})");
            }

            var pwmChannel = _pca9685.CreatePwmChannel(channelNumber);

            _channelsUsed.Add(pwmChannel);

            return pwmChannel;
        }

        /// <summary>
        /// Creates a <see cref="ServoMotor"/> from one of the 4 available PWM channels  in the MotorHat
        /// </summary>
        /// <param name="channelNumber">A valid PWM channel (0, 1, 14 or 15)</param>
        /// <param name="maximumAngle">The maximum angle the servo motor can move represented as a value between 0 and 360.</param>
        /// <param name="minimumPulseWidthMicroseconds">The minimum pulse width, in microseconds, that represent an angle for 0 degrees.</param>
        /// <param name="maximumPulseWidthMicroseconds">The maxnimum pulse width, in microseconds, that represent an angle for maximum angle.</param>
        /// <remarks>
        /// The channelNumber refers to ont of the available PWM channel numbers available in the Motor Hat (0, 1, 14, 15) printed in the device.
        /// </remarks>
        public ServoMotor.ServoMotor CreateServoMotor(int channelNumber, double maximumAngle = 180, double minimumPulseWidthMicroseconds = 1000, double maximumPulseWidthMicroseconds = 2000)
        {
            var pwmChannel = CreatePwmChannel(channelNumber);

            var servo = new ServoMotor.ServoMotor(pwmChannel, maximumAngle, minimumPulseWidthMicroseconds, maximumPulseWidthMicroseconds);

            return servo;
        }

        /// <inheritdoc/>
        public void Dispose()
        {
            foreach (var channel in _channelsUsed)
            {
                channel.Stop();
            }

            _pca9685?.Dispose();
            _pca9685 = null!;
        }
    }
}
