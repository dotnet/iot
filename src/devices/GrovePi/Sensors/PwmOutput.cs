// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using Iot.Device.GrovePiDevice.Models;
using System;
using System.Collections.Generic;
using System.Device.Gpio;
#if NETSTANDARD2_0
using Math = System.MathExtension;
#endif

namespace Iot.Device.GrovePiDevice.Sensors
{
    /// <summary>
    /// PwmOutput class to support hardware PWM on PWM hardware enabled pins
    /// </summary>
    public class PwmOutput
    {
        internal GrovePi _grovePi;
        internal byte _duty;

        /// <summary>
        /// grove sensor port
        /// </summary>
        internal GrovePort _port;

        /// <summary>
        /// PwmOutput constructor
        /// </summary>
        /// <param name="grovePi">The GrovePi class</param>
        /// <param name="port">The grove Port, need to be in the list of SupportedPorts</param>
        public PwmOutput(GrovePi grovePi, GrovePort port)
        {
            if (!SupportedPorts.Contains(port))
                throw new ArgumentException($"Grove port {port} not supported.", nameof(port));
            _grovePi = grovePi;
            _port = port;
            _grovePi.PinMode(_port, PinMode.Output);
            Value = 0;
        }

        /// <summary>
        /// For GrovePi, Value is same as Duty
        /// </summary>
        public byte Value
        { get { return Duty; } set { Duty = value; } }

        /// <summary>
        /// Get/set the PWM duty to use to generate the sound from 0 to 100
        /// </summary>
        public byte Duty
        {
            get { return _duty; }

            set
            {
                var prev = _duty;
                _duty = Math.Clamp(value, (byte)0, (byte)100);
                if (prev != _duty)
                    Start();
            }
        }

        /// <summary>
        /// Starts the PWM duty
        /// </summary>
        public void Start()
        {
            _grovePi.AnalogWrite(_port, _duty);
        }

        /// <summary>
        /// Stop the PWM duty
        /// </summary>
        public void Stop()
        {
            _grovePi.AnalogWrite(_port, 0);
        }

        /// <summary>
        /// Returns the duty in a formated string
        /// </summary>
        /// <returns>Returns the duty in a formated string</returns>
        public override string ToString() => $"Duty: {Value}";

        /// <summary>
        /// Get the duty cycle as a persentage
        /// </summary>
        public byte ValueAsPercent => (byte)(Value / byte.MaxValue);

        /// <summary>
        /// Get the name PWM Output
        /// </summary>
        public string SensorName => "PWM Output";

        /// <summary>
        /// Only Digital PWM are supported
        /// </summary>
        public static List<GrovePort> SupportedPorts => new List<GrovePort>()
        {
            GrovePort.DigitalPin3,
            GrovePort.DigitalPin5,
            GrovePort.DigitalPin6
        };
    }
}
