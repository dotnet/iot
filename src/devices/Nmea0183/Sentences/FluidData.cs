// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnitsNet;

#pragma warning disable CS1591
namespace Iot.Device.Nmea0183.Sentences
{
    public record FluidData
    {
        public FluidData(FluidType type, Ratio level, Volume volume, int tankNumber, bool highLevelIsGood)
        {
            Type = type;
            Level = level;
            Volume = volume;
            TankNumber = tankNumber;
            HighLevelIsGood = highLevelIsGood;
        }

        public FluidType Type { get; }

        public Ratio Level { get; }

        public Volume Volume { get; }

        public int TankNumber { get; }

        public bool HighLevelIsGood { get; }
    }
}
