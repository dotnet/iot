// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System;
using System.Collections.Generic;
using System.Text;
using Iot.Device.Common;
using Iot.Device.Nmea0183;
using UnitsNet;

namespace Nmea.Simulator
{
    internal class SimulatorData : ICloneable
    {
        public SimulatorData()
        {
            Position = new GeographicPosition(47.49, 9.50, 451.2);
            Course = Angle.FromDegrees(350);
            SpeedOverGround = Speed.FromKnots(4.8);
            WindSpeedRelative = Speed.FromKnots(10.2);
            WindDirectionRelative = Angle.FromDegrees(-10.0);
            SpeedTroughWater = Speed.FromKnots(5.2);
        }

        public Angle WindDirectionRelative { get; set; }

        public Speed WindSpeedRelative { get; set; }

        public GeographicPosition Position
        {
            get;
            set;
        }

        public Angle Course
        {
            get;
            set;
        }

        public Speed SpeedOverGround
        {
            get;
            set;
        }

        public Speed SpeedTroughWater { get; set; }

        object ICloneable.Clone()
        {
            return MemberwiseClone();
        }

        public SimulatorData Clone()
        {
            return (SimulatorData)MemberwiseClone();
        }
    }
}
