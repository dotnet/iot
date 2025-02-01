// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System.Diagnostics.CodeAnalysis;
using System.Threading;

namespace System.Device.Gpio.Drivers;

/// <summary>
/// This driver uses libgpiod to get user-level access to the GPIO ports.
/// It supersedes the SysFsDriver, but requires that libgpiod is installed.
/// </summary>
public class LibGpiodDriver : UnixDriver
{
    private GpioDriver _driver;

    /// <summary>
    /// Creates an instance of the LibGpiodDriver for the most suitable chip
    /// </summary>
    /// <remarks>
    /// The driver version is chosen based on the installed libgpiod version on the system. To select a specific library version use another
    /// constructor or specify the environment variable DOTNET_IOT_LIBGPIOD_DRIVER_VERSION, see documentation
    /// </remarks>
    public LibGpiodDriver()
    {
        _driver = LibGpiodDriverFactory.Instance.Create();
    }

    /// <summary>
    /// Creates an instance of the LibGpiodDriver
    /// </summary>
    /// <param name="gpioChip">The number of the GPIO chip to drive</param>
    /// <remarks>
    /// The driver version is chosen based on the installed libgpiod version on the system. To select a specific library version use the
    /// other constructor or specify the environment variable DOTNET_IOT_LIBGPIOD_DRIVER_VERSION, see documentation
    /// </remarks>
    public LibGpiodDriver(int gpioChip)
    {
        var versionedLibgpiodDriver = LibGpiodDriverFactory.Instance.Create(gpioChip);
        _driver = versionedLibgpiodDriver;
    }

    /// <summary>
    /// Creates an instance of the LigGpiodDriver that targets specific version/s of libgpiod
    /// </summary>
    /// <param name="gpioChip">The number of the GPIO chip to drive</param>
    /// <param name="driverVersion">Version of the libgpiod driver to create</param>
    /// <remarks>Alternatively, specify the environment variable DOTNET_IOT_LIBGPIOD_DRIVER_VERSION, see documentation</remarks>
    [Experimental(DiagnosticIds.SDGPIO0001, UrlFormat = DiagnosticIds.UrlFormat)]
    public LibGpiodDriver(int gpioChip, Version driverVersion)
    {
        var versionedLibgpiodDriver = LibGpiodDriverFactory.Instance.Create(gpioChip, driverVersion);
        _driver = versionedLibgpiodDriver;
    }

    /// <summary>
    /// A collection of driver versions that correspond to the installed versions of libgpiod on this system. Each driver is dependent
    /// on specific libgpiod version/s. If the collection is empty, it indicates that libgpiod might not be installed or could not be detected.
    /// </summary>
    public static Version[] GetAvailableVersions() => LibGpiodDriverFactory.Instance.DriverCandidates;

    /// <inheritdoc/>
    protected internal override int PinCount => GetDriver().PinCount;

    /// <inheritdoc/>
    protected internal override void AddCallbackForPinValueChangedEvent(int pinNumber, PinEventTypes eventTypes, PinChangeEventHandler callback)
    {
        GetDriver().AddCallbackForPinValueChangedEvent(pinNumber, eventTypes, callback);
    }

    /// <inheritdoc/>
    protected internal override void ClosePin(int pinNumber)
    {
        GetDriver().ClosePin(pinNumber);
    }

    /// <inheritdoc/>
    protected internal override int ConvertPinNumberToLogicalNumberingScheme(int pinNumber)
    {
        return GetDriver().ConvertPinNumberToLogicalNumberingScheme(pinNumber);
    }

    /// <inheritdoc/>
    protected internal override PinMode GetPinMode(int pinNumber)
    {
        return GetDriver().GetPinMode(pinNumber);
    }

    /// <inheritdoc/>
    protected internal override bool IsPinModeSupported(int pinNumber, PinMode mode)
    {
        return GetDriver().IsPinModeSupported(pinNumber, mode);
    }

    /// <inheritdoc/>
    protected internal override void OpenPin(int pinNumber)
    {
        GetDriver().OpenPin(pinNumber);
    }

    /// <inheritdoc/>
    protected internal override PinValue Read(int pinNumber)
    {
        return GetDriver().Read(pinNumber);
    }

    /// <inheritdoc/>
    protected internal override void Toggle(int pinNumber)
    {
        GetDriver().Toggle(pinNumber);
    }

    /// <inheritdoc/>
    protected internal override void RemoveCallbackForPinValueChangedEvent(int pinNumber, PinChangeEventHandler callback)
    {
        GetDriver().RemoveCallbackForPinValueChangedEvent(pinNumber, callback);
    }

    /// <inheritdoc/>
    protected internal override void SetPinMode(int pinNumber, PinMode mode)
    {
        GetDriver().SetPinMode(pinNumber, mode);
    }

    /// <inheritdoc />
    protected internal override void SetPinMode(int pinNumber, PinMode mode, PinValue initialValue)
    {
        GetDriver().SetPinMode(pinNumber, mode, initialValue);
    }

    /// <inheritdoc/>
    protected internal override WaitForEventResult WaitForEvent(int pinNumber, PinEventTypes eventTypes, CancellationToken cancellationToken)
    {
        return GetDriver().WaitForEvent(pinNumber, eventTypes, cancellationToken);
    }

    /// <inheritdoc/>
    protected internal override void Write(int pinNumber, PinValue value)
    {
        GetDriver().Write(pinNumber, value);
    }

    /// <inheritdoc />
    public override ComponentInformation QueryComponentInformation()
    {
        var ret = new ComponentInformation(this, "Libgpiod Wrapper driver");
        ret.AddSubComponent(GetDriver().QueryComponentInformation());
        return ret;
    }

    private GpioDriver GetDriver()
    {
        if (_driver == null)
        {
            throw new ObjectDisposedException(nameof(LibGpiodDriver));
        }

        return _driver;
    }

    /// <inheritdoc/>
    protected override void Dispose(bool disposing)
    {
        if (_driver != null && disposing)
        {
            _driver.Dispose();
            _driver = null!;
        }

        base.Dispose(disposing);
    }
}
