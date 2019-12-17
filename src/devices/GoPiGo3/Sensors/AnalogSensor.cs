// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using System;
using System.Collections.Generic;
using Iot.Device.GoPiGo3.Models;

namespace Iot.Device.GoPiGo3.Sensors
{
    /// <summary>
    /// A generic Analog Sensor class
    /// </summary>
    public class AnalogSensor : ISensor
    {
        internal readonly GrovePort _mode;
        internal GoPiGo _goPiGo;

        /// <summary>
        /// Constructor for the generic Analog Sensor
        /// </summary>
        /// <param name="goPiGo">The GoPiGo3 class</param>
        /// <param name="port">The Grove Port, need to be in the list of SupportedPorts</param>
        public AnalogSensor(GoPiGo goPiGo, GrovePort port)
        {
            if (!SupportedPorts.Contains(port))
            {
                throw new ArgumentException($"Error: Grove Port not supported");
            }

            _goPiGo = goPiGo;
            Port = port;
            _goPiGo.SetGroveType(port, GroveSensorType.Custom);
            _goPiGo.SetGroveMode(port, GroveInputOutput.InputAnalog);
            _mode = Port == GrovePort.Grove1 ? GrovePort.Grove1Pin1 : GrovePort.Grove2Pin1;
        }

        /// <summary>
        /// Get the value of the sensor from 0 to MaxAdc
        /// </summary>
        public int Value => _goPiGo.GetGroveAnalog(_mode);

        /// <summary>
        /// The maximum ADC value 4095 for GoPiGo3
        /// </summary>
        public int MaxAdc => 4095;

        /// <summary>
        /// Get the read value as a percentage from 0 to 100
        /// </summary>
        public byte ValueAsPercent => (Value >= 0) ? (byte)(100 * Value / MaxAdc) : byte.MaxValue;

        /// <summary>
        /// Get the value as a string
        /// </summary>
        public override string ToString() => Value.ToString();

        /// <summary>
        /// Get the Grove Port
        /// </summary>
        public GrovePort Port { get; internal set; }

        /// <summary>
        /// List the supported Grove ports for the sensor
        /// </summary>
        public static List<GrovePort> SupportedPorts => new List<GrovePort>() { GrovePort.Grove1, GrovePort.Grove2 };

        /// <summary>
        /// Get the sensor name "Analog Sensor"
        /// </summary>
        public string SensorName => "Analog Sensor";

    }
}
