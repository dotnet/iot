// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using Iot.Device.GoPiGo3.Models;
using System;
using System.Collections.Generic;

namespace Iot.Device.GoPiGo3.Sensors
{
    /// <summary>
    /// DigitalOutput class to control a digital output 
    /// </summary>
    public class DigitalOutput : ISensor
    {
        internal GoPiGo _goPiGo;
        internal GroovePort _mode;
        internal bool _value;

        /// <summary>
        /// 
        /// </summary>
        /// <param name="goPiGo">The GoPiGo3 class</param>
        /// <param name="port">The Groove Port, need to be in the list of SupportedPorts</param>
        public DigitalOutput(GoPiGo goPiGo, GroovePort port)
        {
            if (!SupportedPorts.Contains(port))
                throw new ArgumentException($"Error: Groove Port not supported");
            _goPiGo = goPiGo;
            Port = port;
            _goPiGo.SetGrooveType(port, GrooveSensorType.Custom);
            _mode = (port == GroovePort.Groove1) ? GroovePort.Groove1Pin1 : GroovePort.Groove2Pin1;
            _goPiGo.SetGrooveMode(_mode, GrooveInputOutput.OutputDigital);
            _value = false;
        }

        /// <summary>
        /// Get/set the output. 0 for low, 1 for high. Anything else than 0 will set the high state
        /// </summary>
        public int Value
        {
            get { return _value ? 1 : 0; }
            set
            {
                _value = value != 0;
                _goPiGo.SetGrooveState(_mode, _value);
            }
        }

        /// <summary>
        /// Get "High" when output is set to high, "Low" if set to low state
        /// </summary>
        public override string ToString() => _value ? "High" : "Low";

        /// <summary>
        /// Get the sensor name "Digital Output"
        /// </summary>
        public string SensorName => "Digital Output";

        public GroovePort Port { get; internal set; }

        public List<GroovePort> SupportedPorts => new List<GroovePort>() { GroovePort.Groove1, GroovePort.Groove2 };
    }
}
