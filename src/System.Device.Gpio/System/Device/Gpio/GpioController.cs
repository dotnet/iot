// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using System.Collections.Generic;
using System.Device.Gpio.Drivers;
using System.Disposables;
using System.IO;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Win32;

namespace System.Device.Gpio
{
    /// <summary>
    /// Represents a general-purpose I/O (GPIO) controller.
    /// </summary>
    public sealed class GpioController : IGpioController
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

        /// <inheritdoc/>
        public PinNumberingScheme NumberingScheme { get; }

        /// <inheritdoc/>
        public int PinCount => _driver.PinCount;

        /// <summary>
        /// Gets the logical pin number in the controller's numbering scheme.
        /// </summary>
        /// <param name="pinNumber">The pin number in the controller's numbering scheme.</param>
        /// <returns>The logical pin number in the controller's numbering scheme.</returns>
        private int GetLogicalPinNumber(int pinNumber)
        {
            return (NumberingScheme == PinNumberingScheme.Logical) ? pinNumber : _driver.ConvertPinNumberToLogicalNumberingScheme(pinNumber);
        }

        /// <inheritdoc/>
        public void OpenPin(int pinNumber)
        {
            int logicalPinNumber = GetLogicalPinNumber(pinNumber);
            if (_openPins.Contains(logicalPinNumber))
            {
                throw new InvalidOperationException($"Pin {logicalPinNumber} is already open.");
            }

            _driver.OpenPin(logicalPinNumber);
            _openPins.Add(logicalPinNumber);
        }

        /// <inheritdoc/>
        public void OpenPin(int pinNumber, PinMode mode)
        {
            OpenPin(pinNumber);
            SetPinMode(pinNumber, mode);
        }

        /// <inheritdoc/>
        public void ClosePin(int pinNumber)
        {
            int logicalPinNumber = GetLogicalPinNumber(pinNumber);
            if (!_openPins.Contains(logicalPinNumber))
            {
                throw new InvalidOperationException($"Can not close pin {logicalPinNumber} because it is not open.");
            }

            _driver.ClosePin(logicalPinNumber);
            _openPins.Remove(logicalPinNumber);
        }

        /// <inheritdoc/>
        public void SetPinMode(int pinNumber, PinMode mode)
        {
            int logicalPinNumber = GetLogicalPinNumber(pinNumber);
            if (!_openPins.Contains(logicalPinNumber))
            {
                throw new InvalidOperationException($"Can not set a mode to pin {logicalPinNumber} because it is not open.");
            }

            if (!_driver.IsPinModeSupported(logicalPinNumber, mode))
            {
                throw new InvalidOperationException($"Pin {pinNumber} does not support mode {mode}.");
            }

            _driver.SetPinMode(logicalPinNumber, mode);
        }

        /// <inheritdoc/>
        public PinMode GetPinMode(int pinNumber)
        {
            int logicalPinNumber = GetLogicalPinNumber(pinNumber);
            if (!_openPins.Contains(logicalPinNumber))
            {
                throw new InvalidOperationException($"Can not get the mode of pin {logicalPinNumber} because it is not open.");
            }

            return _driver.GetPinMode(logicalPinNumber);
        }

        /// <inheritdoc/>
        public bool IsPinOpen(int pinNumber)
        {
            int logicalPinNumber = GetLogicalPinNumber(pinNumber);
            return _openPins.Contains(logicalPinNumber);
        }

        /// <inheritdoc/>
        public bool IsPinModeSupported(int pinNumber, PinMode mode)
        {
            int logicalPinNumber = GetLogicalPinNumber(pinNumber);
            return _driver.IsPinModeSupported(logicalPinNumber, mode);
        }

        /// <inheritdoc/>
        public PinValue Read(int pinNumber)
        {
            int logicalPinNumber = GetLogicalPinNumber(pinNumber);
            if (!_openPins.Contains(logicalPinNumber))
            {
                throw new InvalidOperationException($"Can not write to pin {logicalPinNumber} because it is not open.");
            }

            return _driver.Read(logicalPinNumber);
        }

        /// <inheritdoc/>
        public void Write(int pinNumber, PinValue value)
        {
            int logicalPinNumber = GetLogicalPinNumber(pinNumber);
            if (!_openPins.Contains(logicalPinNumber))
            {
                throw new InvalidOperationException($"Can not write to pin {logicalPinNumber} because it is not open.");
            }

            if (_driver.GetPinMode(logicalPinNumber) != PinMode.Output)
            {
                throw new InvalidOperationException($"Can not write to pin {logicalPinNumber} because it is not set to Output mode.");
            }

            _driver.Write(logicalPinNumber, value);
        }

