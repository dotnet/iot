// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System;
using System.Collections.Generic;
using System.Device.Analog;
using System.Device.Gpio;
using System.Text;
using UnitsNet;

namespace Iot.Device.Arduino
{
    internal class ArduinoAnalogInputPin : AnalogInputPin
    {
        private readonly SupportedPinConfiguration _configuration;
        private int _autoReportingReferenceCount;
        private ArduinoBoard _board;

        public ArduinoAnalogInputPin(ArduinoBoard board, AnalogController controller, SupportedPinConfiguration configuration,
            int pinNumber, ElectricPotential voltageReference)
            : base(controller, pinNumber, voltageReference)
        {
            _board = board;
            _configuration = configuration;
        }

        public override void EnableAnalogValueChangedEvent(GpioController? masterController, int masterPin)
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
            if (_autoReportingReferenceCount == 0)
            {
                throw new InvalidOperationException("Attempt to disable event when no events are connected");
            }

            _autoReportingReferenceCount -= 1;
            if (_autoReportingReferenceCount == 0)
            {
                _board.Firmata.AnalogPinValueUpdated -= FirmataOnAnalogPinValueUpdated;
            }
        }

        private void FirmataOnAnalogPinValueUpdated(int channel, uint rawvalue)
        {
            if (_autoReportingReferenceCount > 0)
            {
                int physicalPin = Controller.ConvertAnalogChannelNumberToPinNumber(channel);
                ElectricPotential voltage = ConvertToVoltage(rawvalue);
                var message = new ValueChangedEventArgs(rawvalue, voltage, physicalPin, TriggerReason.Timed);
                FireValueChanged(message);
            }
        }

        public override ElectricPotential MinVoltage => ElectricPotential.Zero;

        /// <summary>
        /// The arduino would theoretically allow for an external analog reference, but firmata currently doesn't support that.
        /// </summary>
        public override ElectricPotential MaxVoltage => VoltageReference;

        /// <summary>
        /// The ADC resolution for this pin, as reported by the firmware.
        /// </summary>
        public override int AdcResolutionBits => _configuration.AnalogInputResolutionBits;

        /// <summary>
        /// Reads the analog raw value from a given pin.
        /// </summary>
        /// <returns>The analog raw value of the given pin</returns>
        /// <remarks>
        /// This returns the last cached value from the board. The board sends regular updates when a value changes,
        /// but this does not request an update before returning a value, so that the read value might be incorrect
        /// if the analog pin has just been opened.
        /// </remarks>
        public override uint ReadRaw()
        {
            return _board.Firmata.GetAnalogRawValue(PinNumber);
        }
    }
}
