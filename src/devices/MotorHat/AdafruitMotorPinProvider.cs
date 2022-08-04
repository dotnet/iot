// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System;

namespace Iot.Device.MotorHat
{
    /// <summary>
    /// <see cref="MotorPinProvider"/> implementation for AdaFruit/Aliexpress
    /// </summary>
    /// <remarks>
    /// These correspond to motor hat screw terminals M1, M2, M3 and M4.
    /// </remarks>
    public class AdafruitMotorPinProvider : IMotorPinProvider
    {
        /// <inheritdoc/>
        public MotorPins GetPinsForMotor(int index)
        {
            return index switch
            {
                1 => new MotorPins(8, 9, 10),
                2 => new MotorPins(13, 12, 11),
                3 => new MotorPins(2, 3, 4),
                4 => new MotorPins(7, 6, 5),
                _ => throw new ArgumentException($"MotorHat Motor must be between 1 and 4 inclusive. {nameof(index)}: {index}", nameof(index))
            };
        }
    }
}
