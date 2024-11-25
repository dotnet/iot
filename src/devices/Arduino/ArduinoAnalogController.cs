// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Device.Analog;
using System.Device.Gpio;
using System.Linq;
using System.Text;
using System.Threading;
using UnitsNet;

namespace Iot.Device.Arduino
{
    internal class ArduinoAnalogController : AnalogController
    {
        private readonly ArduinoBoard _board;
        private readonly IReadOnlyList<SupportedPinConfiguration> _supportedPinConfigurations;

        public ArduinoAnalogController(ArduinoBoard board,
            IReadOnlyList<SupportedPinConfiguration> supportedPinConfigurations)
        {
            _board = board ?? throw new ArgumentNullException(nameof(board));
            _supportedPinConfigurations = supportedPinConfigurations ?? throw new ArgumentNullException(nameof(supportedPinConfigurations));
            PinCount = _supportedPinConfigurations.Count;

            // Note: While the Arduino does have an external analog input reference pin, Firmata doesn't allow configuring it.
            VoltageReference = ElectricPotential.FromVolts(5.0);
        }

        public override int PinCount
        {
            get;
        }

        /// <inheritdoc />
        public override int ConvertPinNumberToAnalogChannelNumber(int pinNumber)
        {
            if (pinNumber >= 0 && pinNumber < _supportedPinConfigurations.Count)
            {
                int channel = _supportedPinConfigurations[pinNumber].AnalogPinNumber;
                if (channel != 127)
                {
                    return channel;
                }

                return -1;
            }

            throw new InvalidOperationException($"Pin {pinNumber} is not a valid input pin.");
        }

        /// <inheritdoc />
        public override int ConvertAnalogChannelNumberToPinNumber(int analogChannelNumber)
        {
            for (int i = 0; i < _supportedPinConfigurations.Count; i++)
            {
                if (_supportedPinConfigurations[i].AnalogPinNumber == analogChannelNumber)
                {
                    return i;
                }
            }

            return -1;
        }

        public override bool SupportsAnalogInput(int pinNumber)
        {
            if (pinNumber >= _supportedPinConfigurations.Count || pinNumber < 0)
            {
                return false;
            }

            return _supportedPinConfigurations[pinNumber].PinModes.Contains(SupportedMode.AnalogInput);
        }

        protected override AnalogInputPin OpenPinCore(int pinNumber)
        {
            _board.Firmata.SetPinMode(pinNumber, SupportedMode.AnalogInput);
            _board.Firmata.EnableAnalogReporting(pinNumber, ConvertPinNumberToAnalogChannelNumber(pinNumber));
            return new ArduinoAnalogInputPin(_board, this, _supportedPinConfigurations[pinNumber], pinNumber, VoltageReference);
        }

        public override void ClosePin(AnalogInputPin pin)
        {
            base.ClosePin(pin);
            _board.Firmata.DisableAnalogReporting(pin.PinNumber, ConvertPinNumberToAnalogChannelNumber(pin.PinNumber));
        }
    }
}
