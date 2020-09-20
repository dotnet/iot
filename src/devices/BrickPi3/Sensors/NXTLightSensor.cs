// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using System;
using System.ComponentModel;
using System.IO;
using System.Threading;
using Iot.Device.BrickPi3.Extensions;
using Iot.Device.BrickPi3.Models;

namespace Iot.Device.BrickPi3.Sensors
{
    /// <summary>
    /// Sensor mode when using a NXT light sensor
    /// </summary>
    public enum LightMode
    {
        /// <summary>
        /// Use the lgith sensor to read reflected light
        /// </summary>
        Relection = SensorType.NXTLightOn,

        /// <summary>
        /// Use the light sensor to detect the light intensity
        /// </summary>
        Ambient = SensorType.NXTLightOn
    }

    /// <summary>
    /// Create a NXT Light sensor
    /// </summary>
    public class NXTLightSensor : INotifyPropertyChanged, ISensor
    {
        private LightMode _lightMode;
        private Brick _brick = null;
        private Timer _timer = null;
        private int _periodRefresh;
        private int _value;
        private string _valueAsString;

        /// <summary>
        /// Initialize a NXT Light Sensor
        /// </summary>
        /// <param name="brick">Interface to main Brick component</param>
        /// <param name="port">Sensor port</param>
        public NXTLightSensor(Brick brick, SensorPort port)
            : this(brick, port, LightMode.Relection, 1000)
        {
        }

        /// <summary>
        /// Initialize a NXT Light Sensor
        /// </summary>
        /// <param name="brick">Interface to main Brick component</param>
        /// <param name="port">Sensor port</param>
        /// <param name="mode">Light mode</param>
        public NXTLightSensor(Brick brick, SensorPort port, LightMode mode)
            : this(brick, port, mode, 1000)
        {
        }

        /// <summary>
        /// Initialize a NXT Light Sensor
        /// </summary>
        /// <param name="brick">Interface to main Brick component</param>
        /// <param name="port">Sensor port</param>
        /// <param name="mode">Light mode</param>
        /// <param name="timeout">Period in millisecond to check sensor value changes</param>
        public NXTLightSensor(Brick brick, SensorPort port, LightMode mode, int timeout)
        {
            _brick = brick;
            Port = port;
            _lightMode = mode;
            CutOff = 512;
            brick.SetSensorType((byte)Port, (SensorType)mode);
            _periodRefresh = timeout;
            _timer = new Timer(UpdateSensor, this, TimeSpan.FromMilliseconds(timeout), TimeSpan.FromMilliseconds(timeout));
        }

        private void StopTimerInternal()
        {
            if (_timer != null)
            {
                _timer.Dispose();
                _timer = null;
            }
        }

        private void OnPropertyChanged(string name)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
        }

        /// <summary>
        /// To notify a property has changed. The minimum time can be set up
        /// with timeout property
        /// </summary>
        public event PropertyChangedEventHandler PropertyChanged;

        /// <summary>
        /// Period to refresh the notification of property changed in milliseconds
        /// </summary>
        public int PeriodRefresh
        {
            get
            {
                return _periodRefresh;
            }

            set
            {
                _periodRefresh = value;
                _timer.Change(TimeSpan.FromMilliseconds(_periodRefresh), TimeSpan.FromMilliseconds(_periodRefresh));
            }
        }

        /// <summary>
        /// Return the raw value of the sensor
        /// </summary>
        public int Value
        {
            get
            {
                return ReadRaw();
            }

            internal set
            {
                if (value != _value)
                {
                    _value = value;
                    OnPropertyChanged(nameof(Value));
                }
            }
        }

        /// <summary>
        /// Return the raw value  as a string of the sensor
        /// </summary>
        public string ValueAsString
        {
            get
            {
                return ReadAsString();
            }

            internal set
            {
                if (_valueAsString != value)
                {
                    _valueAsString = value;
                    OnPropertyChanged(nameof(ValueAsString));
                }
            }
        }

        /// <summary>
        /// Update the sensor and this will raised an event on the interface
        /// </summary>
        public void UpdateSensor(object state)
        {
            Value = ReadRaw();
            ValueAsString = ReadAsString();
        }

        /// <summary>
        /// This is used to change the level which indicate if the sensor
        /// is on something dark or clear
        /// </summary>
        public int CutOff { get; set; }

        /// <summary>
        /// Light mode
        /// </summary>
        public LightMode LightMode
        {
            get
            {
                return _lightMode;
            }

            set
            {
                if (value != _lightMode)
                {
                    _lightMode = value;
                    _brick.SetSensorType((byte)Port, (SensorType)_lightMode);
                }
            }
        }

        /// <summary>
        /// Sensor port
        /// </summary>
        public SensorPort Port { get; }

        /// <summary>
        /// Moves to next mode
        /// </summary>
        public void SelectNextMode()
        {
            LightMode = LightMode.Next();
        }

        /// <summary>
        /// Moves to previous mode
        /// </summary>
        public void SelectPreviousMode()
        {
            LightMode = LightMode.Previous();
        }

        /// <summary>
        /// Number of modes supported
        /// </summary>
        /// <returns>Number of modes</returns>
        public int NumberOfModes()
        {
            return Enum.GetNames(typeof(LightMode)).Length;
        }

        /// <summary>
        /// Selected mode
        /// </summary>
        /// <returns>String representing selected mode</returns>
        public string SelectedMode()
        {
            return LightMode.ToString();
        }

        /// <summary>
        /// Reads raw data from the sensor
        /// </summary>
        /// <returns>Integer value read from the light sensor</returns>
        public int ReadRaw()
        {
            try
            {
                var ret = _brick.GetSensor((byte)Port);
                return (ret[0] + (ret[1] >> 8));
            }
            catch (Exception ex) when (ex is IOException)
            {
                return int.MaxValue;
            }
        }

        /// <summary>
        /// Reads data from the sensor and coverts it to string
        /// </summary>
        /// <returns>String value representing the reading</returns>
        public string ReadAsString()
        {
            return (ReadRaw() > CutOff) ? "Dark" : "Clear";
        }

        /// <summary>
        /// Gets sensor name
        /// </summary>
        /// <returns>Sensor name</returns>
        public string GetSensorName()
        {
            return "NXT Light";
        }
    }
}
