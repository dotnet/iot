// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using System.Collections.Generic;
using System.IO;
using System.Runtime.InteropServices;
using System.Text.RegularExpressions;

namespace System.Device.Gpio
{
    /// <summary>
    /// Main class to interact with when trying to control Gpio pins.
    /// </summary>
    public sealed class GpioController : IDisposable
    {
        /// <summary>
        /// Private enum describing the type of board the program is running on.
        /// </summary>
        private enum BoardType
        {
            RaspberryPi3,
            Hummingboard,
            Unknown
        }
        private IGpioDriver _driver;
        private HashSet<int> _openPins;
        private const string _cpuInfoPath = "/proc/cpuinfo";
        private const string _raspberryPiHardware = "BCM2835";
        private const string _hummingBoardHardware = @"Freescale i.MX6 Quad/DualLite (Device Tree)";

        /// <summary>
        /// The numbering scheme used to identify pins on the board.
        /// </summary>
        public PinNumberingScheme NumberingScheme { get; }

        /// <summary>
        /// Hiding the controller to implement the factory creation pattern.
        /// </summary>
        private GpioController() { }

        /// <summary>
        /// Constructor that takes in a driver and a numbering scheme. Hidden in order to implement the factory pattern.
        /// </summary>
        /// <param name="driver">The driver to be used for pin operations.</param>
        /// <param name="numberingScheme">The numbering scheme used to identify pins on the board.</param>
        private GpioController(IGpioDriver driver, PinNumberingScheme numberingScheme)
        {
            _driver = driver;
            NumberingScheme = numberingScheme;
            _openPins = new HashSet<int>();
        }

        /// <summary>
        /// Default factory method. It will default to the Logical numbering scheme and it will predict which driver is the best for the board you are running in.
        /// </summary>
        /// <returns>A GpioController instance</returns>
        public static GpioController GetController()
        {
            return GetController(PinNumberingScheme.Logical);
        }

        /// <summary>
        /// Factory method that takes in a numbering scheme. It will predict which driver is the best for the board you are running in.
        /// </summary>
        /// <param name="numberingScheme">The numbering scheme used to identify pins on the board.</param>
        /// <returns>A GpioController instance</returns>
        public static GpioController GetController(PinNumberingScheme numberingScheme)
        {
            IGpioDriver driver;
            if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
            {
                driver = new Windows10Driver();
            }
            else
            {
                BoardType boardType = GetBoardHardware();
                switch (boardType)
                {
                    case BoardType.RaspberryPi3:
                        driver = new RaspberryPi3Driver();
                        break;
                    case BoardType.Hummingboard:
                        driver = new HummingboardDriver();
                        break;
                    case BoardType.Unknown:
                    default:
                        driver = new UnixDriver();
                        break;
                }
            }
            return GetController(driver, numberingScheme);
        }

        /// <summary>
        /// Factory method that takes in a numbering scheme and the driver to be used.
        /// </summary>
        /// <param name="driver">The driver to be used for pin operations.</param>
        /// <param name="numberingScheme">The numbering scheme used to identify pins on the board.</param>
        /// <returns>A GpioController instance</returns>
        public static GpioController GetController(IGpioDriver driver, PinNumberingScheme numberingScheme)
        {
            return new GpioController(driver, numberingScheme);
        }

        /// <summary>
        /// Private method that parses /proc/cpuinfo file to try to check the board you are running in.
        /// </summary>
        /// <returns>The board type in which the program is running in.</returns>
        private static BoardType GetBoardHardware()
        {
            string[] cpuInfoLines = File.ReadAllLines(_cpuInfoPath);
            Regex regex = new Regex(@"Hardware\s*:\s*(.*)");
            foreach (string cpuInfoLine in cpuInfoLines)
            {
                Match match = regex.Match(cpuInfoLine);
                if (match.Success)
                {
                    if (match.Groups.Count > 1)
                    {
                        if (match.Groups[1].Value == _raspberryPiHardware)
                        {
                            return BoardType.RaspberryPi3;
                        }
                        if (match.Groups[1].Value == _hummingBoardHardware)
                        {
                            return BoardType.Hummingboard;
                        }
                        return BoardType.Unknown;
                    }
                }
            }
            return BoardType.Unknown;
        }

        /// <summary>
        /// Returns the number of pins on the board
        /// </summary>
        public int PinCount
        {
            get
            {
                return _driver.PinCount;
            }
        }

        /// <summary>
        /// Converts a pin number specified on the controller's pin numbering into the logical numbering that the driver understands
        /// </summary>
        /// <param name="pinNumber">The pin number using the controller's numbering scheme.</param>
        /// <returns>The pin number using the logical numbering.</returns>
        private int ConvertPinNumberToLogicalNumberingScheme(int pinNumber)
        {
            return _driver.ConvertPinNumberToLogicalNumberingScheme(pinNumber);
        }

        /// <summary>
        /// Opens a pin in order for it to be ready to use.
        /// </summary>
        /// <param name="pinNumber">The pin number in the controller's numbering scheme.</param>
        public void OpenPin(int pinNumber)
        {
            int logicalPinNumber = (NumberingScheme == PinNumberingScheme.Logical) ? pinNumber : ConvertPinNumberToLogicalNumberingScheme(pinNumber);
            if (_openPins.Contains(logicalPinNumber))
            {
                throw new GpioException("The selected pin is already open.");
            }
            _driver.OpenPin(logicalPinNumber);
            _openPins.Add(logicalPinNumber);
        }

