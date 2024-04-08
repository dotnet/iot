// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System.Threading;

namespace System.Device.Gpio.Drivers;

/// <summary>
/// This driver uses libgpiod to get user-level access to the GPIO ports.
/// It supersedes the SysFsDriver, but requires that libgpiod is installed.
/// </summary>
public class LibGpiodDriver : UnixDriver
{
    private readonly GpioDriver _driver;

    /// <summary>
    /// Creates an instance of the LibGpiodDriver
    /// </summary>
    /// <param name="gpioChip">The number of the GPIO chip to drive</param>
    /// <remarks>
    /// The driver version is chosen based on the installed libgpiod version on the system. To select a specific library version use the
    /// other constructor or specify the environment variable DOTNET_IOT_LIBGPIOD_DRIVER_VERSION, see documentation
    /// </remarks>
    public LibGpiodDriver(int gpioChip = 0)
    {
        LibGpiodDriverFactory.VersionedLibgpiodDriver versionedLibgpiodDriver = LibGpiodDriverFactory.Instance.Create(gpioChip);
        _driver = versionedLibgpiodDriver.LibGpiodDriver;
        Version = versionedLibgpiodDriver.DriverVersion;
    }

    /// <summary>
    /// Creates an instance of the LigGpiodDriver that targets specific version/s of libgpiod
    /// <see cref="LibGpiodDriverVersion.V1"/> supports libgpiod.so.1.x to libgpiod.so.2.x
    /// and <see cref="LibGpiodDriverVersion.V2"/> supports libgpiod.so.3.x
    /// </summary>
    /// <param name="gpioChip">The number of the GPIO chip to drive</param>
    /// <param name="driverVersion">Version of the libgpiod driver to create</param>
    /// <remarks>Alternatively, specify the environment variable DOTNET_IOT_LIBGPIOD_DRIVER_VERSION, see documentation</remarks>
    public LibGpiodDriver(int gpioChip, LibGpiodDriverVersion driverVersion)
    {
        LibGpiodDriverFactory.VersionedLibgpiodDriver versionedLibgpiodDriver = LibGpiodDriverFactory.Instance.Create(gpioChip, driverVersion);
        _driver = versionedLibgpiodDriver.LibGpiodDriver;
        Version = versionedLibgpiodDriver.DriverVersion;
    }

    /// <summary>
    /// Version of the libgpiod driver
    /// </summary>
    public LibGpiodDriverVersion Version { get; protected set; }

    /// <summary>
    /// A collection of driver versions that correspond to the installed versions of libgpiod on this system. Each driver is dependent
    /// on specific libgpiod version/s. If the collection is empty, it indicates that libgpiod might not be installed or could not be detected.
    /// </summary>
    public static LibGpiodDriverVersion[] GetAvailableVersions() => LibGpiodDriverFactory.Instance.DriverCandidates;

    /// <inheritdoc/>
    protected internal override int PinCount => _driver.PinCount;

    /// <inheritdoc/>
    protected internal override void AddCallbackForPinValueChangedEvent(int pinNumber, PinEventTypes eventTypes, PinChangeEventHandler callback)
    {
        _driver.AddCallbackForPinValueChangedEvent(pinNumber, eventTypes, callback);
    }

    /// <inheritdoc/>
    protected internal override void ClosePin(int pinNumber)
    {
        _driver.ClosePin(pinNumber);
    }

    /// <inheritdoc/>
    protected internal override int ConvertPinNumberToLogicalNumberingScheme(int pinNumber)
    {
        return _driver.ConvertPinNumberToLogicalNumberingScheme(pinNumber);
    }

    /// <inheritdoc/>
    protected internal override PinMode GetPinMode(int pinNumber)
    {
        return _driver.GetPinMode(pinNumber);
    }

    /// <inheritdoc/>
    protected internal override bool IsPinModeSupported(int pinNumber, PinMode mode)
    {
        return _driver.IsPinModeSupported(pinNumber, mode);
    }

    /// <inheritdoc/>
    protected internal override void OpenPin(int pinNumber)
    {
        _driver.OpenPin(pinNumber);
    }

    /// <inheritdoc/>
    protected internal override PinValue Read(int pinNumber)
    {
        return _driver.Read(pinNumber);
    }

    /// <inheritdoc/>
    protected internal override void Toggle(int pinNumber)
    {
        _driver.Toggle(pinNumber);
    }

    /// <inheritdoc/>
    protected internal override void RemoveCallbackForPinValueChangedEvent(int pinNumber, PinChangeEventHandler callback)
    {
        _driver.RemoveCallbackForPinValueChangedEvent(pinNumber, callback);
    }

    /// <inheritdoc/>
    protected internal override void SetPinMode(int pinNumber, PinMode mode)
    {
        _driver.SetPinMode(pinNumber, mode);
    }

    /// <inheritdoc />
    protected internal override void SetPinMode(int pinNumber, PinMode mode, PinValue initialValue)
    {
        _driver.SetPinMode(pinNumber, mode, initialValue);
    }

    /// <inheritdoc/>
    protected internal override WaitForEventResult WaitForEvent(int pinNumber, PinEventTypes eventTypes, CancellationToken cancellationToken)
    {
        return _driver.WaitForEvent(pinNumber, eventTypes, cancellationToken);
    }

    /// <inheritdoc/>
    protected internal override void Write(int pinNumber, PinValue value)
    {
        _driver.Write(pinNumber, value);
    }

    /// <inheritdoc />
    public override ComponentInformation QueryComponentInformation()
    {
        return _driver.QueryComponentInformation();
    }

    /// <inheritdoc/>
    protected override void Dispose(bool disposing)
    {
        _driver.Dispose();
        base.Dispose(disposing);
    }
}
