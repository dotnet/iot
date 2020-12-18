// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System;
using System.Collections.Generic;
using System.Device.Gpio;
using System.Linq;
using System.Text;

namespace Iot.Device.Arduino
{
    /// <summary>
    /// Describes the capabilities of a pin
    /// </summary>
    public class SupportedPinConfiguration
    {
        internal SupportedPinConfiguration(int pin)
        {
            Pin = pin;
            PinModes = new List<SupportedMode>();
            PwmResolutionBits = 0;
            AnalogInputResolutionBits = 1; // binary
            AnalogPinNumber = 127; // = Not an analog pin
        }

        /// <summary>
        /// The pin number
        /// </summary>
        public int Pin
        {
            get;
        }

        /// <summary>
        /// The list of supported modes for this pin
        /// </summary>
        public List<SupportedMode> PinModes
        {
            get;
        }

        /// <summary>
        /// The width of the PWM register, typical value is 10 (the maximum value is then 1023)
        /// </summary>
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

        /// <inheritdoc />
        public override string ToString()
        {
            string pinModes = String.Join(", ", PinModes);
            return $"{nameof(Pin)}: {Pin}, {nameof(PinModes)}: [{pinModes}], {nameof(PwmResolutionBits)}: {PwmResolutionBits}, {nameof(AnalogInputResolutionBits)}: {AnalogInputResolutionBits}, {nameof(AnalogPinNumber)}: {AnalogPinNumber}";
        }
    }
}
