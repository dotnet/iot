using System;
using System.Collections.Generic;
using System.Device.Analog;
using System.Device.Gpio;
using System.Linq;
using System.Text;
using System.Threading;

namespace Iot.Device.Arduino
{
    internal class ArduinoAnalogControllerDriver : AnalogControllerDriver
    {
        private readonly ArduinoBoard _board;
        private readonly List<SupportedPinConfiguration> _supportedPinConfigurations;
        private readonly Dictionary<int, ValueChangeEventHandler> _callbacks;
        private int _autoReportingReferenceCount;
        private int _firstAnalogPin;

        public ArduinoAnalogControllerDriver(ArduinoBoard board,
            List<SupportedPinConfiguration> supportedPinConfigurations)
        {
            _board = board ?? throw new ArgumentNullException(nameof(board));
            _supportedPinConfigurations = supportedPinConfigurations ?? throw new ArgumentNullException(nameof(supportedPinConfigurations));
            _callbacks = new Dictionary<int, ValueChangeEventHandler>();
            _autoReportingReferenceCount = 0;
            PinCount = _supportedPinConfigurations.Count;
            VoltageReference = 5.0;
            // Number of the first analog pin. Serves for converting between logical A0-based pin numbers and digital pin numbers.
            // The value of this is 14 for most arduinos.
            var firstPin = _supportedPinConfigurations.FirstOrDefault(x => x.PinModes.Contains(SupportedMode.ANALOG_INPUT));
            if (firstPin != null)
            {
                _firstAnalogPin = firstPin.Pin;
            }
            else
            {
                _firstAnalogPin = 0;
            }
        }

        public override int PinCount
        {
            get;
        }

        protected override int ConvertPinNumberToLogicalNumberingScheme(int pinNumber)
        {
            return pinNumber - _firstAnalogPin;
        }

        protected override int ConvertLogicalNumberingSchemeToPinNumber(int logicalPinNumber)
        {
            return logicalPinNumber + _firstAnalogPin;
        }

        public override bool SupportsAnalogInput(int pinNumber)
        {
            return _supportedPinConfigurations[pinNumber].PinModes.Contains(SupportedMode.ANALOG_INPUT);
        }

        public override void EnableAnalogValueChangedEvent(int pinNumber, GpioController masterController, int masterPin)
        {
            // The pin is already open, so analog reporting is enabled, we just need to forward it.
            if (_autoReportingReferenceCount == 0)
            {
                _board.Firmata.AnalogPinValueUpdated += FirmataOnAnalogPinValueUpdated;
            }

            _autoReportingReferenceCount += 1;
        }

        public override void DisableAnalogValueChangedEvent(int pinNumber)
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
                int physicalPin = ConvertLogicalNumberingSchemeToPinNumber(pin);
                double voltage = ConvertToVoltage(physicalPin, rawvalue);
                var message = new ValueChangedEventArgs(rawvalue, voltage, physicalPin, TriggerReason.Timed);
                FireValueChanged(message);
            }
        }

        public override void OpenPin(int pinNumber)
        {
            if (!_supportedPinConfigurations[pinNumber].PinModes.Contains(SupportedMode.ANALOG_INPUT))
            {
                throw new NotSupportedException($"Pin {pinNumber} does not support Analog input");
            }

            _board.Firmata.SetPinMode(pinNumber, SupportedMode.ANALOG_INPUT);
            _board.Firmata.EnableAnalogReporting(ConvertPinNumberToLogicalNumberingScheme(pinNumber));
        }

        public override void ClosePin(int pinNumber)
        {
            _board.Firmata.DisableAnalogReporting(ConvertPinNumberToLogicalNumberingScheme(pinNumber));
        }

        /// <summary>
        /// Return the resolution of an analog input pin.
        /// </summary>
        /// <param name="pinNumber">The pin number</param>
        /// <param name="numberOfBits">Returns the resolution of the ADC in number of bits, including the sign bit (if applicable)</param>
        /// <param name="minVoltage">Minimum measurable voltage</param>
        /// <param name="maxVoltage">Maximum measurable voltage</param>
        public override void QueryResolution(int pinNumber, out int numberOfBits, out double minVoltage, out double maxVoltage)
        {
            numberOfBits = _supportedPinConfigurations[pinNumber].AnalogInputResolutionBits;
            minVoltage = 0.0;
            maxVoltage = VoltageReference;
        }

        public override uint ReadRaw(int pinNumber)
        {
            return _board.Firmata.GetAnalogRawValue(ConvertPinNumberToLogicalNumberingScheme(pinNumber));
        }

        protected override void Dispose(bool disposing)
        {
            _callbacks.Clear();
            base.Dispose(disposing);
        }
    }
}
