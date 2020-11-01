// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using System;
using System.Collections.Generic;
using System.Device.Analog;
using System.Device.Gpio;
using System.Linq;
using System.Text;
using System.Threading;

namespace Iot.Device.Arduino
{
    internal class ArduinoAnalogController : AnalogController
    {
        private readonly ArduinoBoard _board;
        private readonly List<SupportedPinConfiguration> _supportedPinConfigurations;
        private readonly Dictionary<int, ValueChangedEventHandler> _callbacks;

        public ArduinoAnalogController(ArduinoBoard board,
            List<SupportedPinConfiguration> supportedPinConfigurations, PinNumberingScheme scheme)
            : base(scheme)
        {
            _board = board ?? throw new ArgumentNullException(nameof(board));
            _supportedPinConfigurations = supportedPinConfigurations ?? throw new ArgumentNullException(nameof(supportedPinConfigurations));
            _callbacks = new Dictionary<int, ValueChangedEventHandler>();
            PinCount = _supportedPinConfigurations.Count;

            // Note: While the Arduino does have an external analog input reference pin, Firmata doesn't allow configuring it.
            VoltageReference = 5.0;
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
                if (_supportedPinConfigurations[i].PinModes.Contains(SupportedMode.ANALOG_INPUT))
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
                if (_supportedPinConfigurations[i].PinModes.Contains(SupportedMode.ANALOG_INPUT))
                {
                    numberAnalogPinsFound++;
                    if (logicalPinNumber == numberAnalogPinsFound - 1)
                    {
                        return i;
                    }
                }
            }

            throw new InvalidOperationException($"Pin A{logicalPinNumber} is not existing");
        }

        public override bool SupportsAnalogInput(int pinNumber)
        {
            return _supportedPinConfigurations[pinNumber].PinModes.Contains(SupportedMode.ANALOG_INPUT);
        }

        protected override AnalogInputPin OpenPinInternal(int pinNumber)
        {
            // This method is called with the logical pin numbering (input pin A0 is 0, A1 is 1, etc)
            // but the SetPinMode method operates on the global numbers
            int fullNumber = ConvertLogicalNumberingSchemeToPinNumber(pinNumber);
            _board.Firmata.SetPinMode(fullNumber, SupportedMode.ANALOG_INPUT);
            _board.Firmata.EnableAnalogReporting(pinNumber);
            return new ArduinoAnalogInputPin(_board, this, _supportedPinConfigurations[fullNumber], pinNumber, VoltageReference);
        }

        public override void Close(AnalogInputPin pin)
        {
            _board.Firmata.DisableAnalogReporting(pin.PinNumber);
        }

        protected override void Dispose(bool disposing)
        {
            _callbacks.Clear();
            base.Dispose(disposing);
        }
    }
}
