// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using Iot.Device.GoPiGo3.Models;
using System;
using System.Collections.Generic;

namespace Iot.Device.GoPiGo3.Sensors
{
    /// <summary>
    /// A generic Analog Sensor class
    /// </summary>
    public class AnalogSensor : ISensor
    {
        internal GoPiGo _goPiGo;
        internal GroovePort _mode;

        /// <summary>
        /// Constructor for the generic Analog Sensor
        /// </summary>
        /// <param name="goPiGo">The GoPiGo3 class</param>
        /// <param name="port">The Groove Port, need to be in the list of SupportedPorts</param>
        public AnalogSensor(GoPiGo goPiGo, GroovePort port)
        {
            if (!SupportedPorts.Contains(port))
                throw new ArgumentException($"Error: Groove Port not supported");
            _goPiGo = goPiGo;
            Port = port;
            _goPiGo.SetGrooveType(port, GrooveSensorType.Custom);
            _goPiGo.SetGrooveMode(port, GrooveInputOutput.InputAnalog);
            _mode = Port == GroovePort.Groove1 ? GroovePort.Groove1Pin1 : GroovePort.Groove2Pin1;
        }

        /// <summary>
        /// Get the value of the sensor from 0 to MaxAdc
        /// </summary>
        public int Value => _goPiGo.GetGrooveAnalog(_mode);

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
        /// Get the Groove Port
        /// </summary>
        public GroovePort Port { get; internal set; }

        public List<GroovePort> SupportedPorts => new List<GroovePort>() { GroovePort.Groove1, GroovePort.Groove2 };

        /// <summary>
        /// Get the sensor name "Analog Sensor"
        /// </summary>
        public string SensorName => "Analog Sensor";

    }
}
