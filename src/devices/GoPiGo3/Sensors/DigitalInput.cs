// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using Iot.Device.GoPiGo3.Models;
using System;
using System.Collections.Generic;

namespace Iot.Device.GoPiGo3.Sensors
{
    /// <summary>
    /// DigitalInput class is a generic class to support digital input sensors
    /// </summary>
    public class DigitalInput : ISensor
    {
        internal GoPiGo _goPiGo;
        internal GroovePort _mode;

        /// <summary>
        /// 
        /// </summary>
        /// <param name="goPiGo">The GoPiGo3 class</param>
        /// <param name="port">The Groove Port, need to be in the list of SupportedPorts</param>
        public DigitalInput(GoPiGo goPiGo, GroovePort port)
        {
            if (!SupportedPorts.Contains(port))
                throw new ArgumentException($"Error: Groove Port not supported");
            _goPiGo = goPiGo;
            Port = port;
            _goPiGo.SetGrooveType(port, GrooveSensorType.Custom);
            _mode = (port == GroovePort.Groove1) ? GroovePort.Groove1Pin1 : GroovePort.Groove2Pin1;
            _goPiGo.SetGrooveMode(_mode, GrooveInputOutput.InputDigital);
        }

        /// <summary>
        /// Get the state of the digital pin
        /// </summary>
        public int Value => _goPiGo.GetGrooveState(_mode);        

        /// <summary>
        /// Get "High" when reading high, "Low" otherwise
        /// </summary>
        public string ValueAsString => _goPiGo.GetGrooveState(_mode) !=0 ? "High" : "Low";        

        /// <summary>
        /// Get the sensor name "Digital Input"
        /// </summary>
        public string SensorName => "Digital Input";

        public GroovePort Port { get; internal set; }

        public List<GroovePort> SupportedPorts => new List<GroovePort>() { GroovePort.Groove1, GroovePort.Groove2 };
    }
}
