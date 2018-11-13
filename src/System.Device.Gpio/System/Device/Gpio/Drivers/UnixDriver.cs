// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace System.Device.Gpio.Drivers
{
    public class UnixDriver : GpioDriver
    {
        private const string _gpioBasePath = "/sys/class/gpio";
        private List<int> _exportedPins = new List<int>();

        protected internal override int PinCount => throw new PlatformNotSupportedException("This driver is generic so it can not enumerate how many pins are available.");

        protected internal override int ConvertPinNumberToLogicalNumberingScheme(int pinNumber) => throw new PlatformNotSupportedException("This driver is generic so it can not perform conversions between pin numbering schemes.");

        protected internal override void OpenPin(int pinNumber)
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
                catch (UnauthorizedAccessException e)
                {
                    // Wrapping the exception in order to get a better message
                    throw new UnauthorizedAccessException("Opening pins requires root permissions.", e);
                }
            }
        }

        protected internal override void ClosePin(int pinNumber)
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
                catch (UnauthorizedAccessException e)
                {
                    throw new UnauthorizedAccessException("Closing pins requires root permissions.", e);
                }
            }
        }

        protected internal override void SetPinMode(int pinNumber, PinMode mode)
        {
            if (mode == PinMode.InputPullDown || mode == PinMode.InputPullUp)
            {
                throw new PlatformNotSupportedException("This driver is generic so it does not support Input Pull Down or Input Pull Up modes.");
            }
            string directionPath = $"{_gpioBasePath}/gpio{pinNumber}/direction";
            string sysfsMode = ConvertPinModeToSysFsMode(mode);
            if (File.Exists(directionPath))
            {
                try
                {
                    File.WriteAllText(directionPath, sysfsMode);
                }
                catch (UnauthorizedAccessException e)
                {
                    throw new UnauthorizedAccessException("Setting a mode to a pin requires root permissions.", e);
                }
            }
            else
            {
                throw new InvalidOperationException("There was an attempt to set a mode to a pin that is not yet open.");
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
            throw new PlatformNotSupportedException($"{mode} is not supported by this driver.");
        }

        protected internal override PinValue Read(int pinNumber)
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
                catch (UnauthorizedAccessException e)
                {
                    throw new UnauthorizedAccessException("Reading a pin value requires root permissions.", e);
                }
            }
            else
            {
                throw new InvalidOperationException("There was an attempt to read from a pin that is not yet opened.");
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

        protected internal override void Write(int pinNumber, PinValue value)
        {
            string valuePath = $"{_gpioBasePath}/gpio{pinNumber}/value";
            if (File.Exists(valuePath))
            {
                try
                {
                    string sysFsValue = ConvertPinValueToSysFs(value);
                    File.WriteAllText(valuePath, sysFsValue);
                }
                catch (UnauthorizedAccessException e)
                {
                    throw new UnauthorizedAccessException("Reading a pin value requires root permissions.", e);
                }
            }
            else
            {
                throw new InvalidOperationException("There was an attempt to write to a pin that is not yet opened.");
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

        protected internal override bool IsPinModeSupported(int pinNumber, PinMode mode)
        {
            // Unix driver does not support pull up or pull down resistors.
            if (mode == PinMode.InputPullDown || mode == PinMode.InputPullUp)
                return false;
            return true;
        }

        protected internal override WaitForEventResult WaitForEvent(int pinNumber, PinEventTypes eventType, CancellationToken cancellationToken)
        {
            throw new NotImplementedException();
        }

        protected override void Dispose(bool disposing)
        {
            while (_exportedPins.Count > 0)
            {
                ClosePin(_exportedPins.FirstOrDefault());
            }
            base.Dispose(disposing);
        }

        protected internal override void AddCallbackForPinValueChangedEvent(int pinNumber, PinEventTypes eventType, PinChangeEventHandler callback)
        {
            throw new NotImplementedException();
        }

        protected internal override void RemoveCallbackForPinValueChangedEvent(int pinNumber, PinChangeEventHandler callback)
        {
            throw new NotImplementedException();
        }
    }
}
