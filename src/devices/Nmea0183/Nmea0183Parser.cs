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
    /// Parser for NMEA0183. This is the default parser used by this library.
    /// </summary>
    public class Nmea0183Parser : NmeaParser
    {
        /// <summary>
        /// Creates a new instance of the NmeaParser, taking an input and an output stream
        /// </summary>
        /// <param name="interfaceName">Friendly name of this interface (used for filtering and eventually logging)</param>
        /// <param name="dataSource">Data source (may be connected to a serial port, a network interface, or whatever). It is recommended to use a blocking Stream,
        /// to prevent unnecessary polling</param>
        /// <param name="dataSink">Optional data sink, to send information. Can be null, and can be identical to the source stream</param>
        public Nmea0183Parser(string interfaceName, Stream dataSource, Stream? dataSink)
            : base(interfaceName, dataSource, dataSink, new Raw8BitEncoding())
        {
        }

        /// <inheritdoc/>
        protected internal override TalkerSentence? ParseSentence(string currentLine, out NmeaError error)
        {
            return TalkerSentence.FromSentenceString(currentLine, ExclusiveTalkerId, out error);
        }

        /// <inheritdoc/>
        protected internal override void FormatAndSendSentence(NmeaSentence sentence)
        {
            TalkerSentence ts = new TalkerSentence(sentence);
            string dataToSend = ts.ToString() + "\r\n";
            byte[] buffer = StreamEncoding.GetBytes(dataToSend);

            Sink?.Write(buffer, 0, buffer.Length);
        }
    }
}
