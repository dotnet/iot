// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

namespace Iot.Device.Media
{
    /// <summary>
    /// The type of a video device's control.
    /// </summary>
    public enum VideoDeviceValueType : uint
    {
        /// <summary>
        /// Exposure Type
        /// </summary>
        ExposureType = 10094849,

        /// <summary>
        /// Exposure Time
        /// </summary>
        ExposureTime = 10094850,

        /// <summary>
        /// Sharpness
        /// </summary>
        Sharpness = 9963803,

        /// <summary>
        /// Contrast
        /// </summary>
        Contrast = 9963777,

        /// <summary>
        /// Brightness
        /// </summary>
        Brightness = 9963776,

        /// <summary>
        /// Saturation
        /// </summary>
        Saturation = 9963778,

        /// <summary>
        /// Gamma
        /// </summary>
        Gamma = 9963792,

        /// <summary>
        /// Gain
        /// </summary>
        Gain = 9963795,

        /// <summary>
        /// Rotate
        /// </summary>
        Rotate = 9963810,

        /// <summary>
        /// Horizontal Flip
        /// </summary>
        HorizontalFlip = 9963796,

        /// <summary>
        /// Vertical Flip
        /// </summary>
        VerticalFlip = 9963797,

        /// <summary>
        /// Power Line Frequency
        /// </summary>
        PowerLineFrequency = 9963800,

        /// <summary>
        /// White Balance Temperature
        /// </summary>
        WhiteBalanceTemperature = 9963802,

        /// <summary>
        /// Color Effect
        /// </summary>
        ColorEffect = 9963807,

        /// <summary>
        /// White Balance Effect
        /// </summary>
        WhiteBalanceEffect = 10094868,

        /// <summary>
        /// Scene Mode
        /// </summary>
        SceneMode = 10094874,
    }
}
