// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Iot.Device.Nmea0183.Sentences;

namespace Iot.Device.Nmea0183
{
    /// <summary>
    /// Class creates an NMEA parser on demand. A parser transforms the input into a sequence of <see cref="TalkerSentence"/>
    /// and eventually <see cref="NmeaSentence"/> items.
    /// </summary>
    public interface INmeaParserFactory
    {
        /// <summary>
        /// Creates the parser.
        /// </summary>
        /// <param name="interfaceName">Interface name</param>
        /// <param name="source">The input stream for the parser</param>
        /// <param name="sink">The output stream for the parser, can be null or identical to <paramref name="source"/></param>
        public NmeaParser CreateParser(string interfaceName, Stream source, Stream? sink);
    }
}
