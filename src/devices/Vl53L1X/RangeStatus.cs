// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

namespace Iot.Device.Vl53L1X
{
    /// <summary>
    /// The range status of the device.
    /// There are five range statuses: 0, 1, 2, 4, and 7. When the range status is 0, there is no error.
    /// Range status 1 and 2 are error warnings while range status 4 and 7 are errors.
    /// </summary>
    public enum RangeStatus : byte
    {
        /// <summary>
        /// No error has occured.
        /// </summary>
        NoError = 0,

        /// <summary>
        /// SigmaFailure.
        /// This means that the repeatability or standard deviation of the measurement is bad due to a decreasing signal noise ratio.
        /// Increasing the timing budget can improve the standard deviation and avoid a range status 1.
        /// </summary>
        SigmaFailure = 1,

        /// <summary>
        /// SignalFailure.
        /// This means that the return signal is too week to return a good answer.
        /// The reason is because the target is too far, or the target is not reflective enough, or the target is too small.
        /// Increasing the timing budget might help, but there may simply be no target available.
        /// </summary>
        SignalFailure = 2,

        /// <summary>
        /// OutOfBounds.
        /// This means that the sensor is ranging in a "non-appropriated" zone and the measured result may be inconsistent.
        /// This status is considered as a warning but, in general, it happens when a target is at the maximum distance possible from the sensor, i.e. around 5 m.
        /// However, this is only for very bright targets.
        /// </summary>
        OutOfBounds = 4,

        /// <summary>
        /// WrapAround.
        /// This situation may occur when the target is very reflective and the distance to the target/sensor is longer than the physical limited distance measurable by the sensor.
        /// Such distances include approximately 5 m when the senor is in Long distance mode and approximately 1.3 m when the sensor is in Short distance mode.
        /// </summary>
        WrapAround = 7,
    }
}
