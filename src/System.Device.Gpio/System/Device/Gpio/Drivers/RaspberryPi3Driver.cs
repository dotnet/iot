// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System.Runtime.CompilerServices;
using System.Threading;
using System.Threading.Tasks;

namespace System.Device.Gpio.Drivers;

/// <summary>
/// A GPIO driver for the Raspberry Pi 3 or 4, running Raspbian or Raspberry Pi OS (or, with some limitations, ubuntu)
/// </summary>
public class RaspberryPi3Driver : GpioDriver
{
    private GpioDriver _internalDriver;
    private RaspberryPi3LinuxDriver? _linuxDriver;

    /* private delegates for register Properties */
    private delegate void Set_Register(ulong value);
    private delegate ulong Get_Register();

    private readonly Set_Register _setSetRegister;
    private readonly Get_Register _getSetRegister;
    private readonly Set_Register _setClearRegister;
    private readonly Get_Register _getClearRegister;

    /// <summary>
    /// Used to set the Alternate Pin Mode on Raspberry Pi 3/4.
    /// The actual pin function for anything other than Input or Output is dependent
    /// on the pin and can be looked up in the Raspi manual.
    /// </summary>
    public enum AltMode
    {
        /// <summary>
        /// The mode is unknown
        /// </summary>
        Unknown,

        /// <summary>
        /// Gpio mode input
        /// </summary>
        Input,

        /// <summary>
        /// Gpio mode output
        /// </summary>
        Output,

        /// <summary>
        /// Mode ALT0
        /// </summary>
        Alt0,

        /// <summary>
        /// Mode ALT1
        /// </summary>
        Alt1,

        /// <summary>
        /// Mode ALT2
        /// </summary>
        Alt2,

        /// <summary>
        /// Mode ALT3
        /// </summary>
        Alt3,

        /// <summary>
        /// Mode ALT4
        /// </summary>
        Alt4,

        /// <summary>
        /// Mode ALT5
        /// </summary>
        Alt5,
    }

    /// <summary>
    /// Creates an instance of the RaspberryPi3Driver.
    /// This driver works on Raspberry 3 or 4, both on Linux and on Windows
    /// </summary>
    public RaspberryPi3Driver()
    {
        if (Environment.OSVersion.Platform == PlatformID.Unix)
        {
            _linuxDriver = CreateInternalRaspberryPi3LinuxDriver(out RaspberryBoardInfo boardInfo);

            if (_linuxDriver == null)
            {
                throw new PlatformNotSupportedException($"Not a supported Raspberry Pi type: {boardInfo.BoardModel} (0x{((int)boardInfo.BoardModel):X4})");
            }

            _setSetRegister = (value) => _linuxDriver.SetRegister = value;
            _setClearRegister = (value) => _linuxDriver.ClearRegister = value;
            _getSetRegister = () => _linuxDriver.SetRegister;
            _getClearRegister = () => _linuxDriver.ClearRegister;
            _internalDriver = _linuxDriver;
        }
        else
        {
            _internalDriver = CreateWindows10GpioDriver();
            _setSetRegister = (value) => throw new PlatformNotSupportedException();
            _setClearRegister = (value) => throw new PlatformNotSupportedException();
            _getSetRegister = () => throw new PlatformNotSupportedException();
            _getClearRegister = () => throw new PlatformNotSupportedException();
        }
    }

    internal RaspberryPi3Driver(RaspberryPi3LinuxDriver linuxDriver)
    {
        if (Environment.OSVersion.Platform == PlatformID.Unix)
        {
            _linuxDriver = linuxDriver;
            _setSetRegister = (value) => linuxDriver.SetRegister = value;
            _setClearRegister = (value) => linuxDriver.ClearRegister = value;
            _getSetRegister = () => linuxDriver.SetRegister;
            _getClearRegister = () => linuxDriver.ClearRegister;
            _internalDriver = linuxDriver;
        }
        else
        {
            throw new NotSupportedException("This ctor is for internal use only");
        }
    }

    /// <summary>
    /// True if the driver supports <see cref="SetAlternatePinMode"/> and <see cref="GetAlternatePinMode"/>.
    /// </summary>
    public bool AlternatePinModeSettingSupported => _linuxDriver != null;

    internal static RaspberryPi3LinuxDriver? CreateInternalRaspberryPi3LinuxDriver(out RaspberryBoardInfo boardInfo)
    {
        boardInfo = RaspberryBoardInfo.LoadBoardInfo();
        return boardInfo.BoardModel switch
        {
            RaspberryBoardInfo.Model.RaspberryPi3B or
            RaspberryBoardInfo.Model.RaspberryPi3APlus or
            RaspberryBoardInfo.Model.RaspberryPi3BPlus or
            RaspberryBoardInfo.Model.RaspberryPiZeroW or
            RaspberryBoardInfo.Model.RaspberryPiZero2W or
            RaspberryBoardInfo.Model.RaspberryPi4 or
            RaspberryBoardInfo.Model.RaspberryPi400 => new RaspberryPi3LinuxDriver(),
            RaspberryBoardInfo.Model.RaspberryPiComputeModule4 or
            RaspberryBoardInfo.Model.RaspberryPiComputeModule3 => new RaspberryPiCm3Driver(),
            _ => null,
        };
    }

    [MethodImpl(MethodImplOptions.NoInlining)]
    private static GpioDriver CreateWindows10GpioDriver()
    {
        throw new PlatformNotSupportedException();
    }

