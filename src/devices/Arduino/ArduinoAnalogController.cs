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
            IReadOnlyList<SupportedPinConfiguration> supportedPinConfigurations, PinNumberingScheme scheme)
            : base(scheme)
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

        public override int ConvertPinNumberToLogicalNumberingScheme(int pinNumber)
        {
            int numberAnalogPinsFound = 0;
            for (int i = 0; i < _supportedPinConfigurations.Count; i++)
            {
                if (_supportedPinConfigurations[i].PinModes.Contains(SupportedMode.AnalogInput))
                {
                    numberAnalogPinsFound++;
                    if (pinNumber == i)
                    {
                        return numberAnalogPinsFound - 1;
                    }
                }
            }

            throw new InvalidOperationException($"Pin {pinNumber} is not a valid analog input pin.");
        }

        public override int ConvertLogicalNumberingSchemeToPinNumber(int logicalPinNumber)
        {
            int numberAnalogPinsFound = 0;
            for (int i = 0; i < _supportedPinConfigurations.Count; i++)
            {
                if (_supportedPinConfigurations[i].PinModes.Contains(SupportedMode.AnalogInput))
                {
                    numberAnalogPinsFound++;
                    if (logicalPinNumber == numberAnalogPinsFound - 1)
                    {
                        return i;
                    }
                }
            }

            throw new InvalidOperationException($"Pin A{logicalPinNumber} does not exist");
        }

        public override bool SupportsAnalogInput(int pinNumber)
        {
            return _supportedPinConfigurations[pinNumber].PinModes.Contains(SupportedMode.AnalogInput);
        }

        protected override AnalogInputPin OpenPinCore(int pinNumber)
        {
            // This method is called with the logical pin numbering (input pin A0 is 0, A1 is 1, etc)
            // but the SetPinMode method operates on the global numbers
            int fullNumber = ConvertLogicalNumberingSchemeToPinNumber(pinNumber);
            _board.Firmata.SetPinMode(fullNumber, SupportedMode.AnalogInput);
            _board.Firmata.EnableAnalogReporting(pinNumber);
            return new ArduinoAnalogInputPin(_board, this, _supportedPinConfigurations[fullNumber], pinNumber, VoltageReference);
        }

        public override void ClosePin(AnalogInputPin pin)
        {
            _board.Firmata.DisableAnalogReporting(pin.PinNumber);
        }
    }
}
