// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using Iot.Device.GoPiGo3.Models;
using System;
using System.Collections.Generic;
using System.IO;

namespace Iot.Device.GoPiGo3.Sensors
{
    /// <summary>
    /// UltraSonicSensor class to support ultrasonis sensor types
    /// </summary>
    public class UltraSonicSensor : ISensor
    {
        private GoPiGo _goPiGo;

        public GroovePort Port { get; internal set; }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="goPiGo">The GoPiGo3 class</param>
        /// <param name="port">The Groove Port, need to be in the list of SupportedPorts</param>
        public UltraSonicSensor(GoPiGo goPiGo, GroovePort port)
        {
            if (!SupportedPorts.Contains(port))
                throw new ArgumentException($"Error: Groove Port not supported");
            _goPiGo = goPiGo;
            Port = port;
            _goPiGo.SetGrooveType(port, GrooveSensorType.Ultrasonic);
        }

        /// <summary>
        /// Return the raw value of the sensor
        /// </summary>
        public int Value
        {
            get
            {
                try
                {
                    return (_goPiGo.GetGrooveValue(Port)[0] << 8) + _goPiGo.GetGrooveValue(Port)[1];
                }
                catch (IOException)
                {
                    return -1;
                }
            }
        }

        /// <summary>
        /// Return the raw value  as a string of the sensor
        /// </summary>
        public override string ToString() => $"{Value} cm";

        public List<GroovePort> SupportedPorts => new List<GroovePort> { GroovePort.Groove1, GroovePort.Groove2 };

        /// <summary>
        /// Get the sensor name "Ultrasonic Sensor"
        /// </summary>
        public string SensorName => "Ultrasonic Sensor";
    }
}
