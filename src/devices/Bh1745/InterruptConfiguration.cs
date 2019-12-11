// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

namespace Iot.Device.Bh1745
{
    /// <summary>
    /// Represents the configuration for the interrupt persistence functionality of the Bh1745.
    /// </summary>
    public enum InterruptPersistence : byte
    {
        /// <summary>
        /// Interrupt status is toggled at each measurement end.
        /// </summary>
        ToggleMeasurementEnd = 0b00,

        /// <summary>
        /// Interrupt status is updated at each measurement end.
        /// </summary>
        UpdateMeasurementEnd = 0b01,

        /// <summary>
        /// Interrupt status is updated if 4 consecutive threshold judgments are the same.
        /// </summary>
        UpdateConsecutiveX4 = 0b10,

        /// <summary>
        /// Interrupt status is updated if 8 consecutive threshold judgments are the same.
        /// </summary>
        UpdateConsecutiveX8 = 0b11
    }

    /// <summary>
    /// Represents the interrupt source which is one of the 4 color channels of the Bh1745.
    /// </summary>
    public enum InterruptSource : byte
    {
        /// <summary>
        /// The red color channel.
        /// </summary>
        RedChannel = 0b00,

        /// <summary>
        /// The green color channel.
        /// </summary>
        GreenChannel = 0b01,

        /// <summary>
        /// The blue color channel.
        /// </summary>
        BlueChannel = 0b10,

        /// <summary>
        /// The clear color channel.
        /// </summary>
        ClearChannel = 0b11
    }

    /// <summary>
    /// Represents the latch behavior of the interrupt pin of the Bh1745.
    /// </summary>
    public enum LatchBehavior : byte
    {
        /// <summary>
        /// Interrupt pin is latched until interrupt register is read or initialized.
        /// or initialized.
        /// </summary>
        LatchUntilReadOrInitialized = 0b00,

        /// <summary>
        /// Interrupt pin is latched after each measurement
        /// </summary>
        LatchEachMeasurement = 0b01
    }

    /// <summary>
    /// Represents the state of the interrupt pin of the Bh1745.
    /// </summary>
    public enum InterruptStatus : byte
    {
        /// <summary>
        /// Default state in which the interrupt pin is not initialized (active).
        /// </summary>
        Active = 0b00,

        /// <summary>
        /// Sets the pin to high impedance (inactive).
        /// </summary>
        Inactive = 0b01
    }
}
