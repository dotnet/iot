// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System;

namespace Iot.Device.MotorHat
{
    /// <summary>
    /// <see cref="MotorPinProvider"/> implementation for Waveshare
    /// </summary>
    /// <remarks>
    /// These correspond to motor hat screw terminals M1 and M2
    /// </remarks>
    public class WaveshareMotorPinProvider : IMotorPinProvider
    {
        /// <inheritdoc/>
        public MotorPins GetPinsForMotor(int index)
        {
            return index switch
            {
                1 => new MotorPins(0, 1, 2),
                2 => new MotorPins(5, 4, 3),
                _ => throw new ArgumentException($"MotorHat Motor must be either 1 or 2. {nameof(index)}: {index}", nameof(index))
            };
        }
    }
}
