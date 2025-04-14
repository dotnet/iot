// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System.Diagnostics.CodeAnalysis;

namespace System.Device.Gpio.Drivers;

/// <summary>
/// The base class for the standard unix drivers
/// </summary>
public abstract class UnixDriver : GpioDriver
{
    /// <summary>
    /// Construct an instance of an unix driver.
    /// </summary>
    protected UnixDriver()
    {
        if (Environment.OSVersion.Platform != PlatformID.Unix)
        {
            throw new PlatformNotSupportedException(GetType().Name + " is only supported on Linux/Unix");
        }
    }

    /// <summary>
    /// Static factory method
    /// </summary>
    /// <returns>An instance of GpioDriver, depending on which one fits</returns>
    // TODO: remove try catch after https://github.com/dotnet/corefx/issues/32015 deployed
    public static UnixDriver Create()
    {
        if (Environment.OSVersion.Platform != PlatformID.Unix)
        {
            throw new PlatformNotSupportedException(nameof(UnixDriver) + " is only supported on Linux/Unix");
        }

        UnixDriver? driver = null;

        if (TryCreate(() => new LibGpiodDriver(0), out driver))
        {
            return driver;
        }

#pragma warning disable SDGPIO0001
        if (TryCreate(() => new LibGpiodV2Driver(0), out driver))
#pragma warning restore SDGPIO0001
        {
            return driver;
        }

        if (TryCreate(() => new SysFsDriver(), out driver))
        {
            return driver;
        }

        throw new PlatformNotSupportedException("No unix driver appears to be runnable");
    }

    private static bool TryCreate(Func<UnixDriver> creationAction, [NotNullWhen(true)]out UnixDriver? driver)
    {
        try
        {
            driver = creationAction();
        }
        catch (Exception x) when (x is PlatformNotSupportedException || x is DllNotFoundException)
        {
            driver = null;
            return false;
        }

        return true;
    }
}
