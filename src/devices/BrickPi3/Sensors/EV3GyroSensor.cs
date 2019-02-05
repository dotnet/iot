// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using Iot.Device.BrickPi3.Extensions;
using Iot.Device.BrickPi3.Models;
using System;
using System.ComponentModel;
using System.IO;
using System.Threading;

namespace Iot.Device.BrickPi3.Sensors
{
    /// <summary>
    /// Sensor modes when using a EV3 Gyro sensor
    /// </summary>
    public enum GyroMode
    {
#pragma warning disable
        /// <summary>
        /// Result will be in degrees
        /// </summary>
        Angle = SensorType.EV3GyroAbs,
        /// <summary>
        /// Result will be in degrees per second
        /// </summary>
        AngularVelocity = SensorType.EV3GyroDps
#pragma warning restore
    };

    /// <summary>
    /// Create a EV3 Gyro sensor
    /// </summary>
    public class EV3GyroSensor : INotifyPropertyChanged, ISensor
    {

        private Brick _brick = null;
        private GyroMode _gmode;
        private Timer _timer = null;
        private int _value;
        private string _valueAsString;
        private int _periodRefresh;

        /// <summary>
        /// Initialize an EV3 Gyro Sensor
        /// </summary>
        /// <param name="port">Sensor port</param>
        public EV3GyroSensor(Brick brick, SensorPort port) : this(brick, port, GyroMode.Angle) { }

        /// <summary>
        /// Initialize an EV3 Gyro Sensor
        /// </summary>
        /// <param name="port">Sensor port</param>
        /// <param name="mode">Gyro mode</param>
        public EV3GyroSensor(Brick brick, SensorPort port, GyroMode mode) : this(brick, port, mode, 1000) { }

        /// <summary>
        /// Initialize an EV3 Gyro Sensor
        /// </summary>
        /// <param name="port">Sensor port</param>
        /// <param name="mode">Gyro mode</param>
        /// <param name="timeout">Period in millisecond to check sensor value changes</param>
        public EV3GyroSensor(Brick brick, SensorPort port, GyroMode mode, int timeout)
        {
            _brick = brick;
            Port = port;
            _gmode = mode;
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
        /// Return the raw value of the sensor
        /// </summary>
        public int Value
        {
            get { return ReadRaw(); }
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
            get { return ReadAsString(); }
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
        /// To notify a property has changed. The minimum time can be set up
        /// with timeout property
        /// </summary>
        public event PropertyChangedEventHandler PropertyChanged;

        /// <summary>
        /// Period to refresh the notification of property changed in milliseconds
        /// </summary>
        public int PeriodRefresh
        {
            get { return _periodRefresh; }
            set
            {
                _periodRefresh = value;
                _timer.Change(TimeSpan.FromMilliseconds(_periodRefresh), TimeSpan.FromMilliseconds(_periodRefresh));
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
        /// Gets or sets the Gyro mode. 
        /// </summary>
        /// <value>The mode.</value>
        public GyroMode Mode
        {
            get { return _gmode; }

            set
            {
                if (_gmode != value)
                {
                    _gmode = value;
                    _brick.SetSensorType((byte)Port, (SensorType)_gmode);
                }
            }
        }

        public SensorPort Port { get; }

        /// <summary>
        /// Reads the sensor value as a string.
        /// </summary>
        /// <returns>The value as a string</returns>
        public string ReadAsString()
        {
            string s = "";
            switch (_gmode)
            {
                case GyroMode.Angle:
                    s = Read().ToString() + " degree";
                    break;
                case GyroMode.AngularVelocity:
                    s = Read().ToString() + " deg/sec";
                    break;
            }

            return s;
        }

        /// <summary>
        /// Reset the sensor
        /// </summary>
        private void Reset()
        {
            if (Mode == GyroMode.Angle)
            {
                Mode = GyroMode.AngularVelocity;
                Thread.Sleep(100);
                Mode = GyroMode.Angle;
            }
            else
            {
                Mode = GyroMode.Angle;
                Thread.Sleep(100);
                Mode = GyroMode.AngularVelocity;
            }
        }

        /// <summary>
        /// Get the number of rotations (a rotation is 360 degrees) - only makes sense when in angle mode
        /// </summary>
        /// <returns>The number of rotations</returns>
        public int RotationCount()
        {
            var ret = ReadRaw();
            if (ret == int.MaxValue)
                return ret;
            return (Mode == GyroMode.Angle) ? ret / 360 : 0;
        }

        /// <summary>
        /// Read the gyro sensor value. The returned value depends on the mode. 
        /// </summary>
        public int Read()
        {
            var ret = ReadRaw();
            if (ret == int.MaxValue)
                return ret;
            return (Mode == GyroMode.Angle) ? ret % 360 : ret;
        }

        /// <summary>
        /// Read the sensor value
        /// </summary>
        /// <returns>Value as a int</returns>
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

        public string GetSensorName()
        {
            return "EV3 Gyro";
        }

        public void SelectNextMode()
        {
            Mode = Mode.Next();
        }

        public void SelectPreviousMode()
        {
            Mode = Mode.Previous();
        }

        public int NumberOfModes()
        {
            return Enum.GetNames(typeof(GyroMode)).Length;
        }

        public string SelectedMode()
        {
            return Mode.ToString();
        }
    }
}
