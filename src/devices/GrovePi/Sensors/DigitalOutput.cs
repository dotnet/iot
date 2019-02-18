using Iot.Device.GrovePiDevice.Models;
using System;
using System.Collections.Generic;
using System.Text;

namespace Iot.Device.GrovePiDevice.Sensors
{
    /// <summary>
    /// DigitalOutput class is a generic output class
    /// </summary>
    public class DigitalOutput : ISensor<PinLevel>
    {
        internal GrovePi _grovePi;
        internal PinLevel _value;

        /// <summary>
        /// DigitalOutput constructor
        /// </summary>
        /// <param name="grovePi">The GrovePi class</param>
        /// <param name="port">The grove Port, need to be in the list of SupportedPorts</param>
        public DigitalOutput(GrovePi grovePi, GrovePort port)
        {
            if (!SupportedPorts.Contains(port))
                throw new ArgumentException($"Error: grove Port not supported");
            _grovePi = grovePi;
            Port = port;
            _grovePi.PinMode(Port, PinMode.Output);
            Value = 0;
        }

        /// <summary>
        /// Get/Set the pin level to either high, either low
        /// </summary>
        public PinLevel Value
        {
            get { return _value; }

            set
            {
                _value = value;
                _grovePi.DigitalWrite(Port, _value);
            }
        }

        /// <summary>
        /// Returns the pin level as a string
        /// </summary>
        /// <returns>Returns the pin level as a string</returns>
        public override string ToString() => Value.ToString();

        /// <summary>
        /// Get the name Digital Output
        /// </summary>
        public string SensorName => "Digital Output";

        /// <summary>
        /// grove sensor port
        /// </summary>
        public GrovePort Port { get; internal set; }

        /// <summary>
        /// Only Digital ports including the analogic sensors (A0 = D14, A1 = D15, A2 = D16)
        /// </summary>
        static public List<GrovePort> SupportedPorts => new List<GrovePort>()
        {
            GrovePort.DigitalPin2,
            GrovePort.DigitalPin3,
            GrovePort.DigitalPin4,
            GrovePort.DigitalPin5,
            GrovePort.DigitalPin6,
            GrovePort.DigitalPin7,
            GrovePort.DigitalPin8,
            GrovePort.DigitalPin14,
            GrovePort.DigitalPin15,
            GrovePort.DigitalPin16
        };
    }
}
