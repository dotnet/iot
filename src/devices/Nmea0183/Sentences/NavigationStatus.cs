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
    /// Navigation status, for use in <see cref="RecommendedMinimumNavigationInformation"/>
    /// </summary>
    public enum NavigationStatus : byte
    {
        /// <summary>
        /// Valid is represented with an "A"
        /// </summary>
        Valid = (byte)'A',

        /// <summary>
        /// A warning is represented by a "V"
        /// </summary>
        NavigationReceiverWarning = (byte)'V',
    }
}
