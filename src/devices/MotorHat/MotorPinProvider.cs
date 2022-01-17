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
    /// Abstract base class for creating DCMotor instances
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

    /// <summary>
    /// <see cref="MotorPinProvider"/> implementation for AdaFruit/Aliexpress
    /// </summary>
    /// <remarks>
    /// These correspond to motor hat screw terminals M1, M2, M3 and M4.
    /// </remarks>
    public class AdafruitMotorPinProvider : IMotorPinProvider
    {
        /// <summary>
        /// Gets the <see cref="MotorPins"/> for motor at the specified <paramref name="index"/>
        /// </summary>
        /// <param name="index">The index of the motor to get pins for</param>
        /// <returns></returns>
        public MotorPins GetPinsForMotor(int index)
        {
            return index switch
            {
                1 => new MotorPins(8, 9, 10),
                2 => new MotorPins(13, 12, 11),
                3 => new MotorPins(2, 3, 4),
                4 => new MotorPins(7, 6, 5),
                _ => throw new ArgumentException(nameof(index), $"MotorHat Motor must be between 1 and 4 inclusive. {nameof(index)}: {index}")
            };
        }
    }

    /// <summary>
    /// <see cref="MotorPinProvider"/> implementation for AdaFruit/Aliexpress
    /// </summary>
    /// <remarks>
    /// These correspond to motor hat screw terminals M1 and M2
    /// </remarks>
    public class WaveshareMotorPinProvider : IMotorPinProvider
    {
        /// <summary>
        /// Gets the <see cref="MotorPins"/> for motor at the specified <paramref name="index"/>
        /// </summary>
        /// <param name="index">The index of the motor to get pins for</param>
        /// <returns></returns>
        public MotorPins GetPinsForMotor(int index)
        {
            return index switch
            {
                1 => new MotorPins(0, 1, 2),
                2 => new MotorPins(5, 4, 3),
                _ => throw new ArgumentException(nameof(index), $"MotorHat Motor must be either 1 or 2. {nameof(index)}: {index}")
            };
        }
    }
}
