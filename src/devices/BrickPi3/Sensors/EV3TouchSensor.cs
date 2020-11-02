// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System;
using System.ComponentModel;
using System.IO;
using System.Threading;
using Iot.Device.BrickPi3.Models;

namespace Iot.Device.BrickPi3.Sensors
{
    /// <summary>
    /// Create e EV3 Touch sensor
    /// </summary>
    public class EV3TouchSensor : INotifyPropertyChanged, ISensor
    {
        private Brick _brick = null;
        private Timer _timer = null;
        private int _value;
        private string _valueAsString;
        private int _periodRefresh;

        /// <summary>
        /// Initialise an EV3 Touch sensor
        /// </summary>
        /// <param name="brick">Interface to main Brick component</param>
        /// <param name="port">Sensor port</param>
        public EV3TouchSensor(Brick brick, SensorPort port)
            : this(brick, port, 1000)
        {
        }

        /// <summary>
        /// Initialize an EV3 Touch Sensor
        /// </summary>
        /// <param name="brick">Interface to main Brick component</param>
        /// <param name="port">Sensor port</param>
        /// <param name="timeout">Period in millisecond to check sensor value changes</param>
        public EV3TouchSensor(Brick brick, SensorPort port, int timeout)
        {
            _brick = brick;
            Port = port;
            brick.SetSensorType((byte)Port, SensorType.EV3Touch);
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
        /// Reads the sensor value as a string.
        /// </summary>
        /// <returns>The value as a string</returns>
        public string ReadAsString()
        {
            return IsPressed() ? "Pressed" : "Not pressed";
        }

        /// <summary>
        /// Determines whether the touch sensor is pressed.
        /// </summary>
        /// <returns><c>true</c> if the sensor is pressed; otherwise, <c>false</c>.</returns>
        public bool IsPressed()
        {
            return ReadRaw() > 0;
        }

        /// <summary>
        /// Reads the raw sensor value
        /// </summary>
        /// <returns>The raw.</returns>
        public int ReadRaw()
        {
            try
            {
                return _brick.GetSensor((byte)Port)[0];
            }
            catch (Exception ex) when (ex is IOException)
            {
                return int.MaxValue;
            }
        }

        /// <summary>
        /// Return port
        /// </summary>
        public SensorPort Port { get; }

        /// <summary>
        /// Gets sensor name
        /// </summary>
        /// <returns>Sensor name</returns>
        public string GetSensorName()
        {
            return "EV3 Touch";
        }

        /// <summary>
        /// Number of modes supported
        /// </summary>
        /// <returns>Number of modes</returns>
        public int NumberOfModes()
        {
            return 1;
        }

        /// <summary>
        /// Selected mode
        /// </summary>
        /// <returns>String representing selected mode</returns>
        public string SelectedMode()
        {
            return "Analog";
        }

        /// <summary>
        /// Moves to next mode
        /// </summary>
        public void SelectNextMode()
        {
        }

        /// <summary>
        /// Moves to previous mode
        /// </summary>
        public void SelectPreviousMode()
        {
        }
    }
}
