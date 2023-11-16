// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

namespace Iot.Device.Vcnl4040
{
    /// <summary>
    /// Represents the set of parameters determining the receiver configuration for the ambient light sensor.
    /// </summary>
    /// <param name="ExtendedOutputRange">Specifies whether the standard output range of 12-bit (4096 counts)
    /// or the extended range of 16-bit (65535 counts) is used.</param>
    /// <param name="CancellationLevel">Sets a value that is subtracted from the measurement value.
    /// This allows adjustments for ambient light or an initial object distance.</param>
    /// Note: this is limited by the internal 16-bit ADC, therefore most useful when using the 12-bit range.
    /// <param name="WhiteChannelEnabled">Enables the white channel measurement.</param>
    /// <param name="SunlightCancellationEnabled">Enables the sunlight cancellation feature.</param>
    public record ReceiverConfiguration(
        bool ExtendedOutputRange,
        ushort CancellationLevel,
        bool WhiteChannelEnabled,
        bool SunlightCancellationEnabled);
}
