// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using Iot.Device.GrovePiDevice.Models;
using System;
using System.Collections.Generic;
using System.Device.Gpio;

namespace Iot.Device.GrovePiDevice.Sensors
{
    /// <summary>
    /// DigitalInput is a generic calss for digital input
    /// </summary>
    public class DigitalInput : ISensor<PinValue>
    {
        internal GrovePi _grovePi;

        /// <summary>
        /// 
        /// </summary>
        /// <param name="grovePi">The GrovePi class</param>
        /// <param name="port">The grove Port, need to be in the list of SupportedPorts</param>
        public DigitalInput(GrovePi grovePi, GrovePort port)
        {
            if (!SupportedPorts.Contains(port))
                throw new ArgumentException($"Error: grove Port not supported");
            _grovePi = grovePi;
            Port = port;
            _grovePi.PinMode(Port, PinMode.Input);
        }

        /// <summary>
        /// Get the pin level, either high either low
        /// </summary>
        public PinValue Value => _grovePi.DigitalRead(Port);

        /// <summary>
        /// Returns the value as a string
        /// </summary>
        /// <returns>Returns the value as a string</returns>
        public override string ToString() => Value.ToString();

        /// <summary>
        /// Get the name Digital Input
        /// </summary>
        public string SensorName => "Digital Input";

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
