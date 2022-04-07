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
            Position = new GeographicPosition(47.45, 9.59, 451.2);
            Course = Angle.FromDegrees(350);
            Speed = Speed.FromKnots(4.8);
        }

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

        public Speed Speed
        {
            get;
            set;
        }

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
