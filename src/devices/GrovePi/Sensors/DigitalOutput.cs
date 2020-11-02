// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System;
using System.Collections.Generic;
using System.Device.Gpio;
using Iot.Device.GrovePiDevice.Models;

namespace Iot.Device.GrovePiDevice.Sensors
{
    /// <summary>
    /// DigitalOutput class is a generic output class
    /// </summary>
    public class DigitalOutput
    {
        internal GrovePi _grovePi;
        internal PinValue _value;

        /// <summary>
        /// grove sensor port
        /// </summary>
        internal GrovePort _port;

        /// <summary>
        /// DigitalOutput constructor
        /// </summary>
        /// <param name="grovePi">The GrovePi class</param>
        /// <param name="port">The grove Port, need to be in the list of SupportedPorts</param>
        public DigitalOutput(GrovePi grovePi, GrovePort port)
        {
            if (!SupportedPorts.Contains(port))
            {
                throw new ArgumentException($"Grove port {port} not supported.", nameof(port));
            }

            _grovePi = grovePi;
            _port = port;
            _grovePi.PinMode(_port, PinMode.Output);
            Value = 0;
        }

        /// <summary>
        /// Get/Set the pin level to either high, either low
        /// </summary>
        public PinValue Value
        {
            get
            {
                return _value;
            }

            set
            {
                _value = value;
                _grovePi.DigitalWrite(_port, _value);
            }
        }

        /// <summary>
        /// Returns the pin level as a string.
        /// </summary>
        /// <returns>Returns the pin level as a string</returns>
        public override string ToString() => Value.ToString();

        /// <summary>
        /// Get the name Digital Output
        /// </summary>
        public string SensorName => "Digital Output";

        /// <summary>
        /// Only Digital ports including the analogic sensors (A0 = D14, A1 = D15, A2 = D16)
        /// </summary>
        public static List<GrovePort> SupportedPorts => new List<GrovePort>()
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
