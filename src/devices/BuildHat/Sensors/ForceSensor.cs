﻿using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Iot.Device.BuildHat.Models;

namespace Iot.Device.BuildHat.Sensors
{
    /// <summary>
    /// Spike force sensor
    /// </summary>
    public class ForceSensor : ActiveSensor
    {
        private int _force;
        private bool _hasForceUpdated;
        private bool _continuous;
        private bool _isPressed;
        private bool _hasIsPressedUpdated;

        /// <summary>
        /// Gets the force in Newtown.
        /// </summary>
        public int Force
        {
            get => _force;
            internal set
            {
                if (_force != value)
                {
                    _force = value;
                    OnPropertyChanged(nameof(Force));
                }

                _hasForceUpdated = true;
                OnPropertyUpdated(nameof(Force));
            }
        }

        /// <summary>
        /// Gets the force in Newtown.
        /// </summary>
        public bool IsPressed
        {
            get => _isPressed;
            internal set
            {
                if (_isPressed != value)
                {
                    _isPressed = value;
                    OnPropertyChanged(nameof(IsPressed));
                }

                _hasIsPressedUpdated = true;
                OnPropertyUpdated(nameof(IsPressed));
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

        internal ForceSensor(Brick brick, SensorPort port)
            : base(brick, port, SensorType.SpikePrimeForceSensor)
        {
        }

        /// <summary>
        /// Gets the force in N
        /// </summary>
        /// <returns></returns>
        public int GetDistance()
        {
            if (SetupModeAndRead(0, ref _hasForceUpdated, ContinousMeasurement))
            {
                return Force;
            }

            throw new IOException("Can't measure the force.");
        }

        /// <summary>
        /// Gets the force in N
        /// </summary>
        /// <returns></returns>
        public bool GetPressed()
        {
            if (SetupModeAndRead(0, ref _hasIsPressedUpdated, ContinousMeasurement))
            {
                return IsPressed;
            }

            throw new IOException("Can't measure is the sensor is pressed.");
        }

        /// <inheritdoc/>
        public override string SensorName => "SPIKE force sensor sensor";
    }
}