    private GpioDriver InternalDriver
    {
        get
        {
            if (_internalDriver == null)
            {
                throw new ObjectDisposedException("Driver is disposed");
            }

            return _internalDriver;
        }
    }

    /// <inheritdoc/>
    protected internal override int PinCount => InternalDriver.PinCount;

    /// <inheritdoc/>
    protected internal override void AddCallbackForPinValueChangedEvent(int pinNumber, PinEventTypes eventTypes, PinChangeEventHandler callback) => InternalDriver.AddCallbackForPinValueChangedEvent(pinNumber, eventTypes, callback);

    /// <inheritdoc/>
    protected internal override void ClosePin(int pinNumber) => InternalDriver.ClosePin(pinNumber);

    /// <inheritdoc/>
    protected internal override int ConvertPinNumberToLogicalNumberingScheme(int pinNumber)
    {
        return InternalDriver.ConvertPinNumberToLogicalNumberingScheme(pinNumber);
    }

    /// <inheritdoc/>
    protected internal override PinMode GetPinMode(int pinNumber) => InternalDriver.GetPinMode(pinNumber);

    /// <inheritdoc/>
    protected internal override bool IsPinModeSupported(int pinNumber, PinMode mode) => InternalDriver.IsPinModeSupported(pinNumber, mode);

    /// <inheritdoc/>
    protected internal override void OpenPin(int pinNumber) => InternalDriver.OpenPin(pinNumber);

    /// <inheritdoc/>
    protected internal override PinValue Read(int pinNumber) => InternalDriver.Read(pinNumber);

    /// <inheritdoc/>
    protected internal override void Toggle(int pinNumber) => InternalDriver.Toggle(pinNumber);

    /// <inheritdoc/>
    protected internal override void RemoveCallbackForPinValueChangedEvent(int pinNumber, PinChangeEventHandler callback) => InternalDriver.RemoveCallbackForPinValueChangedEvent(pinNumber, callback);

    /// <inheritdoc/>
    protected internal override void SetPinMode(int pinNumber, PinMode mode) => InternalDriver.SetPinMode(pinNumber, mode);

    /// <inheritdoc/>
    protected internal override void SetPinMode(int pinNumber, PinMode mode, PinValue initialValue) => InternalDriver.SetPinMode(pinNumber, mode, initialValue);

    /// <inheritdoc/>
    protected internal override WaitForEventResult WaitForEvent(int pinNumber, PinEventTypes eventTypes, CancellationToken cancellationToken) => InternalDriver.WaitForEvent(pinNumber, eventTypes, cancellationToken);

    /// <inheritdoc/>
    protected internal override ValueTask<WaitForEventResult> WaitForEventAsync(int pinNumber, PinEventTypes eventTypes, CancellationToken cancellationToken) => InternalDriver.WaitForEventAsync(pinNumber, eventTypes, cancellationToken);

    /// <inheritdoc/>
    protected internal override void Write(int pinNumber, PinValue value) => InternalDriver.Write(pinNumber, value);

    /// <summary>
    /// Retrieve the current alternate pin mode for a given logical pin.
    /// This works also with closed pins.
    /// </summary>
    /// <param name="pinNumber">Pin number in the logical scheme of the driver</param>
    /// <returns>Current pin mode</returns>
    public AltMode GetAlternatePinMode(int pinNumber)
    {
        if (_linuxDriver == null)
        {
            throw new NotSupportedException("This operation is not supported with the current driver.");
        }

        return _linuxDriver.GetAlternatePinMode(pinNumber);
    }

    /// <summary>
    /// Set the specified alternate mode for the given pin.
    /// Check the manual to know what each pin can do.
    /// </summary>
    /// <param name="pinNumber">Pin number in the logcal scheme of the driver</param>
    /// <param name="altPinMode">Alternate mode to set</param>
    /// <exception cref="NotSupportedException">This mode is not supported by this driver (or by the given pin)</exception>
    /// <remarks>The method is intended for usage by higher-level abstraction interfaces. User code should be very careful when using this method.</remarks>
    public void SetAlternatePinMode(int pinNumber, AltMode altPinMode)
    {
        if (_linuxDriver == null)
        {
            throw new NotSupportedException("This operation is not supported with the current driver.");
        }

        _linuxDriver.SetAlternatePinMode(pinNumber, altPinMode);
    }

    /// <summary>
    /// Allows directly setting the "Set pin high" register. Used for special applications only
    /// </summary>
    protected ulong SetRegister
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        get => _getSetRegister();
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        set => _setSetRegister(value);
    }

    /// <summary>
    /// Allows directly setting the "Set pin low" register. Used for special applications only
    /// </summary>
    protected ulong ClearRegister
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        get => _getClearRegister();
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        set => _setClearRegister(value);
    }

    /// <inheritdoc />
    protected override void Dispose(bool disposing)
    {
        _internalDriver?.Dispose();
        _internalDriver = null!;
        base.Dispose(disposing);
    }

    /// <inheritdoc />
    public override ComponentInformation QueryComponentInformation()
    {
        var ret = new ComponentInformation(this, "Generic Raspberry Pi Wrapper driver");
        ret.AddSubComponent(_internalDriver.QueryComponentInformation());
#pragma warning disable SDGPIO0001
        ret.Properties["ChipInfo"] = _internalDriver.GetChipInfo().ToString();
#pragma warning restore SDGPIO0001
        return ret;
    }
}
