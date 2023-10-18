// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

namespace System.Device.I2c;

/// <summary>
/// The connection settings of a device on an I2C bus.
/// </summary>
public sealed class I2cConnectionSettings : IEquatable<I2cConnectionSettings>
{
    private I2cConnectionSettings()
    {
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="I2cConnectionSettings"/> class.
    /// </summary>
    /// <param name="busId">The bus ID the I2C device is connected to.</param>
    /// <param name="deviceAddress">The bus address of the I2C device.</param>
    public I2cConnectionSettings(int busId, int deviceAddress)
    {
        BusId = busId;
        DeviceAddress = deviceAddress;
    }

    internal I2cConnectionSettings(I2cConnectionSettings other)
    {
        BusId = other.BusId;
        DeviceAddress = other.DeviceAddress;
    }

    /// <summary>
    /// The bus ID the I2C device is connected to.
    /// </summary>
    public int BusId { get; }

    /// <summary>
    /// The bus address of the I2C device.
    /// </summary>
    public int DeviceAddress { get; }

    /// <summary>
    /// Equality comparator
    /// </summary>
    /// <param name="other">The other instance</param>
    /// <returns>True or false</returns>
    public bool Equals(I2cConnectionSettings? other)
    {
        if (ReferenceEquals(null, other))
        {
            return false;
        }

        if (ReferenceEquals(this, other))
        {
            return true;
        }

        return BusId == other.BusId && DeviceAddress == other.DeviceAddress;
    }

    /// <summary>
    /// Basic equality overload
    /// </summary>
    /// <param name="obj">The other instance</param>
    /// <returns>True on value equality, false otherwise</returns>
    public override bool Equals(object? obj)
    {
        return ReferenceEquals(this, obj) || (obj is I2cConnectionSettings other && Equals(other));
    }

    /// <summary>
    /// Standard hash function
    /// </summary>
    /// <returns>A hash code</returns>
    public override int GetHashCode()
    {
        unchecked
        {
            return (BusId * 397) ^ DeviceAddress;
        }
    }
}
