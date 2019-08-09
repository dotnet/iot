// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

namespace Iot.Device.Media
{
    /// <summary>
    /// The type of a video device's control.
    /// </summary>
    public enum VideoDeviceValueType : uint
    {
        ExposureType = 10094849,
        ExposureTime = 10094850,
        Sharpness = 9963803,
        Contrast = 9963777,
        Brightness = 9963776,
        Saturation = 9963778,
        Gamma = 9963792,
        Gain = 9963795,
        Rotate = 9963810,
        HorizontalFlip = 9963796,
        VerticalFlip = 9963797,
        PowerLineFrequency = 9963800,
        WhiteBalanceTemperature = 9963802,
        ColorEffect = 9963807,
        WhiteBalanceEffect = 10094868,
        SceneMode = 10094874,
    }
}