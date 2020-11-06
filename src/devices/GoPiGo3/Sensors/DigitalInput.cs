// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System;
using System.Collections.Generic;
using Iot.Device.GoPiGo3.Models;

namespace Iot.Device.GoPiGo3.Sensors
{
    /// <summary>
    /// DigitalInput class is a generic class to support digital input sensors
    /// </summary>
    public class DigitalInput : ISensor
    {
        internal readonly GrovePort _mode;
        internal GoPiGo _goPiGo;

        /// <summary>
        /// Creates an instance of <see cref="DigitalInput"/>
        /// </summary>
        /// <param name="goPiGo">The GoPiGo3 class</param>
        /// <param name="port">The Grove Port, need to be in the list of SupportedPorts</param>
        public DigitalInput(GoPiGo goPiGo, GrovePort port)
        {
            if (!SupportedPorts.Contains(port))
            {
                throw new ArgumentException($"Error: Grove Port not supported");
            }

            _goPiGo = goPiGo;
            Port = port;
            _goPiGo.SetGroveType(port, GroveSensorType.Custom);
            _mode = (port == GrovePort.Grove1) ? GrovePort.Grove1Pin1 : GrovePort.Grove2Pin1;
            _goPiGo.SetGroveMode(_mode, GroveInputOutput.InputDigital);
        }

        /// <summary>
        /// Get the state of the digital pin
        /// </summary>
        public int Value => _goPiGo.GetGroveState(_mode);

        /// <summary>
        /// Get "High" when reading high, "Low" otherwise
        /// </summary>
        public override string ToString() => _goPiGo.GetGroveState(_mode) != 0 ? "High" : "Low";

        /// <summary>
        /// Get the sensor name "Digital Input"
        /// </summary>
        public string SensorName => "Digital Input";

        /// <summary>
        /// Grove port
        /// </summary>
        public GrovePort Port { get; internal set; }

        /// <summary>
        /// List the supported Grove ports for the sensor
        /// </summary>
        public static List<GrovePort> SupportedPorts => new List<GrovePort>() { GrovePort.Grove1, GrovePort.Grove2 };
    }
}
