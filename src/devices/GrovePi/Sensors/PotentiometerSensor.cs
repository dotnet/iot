using Iot.Device.GrovePiDevice.Models;
using System;
using System.Collections.Generic;
using System.Text;

namespace Iot.Device.GrovePiDevice.Sensors
{
    public class PotentiometerSensor : AnalogSensor
    {
        /// <summary>
        /// 
        /// </summary>
        /// <param name="goPiGo">The GoPiGo3 class</param>
        /// <param name="port">The grove Port, need to be in the list of SupportedPorts</param>
        public PotentiometerSensor(GrovePi grovePi, GrovePort port) : base(grovePi, port)
        { }

        /// <summary>
        /// Returns the value as a percent from 0 % to 100 %
        /// </summary>
        /// <returns>Returns the value as a percent from 0 % to 100 %</returns>
        public override string ToString() => $"{ValueAsPercent} %";

        /// <summary>
        /// Get the name Potentiometer Sensor
        /// </summary>
        public new string SensorName => "Potentiometer Sensor";
    }
}
