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
    /// Industry code used for proprietary NMEA2000 messages.
    /// In about all known cases, the value of this field will be "Marine"
    /// </summary>
    public enum IndustryCode
    {
        /// <summary>
        /// Anything else
        /// </summary>
        Global = 0,

        /// <summary>
        /// Highway patrol
        /// </summary>
        Highway = 1,

        /// <summary>
        /// Farming and agriculture
        /// </summary>
        Agriculture = 2,

        /// <summary>
        /// Construction equipment
        /// </summary>
        Construction = 3,

        /// <summary>
        /// Marine equipment. This is the value used in almost all cases.
        /// </summary>
        Marine = 4,

        /// <summary>
        /// Industrial equipment
        /// </summary>
        Industrial = 5,
    }
}
