using System;
using System.Collections.Generic;
using System.Device.Gpio;
using System.Linq;
using System.Text;

namespace Iot.Device.Arduino
{
    internal class SupportedPinConfiguration
    {
        public SupportedPinConfiguration(int pin)
        {
            Pin = pin;
            PinModes = new List<SupportedMode>();
            PwmResolutionBits = 0;
            AnalogInputResolutionBits = 1; // binary
            AnalogPinNumber = 127; // = Not an analog pin
        }

        public int Pin
        {
            get;
        }

        public List<SupportedMode> PinModes
        {
            get;
        }

        public int PwmResolutionBits
        {
            get;
            internal set;
        }

        /// <summary>
        /// This contains the resolution of an analog input channel, in bits
        /// </summary>
        public int AnalogInputResolutionBits
        {
            get;
            internal set;
        }

        /// <summary>
        /// This gets the number of the analog input pin, as commonly used by Arduino software
        /// </summary>
        public byte AnalogPinNumber
        {
            get;
            internal set;
        }

        public override string ToString()
        {
            string pinModes = String.Join(", ", PinModes);
            return $"{nameof(Pin)}: {Pin}, {nameof(PinModes)}: [{pinModes}], {nameof(PwmResolutionBits)}: {PwmResolutionBits}, {nameof(AnalogInputResolutionBits)}: {AnalogInputResolutionBits}, {nameof(AnalogPinNumber)}: {AnalogPinNumber}";
        }
    }
}
