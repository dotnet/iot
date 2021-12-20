using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Iot.Device.BuildHat;
using Iot.Device.BuildHat.Models;
using Iot.Device.BuildHat.Sensors;
using SixLabors.ImageSharp;

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

        internal WeDoTiltSensor(Brick brick, SensorPort port)
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
