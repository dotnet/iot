// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using Iot.Device.GrovePiDevice.Models;
using System.Device.Gpio;

namespace Iot.Device.GrovePiDevice.Sensors
{
    public class Relay : DigitalOutput
    {
        /// <summary>
        /// Relay constructor
        /// </summary>
        /// <param name="grovePi">The GrovePi class</param>
        /// <param name="port">The grove Port, need to be in the list of SupportedPorts</param>
        public Relay(GrovePi grovePi, GrovePort port) : this(grovePi, port, false)
        { }

        /// <summary>
        /// Relay constructor
        /// </summary>
        /// <param name="grovePi">The GrovePi class</param>
        /// <param name="port">The grove Port, need to be in the list of SupportedPorts</param>
        /// <param name="inverted">If inverted, the relay is on when output is low and off when output is high</param>
        public Relay(GrovePi grovePi, GrovePort port, bool inverted) : base(grovePi, port)
        {
            IsInverted = inverted;
        }

        /// <summary>
        /// True when the relay is on
        /// </summary>
        public bool IsOn => (_value == PinValue.High) && !IsInverted;

        /// <summary>
        /// Switch on the relay
        /// </summary>
        public void On()
        {
            Value = PinValue.Low;
        }

        /// <summary>
        /// Switch off the relay
        /// </summary>
        public void Off()
        {
            Value = PinValue.High;
        }

        public bool IsInverted { get; internal set; }

        /// <summary>
        /// Get/set the state of the relay, 0 for off, 1 for on. Wehn set, anything not 0 will be considered as on
        /// </summary>
        public new PinValue Value
        {
            get { return _value; }
            set
            {
                _value = value;
                _grovePi.DigitalWrite(_port, ((_value == PinValue.High) && !IsInverted) ? PinValue.High : PinValue.Low);
            }
        }

        /// <summary>
        /// Get "On" when relay if on and "Off" when relay is off
        /// </summary>
        public override string ToString() => ((_value == PinValue.High) && !IsInverted) ? "On" : "Off";

        /// <summary>
        /// Get the name Relay
        /// </summary>
        public new string SensorName => "Relay";
    }
}
