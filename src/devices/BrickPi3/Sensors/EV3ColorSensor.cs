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
    /// Create a EV3 Color sensor
    /// </summary>
    public class EV3ColorSensor : INotifyPropertyChanged, ISensor
    {
        private const int RedIndex = 0;
        private const int GreenIndex = 1;
        private const int BlueIndex = 2;
        private const int BackgroundIndex = 3;

        private Brick _brick = null;
        private ColorSensorMode _colorMode;
        private Int16[] _rawValues = new Int16[4];
        private Timer _timer = null;
        private int _periodRefresh;
        private int _value;
        private string _valueAsString;

        /// <summary>
        /// Initialize an EV3 Color Sensor
        /// </summary>
        /// <param name="brick">Interface to main Brick component</param>
        /// <param name="port">Sensor port</param>
        public EV3ColorSensor(Brick brick, SensorPort port)
            : this(brick, port, ColorSensorMode.Color, 1000)
        {
        }

        /// <summary>
        /// Initialize an EV3 Color Sensor
        /// </summary>
        /// <param name="brick">Interface to main Brick component</param>
        /// <param name="port">Sensor port</param>
        /// <param name="mode">Color mode</param>
        public EV3ColorSensor(Brick brick, SensorPort port, ColorSensorMode mode)
            : this(brick, port, mode, 1000)
        {
        }

        /// <summary>
        /// Initilaize an EV3 Color Sensor
        /// </summary>
        /// <param name="brick">Interface to main Brick component</param>
        /// <param name="port">Sensor port</param>
        /// <param name="mode">Color mode</param>
        /// <param name="timeout">Period in millisecond to check sensor value changes</param>
        public EV3ColorSensor(Brick brick, SensorPort port, ColorSensorMode mode, int timeout)
        {
            _brick = brick;
            Port = port;
            _colorMode = mode;
            brick.SetSensorType((byte)Port, GetEV3Mode(mode));
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

        private SensorType GetEV3Mode(ColorSensorMode mode)
        {
            SensorType ret = SensorType.EV3ColorReflected;
            switch (mode)
            {
                case ColorSensorMode.Color:
                    ret = SensorType.EV3ColorColor;
                    break;
                case ColorSensorMode.Reflection:
                    ret = SensorType.EV3ColorReflected;
                    break;
                case ColorSensorMode.Green:
                    ret = SensorType.EV3ColorRawReflected;
                    break;
                case ColorSensorMode.Blue:
                    ret = SensorType.EV3ColorColorComponents;
                    break;
                case ColorSensorMode.Ambient:
                    ret = SensorType.EV3ColorAmbient;
                    break;
            }

            return ret;
        }

        /// <summary>
        /// Set or get the color mode
        /// </summary>
        public ColorSensorMode ColorMode
        {
            get
            {
                return _colorMode;
            }

            set
            {
                if (value != _colorMode)
                {
                    _colorMode = value;
                    _brick.SetSensorType((byte)Port, GetEV3Mode(_colorMode));
                }
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

        private void GetRawValues()
        {
            try
            {
                var ret = _brick.GetSensor((byte)Port);
                switch (_colorMode)
                {
                    case ColorSensorMode.Color:
                    case ColorSensorMode.Reflection:
                    case ColorSensorMode.Ambient:
                        _rawValues[BackgroundIndex] = ret[0];
                        _rawValues[BlueIndex] = 0;
                        _rawValues[GreenIndex] = 0;
                        _rawValues[RedIndex] = 0;
                        break;
                    case ColorSensorMode.Green:
                    case ColorSensorMode.Blue:
                        _rawValues[BackgroundIndex] = ret[0];
                        _rawValues[BlueIndex] = ret[1];
                        _rawValues[GreenIndex] = ret[2];
                        _rawValues[RedIndex] = ret[3];
                        break;
                }
            }
            catch (Exception ex) when (ex is IOException)
            {
            }
        }

        /// <summary>
        /// Get the raw value
        /// </summary>
        /// <returns></returns>
        public int ReadRaw()
        {
            int val = 0;
            switch (_colorMode)
            {
                case ColorSensorMode.Color:
                case ColorSensorMode.Reflection:
                case ColorSensorMode.Ambient:
                    val = (int)ReadColor();
                    break;
                case ColorSensorMode.Green:
                case ColorSensorMode.Blue:
                    val = CalculateRawAverage();
                    break;
            }

            return val;
        }

        /// <summary>
        /// Read the intensity of the reflected or ambient light in percent. In color mode the color index is returned
        /// </summary>
        public int Read()
        {
            int val = 0;
            switch (_colorMode)
            {
                case ColorSensorMode.Color:
                case ColorSensorMode.Reflection:
                case ColorSensorMode.Ambient:
                    val = (int)ReadColor();
                    break;
                default:
                    val = CalculateRawAverageAsPct();
                    break;
            }

            return val;
        }

        private int CalculateRawAverage()
        {
            if (_colorMode == ColorSensorMode.Color)
            {
                return 0;
            }
            else
            {
                try
                {
                    var ret = _brick.GetSensor((byte)Port);
                    return (ret[0] + (ret[1] >> 8) + (ret[2] >> 16) + ret[3] >> 24) / 255 / 3;
                }
                catch (Exception ex) when (ex is IOException)
                {
                    return 0;
                }
            }
        }

        private int CalculateRawAverageAsPct()
        {
            // Need to find out what is the ADC resolution
            // 1023 is probably the correct one
            return (CalculateRawAverage() * 100) / 1023;
        }

        /// <summary>
        /// Read the test value
        /// </summary>
        /// <returns></returns>
        public string ReadTest()
        {
            GetRawValues();
            string ret = string.Empty;
            for (int i = 0; i < _rawValues.Length; i++)
            {
                ret += " " + _rawValues[i];
            }

            ret += " " + (_rawValues[BackgroundIndex] + (_rawValues[BlueIndex] >> 8) + (_rawValues[GreenIndex] >> 16) + _rawValues[RedIndex] >> 24).ToString();
            return ret;
        }

        /// <summary>
        /// Get the color as a string
        /// </summary>
        /// <returns></returns>
        public string ReadAsString()
        {
            string s = string.Empty;
            switch (_colorMode)
            {
                case ColorSensorMode.Color:
                    s = ReadColor().ToString();
                    break;
                case ColorSensorMode.Reflection:
                case ColorSensorMode.Green:
                case ColorSensorMode.Blue:
                    s = Read().ToString();
                    break;
                case ColorSensorMode.Ambient:
                    s = Read().ToString();
                    break;
            }

            return s;
        }

        /// <summary>
        /// Reads the color.
        /// </summary>
        /// <returns>The color.</returns>
        public Color ReadColor()
        {
            Color color = Color.None;
            if (_colorMode == ColorSensorMode.Color)
            {
                try
                {
                    color = (Color)_brick.GetSensor((byte)Port)[0];
                }
                catch (Exception ex) when (ex is IOException)
                {
                    color = Color.None;
                }
            }

            return color;
        }

        /// <summary>
        /// Reads the color of the RGB.
        /// </summary>
        /// <returns>The RGB color.</returns>
        public RGBColor ReadRGBColor()
        {
            GetRawValues();
            return new RGBColor((byte)_rawValues[RedIndex], (byte)_rawValues[GreenIndex], (byte)_rawValues[BlueIndex]);
        }

        private Color CalculateColor()
        {
            GetRawValues();
            int red = _rawValues[RedIndex];
            int blue = _rawValues[BlueIndex];
            int green = _rawValues[GreenIndex];
            int blank = _rawValues[BackgroundIndex];
            // we have calibrated values, now use them to determine the color

            // The following algorithm comes from the 1.29 Lego firmware.
            if (red > blue && red > green)
            {
                // Red dominant color
                if (red < 65 || (blank < 40 && red < 110))
                {
                    return Color.Black;
                }

                if (((blue >> 2) + (blue >> 3) + blue < green) &&
                        ((green << 1) > red))
                {
                    return Color.Yellow;
                }

                if ((green << 1) - (green >> 2) < red)
                {
                    return Color.Red;
                }

                if (blue < 70 || green < 70 || (blank < 140 && red < 140))
                {
                    return Color.Black;
                }

                return Color.White;
            }
            else if (green > blue)
            {
                // Green dominant color
                if (green < 40 || (blank < 30 && green < 70))
                {
                    return Color.Black;
                }

                if ((blue << 1) < red)
                {
                    return Color.Yellow;
                }

                if ((red + (red >> 2)) < green ||
                        (blue + (blue >> 2)) < green)
                {
                    return Color.Green;
                }

                if (red < 70 || blue < 70 || (blank < 140 && green < 140))
                {
                    return Color.Black;
                }

                return Color.White;
            }
            else
            {
                // Blue dominant color
                if (blue < 48 || (blank < 25 && blue < 85))
                {
                    return Color.Black;
                }

                if ((((red * 48) >> 5) < blue && ((green * 48) >> 5) < blue) ||
                        ((red * 58) >> 5) < blue || ((green * 58) >> 5) < blue)
                {
                    return Color.Blue;
                }

                if (red < 60 || green < 60 || (blank < 110 && blue < 120))
                {
                    return Color.Black;
                }

                if ((red + (red >> 3)) < blue || (green + (green >> 3)) < blue)
                {
                    return Color.Blue;
                }

                return Color.White;
            }
        }

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
            return "EV3 Color Sensor";
        }

        /// <summary>
        /// Moves to next mode
        /// </summary>
        public void SelectNextMode()
        {
            ColorMode = ColorMode.Next();
        }

        /// <summary>
        /// Moves to previous mode
        /// </summary>
        public void SelectPreviousMode()
        {
            ColorMode = ColorMode.Previous();
        }

        /// <summary>
        /// Number of modes
        /// </summary>
        /// <returns>Number of modes supported</returns>
        public int NumberOfModes()
        {
            return Enum.GetNames(typeof(ColorSensorMode)).Length;
        }

        /// <summary>
        /// Selected mode
        /// </summary>
        /// <returns>String representing selected mode</returns>
        public string SelectedMode()
        {
            return ColorMode.ToString();
        }
    }
}
