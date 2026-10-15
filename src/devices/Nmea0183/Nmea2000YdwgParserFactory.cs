// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Iot.Device.Nmea0183
{
    /// <summary>
    /// The parser factory for YDWG devices.
    /// </summary>
    public sealed class Nmea2000YdwgParserFactory : INmeaParserFactory
    {
        /// <inheritdoc/>
        public NmeaParser CreateParser(string interfaceName, Stream source, Stream? sink)
        {
            return new Nmea2000YdwgParser(interfaceName, source, sink);
        }
    }
}
