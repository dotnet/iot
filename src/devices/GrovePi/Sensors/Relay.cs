using System;
using System.Collections.Generic;
using System.Text;
using Iot.Device.GrovePiDevice.Models;

namespace Iot.Device.GrovePiDevice.Sensors
{
    public class Relay : DigitalOutput
    {
        /// <summary>
        /// 
        /// </summary>
        /// <param name="grovePi">The GoPiGo3 class</param>
        /// <param name="port">The grove Port, need to be in the list of SupportedPorts</param>
        public Relay(GrovePi grovePi, GrovePort port) : this(grovePi, port, false)
        { }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="goPiGo">The GoPiGo3 class</param>
        /// <param name="port">The grove Port, need to be in the list of SupportedPorts</param>
        /// <param name="inverted">If inverted, the relay is on when output is low and off when output is high</param>
        public Relay(GrovePi grovePi, GrovePort port, bool inverted) : base(grovePi, port)
        {
            IsInverted = inverted;
        }

        /// <summary>
        /// Switch on the relay
        /// </summary>
        public void On()
        {
            Value = PinLevel.Low;
        }

        /// <summary>
        /// Switch off the relay
        /// </summary>
        public void Off()
        {
            Value = PinLevel.High;
        }

        public bool IsInverted { get; set; }

        /// <summary>
        /// Get/set the state of the relay, 0 for off, 1 for on. Wehn set, anything not 0 will be considered as on
        /// </summary>
        public new PinLevel Value
        {
            get { return _value; }
            set
            {
                _value = value;
                _grovePi.DigitalWrite(Port, ((_value == PinLevel.High) && !IsInverted) ? PinLevel.High : PinLevel.Low);
            }
        }

        /// <summary>
        /// Get "On" when relay if on and "Off" when relay is off
        /// </summary>
        public override string ToString() => (_value == PinLevel.High) ? "On" : "Off";

        /// <summary>
        /// Get the name Relay
        /// </summary>
        public new string SensorName => "Relay";
    }
}
