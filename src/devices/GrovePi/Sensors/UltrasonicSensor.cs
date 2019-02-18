using System;
using System.Buffers.Binary;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using Iot.Device.GrovePiDevice.Models;

namespace Iot.Device.GrovePiDevice.Sensors
{
    /// <summary>
    /// UltrasonicSensor class to support ultrasonic Grove sensors
    /// </summary>
    public class UltrasonicSensor : ISensor<int>
    {
        private GrovePi _grovePi;

        /// <summary>
        /// UltrasonicSensor constructor
        /// </summary>
        /// <param name="grovePi">The GrovePi class</param>
        /// <param name="port">The grove Port, need to be in the list of SupportedPorts</param>
        public UltrasonicSensor(GrovePi grovePi, GrovePort port)
        {
            if (!SupportedPorts.Contains(port))
                throw new ArgumentException($"Error: grove Port not supported");
            _grovePi = grovePi;
            Port = port;
        }

        /// <summary>
        /// Get the distance in centimeter
        /// if -1, then you have an error
        /// </summary>
        public int Value
        {
            get
            {
                _grovePi.WriteCommand(GrovePiCommands.UltrasonicRead, Port, 0, 0);
                // Need to wait 50 millisecond before reading the value
                Thread.Sleep(50);
                var ret = _grovePi.ReadCommand(GrovePiCommands.UltrasonicRead, Port);
                return BinaryPrimitives.ReadInt16BigEndian(ret.AsSpan(1, 2));
            }
        }

        /// <summary>
        /// Returns the distance formated in centimeter
        /// </summary>
        /// <returns>Returns the distance formated in centimeter</returns>
        public override string ToString() => $"{Value} cm";

        /// <summary>
        /// Get the name Ultrasonic Sensor
        /// </summary>
        public string SensorName => "Ultrasonic Sensor";

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
