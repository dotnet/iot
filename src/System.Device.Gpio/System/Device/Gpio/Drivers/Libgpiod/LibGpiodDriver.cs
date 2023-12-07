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
    private readonly UnixDriver _versionedLibgpiodDriver;

    /// <summary>
    /// Creates an instance of the LigGpiodDriver that matches the installed version of libgpiod.
    /// </summary>
    /// <param name="gpioChip">The number of the GPIO chip to drive.</param>
    public LibGpiodDriver(int gpioChip = 0)
    {
        _versionedLibgpiodDriver = LibGpiodDriverFactory.Create(gpioChip);
    }

    /// <inheritdoc/>
    protected internal override int PinCount => _versionedLibgpiodDriver.PinCount;

    /// <inheritdoc/>
    protected internal override void AddCallbackForPinValueChangedEvent(int pinNumber, PinEventTypes eventTypes, PinChangeEventHandler callback)
    {
        _versionedLibgpiodDriver.AddCallbackForPinValueChangedEvent(pinNumber, eventTypes, callback);
    }

    /// <inheritdoc/>
    protected internal override void ClosePin(int pinNumber)
    {
        _versionedLibgpiodDriver.ClosePin(pinNumber);
    }

    /// <inheritdoc/>
    protected internal override int ConvertPinNumberToLogicalNumberingScheme(int pinNumber)
    {
        return _versionedLibgpiodDriver.ConvertPinNumberToLogicalNumberingScheme(pinNumber);
    }

    /// <inheritdoc/>
    protected internal override PinMode GetPinMode(int pinNumber)
    {
        return _versionedLibgpiodDriver.GetPinMode(pinNumber);
    }

    /// <inheritdoc/>
    protected internal override bool IsPinModeSupported(int pinNumber, PinMode mode)
    {
        return _versionedLibgpiodDriver.IsPinModeSupported(pinNumber, mode);
    }

    /// <inheritdoc/>
    protected internal override void OpenPin(int pinNumber)
    {
        _versionedLibgpiodDriver.OpenPin(pinNumber);
    }

    /// <inheritdoc/>
    protected internal override PinValue Read(int pinNumber)
    {
        return _versionedLibgpiodDriver.Read(pinNumber);
    }

    /// <inheritdoc/>
    protected internal override void Toggle(int pinNumber)
    {
        _versionedLibgpiodDriver.Toggle(pinNumber);
    }

    /// <inheritdoc/>
    protected internal override void RemoveCallbackForPinValueChangedEvent(int pinNumber, PinChangeEventHandler callback)
    {
        _versionedLibgpiodDriver.RemoveCallbackForPinValueChangedEvent(pinNumber, callback);
    }

    /// <inheritdoc/>
    protected internal override void SetPinMode(int pinNumber, PinMode mode)
    {
        _versionedLibgpiodDriver.SetPinMode(pinNumber, mode);
    }

    /// <inheritdoc />
    protected internal override void SetPinMode(int pinNumber, PinMode mode, PinValue initialValue)
    {
        _versionedLibgpiodDriver.SetPinMode(pinNumber, mode, initialValue);
    }

    /// <inheritdoc/>
    protected internal override WaitForEventResult WaitForEvent(int pinNumber, PinEventTypes eventTypes, CancellationToken cancellationToken)
    {
        return _versionedLibgpiodDriver.WaitForEvent(pinNumber, eventTypes, cancellationToken);
    }

    /// <inheritdoc/>
    protected internal override void Write(int pinNumber, PinValue value)
    {
        _versionedLibgpiodDriver.Write(pinNumber, value);
    }

    /// <inheritdoc />
    public override ComponentInformation QueryComponentInformation()
    {
        return _versionedLibgpiodDriver.QueryComponentInformation();
    }

    /// <inheritdoc/>
    protected override void Dispose(bool disposing)
    {
        _versionedLibgpiodDriver.Dispose();
        base.Dispose(disposing);
    }
}
