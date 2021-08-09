// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System;
using System.Device.Gpio;
using System.Linq;
using System.Threading;

namespace Iot.Device.Ft232H
{
    /// <summary>
    /// GPIO driver for the FT232H
    /// </summary>
    public class Ft232HGpio : GpioDriver
    {
        /// <summary>
        /// Store the FTDI Device Information
        /// </summary>
        public Ft232HDevice DeviceInformation { get; private set; }

        /// <inheritdoc/>
        protected override int PinCount => Ft232HDevice.PinCountConst;

        /// <summary>
        /// Creates a GPIO Driver
        /// </summary>
        /// <param name="deviceInformation">The FT232H device</param>
        internal Ft232HGpio(Ft232HDevice deviceInformation)
        {
            DeviceInformation = deviceInformation;
            // Open device
            DeviceInformation.GetHandle();
        }

        /// <inheritdoc/>
        protected override int ConvertPinNumberToLogicalNumberingScheme(int pinNumber) => pinNumber;

        /// <inheritdoc/>
        protected override void OpenPin(int pinNumber)
        {
            if ((pinNumber < 0) || (pinNumber >= PinCount))
            {
                throw new ArgumentException($"Pin number can only be between 0 and {PinCount - 1}");
            }

            if (DeviceInformation.IsI2cMode && (
                (pinNumber >= 0) && (pinNumber <= 2)))
            {
                throw new ArgumentException($"Can't open pin 0, 1 or 2 while I2C mode is on");
            }

            if (DeviceInformation.IsSpiMode && (
                (pinNumber >= 0) && (pinNumber <= 2)))
            {
                throw new ArgumentException($"Can't open pin 0, 1 or 2 while SPI mode is on");
            }

            if (DeviceInformation._connectionSettings.Where(m => m.ChipSelectLine == pinNumber).Any())
            {
                throw new ArgumentException($"Pin already open or used as Chip Select");
            }

            DeviceInformation._PinOpen[pinNumber] = true;
        }

        /// <inheritdoc/>
        protected override void ClosePin(int pinNumber)
        {
            DeviceInformation._PinOpen[pinNumber] = false;
        }

        /// <inheritdoc/>
        protected override void SetPinMode(int pinNumber, PinMode mode)
        {
            if (pinNumber < 8)
            {
                if (mode != PinMode.Output)
                {
                    byte mask = 0xFF;
                    mask &= (byte)(~(1 << pinNumber));
                    DeviceInformation._gpioLowDir &= mask;
                }
                else
                {
                    DeviceInformation._gpioLowDir |= (byte)(1 << pinNumber);
                }

                DeviceInformation.SetGpioValuesLow();
            }
            else
            {
                if (mode != PinMode.Output)
                {
                    byte mask = 0xFF;
                    mask &= (byte)(~(1 << (pinNumber - 8)));
                    DeviceInformation._gpioHighDir &= mask;
                }
                else
                {
                    DeviceInformation._gpioHighDir |= (byte)(1 << (pinNumber - 8));
                }

                DeviceInformation.SetGpioValuesHigh();
            }
        }

        /// <inheritdoc/>
        protected override PinMode GetPinMode(int pinNumber)
        {
            if (pinNumber < 8)
            {
                return ((DeviceInformation._gpioLowDir >> pinNumber) & 0x01) == 0x01 ? PinMode.Output : PinMode.Input;
            }

            return ((DeviceInformation._gpioHighDir >> (pinNumber - 8)) & 0x01) == 0x01 ? PinMode.Output : PinMode.Input;
        }

        /// <inheritdoc/>
        protected override bool IsPinModeSupported(int pinNumber, PinMode mode)
        {
            if ((mode == PinMode.InputPullDown) || (mode == PinMode.InputPullUp))
            {
                return false;
            }

            return true;
        }

        /// <inheritdoc/>
        protected override PinValue Read(int pinNumber)
        {
            if (pinNumber < 8)
            {
                var val = DeviceInformation.GetGpioValuesLow();
                return (((val >> pinNumber) & 0x01) == 0x01) ? PinValue.High : PinValue.Low;
            }
            else
            {
                var valhigh = DeviceInformation.GetGpioValuesHigh();
                return (((valhigh >> (pinNumber - 8)) & 0x01) == 0x01) ? PinValue.High : PinValue.Low;
            }
        }

        /// <inheritdoc/>
        protected override void Write(int pinNumber, PinValue value)
        {
            if (pinNumber < 8)
            {
                if (value == PinValue.High)
                {
                    DeviceInformation._gpioLowData |= (byte)(1 << pinNumber);
                }
                else
                {
                    byte mask = 0xFF;
                    mask &= (byte)(~(1 << pinNumber));
                    DeviceInformation._gpioLowData &= mask;
                }

                DeviceInformation.SetGpioValuesLow();
            }
            else
            {
                if (value == PinValue.High)
                {
                    DeviceInformation._gpioHighData |= (byte)(1 << (pinNumber - 8));
                }
                else
                {
                    byte mask = 0xFF;
                    mask &= (byte)(~(1 << (pinNumber - 8)));
                    DeviceInformation._gpioHighData &= mask;
                }

                DeviceInformation.SetGpioValuesHigh();
            }
        }

        /// <inheritdoc/>
        protected override WaitForEventResult WaitForEvent(int pinNumber, PinEventTypes eventTypes, CancellationToken cancellationToken)
        {
            throw new NotImplementedException();
        }

        /// <inheritdoc/>
        protected override void AddCallbackForPinValueChangedEvent(int pinNumber, PinEventTypes eventTypes, PinChangeEventHandler callback)
        {
            throw new NotImplementedException();
        }

        /// <inheritdoc/>
        protected override void RemoveCallbackForPinValueChangedEvent(int pinNumber, PinChangeEventHandler callback)
        {
            throw new NotImplementedException();
        }
    }
}
