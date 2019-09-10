// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using Iot.Device.GoPiGo3.Models;
using System;
using System.Collections.Generic;
#if NETSTANDARD2_0
using Math = System.MathExtension;
#endif

namespace Iot.Device.GoPiGo3.Sensors
{
    /// <summary>
    /// Buzzeer class to support buzzers, note this class is using PWM to control the buzzer
    /// </summary>
    public class Buzzer : ISensor
    {
        private GoPiGo _goPiGo;
        private readonly GrovePort _mode;
        private int _value;
        private byte _duty;

        /// <summary>
        /// Constructor for the Buzzer class
        /// </summary>
        /// <param name="goPiGo">The GoPiGo3 class</param>
        /// <param name="port">The Grove Port, need to be in the list of SupportedPorts</param>
        public Buzzer(GoPiGo goPiGo, GrovePort port) : this(goPiGo, port, 50)
        { }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="goPiGo">The GoPiGo3 class</param>
        /// <param name="port">The Grove Port, need to be in the list of SupportedPorts</param>
        /// <param name="duty">The PWM duty to use to generate the sound from 0 to 100</param>
        public Buzzer(GoPiGo goPiGo, GrovePort port, byte duty)
        {
            if (!SupportedPorts.Contains(port))
                throw new ArgumentException($"Error: Grove Port not supported");
            _goPiGo = goPiGo;
            Port = port;
            _goPiGo.SetGroveType(port, GroveSensorType.Custom);
            _mode = (port == GrovePort.Grove1) ? GrovePort.Grove1Pin1 : GrovePort.Grove2Pin1;
            _goPiGo.SetGroveMode(_mode, GroveInputOutput.OutputPwm);
            Duty = duty;
            Value = 24_000; //The default value
            Stop();
        }

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
                {
                    Start();
                }
            }
        }

        /// <summary>
        /// Starts the buzzer
        /// </summary>
        public void Start()
        {
            _goPiGo.SetGrovePwmDuty(_mode, Duty);
        }

        /// <summary>
        /// Stop the buzzer
        /// </summary>
        public void Stop()
        {
            _goPiGo.SetGrovePwmDuty(_mode, 0);
        }

        /// <summary>
        /// Get/set the frequency in Hz
        /// </summary>
        public int Value
        {
            get { return _value; }

            set
            {
                _value = value;
                _goPiGo.GetGrovePwmFrequency(Port, (uint)_value);
            }
        }

        /// <summary>
        /// Get the frequency in Hertz
        /// </summary>
        public override string ToString() => $"{Value} Hz";

        /// <summary>
        /// Grove port
        /// </summary>
        public GrovePort Port { get; internal set; }

        /// <summary>
        /// List the supported Grove ports for the sensor
        /// </summary>
        public List<GrovePort> SupportedPorts => new List<GrovePort>() { GrovePort.Grove1, GrovePort.Grove2 };

        /// <summary>
        /// Get the sensor name "Buzzer"
        /// </summary>
        public string SensorName => "Buzzer";
    }
}
