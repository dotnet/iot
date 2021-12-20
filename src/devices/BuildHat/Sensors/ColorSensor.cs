using System;
using System.Diagnostics.SymbolStore;
using System.IO;
using System.Linq;
using System.Threading;
using Iot.Device.BuildHat.Models;
using Microsoft.VisualBasic;
using SixLabors.ImageSharp;

namespace Iot.Device.BuildHat.Sensors
{
    /// <summary>
    /// Color sensor
    /// </summary>
    public class ColorSensor : ActiveSensor
    {
        internal Color _color;
        internal bool _isColorDetected;
        internal bool _hasColorUpdated;

        /// <summary>
        /// Gets the last measured Color
        /// </summary>
        public Color Color
        {
            get => _color;
            internal set
            {
                if (_color != value)
                {
                    _color = value;
                    OnPropertyChanged(nameof(Color));
                }

                _hasColorUpdated = true;
                OnPropertyUpdated(nameof(Color));
            }
        }

        /// <summary>
        /// Gets true if a color is detected.
        /// </summary>
        public bool IsColorDetected
        {
            get => _isColorDetected;
            internal set
            {
                if (_isColorDetected != value)
                {
                    _isColorDetected = value;
                    OnPropertyChanged(nameof(IsColorDetected));
                }

                OnPropertyUpdated(nameof(IsColorDetected));
            }
        }

        /// <summary>
        /// Creates a color sensor.
        /// </summary>
        /// <param name="brick">The brick.</param>
        /// <param name="port">The port.</param>
        /// <param name="type">The sensor type</param>
        public ColorSensor(Brick brick, SensorPort port, SensorType type)
            : base(brick, port, type)
        {
            if (SensorType == SensorType.SpikePrimeColorSensor)
            {
                Brick.SendRawCommand($"port {(byte)Port} ; plimit 1 ; set -1");
            }
            else if (SensorType == SensorType.ColourAndDistanceSensor)
            {
                Brick.SwitchSensorOn(Port);
            }

        }

        /// <summary>
        /// Gets the color, measure the nulmber of setup times.
        /// </summary>
        /// <returns>The color.</returns>
        public Color GetColor()
        {
            _hasColorUpdated = false;
            DateTime dt = DateTime.Now.AddSeconds(TimeoutMeasuresSeconds);
            Brick.SelectModeAndRead(Port, 6, true);

            ////while (!_hasColorUpdated && (dt < DateTime.Now))
            while (!_hasColorUpdated)
            {
                Thread.Sleep(10);
            }

            if (_hasColorUpdated)
            {
                return Color;
            }

            throw new IOException("Can't measure the color.");
        }

        /// <summary>
        /// Gets the reflected component.
        /// </summary>
        /// <returns>The reflected component.</returns>
        public int GetReflectedLight()
        {
            _hasValueAsStringUpdated = false;
            DateTime dt = DateTime.Now.AddSeconds(TimeoutMeasuresSeconds);
            Brick.SelectModeAndRead(Port, 3, true);
            while (!_hasColorUpdated && (dt < DateTime.Now))
            {
                Thread.Sleep(10);
            }

            var val = ValuesAsString.ToArray();
            if (_hasColorUpdated && (val != null) && (val.Length == 2))
            {
                return Convert.ToInt32(val[1]);
            }

            throw new IOException("Can't measure the reflected light.");
        }

        /// <inheritdoc/>
        public override string GetSensorName() => "Color sensor";
    }
}
