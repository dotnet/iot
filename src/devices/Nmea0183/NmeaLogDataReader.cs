// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Threading;
using Iot.Device.Nmea0183.Sentences;

namespace Iot.Device.Nmea0183
{
    /// <summary>
    /// This source can be used to play back a recorded log file.
    /// The playback currently happens as fast as possible, the time stamp in the log is ignored.
    /// </summary>
    public class NmeaLogDataReader : NmeaSinkAndSource
    {
        private readonly IEnumerable<string> _filesToRead;

        /// <summary>
        /// Reads a log file and uses it as a source
        /// </summary>
        /// <param name="interfaceName">Name of this interface</param>
        /// <param name="filesToRead">Files to read. Either a | delimited log or a plain text file</param>
        public NmeaLogDataReader(string interfaceName, IEnumerable<string> filesToRead)
            : base(interfaceName)
        {
            _filesToRead = filesToRead;
        }

        /// <summary>
        /// Reads a log file and uses it as a source
        /// </summary>
        /// <param name="interfaceName">Name of this interface</param>
        /// <param name="fileToRead">File to read. Either a | delimited log or a plain text file</param>
        public NmeaLogDataReader(string interfaceName, string fileToRead)
            : base(interfaceName)
        {
            _filesToRead = new List<string>()
            {
                fileToRead
            };
        }

        /// <inheritdoc />
        public override void StartDecode()
        {
            // TODO: This is a bit memory hungry, because all data gets read in first, but the implementation is only suited for statistical analysis now and
            // not for replaying large data sets. Can be improved later.
            MemoryStream ms = new MemoryStream();
            foreach (var fileToRead in _filesToRead)
            {
                using (StreamReader sr = File.OpenText(fileToRead))
                {
                    string? line = sr.ReadLine();
                    Encoding raw = new Raw8BitEncoding();
                    while (line != null)
                    {
                        if (line.Contains("|"))
                        {
                            var splits = line.Split(new char[]
                            {
                                '|'
                            }, StringSplitOptions.None);
                            line = splits[2]; // Raw message
                        }

                        // Pack data into memory stream, from which the parser will get it again
                        byte[] bytes = raw.GetBytes(line + "\r\n");
                        ms.Write(bytes, 0, bytes.Length);
                        line = sr.ReadLine();
                    }
                }
            }

            ms.Position = 0;
            ManualResetEvent ev = new ManualResetEvent(false);
            var parser = new NmeaParser(InterfaceName, ms, null);
            parser.SuppressOutdatedMessages = false; // parse all incoming messages, ignoring any timing
            parser.OnNewSequence += ForwardDecoded;
            parser.OnParserError += (source, s, error) =>
            {
                if (error == NmeaError.PortClosed)
                {
                    ev.Set();
                }
            };
            parser.StartDecode();
            ev.WaitOne(); // Wait for end of file
            parser.StopDecode();
            ms.Dispose();
        }

        private void ForwardDecoded(NmeaSinkAndSource source, NmeaSentence sentence)
        {
            DispatchSentenceEvents(sentence);
        }

        /// <inheritdoc />
        public override void SendSentence(NmeaSinkAndSource source, NmeaSentence sentence)
        {
        }

        /// <inheritdoc />
        public override void StopDecode()
        {
        }
    }
}