        /// <summary>
        /// Opens a pin and sets it to a specific mode
        /// </summary>
        /// <param name="pinNumber">The pin number in the controller's numbering scheme.</param>
        /// <param name="mode">The mode to be set</param>
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
            int logicalPinNumber = (NumberingScheme == PinNumberingScheme.Logical) ? pinNumber : ConvertPinNumberToLogicalNumberingScheme(pinNumber);
            if (!_openPins.Contains(logicalPinNumber))
            {
                throw new GpioException("Can not close a pin that is not yet opened.");
            }
            _driver.ClosePin(pinNumber);
        }

        /// <summary>
        /// Sets a pin mode to an open pin.
        /// </summary>
        /// <param name="pinNumber">The pin number in the controller's numbering scheme.</param>
        /// <param name="mode">The mode to be set.</param>
        public void SetPinMode(int pinNumber, PinMode mode)
        {
            int logicalPinNumber = (NumberingScheme == PinNumberingScheme.Logical) ? pinNumber : ConvertPinNumberToLogicalNumberingScheme(pinNumber);
            if (!_openPins.Contains(logicalPinNumber))
            {
                throw new GpioException("Can not set a mode to a pin that is not yet opened.");
            }
            if (!isPinModeSupported(pinNumber, mode))
            {
                throw new GpioException("The pin does not support the mode selected.");
            }
            _driver.SetPinMode(pinNumber, mode);
        }

        /// <summary>
        /// Reads the current value of a pin
        /// </summary>
        /// <param name="pinNumber">The pin number in the controller's numbering scheme.</param>
        /// <returns>The Pin value of the pin.</returns>
        public PinValue Read(int pinNumber)
        {
            int logicalPinNumber = (NumberingScheme == PinNumberingScheme.Logical) ? pinNumber : ConvertPinNumberToLogicalNumberingScheme(pinNumber);
            if (!_openPins.Contains(logicalPinNumber))
            {
                throw new GpioException("Can not read from a pin that is not yet opened.");
            }
            return _driver.Read(pinNumber);
        }

        /// <summary>
        /// Writes a value to a pin.
        /// </summary>
        /// <param name="pinNumber">The pin number in the controller's numbering scheme.</param>
        /// <param name="value">The value to be written.</param>
        public void Write(int pinNumber, PinValue value)
        {
            int logicalPinNumber = (NumberingScheme == PinNumberingScheme.Logical) ? pinNumber : ConvertPinNumberToLogicalNumberingScheme(pinNumber);
            if (!_openPins.Contains(logicalPinNumber))
            {
                throw new GpioException("Can not write to a pin that is not yet opened.");
            }
            _driver.Write(pinNumber, value);
        }

        /// <summary>
        /// Blocks execution for a period of time(timeout) or until an event of type eventType is received.
        /// </summary>
        /// <param name="pinNumber">The pin number in the controller's numbering scheme.</param>
        /// <param name="eventType">The event types to listen for.</param>
        /// <param name="timeout">The time to wait for the event.</param>
        /// <returns>A structure that contains the result of the waiting operation.</returns>
        public WaitForEventResult WaitForEvent(int pinNumber, PinEventTypes eventType, int timeout)
        {
            int logicalPinNumber = (NumberingScheme == PinNumberingScheme.Logical) ? pinNumber : ConvertPinNumberToLogicalNumberingScheme(pinNumber);
            if (!_openPins.Contains(logicalPinNumber))
            {
                throw new GpioException("Can not wait for events from a pin that is not yet opened.");
            }
            return _driver.WaitForEvent(logicalPinNumber, eventType, timeout);
        }

        /// <summary>
        /// Checks if a specific pin is open.
        /// </summary>
        /// <param name="pinNumber">The pin number in the controller's numbering scheme.</param>
        /// <returns>True if the pin is open. False otherwise.</returns>
        public bool isPinOpen(int pinNumber)
        {
            int logicalPinNumber = (NumberingScheme == PinNumberingScheme.Logical) ? pinNumber : ConvertPinNumberToLogicalNumberingScheme(pinNumber);
            return _openPins.Contains(logicalPinNumber);
        }

        /// <summary>
        /// Checks if a pinMode is supported by a pin.
        /// </summary>
        /// <param name="pinNumber">The pin number in the controller's numbering scheme.</param>
        /// <param name="mode">The mode to check</param>
        /// <returns>True if the mode is supported by the pin. False otherwise.</returns>
        public bool isPinModeSupported(int pinNumber, PinMode mode)
        {
            int logicalPinNumber = (NumberingScheme == PinNumberingScheme.Logical) ? pinNumber : ConvertPinNumberToLogicalNumberingScheme(pinNumber);
            return _driver.isPinModeSupported(logicalPinNumber, mode);
        }

        private void Dispose(bool disposing)
        {
            foreach (int pin in _openPins)
            {
                _driver.ClosePin(pin);
            }
            _driver.Dispose();
        }

        public void Dispose()
        {
            Dispose(true);
        }

        ~GpioController()
        {
            Dispose(false);
        }
    }
}
