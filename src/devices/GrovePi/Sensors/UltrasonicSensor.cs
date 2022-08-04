// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System;
using System.Buffers.Binary;
using System.Collections.Generic;
using System.Threading;
using Iot.Device.GrovePiDevice.Models;

namespace Iot.Device.GrovePiDevice.Sensors
{
    /// <summary>
    /// UltrasonicSensor class to support ultrasonic Grove sensors
    /// </summary>
    public class UltrasonicSensor
    {
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

        private GrovePi _grovePi;

        /// <summary>
        /// grove sensor port
        /// </summary>
        private GrovePort _port;

        /// <summary>
        /// UltrasonicSensor constructor
        /// </summary>
        /// <param name="grovePi">The GrovePi class</param>
        /// <param name="port">The grove Port, need to be in the list of SupportedPorts</param>
        public UltrasonicSensor(GrovePi grovePi, GrovePort port)
        {
            if (!SupportedPorts.Contains(port))
            {
                throw new ArgumentException("Grove port not supported.", nameof(port));
            }

            _grovePi = grovePi;
            _port = port;
        }

        /// <summary>
        /// Get the distance in centimeter
        /// if -1, then you have an error
        /// </summary>
        public int Value
        {
            get
            {
                _grovePi.WriteCommand(GrovePiCommand.UltrasonicRead, _port, 0, 0);
                // Need to wait at least 50 millisecond before reading the value
                // Having 100 shows better results
                Thread.Sleep(100);
                var ret = _grovePi.ReadCommand(GrovePiCommand.UltrasonicRead, _port);
                return BinaryPrimitives.ReadInt16BigEndian(ret.AsSpan(1, 2));
            }
        }

        /// <summary>
        /// Returns the distance formatted in centimeters.
        /// </summary>
        /// <returns>Returns the distance formatted in centimeters</returns>
        public override string ToString() => $"{Value} cm";

        /// <summary>
        /// Get the name Ultrasonic Sensor
        /// </summary>
        public string SensorName => "Ultrasonic Sensor";
    }
}
