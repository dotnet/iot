// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

namespace Iot.Device.MotorHat
{
    /// <summary>
    /// Represents a Motor PinConfiguration
    /// </summary>
    /// <param name="SpeedPin">The pin controlling the speed of the motor</param>
    /// <param name="In1Pin">The first pin for controlling direction</param>
    /// <param name="In2Pin">The second pin for controlling direction</param>
    /// <remarks>
    /// The PCA9685 PWM controller is used to control the inputs of two dual motor drivers.
    /// Each motor driver circuit has one speed pin and two IN pins.
    /// The PWM pin expects a PWM input signal. The two IN pins expect a logic 0 or 1 input signal.
    /// The variables SpeedPin, In1Pin and In2Pin variables identify which PCA9685 PWM output pins will be used to drive this DCMotor.
    /// The speed variable identifies which PCA9685 output pin is used to drive the PWM input on the motor driver.
    /// And the In1Pin and In2Pin are used to specify which PCA9685 output pins are used to drive the xIN1 and xIN2 input pins of the motor driver.
    /// </remarks>
    public record MotorPins(int SpeedPin, int In1Pin, int In2Pin);
}
