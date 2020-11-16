// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using Iot.Device.GoPiGo3.Models;

namespace Iot.Device.GoPiGo3.Sensors
{
    /// <summary>
    /// Relay class to support relays
    /// </summary>
    public class Relay : DigitalOutput
    {
        /// <summary>
        /// Creates a new instance of <see cref="Relay"/>.
        /// </summary>
        /// <param name="goPiGo">The GoPiGo3 class</param>
        /// <param name="port">The Grove Port, need to be in the list of SupportedPorts</param>
        public Relay(GoPiGo goPiGo, GrovePort port)
            : this(goPiGo, port, false)
        {
        }

        /// <summary>
        /// Creates a new instance of <see cref="Relay"/>.
        /// </summary>
        /// <param name="goPiGo">The GoPiGo3 class</param>
        /// <param name="port">The Grove Port, need to be in the list of SupportedPorts</param>
        /// <param name="inverted">If inverted, the relay is on when output is low and off when output is high</param>
        public Relay(GoPiGo goPiGo, GrovePort port, bool inverted)
            : base(goPiGo, port)
        {
            IsInverted = inverted;
        }

        /// <summary>
        /// Switch on the relay
        /// </summary>
        public void On()
        {
            Value = 1;
        }

        /// <summary>
        /// Switch off the relay
        /// </summary>
        public void Off()
        {
            Value = 0;
        }

        /// <summary>
        /// If inverted, the relay is on when output is low and off when output is high
        /// </summary>
        public bool IsInverted { get; set; }

        /// <summary>
        /// Get/set the state of the relay, 0 for off, 1 for on. Wehn set, anything not 0 will be considered as on
        /// </summary>
        public new int Value
        {
            get
            {
                return _value ? 1 : 0;
            }

            set
            {
                _value = value != 0;
                _goPiGo.SetGroveState(_mode, _value && !IsInverted);
            }
        }

        /// <summary>
        /// Get "On" when relay if on and "Off" when relay is off
        /// </summary>
        public override string ToString() => _value ? "On" : "Off";

        /// <summary>
        /// Get the sensor name "Relay"
        /// </summary>
        public new string SensorName => "Relay";
    }
}
