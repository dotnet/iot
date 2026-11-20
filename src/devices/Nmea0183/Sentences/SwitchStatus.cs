// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Iot.Device.Nmea0183.Sentences
{
    /// <summary>
    /// This indicates the state of a binary switch
    /// </summary>
    public enum SwitchStatus
    {
        /// <summary>
        /// The switch is off (or should be off when commanded)
        /// </summary>
        Off = 0,

        /// <summary>
        /// The switch is on (or should be on when commanded)
        /// </summary>
        On = 1,

        /// <summary>
        /// Reserved value
        /// </summary>
        Reserved = 2,

        /// <summary>
        /// Only when commanding: Do not touch this switch
        /// </summary>
        NoAction = 3,
    }
}
