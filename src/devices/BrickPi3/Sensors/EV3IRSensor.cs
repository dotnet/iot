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
    /// IR channels
    /// </summary>
    public enum IRChannel
    {
#pragma warning disable
        One = 0, Two = 1, Three = 2, Four = 3
#pragma warning restore
    }

    /// <summary>
    /// Sensor mode when using a EV3 IR Sensor
    /// </summary>
    public enum IRMode
    {
        /// <summary>
        /// Use the IR sensor as a distance sensor
        /// </summary>
        Proximity = SensorType.EV3InfraredProximity,

        /// <summary>
        /// Use the IR sensor to detect the location of the IR Remote
        /// </summary>
        Seek = SensorType.EV3InfraredSeek,

        /// <summary>
        /// Use the IR sensor to detect wich Buttons where pressed on the IR Remote
        /// </summary>
        Remote = SensorType.EV3InfraredRemote
    }

    /// <summary>
    /// Class for IR beacon location.
    /// </summary>
    public class BeaconLocation
    {
        /// <summary>
        /// Initializes a new instance of the IR beacon location class.
        /// </summary>
        /// <param name="location">Location.</param>
        /// <param name="distance">Distance.</param>
        public BeaconLocation(int location, int distance)
        {
            Location = location;
            Distance = distance;
        }

        /// <summary>
        /// Gets the location of the beacon ranging from minus to plus increasing clockwise when pointing towards the beacon
        /// </summary>
        /// <value>The location of the beacon.</value>
        public int Location { get; internal set; }

        /// <summary>
        /// Gets the distance of the beacon in CM (0-100)
        /// </summary>
        /// <value>The distance to the beacon.</value>
        public int Distance { get; internal set; }
    }

    /// <summary>
    /// Create a EV3 Infrared Sensor
    /// </summary>
    public class EV3InfraredSensor : INotifyPropertyChanged, ISensor
    {
        private Brick _brick = null;
        private IRMode _mode;
        private Timer _timer = null;
        private int _periodRefresh;

        /// <summary>
        /// Initialize an EV3 IR Sensor
        /// </summary>
        /// <param name="brick">Interface to main Brick component</param>
        /// <param name="port">Sensor port</param>
        public EV3InfraredSensor(Brick brick, SensorPort port)
            : this(brick, port, IRMode.Proximity, 1000)
        {
        }

        /// <summary>
        /// Initializes an EV3 IS Sensor
        /// </summary>
        /// <param name="brick">Interface to main Brick component</param>
        /// <param name="port">Sensor port</param>
        /// <param name="mode">IR mode</param>
        public EV3InfraredSensor(Brick brick, SensorPort port, IRMode mode)
            : this(brick, port, mode, 1000)
        {
        }

        /// <summary>
        /// Initialize an EV3 IR Sensor
        /// </summary>
        /// <param name="brick">Interface to main Brick component</param>
        /// <param name="port">Sensor port</param>
        /// <param name="mode">IR mode</param>
        /// <param name="timeout">Period in millisecond to check sensor value changes</param>
        public EV3InfraredSensor(Brick brick, SensorPort port, IRMode mode, int timeout)
        {
            _brick = brick;
            Mode = mode;
            Port = port;
            Channel = IRChannel.One;
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

        private int _value;
        private string _valueAsString;

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
        /// Gets or sets the IR mode.
        /// </summary>
        /// <value>The mode.</value>
        public IRMode Mode
        {
            get
            {
                return _mode;
            }

            set
            {
                if (_mode != value)
                {
                    _mode = value;
                    _brick.SetSensorType((byte)Port, (SensorType)_mode);
                }
            }
        }

        /// <summary>
        /// Reads the sensor value as a string.
        /// </summary>
        /// <returns>The value as a string</returns>
        public string ReadAsString()
        {
            string s = string.Empty;
            switch (_mode)
            {
                case IRMode.Proximity:
                    s = ReadDistance() + " cm";
                    break;
                case IRMode.Remote:
                    s = ReadRemoteCommand() + " on channel " + Channel;
                    break;
                case IRMode.Seek:
                    s = "Location: " + ReadBeaconLocation() + " Distance: TBD cm";
                    break;
            }

            return s;
        }

        /// <summary>
        /// Read the sensor value. The returned value depends on the mode. Distance in proximity mode.
        /// Remote command number in remote mode. Beacon location in seek mode.
        /// </summary>
        public int Read()
        {
            int value = 0;
            switch (Mode)
            {
                case IRMode.Proximity:
                    value = ReadDistance();
                    break;
                case IRMode.Remote:
                    value = ReadRemoteCommand();
                    break;
                case IRMode.Seek:
                    value = ReadBeaconLocation();
                    break;
            }

            return value;
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
                // depending on the sensor, number of returns are differents
                // SEEK = 8x8 bites, 4 x for each of the four channels, heading and distance
                // PROXIMITY = 1x8 bites
                // REMOTE = 4x8, button pressed per channel
                int value = int.MaxValue;
                switch (_mode)
                {
                    case IRMode.Proximity:
                        value = ret[0];
                        break;
                    case IRMode.Seek:
                        value = ret[0] + (ret[2] << 8) + (ret[4] << 16) + (ret[6] << 24);
                        break;
                    case IRMode.Remote:
                        value = ret[0] + (ret[1] << 8) + (ret[2] << 16) + (ret[3] << 24);
                        break;
                    default:
                        break;
                }

                return value;
            }
            catch (Exception ex) when (ex is IOException)
            {
                return int.MaxValue;
            }
        }

        /// <summary>
        /// Read the distance of the sensor in CM (0-100). This will change mode to proximity
        /// </summary>
        public int ReadDistance()
        {
            if (_mode != IRMode.Proximity)
            {
                Mode = IRMode.Proximity;
            }

            return ReadRaw();
        }

        /// <summary>
        /// Reads commands from the IR-Remote. This will change mode to remote
        /// </summary>
        /// <returns>The remote command.</returns>
        public byte ReadRemoteCommand()
        {
            if (Mode != IRMode.Remote)
            {
                Mode = IRMode.Remote;
            }

            try
            {
                var ret = _brick.GetSensor((byte)Port);
                return ret[(int)Channel];
            }
            catch (Exception ex) when (ex is IOException)
            {
                return byte.MaxValue;
            }
        }

        /// <summary>
        /// Gets the beacon location. This will change the mode to seek
        /// </summary>
        /// <returns>The beacon location.</returns>
        public int ReadBeaconLocation()
        {
            var oldmode = Mode;
            if (Mode != IRMode.Seek)
            {
                Mode = IRMode.Seek;
            }

            try
            {
                var ret = _brick.GetSensor((byte)Port);
                if (Mode != oldmode)
                {
                    Mode = oldmode;
                }

                return (ret[(int)(Channel) * 2] + ret[(int)(Channel) * 2 + 1] >> 8);
            }
            catch (Exception ex) when (ex is IOException)
            {
                if (Mode != oldmode)
                {
                    Mode = oldmode;
                }

                return int.MaxValue;
            }
        }

        /// <summary>
        /// Gets or sets the IR channel used for reading remote commands or beacon location
        /// </summary>
        /// <value>The channel.</value>
        public IRChannel Channel { get; set; }

        /// <summary>
        /// Sensor port
        /// </summary>
        public SensorPort Port { get; }

        /// <summary>
        /// Gets sensor name
        /// </summary>
        /// <returns>Sensor name</returns>
        public string GetSensorName()
        {
            return "EV3 IR";
        }

        /// <summary>
        /// Moves to next mode
        /// </summary>
        public void SelectNextMode()
        {
            Mode = Mode.Next();
        }

        /// <summary>
        /// Moves to previous mode
        /// </summary>
        public void SelectPreviousMode()
        {
            Mode = Mode.Previous();
        }

        /// <summary>
        /// Number of modes supported
        /// </summary>
        /// <returns>Number of modes</returns>
        public int NumberOfModes()
        {
            return Enum.GetNames(typeof(IRMode)).Length;
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
