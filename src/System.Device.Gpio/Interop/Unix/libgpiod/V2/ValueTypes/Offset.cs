// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

namespace System.Device.Gpio.Libgpiod.V2;

/// <summary>
/// Unique identifier of a line.
/// </summary>
internal readonly record struct Offset
{
    private readonly uint _value;

    /// <summary>
    /// Creates an offset with value 0.
    /// </summary>
    public Offset()
    {
        _value = 0;
    }

    private Offset(uint val)
    {
        _value = val;
    }

    /// <summary>
    /// Explicit cast operator Offset -> int
    /// </summary>
    /// <param name="i">The offset value</param>
    public static explicit operator int(Offset i)
    {
        if (i._value > int.MaxValue)
        {
            throw new InvalidOperationException($"Cannot cast offset to int because '{i._value}' falls in negative int range");
        }

        return (int)i._value;
    }

    /// <summary>
    /// Implicit cast operator int -> Offset
    /// </summary>
    /// <param name="i">The offset value</param>
    public static implicit operator Offset(int i)
    {
        if (i < 0)
        {
            throw new InvalidOperationException($"Cannot cast negative int '{i}' to Offset");
        }

        uint u = (uint)i;

        return new Offset(u);
    }

    /// <summary>
    /// Implicit cast operator Offset -> uint
    /// </summary>
    /// <param name="o">The offset</param>
    public static implicit operator uint(Offset o)
    {
        return o._value;
    }

    /// <summary>
    /// Implicit cast operator uint -> Offset
    /// </summary>
    /// <param name="val">The uint value</param>
    public static implicit operator Offset(uint val)
    {
        return new Offset(val);
    }

    /// <summary>
    /// Returns uint value as string
    /// </summary>
    public override string ToString()
    {
        return _value.ToString();
    }
}

/// <summary>
/// Extension methods for <see cref="Offset"/>
/// </summary>
internal static class OffsetExtensions
{
    /// <summary>
    /// Converts Offset[] to uint[]
    /// </summary>
    /// <param name="offsets">Array of offsets</param>
    public static uint[] Convert(this Offset[] offsets)
    {
        return Array.ConvertAll(offsets, o => (uint)o);
    }

    /// <summary>
    /// Converts uint[] to Offset[]
    /// </summary>
    /// <param name="offsets">Array of offsets</param>
    public static Offset[] Convert(this uint[] offsets)
    {
        return Array.ConvertAll(offsets, o => (Offset)o);
    }
}
