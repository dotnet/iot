// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System.IO;
using Iot.Device.BuildHat.Models;

namespace Iot.Device.BuildHat.Sensors
{
    /// <summary>
    /// Color and distance sensor.
    /// </summary>
    public class ColorAndDistanceSensor : ColorSensor
    {
        private int _distance;
        private bool _hasDistanceUpdated;
        private int _counter;
        private bool _hasCounterUpdated;

        /// <summary>
        /// Gets the distance from the object
        /// </summary>
        public int Distance
        {
            get => _distance;
            internal set
            {
                if (_distance != value)
                {
                    _distance = value;
                    OnPropertyChanged(nameof(Distance));
                }

                _hasDistanceUpdated = true;
                OnPropertyUpdated(nameof(Distance));
            }
        }

        /// <summary>
        /// Gets the counter of cumulated object detected.
        /// </summary>
        public int Counter
        {
            get => _counter;
            internal set
            {
                if (_counter != value)
                {
                    _counter = value;
                    OnPropertyChanged(nameof(Counter));
                }

                _hasCounterUpdated = true;
                OnPropertyUpdated(nameof(Counter));
            }
        }

        /// <inheritdoc/>
        public override string SensorName => "Color and distance sensor";

        /// <summary>
        /// Creates a color and distance sensor.
        /// </summary>
        /// <param name="brick">The brick.</param>
        /// <param name="port">The port.</param>
        protected internal ColorAndDistanceSensor(Brick brick, SensorPort port)
            : base(brick, port, SensorType.ColourAndDistanceSensor)
        {
        }

        /// <summary>
        /// Gets the distance of the object from 0 to +10 cm
        /// </summary>
        /// <returns>The distance from 0 to +10 cm.</returns>
        public int GetDistance()
        {
            if (SetupModeAndRead(1, ref _hasDistanceUpdated))
            {
                return Distance;
            }

            throw new IOException("Can't measure the distance.");
        }

        /// <summary>
        /// Gets the the counter of cumulated object detected.
        /// </summary>
        /// <returns>The counter.</returns>
        public int GetCounter()
        {
            if (SetupModeAndRead(2, ref _hasCounterUpdated))
            {
                return Counter;
            }

            throw new IOException("Can't update the counter.");
        }
    }
}
