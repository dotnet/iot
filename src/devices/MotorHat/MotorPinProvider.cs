// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System;

namespace Iot.Device.MotorHat
{
    /// <summary>
    /// Represents a provider for <see cref="MotorPins"/>
    /// </summary>
    public interface IMotorPinProvider
    {
        /// <summary>
        /// Gets the <see cref="MotorPins"/> for motor at the specified <paramref name="index"/>
        /// </summary>
        /// <param name="index">The index of the motor to get pins for</param>
        /// <returns></returns>
        public abstract MotorPins GetPinsForMotor(int index);
    }

    /// <summary>
    /// Static class providing known <see cref="IMotorPinProvider"/> instances
    /// </summary>
    public static class MotorPinProvider
    {
        /// <summary>
        /// <see cref="IMotorPinProvider"/> for AdaFruit motor hat
        /// </summary>
        public static readonly IMotorPinProvider AdaFruit = new AdafruitMotorPinProvider();

        /// <summary>
        /// <see cref="IMotorPinProvider"/> for Aliexpress motor hat
        /// </summary>
        public static readonly IMotorPinProvider Aliexpress = AdaFruit;

        /// <summary>
        /// <see cref="IMotorPinProvider"/> for Waveshare motor hat
        /// </summary>
        public static readonly IMotorPinProvider Waveshare = new WaveshareMotorPinProvider();

        /// <summary>
        /// Default <see cref="IMotorPinProvider"/>
        /// </summary>
        public static readonly IMotorPinProvider Default = AdaFruit;
    }
}
