// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System;
using System.Collections.Generic;
using System.Device;
using System.Device.Gpio;
using System.Globalization;
using System.Linq;
using System.Security.Cryptography;
using System.Threading;
using Iot.Device.FtCommon;

namespace Iot.Device.Ft232H
{
    /// <summary>
    /// GPIO driver for the FT232H
    /// </summary>
    public class Ft232HGpio : GpioDriver
    {
        private readonly Dictionary<int, PinValue> _pinValues = new Dictionary<int, PinValue>();

        /// <summary>
        /// Store the FTDI Device Information
        /// </summary>
        public Ftx232HDevice DeviceInformation { get; private set; }

        /// <inheritdoc/>
        protected override int PinCount => DeviceInformation.PinCount;

        /// <summary>
        /// Creates a GPIO Driver
        /// </summary>
        /// <param name="deviceInformation">The FT232H device</param>
        internal Ft232HGpio(Ftx232HDevice deviceInformation)
        {
            DeviceInformation = deviceInformation;
            // Open device
            DeviceInformation.GetHandle();
            DeviceInformation.InitializeGpio();
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

            // Pin 0, 1 and 2 are used by I2C
            if (DeviceInformation.IsI2cModeEnabled && (
                (pinNumber >= 0) && (pinNumber <= 2)))
            {
                throw new ArgumentException($"Can't open pin 0, 1 or 2 while I2C mode is on");
            }

            // Pin 0, 1 and 2 are used by SPI
            if (DeviceInformation.IsSpiModeEnabled && (
                (pinNumber >= 0) && (pinNumber <= 2)))
            {
                throw new ArgumentException($"Can't open pin 0, 1 or 2 while SPI mode is on");
            }

            if (DeviceInformation.ConnectionSettings.Where(m => m.ChipSelectLine == pinNumber).Any())
            {
                throw new ArgumentException($"Pin already open or used as Chip Select");
            }

            DeviceInformation.PinOpen[pinNumber] = true;
            _pinValues.TryAdd(pinNumber, PinValue.Low);
        }

        /// <inheritdoc/>
        protected override void ClosePin(int pinNumber)
        {
            DeviceInformation.PinOpen[pinNumber] = false;
            _pinValues.Remove(pinNumber);
        }

        /// <inheritdoc/>
        protected override void SetPinMode(int pinNumber, PinMode mode)
        {
            if ((pinNumber < 0) || (pinNumber >= PinCount))
            {
                throw new ArgumentException($"Pin number can only be between 0 and {PinCount - 1}");
            }

            if (pinNumber < 8)
            {
                if (mode != PinMode.Output)
                {
                    byte mask = 0xFF;
                    mask &= (byte)(~(1 << pinNumber));
                    DeviceInformation.GpioLowDir &= mask;
                }
                else
                {
                    DeviceInformation.GpioLowDir |= (byte)(1 << pinNumber);
                }

                DeviceInformation.SetGpioValuesLow();
            }
            else
            {
                if (mode != PinMode.Output)
                {
                    byte mask = 0xFF;
                    mask &= (byte)(~(1 << (pinNumber - 8)));
                    DeviceInformation.GpioHighDir &= mask;
                }
                else
                {
                    DeviceInformation.GpioHighDir |= (byte)(1 << (pinNumber - 8));
                }

                DeviceInformation.SetGpioValuesHigh();
            }
        }

        /// <inheritdoc/>
        protected override PinMode GetPinMode(int pinNumber)
        {
            if ((pinNumber < 0) || (pinNumber >= PinCount))
            {
                throw new ArgumentException($"Pin number can only be between 0 and {PinCount - 1}");
            }

            if (pinNumber < 8)
            {
                return ((DeviceInformation.GpioLowDir >> pinNumber) & 0x01) == 0x01 ? PinMode.Output : PinMode.Input;
            }

            return ((DeviceInformation.GpioHighDir >> (pinNumber - 8)) & 0x01) == 0x01 ? PinMode.Output : PinMode.Input;
        }

        /// <inheritdoc/>
        protected override bool IsPinModeSupported(int pinNumber, PinMode mode)
        {
            return mode == PinMode.Input || mode == PinMode.Output;
        }

        /// <inheritdoc/>
        protected override PinValue Read(int pinNumber)
        {
            if ((pinNumber < 0) || (pinNumber >= PinCount))
            {
                throw new ArgumentException($"Pin number can only be between 0 and {PinCount - 1}");
            }

            if (pinNumber < 8)
            {
                var val = DeviceInformation.GetGpioValuesLow();
                _pinValues[pinNumber] = (((val >> pinNumber) & 0x01) == 0x01) ? PinValue.High : PinValue.Low;
            }
            else
            {
                var valhigh = DeviceInformation.GetGpioValuesHigh();
                _pinValues[pinNumber] = (((valhigh >> (pinNumber - 8)) & 0x01) == 0x01) ? PinValue.High : PinValue.Low;
            }

            return _pinValues[pinNumber];
        }

        /// <inheritdoc/>
        protected override void Toggle(int pinNumber) => Write(pinNumber, !_pinValues[pinNumber]);

        /// <inheritdoc/>
        protected override void Write(int pinNumber, PinValue value)
        {
            if ((pinNumber < 0) || (pinNumber >= PinCount))
            {
                throw new ArgumentException($"Pin number can only be between 0 and {PinCount - 1}");
            }

            if (pinNumber < 8)
            {
                if (value == PinValue.High)
                {
                    DeviceInformation.GpioLowData |= (byte)(1 << pinNumber);
                }
                else
                {
                    byte mask = 0xFF;
                    mask &= (byte)(~(1 << pinNumber));
                    DeviceInformation.GpioLowData &= mask;
                }

                DeviceInformation.SetGpioValuesLow();
            }
            else
            {
                if (value == PinValue.High)
                {
                    DeviceInformation.GpioHighData |= (byte)(1 << (pinNumber - 8));
                }
                else
                {
                    byte mask = 0xFF;
                    mask &= (byte)(~(1 << (pinNumber - 8)));
                    DeviceInformation.GpioHighData &= mask;
                }

                DeviceInformation.SetGpioValuesHigh();
            }

            _pinValues[pinNumber] = value;
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

        /// <inheritdoc />
        public override ComponentInformation QueryComponentInformation()
        {
            var ret = base.QueryComponentInformation();
            ret.Properties["Description"] = DeviceInformation.Description;
            ret.Properties["SerialNumber"] = DeviceInformation.SerialNumber;
            ret.Properties["Channel"] = DeviceInformation.Channel.ToString();
            return ret;
        }
    }
}
