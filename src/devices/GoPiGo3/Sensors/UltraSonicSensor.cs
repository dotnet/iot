// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System;
using System.Collections.Generic;
using System.IO;
using Iot.Device.GoPiGo3.Models;

namespace Iot.Device.GoPiGo3.Sensors
{
    /// <summary>
    /// UltraSonicSensor class to support ultrasonis sensor types
    /// </summary>
    public class UltraSonicSensor : ISensor
    {
        private GoPiGo _goPiGo;

        /// <summary>
        /// Grove port
        /// </summary>
        public GrovePort Port { get; internal set; }

        /// <summary>
        /// Creates a new instance of <see cref="UltraSonicSensor"/>.
        /// </summary>
        /// <param name="goPiGo">The GoPiGo3 class</param>
        /// <param name="port">The Grove Port, need to be in the list of SupportedPorts</param>
        public UltraSonicSensor(GoPiGo goPiGo, GrovePort port)
        {
            if (!SupportedPorts.Contains(port))
            {
                throw new ArgumentException("Grove port not supported", nameof(port));
            }

            _goPiGo = goPiGo;
            Port = port;
            _goPiGo.SetGroveType(port, GroveSensorType.Ultrasonic);
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
                    return (_goPiGo.GetGroveValue(Port)[0] << 8) + _goPiGo.GetGroveValue(Port)[1];
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

        /// <summary>
        /// List the supported Grove ports for the sensor
        /// </summary>
        public static List<GrovePort> SupportedPorts => new List<GrovePort> { GrovePort.Grove1, GrovePort.Grove2 };

        /// <summary>
        /// Get the sensor name "Ultrasonic Sensor"
        /// </summary>
        public string SensorName => "Ultrasonic Sensor";
    }
}
