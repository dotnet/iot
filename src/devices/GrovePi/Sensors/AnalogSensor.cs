using System;
using System.Collections.Generic;
using System.Text;
using Iot.Device.GrovePiDevice.Models;

namespace Iot.Device.GrovePiDevice.Sensors
{
    /// <summary>
    /// AnalogSensor is a generic analog sensor
    /// </summary>
    public class AnalogSensor : ISensor<int>
    {
        internal GrovePi _grovePi;

        /// <summary>
        /// On GrovePi, ADC is 1023
        /// </summary>
        public int MaxAdc => 1023;

        /// <summary>
        /// AnalogSensor constructor
        /// </summary>
        /// <param name="grovePi">The GrovePi class</param>
        /// <param name="port">The grove Port, need to be in the list of SupportedPorts</param>
        public AnalogSensor(GrovePi grovePi, GrovePort port)
        {
            if (!SupportedPorts.Contains(port))
                throw new ArgumentException($"Error: grove Port not supported");
            _grovePi = grovePi;
            Port = port;
            _grovePi.PinMode(Port, PinMode.Input);
        }

        /// <summary>
        /// Get the measurement from 0 to MaxAdc
        /// </summary>
        public int Value => _grovePi.AnalogRead(Port);

        /// <summary>
        /// Returns the measurement as a string
        /// </summary>
        /// <returns>Returns the measurement as a string</returns>
        public override string ToString() => _grovePi.AnalogRead(Port).ToString();

        /// <summary>
        /// Get the value as a percentage from 0 to 100
        /// </summary>
        public byte ValueAsPercent => (Value >= 0) ? (byte)(100 * Value / MaxAdc) : byte.MaxValue;

        /// <summary>
        /// Get the namme Analog Sensor
        /// </summary>
        public string SensorName => "Analog Sensor";

        /// <summary>
        /// grove sensor port
        /// </summary>
        public GrovePort Port { get; internal set; }

        /// <summary>
        /// Only Analogic ports are supported
        /// </summary>
        static public List<GrovePort> SupportedPorts => new List<GrovePort>()
        {
            GrovePort.AnalogPin0,
            GrovePort.AnalogPin1,
            GrovePort.AnalogPin2
        };
    }
}
