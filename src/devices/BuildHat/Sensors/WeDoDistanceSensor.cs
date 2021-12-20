using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Iot.Device.BuildHat.Models;

namespace Iot.Device.BuildHat.Sensors
{
    /// <summary>
    /// WeDo distance sensor.
    /// </summary>
    public class WeDoDistanceSensor : ActiveSensor
    {
        private int _distance;
        private bool _hasDistanceUpdated;
        private bool _continuous;

        /// <summary>
        /// Gets the distance. A number between 0 and 10 cm.
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

        internal WeDoDistanceSensor(Brick brick, SensorPort port)
            : base(brick, port, SensorType.WeDoDistanceSensor)
        {
        }

        /// <summary>
        /// Gets the distance. From 0 to +10 cm.
        /// </summary>
        /// <returns></returns>
        public int GetDistance()
        {
            if (SetupModeAndRead(0, ref _hasDistanceUpdated, ContinousMeasurement))
            {
                return Distance;
            }

            throw new IOException("Can't measure the distance.");
        }
    }
}
