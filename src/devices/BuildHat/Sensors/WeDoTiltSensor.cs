// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System.Drawing;
using System.IO;
using Iot.Device.BuildHat.Models;

namespace Iot.Device.BuildHat.Sensors
{
    /// <summary>
    /// WeDO tilt sensor
    /// </summary>
    public class WeDoTiltSensor : ActiveSensor
    {
        private Point _tilt;
        private bool _hasTiltUpdated;
        private bool _continuous;

        /// <summary>
        /// Gets the tilt. It's an angle from -45 to +45
        /// </summary>
        public Point Tilt
        {
            get => _tilt;
            internal set
            {
                if (_tilt != value)
                {
                    _tilt = value;
                    OnPropertyChanged(nameof(Tilt));
                }

                _hasTiltUpdated = true;
                OnPropertyUpdated(nameof(Tilt));
            }
        }

        /// <summary>
        /// Gets or sets the continuous measurement for this sensor.
        /// </summary>
        public bool ContinousMeasurement
        {
            get => _continuous;
            set
            {
                if (_continuous != value)
                {
                    _continuous = value;
                    if (_continuous)
                    {
                        Brick.SelectModeAndRead(Port, 0, _continuous);
                    }
                    else
                    {
                        Brick.StopContinuousReadingSensor(Port);
                    }
                }
            }
        }

        /// <inheritdoc/>
        public override string SensorName => "WeDo tilt sensor";

        /// <summary>
        /// WeDo tilt sensor.
        /// </summary>
        /// <param name="brick">The brick.</param>
        /// <param name="port">The port.</param>
        protected internal WeDoTiltSensor(Brick brick, SensorPort port)
            : base(brick, port, SensorType.WeDoTiltSensor)
        {
        }

        /// <summary>
        /// Gets the tilt
        /// </summary>
        /// <returns></returns>
        /// <exception cref="IOException"></exception>
        public Point GetTilt()
        {
            if (SetupModeAndRead(0, ref _hasTiltUpdated))
            {
                return Tilt;
            }

            throw new IOException("Can't measure the tilt.");
        }
    }
}
