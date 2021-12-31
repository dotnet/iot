// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System;
using System.Globalization;
using System.Xml.Serialization;

namespace Iot.Device.Nmea0183
{
    public partial class DeviationPoint
    {
        /// <summary>
        /// Generates a string representation of this object
        /// </summary>
        public override string ToString()
        {
            return String.Format(CultureInfo.CurrentCulture, "Heading {0}, Deviation {1}", MagneticHeading, Deviation);
        }
    }
}
