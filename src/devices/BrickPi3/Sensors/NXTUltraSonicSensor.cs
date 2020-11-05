// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System;
using System.ComponentModel;
using System.IO;
using System.Threading;
using Iot.Device.BrickPi3.Extensions;
using Iot.Device.BrickPi3.Models;

namespace Iot.Device.BrickPi3.Sensors
{
    /// <summary>
    /// Sensor mode when using a Sonar sensor
    /// </summary>
    public enum UltraSonicMode
    {
        /// <summary>
        /// Result will be in centimeter
        /// </summary>
        Centimeter = SensorType.EV3UltrasonicCentimeter,

        /// <summary>
        /// Result will be in centi-inch
        /// </summary>
        Inch = SensorType.EV3UltrasonicInches,

        /// <summary>
        /// Sensor is in listen mode
        /// </summary>
        Listen = SensorType.EV3UltrasonicListen
    }

    /// <summary>
    /// Create a NXT Utrasonic sensor
    /// </summary>
    public class NXTUltraSonicSensor : INotifyPropertyChanged, ISensor
    {
        private Brick _brick;
        private Timer _timer;
        private int _periodRefresh;
        private int _value;
        private string? _valueAsString;

        /// <summary>
        /// Initialize a NXT Ultrasonic sensor
        /// </summary>
        /// <param name="brick">Interface to an instance of <see cref="Brick"/></param>
        /// <param name="port">Sensor port</param>
        public NXTUltraSonicSensor(Brick brick, SensorPort port)
            : this(brick, port, UltraSonicMode.Centimeter, 1000)
        {
        }

        /// <summary>
        /// Initialize a NXT Ultrasonic sensor
        /// </summary>
        /// <param name="brick">Interface to an instance of <see cref="Brick"/></param>
        /// <param name="port">Sensor port</param>
        /// <param name="mode">Ultrasonic mode</param>
        public NXTUltraSonicSensor(Brick brick, SensorPort port, UltraSonicMode mode)
            : this(brick, port, mode, 1000)
        {
        }

        /// <summary>
        /// Initialize a NXT Ultrasonic sensor
        /// </summary>
        /// <param name="brick">Interface to an instance of <see cref="Brick"/></param>
        /// <param name="port">Sensor port</param>
        /// <param name="mode">Ultrasonic mode</param>
        /// <param name="timeout">Period in millisecond to check sensor value changes</param>
        public NXTUltraSonicSensor(Brick brick, SensorPort port, UltraSonicMode mode, int timeout)
        {
            _brick = brick;
            Port = port;
            if (UltraSonicMode.Listen == mode)
            {
                mode = UltraSonicMode.Centimeter;
            }

            Mode = mode;
            brick.SetSensorType((byte)Port, SensorType.NXTUltrasonic);
            _periodRefresh = timeout;
            _timer = new Timer(UpdateSensor, this, TimeSpan.FromMilliseconds(timeout), TimeSpan.FromMilliseconds(timeout));
        }

        private void StopTimerInternal()
        {
            _timer?.Dispose();
            _timer = null!;
        }

        private void OnPropertyChanged(string name)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
        }

        /// <summary>
        /// To notify a property has changed. The minimum time can be set up
        /// with timeout property
        /// </summary>
        public event PropertyChangedEventHandler? PropertyChanged;

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
        /// Gets sensor port
        /// </summary>
        public SensorPort Port { get; }

        /// <summary>
        /// Return the raw value of the sensor
        /// </summary>
        public int Value
        {
            // return the stored value, this sensor can't be read too often
            get
            {
                return _value;
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
        public string ValueAsString => ReadAsString();

        /// <summary>
        /// Update the sensor and this will raised an event on the interface
        /// </summary>
        public void UpdateSensor(object? state)
        {
            Value = ReadRaw();
            string sensorState = ReadAsString();
            string? previousValue = _valueAsString;
            _valueAsString = sensorState;
            if (sensorState != previousValue)
            {
                OnPropertyChanged(nameof(ValueAsString));
            }
        }

        /// <summary>
        /// Gets or sets the sonar mode.
        /// </summary>
        /// <value>
        /// The sonar mode
        /// </value>
        public UltraSonicMode Mode { get; set; }

        /// <summary>
        /// Gets sensor name
        /// </summary>
        /// <returns>Sensor name</returns>
        public string GetSensorName()
        {
            return "NXT Ultrasonic";
        }

        /// <summary>
        /// Reads distance as string
        /// </summary>
        /// <returns>String representing distance with units</returns>
        public string ReadAsString()
        {
            return (Mode == UltraSonicMode.Inch) ? ReadDistance().ToString() + " inch" : ReadDistance().ToString() + " cm";
        }

        /// <summary>
        /// Read the distance in either centiinches or centimeter
        /// </summary>
        /// <returns>Distance as a float</returns>
        public double ReadDistance()
        {
            int reading = _value;
            if (reading == int.MaxValue)
            {
                return reading;
            }

            return Mode == UltraSonicMode.Inch ? (reading * 39370) / 100 : reading;
        }

        /// <summary>
        /// The raw value from the sensor
        /// </summary>
        /// <returns>Value as a int</returns>
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
        /// Moves to next mode
        /// </summary>
        public void SelectNextMode()
        {
            Mode = Mode.Next();
            if (Mode == UltraSonicMode.Listen)
            {
                Mode = Mode.Next();
            }
        }

        /// <summary>
        /// Moves to previous mode
        /// </summary>
        public void SelectPreviousMode()
        {
            Mode = Mode.Previous();
            if (Mode == UltraSonicMode.Listen)
            {
                Mode = Mode.Previous();
            }
        }

        /// <summary>
        /// Number of supported modes
        /// </summary>
        /// <returns>Number of modes</returns>
        public int NumberOfModes()
        {
            // listen mode not supported so 1 less mode
            return Enum.GetNames(typeof(UltraSonicMode)).Length - 1;
        }

        /// <summary>
        /// Selected mode
        /// </summary>
        /// <returns>String representing selected mode</returns>
        public string SelectedMode()
        {
            return Mode.ToString();
        }
    }
}
