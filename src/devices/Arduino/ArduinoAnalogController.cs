using System;
using System.Collections.Generic;
using System.Device.Analog;
using System.Device.Gpio;
using System.Linq;
using System.Text;
using System.Threading;

#pragma warning disable CS1591
namespace Iot.Device.Arduino
{
    internal class ArduinoAnalogController : AnalogController
    {
        private readonly ArduinoBoard _board;
        private readonly List<SupportedPinConfiguration> _supportedPinConfigurations;
        private readonly Dictionary<int, ValueChangedEventHandler> _callbacks;
        private int _firstAnalogPin;

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

        public override int ConvertPinNumberToLogicalNumberingScheme(int pinNumber)
        {
            return pinNumber - _firstAnalogPin;
        }

        public override int ConvertLogicalNumberingSchemeToPinNumber(int logicalPinNumber)
        {
            return logicalPinNumber + _firstAnalogPin;
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
            _board.Firmata.DisableAnalogReporting(ConvertPinNumberToLogicalNumberingScheme(pin.PinNumber));
        }

        protected override void Dispose(bool disposing)
        {
            _callbacks.Clear();
            base.Dispose(disposing);
        }
    }
}
