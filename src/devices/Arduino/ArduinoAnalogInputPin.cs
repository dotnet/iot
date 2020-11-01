// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using System;
using System.Collections.Generic;
using System.Device.Analog;
using System.Device.Gpio;
using System.Text;

namespace Iot.Device.Arduino
{
    internal class ArduinoAnalogInputPin : AnalogInputPin
    {
        private readonly SupportedPinConfiguration _configuration;
        private int _autoReportingReferenceCount;
        private ArduinoBoard _board;

        public ArduinoAnalogInputPin(ArduinoBoard board, AnalogController controller, SupportedPinConfiguration configuration,
            int pinNumber, double voltageReference)
            : base(controller, pinNumber, voltageReference)
        {
            _board = board;
            _configuration = configuration;
        }

        public override void EnableAnalogValueChangedEvent(GpioController masterController, int masterPin)
        {
            // The pin is already open, so analog reporting is enabled, we just need to forward it.
            if (_autoReportingReferenceCount == 0)
            {
                _board.Firmata.AnalogPinValueUpdated += FirmataOnAnalogPinValueUpdated;
            }

            _autoReportingReferenceCount += 1;
        }

        public override void DisableAnalogValueChangedEvent()
        {
            _autoReportingReferenceCount -= 1;
            if (_autoReportingReferenceCount == 0)
            {
                _board.Firmata.AnalogPinValueUpdated -= FirmataOnAnalogPinValueUpdated;
            }
        }

        private void FirmataOnAnalogPinValueUpdated(int pin, uint rawvalue)
        {
            if (_autoReportingReferenceCount > 0)
            {
                int physicalPin = Controller.ConvertLogicalNumberingSchemeToPinNumber(pin);
                double voltage = ConvertToVoltage(rawvalue);
                var message = new ValueChangedEventArgs(rawvalue, voltage, physicalPin, TriggerReason.Timed);
                FireValueChanged(message);
            }
        }

        public override void QueryResolution(out int numberOfBits, out double minVoltage, out double maxVoltage)
        {
            numberOfBits = _configuration.AnalogInputResolutionBits;
            minVoltage = 0.0;
            maxVoltage = VoltageReference;
        }

        public override uint ReadRaw()
        {
            return _board.Firmata.GetAnalogRawValue(PinNumber);
        }

        public override bool SupportsAnalogInput()
        {
            return _configuration.PinModes.Contains(SupportedMode.ANALOG_INPUT);
        }
    }
}
