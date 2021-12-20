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
    /// Spike distance sensor.
    /// </summary>
    public class DistanceSensor : ActiveSensor
    {
        private int _distance;
        private bool _hasDistanceUpdated;
        private bool _continuous;

        /// <summary>
        /// Gets the distance. A number is in millimeters.
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
                        Brick.SelectModeAndRead(Port, 1, _continuous);
                    }
                    else
                    {
                        Brick.StopContinuousReadingSensor(Port);
                    }
                }
            }
        }

        internal DistanceSensor(Brick brick, SensorPort port)
            : base(brick, port, SensorType.SpikePrimeUltrasonicDistanceSensor)
        {
            Brick.SendRawCommand($"port {(byte)Port} ; plimit 1 ; set -1\r");
        }

        /// <summary>
        /// Gets the distance. From 0 to +10 cm.
        /// </summary>
        /// <returns></returns>
        public int GetDistance()
        {
            if (SetupModeAndRead(1, ref _hasDistanceUpdated, ContinousMeasurement))
            {
                return Distance;
            }

            throw new IOException("Can't measure the distance.");
        }

        /// <summary>
        /// Adjust the brightness of the eyes.
        /// </summary>
        /// <param name="eyes">The brighness percentage for each of the 4 leds from 0 to 100.</param>
        public void AdjustEyesBrightness(ReadOnlySpan<byte> eyes)
        {
            if (eyes.Length != 4)
            {
                throw new ArgumentException("You must have exactly 4 brightness for the eyes leds.");
            }

            Brick.SendRawCommand($"port {(byte)Port} ; select 5 ; write1 {eyes[0]:X2} {eyes[1]:X2} {eyes[2]:X2} {eyes[3]:X2}");
            if (ContinousMeasurement)
            {
                // Set continuous mode if it was present
                Brick.SelectModeAndRead(Port, 1, _continuous);
            }
        }

        /// <inheritdoc/>
        public override string SensorName => "SPIKE ultrasonic distance sensor";
    }
}
