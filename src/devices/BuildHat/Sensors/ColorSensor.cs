// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System.Drawing;
using System.IO;
using Iot.Device.BuildHat.Models;

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
        private int _reflected;
        private bool _hasReflectedUpdated;
        private int _ambiant;
        private bool _hasAmbiantUpdated;

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
        /// Gets the reflected light.
        /// </summary>
        public int ReflectedLight
        {
            get => _reflected;
            internal set
            {
                if (_reflected != value)
                {
                    _reflected = value;
                    OnPropertyChanged(nameof(ReflectedLight));
                }

                _hasReflectedUpdated = true;
                OnPropertyUpdated(nameof(ReflectedLight));
            }
        }

        /// <summary>
        /// Gets the ambiant light.
        /// </summary>
        public int AmbiantLight
        {
            get => _ambiant;
            internal set
            {
                if (_ambiant != value)
                {
                    _ambiant = value;
                    OnPropertyChanged(nameof(AmbiantLight));
                }

                _hasAmbiantUpdated = true;
                OnPropertyUpdated(nameof(AmbiantLight));
            }
        }

        /// <inheritdoc/>
        public override string SensorName => "Color sensor";

        /// <summary>
        /// Creates a color sensor.
        /// </summary>
        /// <param name="brick">The brick.</param>
        /// <param name="port">The port.</param>
        /// <param name="type">The sensor type</param>
        protected internal ColorSensor(Brick brick, SensorPort port, SensorType type)
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
            if (SetupModeAndRead(6, ref _hasColorUpdated))
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
            if (SetupModeAndRead(3, ref _hasReflectedUpdated))
            {
                return ReflectedLight;
            }

            throw new IOException("Can't measure the reflected light.");
        }

        /// <summary>
        /// Gets the ambiant light.
        /// </summary>
        /// <returns>The ambiant light.</returns>
        public int GetAmbiantLight()
        {
            if (SetupModeAndRead(4, ref _hasAmbiantUpdated))
            {
                return AmbiantLight;
            }

            throw new IOException("Can't measure the ambiant light.");
        }
    }
}
