// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using Iot.Device.BuildHat.Models;

namespace Iot.Device.BuildHat.Motors
{
    /// <summary>
    /// Interface for a motor
    /// </summary>
    public interface IMotor
    {
        /// <summary>
        /// Set the speed of the motor
        /// </summary>
        /// <param name="speed">speed is between -100 and +100</param>
        void SetSpeed(int speed);

        /// <summary>
        /// Stop the Motor
        /// </summary>
        void Stop();

        /// <summary>
        /// Start the motor
        /// </summary>
        void Start();

        /// <summary>
        /// Start with the specified speed
        /// </summary>
        /// <param name="speed">speed is between -100 and +100</param>
        void Start(int speed);

        /// <summary>
        /// Get the speed
        /// </summary>
        /// <returns>speed is between -100 and +100</returns>
        int GetSpeed();

        /// <summary>
        /// Sets the bias of the motor.
        /// </summary>
        /// <param name="bias">Bias, must be between 0 and 1.</param>
        void SetBias(double bias);

        /// <summary>
        /// Sets the power consumption limit.
        /// </summary>
        /// <param name="plimit">The power consumption limit. Must be between 0 and 1.</param>
        void SetPowerLimit(double plimit);

        /// <summary>
        /// Gets the speed of the motor
        /// speed is between -100 and +100
        /// </summary>
        int Speed { get; }

        /// <summary>
        /// Motor port
        /// </summary>
        SensorPort Port { get; }

        /// <summary>
        /// Motor type
        /// </summary>
        SensorType SensorType { get; }

        /// <summary>
        /// Gets the name of the sensor.
        /// </summary>
        /// <returns>The sensor name.</returns>
        string GetMotorName();

        /// <summary>
        /// Gets true if the motor is connected.
        /// </summary>
        bool IsConnected { get; }

        /// <summary>
        /// Floats the motor and stop all constraints on it.
        /// </summary>
        void Float();
    }
}
