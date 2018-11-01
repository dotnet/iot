// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using System.Collections.Generic;
using System.Linq;
using System.IO;

namespace System.Device.Gpio
{
    internal class UnixDriver : IGpioDriver
    {
        private const string _gpioBasePath = "/sys/class/gpio";
        private List<int> _exportedPins = new List<int>();

        public int PinCount => throw new PlatformNotSupportedException("This driver is generic so it can not enumerate how many pins are available.");

        public int ConvertPinNumberToLogicalNumberingScheme(int pinNumber) => throw new PlatformNotSupportedException("This driver is generic so it can not perform conversions between pin numbering schemes.");

        public void OpenPin(int pinNumber)
        {
            string pinPath = $"{_gpioBasePath}/gpio{pinNumber}";
            // If the directory exists, this becomes a no-op since the pin might have been opened already by the some controller or somebody else.
            if (!Directory.Exists(pinPath))
            {
                try
                {
                    File.WriteAllText(Path.Combine(_gpioBasePath, "export"), pinNumber.ToString());
                    _exportedPins.Add(pinNumber);
                }
                catch (UnauthorizedAccessException)
                {
                    throw new GpioException("Opening pins requires root permissions.");
                }
            }
        }

        public void ClosePin(int pinNumber)
        {
            string pinPath = $"{_gpioBasePath}/gpio{pinNumber}";
            // If the directory doesn't exist, this becomes a no-op since the pin was closed already.
            if (Directory.Exists(pinPath))
            {
                try
                {
                    File.WriteAllText(Path.Combine(_gpioBasePath, "unexport"), pinNumber.ToString());
                    _exportedPins.Remove(pinNumber);
                }
                catch (UnauthorizedAccessException)
                {
                    throw new GpioException("Closing pins requires root permissions.");
                }
            }
        }

        public void SetPinMode(int pinNumber, PinMode mode)
        {
            if (mode == PinMode.InputPullDown || mode == PinMode.InputPullUp)
            {
                throw new GpioException("This driver is generic so it does not support Input Pull Down or Input Pull Up modes.");
            }
            string directionPath = $"{_gpioBasePath}/gpio{pinNumber}/direction";
            string sysfsMode = ConvertPinModeToSysFsMode(mode);
            if (File.Exists(directionPath))
            {
                try
                {
                    File.WriteAllText(directionPath, sysfsMode);
                }
                catch (UnauthorizedAccessException)
                {
                    throw new GpioException("Setting a mode to a pin requires root permissions.");
                }
            }
            else
            {
                throw new GpioException("There was an attempt to set a mode to a pin that is not yet open.");
            }
        }

        private string ConvertPinModeToSysFsMode(PinMode mode)
        {
            if (mode == PinMode.Input)
            {
                return "in";
            }
            if (mode == PinMode.Output)
            {
                return "out";
            }
            throw new GpioException($"{mode.ToString()} is not supported by this driver.");
        }

        public PinValue Read(int pinNumber)
        {
            PinValue result = default(PinValue);
            string valuePath = $"{_gpioBasePath}/gpio{pinNumber}/value";
            if (File.Exists(valuePath))
            {
                try
                {
                    string valueContents = File.ReadAllText(valuePath);
                    result = ConvertSysFsValueToPinValue(valueContents);
                }
                catch (UnauthorizedAccessException)
                {
                    throw new GpioException("Reading a pin value requires root permissions.");
                }
            }
            else
            {
                throw new GpioException("There was an attempt to read from a pin that is not yet opened.");
            }
            return result;
        }

        private PinValue ConvertSysFsValueToPinValue(string value)
        {
            PinValue result;
            value = value.Trim();

            switch (value)
            {
                case "0":
                    result = PinValue.Low;
                    break;
                case "1":
                    result = PinValue.High;
                    break;
                default:
                    throw new ArgumentException($"Invalid Gpio pin value {value}");
            }

            return result;
        }

        public void Write(int pinNumber, PinValue value)
        {
            string valuePath = $"{_gpioBasePath}/gpio{pinNumber}/value";
            if (File.Exists(valuePath))
            {
                try
                {
                    string sysFsValue = ConvertPinValueToSysFs(value);
                    File.WriteAllText(valuePath, sysFsValue);
                }
                catch (UnauthorizedAccessException)
                {
                    throw new GpioException("Reading a pin value requires root permissions.");
                }
            }
            else
            {
                throw new GpioException("There was an attempt to write to a pin that is not yet opened.");
            }
        }

        private string ConvertPinValueToSysFs(PinValue value)
        {
            string result = string.Empty;
            switch (value)
            {
                case PinValue.High:
                    result = "1";
                    break;
                case PinValue.Low:
                    result = "0";
                    break;
                default:
                    throw new ArgumentException($"Invalid pin value {value}");
            }
            return result;
        }

        public bool isPinModeSupported(int pinNumber, PinMode mode)
        {
            // Unix driver does not support pull up or pull down resistors.
            if (mode == PinMode.InputPullDown || mode == PinMode.InputPullUp)
                return false;
            return true;
        }

        public WaitForEventResult WaitForEvent(int pinNumber, PinEventTypes eventType, int timeout)
        {
            throw new NotImplementedException();
        }

        public void Dispose()
        {
            while (_exportedPins.Count > 0)
            {
                ClosePin(_exportedPins.FirstOrDefault());
            }
        }
    }
}
