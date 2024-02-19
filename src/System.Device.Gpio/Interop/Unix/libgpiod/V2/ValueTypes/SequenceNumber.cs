// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

namespace System.Device.Gpio.Libgpiod.V2;

/// <summary>
/// Value type for GPIO edge event sequence number
/// </summary>
internal readonly record struct SequenceNumber
{
    private readonly ulong _value;

    /// <summary>
    /// Creates a sequence number with value 0.
    /// </summary>
    public SequenceNumber()
    {
        _value = 0;
    }

    private SequenceNumber(ulong val)
    {
        _value = val;
    }

    /// <summary>
    /// Explicit cast operator SequenceNumber -> ulong
    /// </summary>
    /// <param name="s">The sequenceNumber</param>
    public static explicit operator ulong(SequenceNumber s)
    {
        return s._value;
    }

    /// <summary>
    /// Implicit cast operator ulong -> SequenceNumber
    /// </summary>
    /// <param name="val">The ulong value</param>
    public static implicit operator SequenceNumber(ulong val)
    {
        return new SequenceNumber(val);
    }

    /// <summary>
    /// Returns ulong value as string
    /// </summary>
    public override string ToString()
    {
        return _value.ToString();
    }
}
