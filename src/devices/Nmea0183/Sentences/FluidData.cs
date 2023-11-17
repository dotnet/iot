// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnitsNet;

namespace Iot.Device.Nmea0183.Sentences
{
    /// <summary>
    /// Represents the level of a fluid in a particular tank
    /// </summary>
    public record FluidData
    {
        /// <summary>
        /// Constructs a new instance of this class.
        /// </summary>
        /// <param name="type">The type of fluid</param>
        /// <param name="level">The level of the fluid (0-100%)</param>
        /// <param name="volume">The total volume of the tank</param>
        /// <param name="tankNumber">The number of the tank (if multiple tanks for the same type of fluid are present)</param>
        /// <param name="highLevelIsGood">True if it is "good" to have a full tank (e.g. water, diesel), false if it is better to have
        /// an empty tank (e.g. wastewater)</param>
        public FluidData(FluidType type, Ratio level, Volume volume, int tankNumber, bool highLevelIsGood)
        {
            Type = type;
            Level = level;
            Volume = volume;
            TankNumber = tankNumber;
            HighLevelIsGood = highLevelIsGood;
        }

        /// <summary>
        /// Type of fluid in the tank
        /// </summary>
        public FluidType Type { get; }

        /// <summary>
        /// Tank level
        /// </summary>
        public Ratio Level { get; }

        /// <summary>
        /// Total volume of the tank
        /// </summary>
        public Volume Volume { get; }

        /// <summary>
        /// Number of the tank, useful if several tanks for the same type of fluid exist
        /// </summary>
        public int TankNumber { get; }

        /// <summary>
        /// True if it is "good" to have a full tank (e.g. water, diesel), false if it is better to have
        /// an empty tank (e.g. wastewater)
        /// </summary>
        public bool HighLevelIsGood { get; }
    }
}