        /// <inheritdoc/>
        public WaitForEventResult WaitForEvent(int pinNumber, PinEventTypes eventTypes, TimeSpan timeout)
        {
            using (CancellationTokenSource tokenSource = new CancellationTokenSource(timeout))
            {
                return WaitForEvent(pinNumber, eventTypes, tokenSource.Token);
            }
        }

        /// <inheritdoc/>
        public WaitForEventResult WaitForEvent(int pinNumber, PinEventTypes eventTypes, CancellationToken cancellationToken)
        {
            int logicalPinNumber = GetLogicalPinNumber(pinNumber);
            if (!_openPins.Contains(logicalPinNumber))
            {
                throw new InvalidOperationException($"Can not wait for events from pin {logicalPinNumber} because it is not open.");
            }

            return _driver.WaitForEvent(logicalPinNumber, eventTypes, cancellationToken);
        }

        /// <inheritdoc/>
        public async ValueTask<WaitForEventResult> WaitForEventAsync(int pinNumber, PinEventTypes eventTypes, TimeSpan timeout)
        {
            using (CancellationTokenSource tokenSource = new CancellationTokenSource(timeout))
            {
                return await WaitForEventAsync(pinNumber, eventTypes, tokenSource.Token).ConfigureAwait(false);
            }
        }

        /// <inheritdoc/>
        public ValueTask<WaitForEventResult> WaitForEventAsync(int pinNumber, PinEventTypes eventTypes, CancellationToken token)
        {
            int logicalPinNumber = GetLogicalPinNumber(pinNumber);
            if (!_openPins.Contains(logicalPinNumber))
            {
                throw new InvalidOperationException($"Can not wait for events from pin {logicalPinNumber} because it is not open.");
            }

            return _driver.WaitForEventAsync(logicalPinNumber, eventTypes, token);
        }

        /// <inheritdoc/>
        public void RegisterCallbackForPinValueChangedEvent(int pinNumber, PinEventTypes eventTypes, PinChangeEventHandler callback)
        {
            int logicalPinNumber = GetLogicalPinNumber(pinNumber);
            if (!_openPins.Contains(logicalPinNumber))
            {
                throw new InvalidOperationException($"Can not add callback for pin {logicalPinNumber} because it is not open.");
            }

            _driver.AddCallbackForPinValueChangedEvent(logicalPinNumber, eventTypes, callback);
        }

        /// <inheritdoc/>
        public void UnregisterCallbackForPinValueChangedEvent(int pinNumber, PinChangeEventHandler callback)
        {
            int logicalPinNumber = GetLogicalPinNumber(pinNumber);
            if (!_openPins.Contains(logicalPinNumber))
            {
                throw new InvalidOperationException($"Can not remove callback for pin {logicalPinNumber} because it is not open.");
            }

            _driver.RemoveCallbackForPinValueChangedEvent(logicalPinNumber, callback);
        }

        private void Dispose(bool disposing)
        {
            foreach (int pin in _openPins)
            {
                _driver.ClosePin(pin);
            }

            _openPins.Clear();
            _driver.Dispose();
        }

        /// <inheritdoc/>
        public void Dispose()
        {
            Dispose(true);
        }

        /// <inheritdoc/>
        public void Write(ReadOnlySpan<PinValuePair> pinValuePairs)
        {
            for (int i = 0; i < pinValuePairs.Length; i++)
            {
                Write(pinValuePairs[i].PinNumber, pinValuePairs[i].PinValue);
            }
        }

        /// <inheritdoc/>
        public void Read(Span<PinValuePair> pinValuePairs)
        {
            for (int i = 0; i < pinValuePairs.Length; i++)
            {
                int pin = pinValuePairs[i].PinNumber;
                pinValuePairs[i] = new PinValuePair(pin, Read(pin));
            }
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
            string[] cpuInfoLines = File.ReadAllLines(CpuInfoPath);
            Regex regex = new Regex(@"Hardware\s*:\s*(.*)");
            foreach (string cpuInfoLine in cpuInfoLines)
            {
                Match match = regex.Match(cpuInfoLine);
                if (match.Success)
                {
                    if (match.Groups.Count > 1)
                    {
                        if (match.Groups[1].Value == RaspberryPiHardware)
                        {
                            return new RaspberryPi3Driver();
                        }

                        // Commenting out as HummingBoard driver is not implemented yet, will be added back after implementation
                        // https://github.com/dotnet/iot/issues/76
                        // if (match.Groups[1].Value == HummingBoardHardware)
                        // {
                        //     return new HummingBoardDriver();
                        // }
                        return UnixDriver.Create();
                    }
                }
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
            string baseBoardProduct = Registry.LocalMachine.GetValue(BaseBoardProductRegistryValue, string.Empty).ToString();

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
