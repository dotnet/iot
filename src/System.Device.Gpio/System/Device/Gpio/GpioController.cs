// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System.Collections.Generic;
using System.Device.Gpio.Drivers;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Win32;

namespace System.Device.Gpio
{
    /// <summary>
    /// Represents a general-purpose I/O (GPIO) controller.
    /// </summary>
    public class GpioController : IDisposable
    {
        // Constants used to check the hardware on linux
        private const string CpuInfoPath = "/proc/cpuinfo";
        private const string RaspberryPiHardware = "BCM2835";

        // Constants used to check the hardware on Windows
        private const string BaseBoardProductRegistryValue = @"SYSTEM\HardwareConfig\Current\BaseBoardProduct";
        private const string RaspberryPi2Product = "Raspberry Pi 2";
        private const string RaspberryPi3Product = "Raspberry Pi 3";

        private const string HummingBoardProduct = "HummingBoard-Edge";
        private const string HummingBoardHardware = @"Freescale i.MX6 Quad/DualLite (Device Tree)";

        private readonly GpioDriver _driver;
        private readonly HashSet<int> _openPins;

        /// <summary>
        /// Initializes a new instance of the <see cref="GpioController"/> class that will use the logical pin numbering scheme as default.
        /// </summary>
        public GpioController()
            : this(PinNumberingScheme.Logical)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="GpioController"/> class that will use the specified numbering scheme and driver.
        /// </summary>
        /// <param name="numberingScheme">The numbering scheme used to represent pins provided by the controller.</param>
        /// <param name="driver">The driver that manages all of the pin operations for the controller.</param>
        public GpioController(PinNumberingScheme numberingScheme, GpioDriver driver)
        {
            _driver = driver;
            NumberingScheme = numberingScheme;
            _openPins = new HashSet<int>();
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="GpioController"/> class that will use the specified numbering scheme.
        /// The controller will default to use the driver that best applies given the platform the program is executing on.
        /// </summary>
        /// <param name="numberingScheme">The numbering scheme used to represent pins provided by the controller.</param>
        public GpioController(PinNumberingScheme numberingScheme)
            : this(numberingScheme, GetBestDriverForBoard())
        {
        }

        /// <summary>
        /// The numbering scheme used to represent pins provided by the controller.
        /// </summary>
        public PinNumberingScheme NumberingScheme { get; }

        /// <summary>
        /// The number of pins provided by the controller.
        /// </summary>
        public virtual int PinCount => _driver.PinCount;

        /// <summary>
        /// Gets the logical pin number in the controller's numbering scheme.
        /// </summary>
        /// <param name="pinNumber">The pin number</param>
        /// <returns>The logical pin number in the controller's numbering scheme.</returns>
        protected virtual int GetLogicalPinNumber(int pinNumber)
        {
            return (NumberingScheme == PinNumberingScheme.Logical) ? pinNumber : _driver.ConvertPinNumberToLogicalNumberingScheme(pinNumber);
        }

        /// <summary>
        /// Opens a pin in order for it to be ready to use.
        /// </summary>
        /// <param name="pinNumber">The pin number in the controller's numbering scheme.</param>
        public void OpenPin(int pinNumber)
        {
            if (IsPinOpen(pinNumber))
            {
                throw new InvalidOperationException($"Pin {pinNumber} is already open.");
            }

            OpenPinCore(pinNumber);
            _openPins.Add(pinNumber);
        }

        /// <summary>
        /// Opens a pin in order for it to be ready to use.
        /// </summary>
        /// <param name="pinNumber">The pin number in the controller's numbering scheme.</param>
        protected virtual void OpenPinCore(int pinNumber)
        {
            int logicalPinNumber = GetLogicalPinNumber(pinNumber);
            _driver.OpenPin(logicalPinNumber);
        }

        /// <summary>
        /// Opens a pin and sets it to a specific mode.
        /// </summary>
        /// <param name="pinNumber">The pin number in the controller's numbering scheme.</param>
        /// <param name="mode">The mode to be set.</param>
        public void OpenPin(int pinNumber, PinMode mode)
        {
            OpenPin(pinNumber);
            SetPinMode(pinNumber, mode);
        }

        /// <summary>
        /// Closes an open pin.
        /// </summary>
        /// <param name="pinNumber">The pin number in the controller's numbering scheme.</param>
        public void ClosePin(int pinNumber)
        {
            if (!IsPinOpen(pinNumber))
            {
                throw new InvalidOperationException($"Can not close pin {pinNumber} because it is not open.");
            }

            ClosePinCore(pinNumber);
            _openPins.Remove(pinNumber);
        }

        /// <summary>
        /// Closes an open pin.
        /// </summary>
        /// <param name="pinNumber">The pin number in the controller's numbering scheme.</param>
        protected virtual void ClosePinCore(int pinNumber)
        {
            int logicalPinNumber = GetLogicalPinNumber(pinNumber);
            _driver.ClosePin(logicalPinNumber);
        }

        /// <summary>
        /// Sets the mode to a pin.
        /// </summary>
        /// <param name="pinNumber">The pin number in the controller's numbering scheme.</param>
        /// <param name="mode">The mode to be set.</param>
        public virtual void SetPinMode(int pinNumber, PinMode mode)
        {
            if (!IsPinOpen(pinNumber))
            {
                throw new InvalidOperationException($"Can not set a mode to pin {pinNumber} because it is not open.");
            }

            int logicalPinNumber = GetLogicalPinNumber(pinNumber);
            if (!IsPinModeSupported(pinNumber, mode))
            {
                throw new InvalidOperationException($"Pin {pinNumber} does not support mode {mode}.");
            }

            _driver.SetPinMode(logicalPinNumber, mode);
        }

        /// <summary>
        /// Gets the mode of a pin.
        /// </summary>
        /// <param name="pinNumber">The pin number in the controller's numbering scheme.</param>
        /// <returns>The mode of the pin.</returns>
        public virtual PinMode GetPinMode(int pinNumber)
        {
            if (!IsPinOpen(pinNumber))
            {
                throw new InvalidOperationException($"Can not get the mode of pin {pinNumber} because it is not open.");
            }

            int logicalPinNumber = GetLogicalPinNumber(pinNumber);
            return _driver.GetPinMode(logicalPinNumber);
        }

        /// <summary>
        /// Checks if a specific pin is open.
        /// </summary>
        /// <param name="pinNumber">The pin number in the controller's numbering scheme.</param>
        /// <returns>The status if the pin is open or closed.</returns>
        public virtual bool IsPinOpen(int pinNumber)
        {
            return _openPins.Contains(pinNumber);
        }

        /// <summary>
        /// Checks if a pin supports a specific mode.
        /// </summary>
        /// <param name="pinNumber">The pin number in the controller's numbering scheme.</param>
        /// <param name="mode">The mode to check.</param>
        /// <returns>The status if the pin supports the mode.</returns>
        public virtual bool IsPinModeSupported(int pinNumber, PinMode mode)
        {
            int logicalPinNumber = GetLogicalPinNumber(pinNumber);
            return _driver.IsPinModeSupported(logicalPinNumber, mode);
        }

        /// <summary>
        /// Reads the current value of a pin.
        /// </summary>
        /// <param name="pinNumber">The pin number in the controller's numbering scheme.</param>
        /// <returns>The value of the pin.</returns>
        public virtual PinValue Read(int pinNumber)
        {
            if (!IsPinOpen(pinNumber))
            {
                throw new InvalidOperationException($"Can not read from pin {pinNumber} because it is not open.");
            }

            int logicalPinNumber = GetLogicalPinNumber(pinNumber);
            return _driver.Read(logicalPinNumber);
        }

        /// <summary>
        /// Writes a value to a pin.
        /// </summary>
        /// <param name="pinNumber">The pin number in the controller's numbering scheme.</param>
        /// <param name="value">The value to be written to the pin.</param>
        public virtual void Write(int pinNumber, PinValue value)
        {
            if (!IsPinOpen(pinNumber))
            {
                throw new InvalidOperationException($"Can not write to pin {pinNumber} because it is not open.");
            }

            int logicalPinNumber = GetLogicalPinNumber(pinNumber);
            if (_driver.GetPinMode(logicalPinNumber) != PinMode.Output)
            {
                throw new InvalidOperationException($"Can not write to pin {logicalPinNumber} because it is not set to Output mode.");
            }

            _driver.Write(logicalPinNumber, value);
        }

        /// <summary>
        /// Blocks execution until an event of type eventType is received or a period of time has expired.
        /// </summary>
        /// <param name="pinNumber">The pin number in the controller's numbering scheme.</param>
        /// <param name="eventTypes">The event types to wait for.</param>
        /// <param name="timeout">The time to wait for the event.</param>
        /// <returns>A structure that contains the result of the waiting operation.</returns>
        public WaitForEventResult WaitForEvent(int pinNumber, PinEventTypes eventTypes, TimeSpan timeout)
        {
            using CancellationTokenSource tokenSource = new CancellationTokenSource(timeout);
            return WaitForEvent(pinNumber, eventTypes, tokenSource.Token);
        }

        /// <summary>
        /// Blocks execution until an event of type eventType is received or a cancellation is requested.
        /// </summary>
        /// <param name="pinNumber">The pin number in the controller's numbering scheme.</param>
        /// <param name="eventTypes">The event types to wait for.</param>
        /// <param name="cancellationToken">The cancellation token of when the operation should stop waiting for an event.</param>
        /// <returns>A structure that contains the result of the waiting operation.</returns>
        public virtual WaitForEventResult WaitForEvent(int pinNumber, PinEventTypes eventTypes, CancellationToken cancellationToken)
        {
            if (!IsPinOpen(pinNumber))
            {
                throw new InvalidOperationException($"Can not wait for events from pin {pinNumber} because it is not open.");
            }

            int logicalPinNumber = GetLogicalPinNumber(pinNumber);
            return _driver.WaitForEvent(logicalPinNumber, eventTypes, cancellationToken);
        }

        /// <summary>
        /// Async call to wait until an event of type eventType is received or a period of time has expired.
        /// </summary>
        /// <param name="pinNumber">The pin number in the controller's numbering scheme.</param>
        /// <param name="eventTypes">The event types to wait for.</param>
        /// <param name="timeout">The time to wait for the event.</param>
        /// <returns>A task representing the operation of getting the structure that contains the result of the waiting operation.</returns>
        public async ValueTask<WaitForEventResult> WaitForEventAsync(int pinNumber, PinEventTypes eventTypes, TimeSpan timeout)
        {
            using CancellationTokenSource tokenSource = new CancellationTokenSource(timeout);
            return await WaitForEventAsync(pinNumber, eventTypes, tokenSource.Token).ConfigureAwait(false);
        }

        /// <summary>
        /// Async call until an event of type eventType is received or a cancellation is requested.
        /// </summary>
        /// <param name="pinNumber">The pin number in the controller's numbering scheme.</param>
        /// <param name="eventTypes">The event types to wait for.</param>
        /// <param name="token">The cancellation token of when the operation should stop waiting for an event.</param>
        /// <returns>A task representing the operation of getting the structure that contains the result of the waiting operation</returns>
        public virtual ValueTask<WaitForEventResult> WaitForEventAsync(int pinNumber, PinEventTypes eventTypes, CancellationToken token)
        {
            if (!IsPinOpen(pinNumber))
            {
                throw new InvalidOperationException($"Can not wait for events from pin {pinNumber} because it is not open.");
            }

            int logicalPinNumber = GetLogicalPinNumber(pinNumber);
            return _driver.WaitForEventAsync(logicalPinNumber, eventTypes, token);
        }

        /// <summary>
        /// Adds a callback that will be invoked when pinNumber has an event of type eventType.
        /// </summary>
        /// <param name="pinNumber">The pin number in the controller's numbering scheme.</param>
        /// <param name="eventTypes">The event types to wait for.</param>
        /// <param name="callback">The callback method that will be invoked.</param>
        public virtual void RegisterCallbackForPinValueChangedEvent(int pinNumber, PinEventTypes eventTypes, PinChangeEventHandler callback)
        {
            if (!IsPinOpen(pinNumber))
            {
                throw new InvalidOperationException($"Can not add callback for pin {pinNumber} because it is not open.");
            }

            int logicalPinNumber = GetLogicalPinNumber(pinNumber);
            _driver.AddCallbackForPinValueChangedEvent(logicalPinNumber, eventTypes, callback);
        }

        /// <summary>
        /// Removes a callback that was being invoked for pin at pinNumber.
        /// </summary>
        /// <param name="pinNumber">The pin number in the controller's numbering scheme.</param>
        /// <param name="callback">The callback method that will be invoked.</param>
        public virtual void UnregisterCallbackForPinValueChangedEvent(int pinNumber, PinChangeEventHandler callback)
        {
            if (!IsPinOpen(pinNumber))
            {
                throw new InvalidOperationException($"Can not remove callback for pin {pinNumber} because it is not open.");
            }

            int logicalPinNumber = GetLogicalPinNumber(pinNumber);
            _driver.RemoveCallbackForPinValueChangedEvent(logicalPinNumber, callback);
        }

        /// <summary>
        /// Disposes this instance and closes all open pins associated with this controller.
        /// </summary>
        /// <param name="disposing">True to dispose all instances, false to dispose only unmanaged resources</param>
        protected virtual void Dispose(bool disposing)
        {
            foreach (int pin in _openPins)
            {
                // The list contains the pin in the current NumberingScheme
                ClosePinCore(pin);
            }

            _openPins.Clear();
            _driver.Dispose();
        }

        /// <inheritdoc/>
        public void Dispose()
        {
            Dispose(true);
        }

        /// <summary>
        /// Write the given pins with the given values.
        /// </summary>
        /// <param name="pinValuePairs">The pin/value pairs to write.</param>
        public void Write(ReadOnlySpan<PinValuePair> pinValuePairs)
        {
            for (int i = 0; i < pinValuePairs.Length; i++)
            {
                Write(pinValuePairs[i].PinNumber, pinValuePairs[i].PinValue);
            }
        }

        /// <summary>
        /// Read the given pins with the given pin numbers.
        /// </summary>
        /// <param name="pinValuePairs">The pin/value pairs to read.</param>
        public void Read(Span<PinValuePair> pinValuePairs)
        {
            for (int i = 0; i < pinValuePairs.Length; i++)
            {
                int pin = pinValuePairs[i].PinNumber;
                pinValuePairs[i] = new PinValuePair(pin, Read(pin));
            }
        }

        /// <summary>
        /// Tries to create the GPIO driver that best matches the current hardware
        /// </summary>
        /// <returns>An instance of a GpioDriver that best matches the current hardware</returns>
        /// <exception cref="PlatformNotSupportedException">No matching driver could be found</exception>
        private static GpioDriver GetBestDriverForBoard()
        {
            if (Environment.OSVersion.Platform == PlatformID.Win32NT)
            {
                return GetBestDriverForBoardOnWindows();
            }
            else
            {
                return GetBestDriverForBoardOnLinux();
            }
        }

        /// <summary>
        /// Attempt to get the best applicable driver for the board the program is executing on.
        /// </summary>
        /// <returns>A driver that works with the board the program is executing on.</returns>
        private static GpioDriver GetBestDriverForBoardOnLinux()
        {
            RaspberryPi3LinuxDriver? internalDriver = RaspberryPi3Driver.CreateInternalRaspberryPi3LinuxDriver(out _);

            if (internalDriver is object)
            {
                return new RaspberryPi3Driver(internalDriver);
            }

            return UnixDriver.Create();
        }

        /// <summary>
        /// Attempt to get the best applicable driver for the board the program is executing on.
        /// </summary>
        /// <returns>A driver that works with the board the program is executing on.</returns>
        /// <remarks>
        ///     This really feels like it needs a driver-based pattern, where each driver exposes a static method:
        ///     public static bool IsSpecificToCurrentEnvironment { get; }
        ///     The GpioController could use reflection to find all GpioDriver-derived classes and call this
        ///     static method to determine if the driver considers itself to be the best match for the environment.
        /// </remarks>
        private static GpioDriver GetBestDriverForBoardOnWindows()
        {
#pragma warning disable CA1416 // Registry.LocalMachine is only supported on Windows, but we will only hit this method if we are on Windows.
            string? baseBoardProduct = Registry.LocalMachine.GetValue(BaseBoardProductRegistryValue, string.Empty)?.ToString();
#pragma warning restore CA1416

            if (baseBoardProduct is null)
            {
                throw new Exception("Single board computer type cannot be detected.");
            }

            if (baseBoardProduct == RaspberryPi3Product || baseBoardProduct.StartsWith($"{RaspberryPi3Product} ") ||
                baseBoardProduct == RaspberryPi2Product || baseBoardProduct.StartsWith($"{RaspberryPi2Product} "))
            {
                return new RaspberryPi3Driver();
            }

            if (baseBoardProduct == HummingBoardProduct || baseBoardProduct.StartsWith($"{HummingBoardProduct} "))
            {
                return new HummingBoardDriver();
            }

            // Default for Windows IoT Core on a non-specific device
            return new Windows10Driver();
        }
    }
}
