// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Text;
using Iot.Device.Nmea0183.Sentences;
using UnitsNet;

namespace Iot.Device.Nmea0183
{
    /// <summary>
    /// This is a sink for writing received NMEA sentences to a log file. The log file
    /// can be used for later analysis or replay.
    /// </summary>
    public sealed class LoggingSink : NmeaSinkAndSource
    {
        private object _lock;
        private FileStream? _logFile;
        private TextWriter? _textWriter;
        private NmeaSentence _lastSentence;

        /// <summary>
        /// Creates an instance of this class
        /// </summary>
        /// <param name="name">Name of the sink</param>
        /// <param name="configuration">Logging configuration</param>
        public LoggingSink(string name, LoggingConfiguration? configuration)
        : base(name)
        {
            _logFile = null;
            _lock = new object();
            // So we do not need to do a null test later
            _lastSentence = new CrossTrackError(Length.Zero);
            Configuration = configuration ?? new LoggingConfiguration();
        }

        /// <summary>
        /// Gets the active logging configuration
        /// </summary>
        public LoggingConfiguration Configuration
        {
            get;
        }

        /// <inheritdoc />
        public override void StartDecode()
        {
            lock (_lock)
            {
                if (!string.IsNullOrWhiteSpace(Configuration.Path))
                {
                    StartNewFile();
                }
            }
        }

        private void StartNewFile()
        {
            var now = DateTime.UtcNow;
            string path = Configuration.Path;
            string fileName;
            if (Configuration.SortByDate)
            {
                path = Path.Combine(path, "Log-" + now.ToString("yyyy-MM-dd", CultureInfo.InvariantCulture));
                string file = now.ToString("yyyy-MM-dd-HH-mm", CultureInfo.InvariantCulture);
                fileName = Path.Combine(path, "Nmea-" + file + ".txt");
            }
            else
            {
                string file = now.ToString("yyyy-MM-dd-HH-mm", CultureInfo.InvariantCulture);
                fileName = Path.Combine(path, "Nmea-" + file + ".txt");
            }

            if (_logFile != null && _textWriter != null)
            {
                _textWriter.Flush();
                _textWriter.Close();
                _logFile.Close();
                _textWriter.Dispose();
                _logFile.Dispose();
                _textWriter = null;
                _logFile = null;
            }

            Directory.CreateDirectory(path);
            _logFile = new FileStream(fileName, FileMode.Append, FileAccess.Write);
            _textWriter = new StreamWriter(_logFile);
        }

        /// <inheritdoc />
        public override void SendSentence(NmeaSinkAndSource source, NmeaSentence sentence)
        {
            lock (_lock)
            {
                if (_textWriter != null && _logFile != null)
                {
                    // If talker and ID are the same and the current message is a raw, assume it's the same message again
                    // (Avoids logging the same message as raw and decoded)
                    // But log all AIS messages, they come as raw only
                    if ((_lastSentence.SentenceId != sentence.SentenceId && _lastSentence.TalkerId != sentence.TalkerId) || !(sentence is RawSentence) || sentence.TalkerId == TalkerId.Ais)
                    {
                        string msg = FormattableString.Invariant(
                            $"{DateTime.UtcNow:s}|{source.InterfaceName}|${sentence.TalkerId}{sentence.SentenceId},{sentence.ToNmeaParameterList()}|{sentence.ToReadableContent()}");
                        _textWriter.WriteLine(msg);
                    }

                    if ((_logFile.Length > Configuration.MaxFileSize) && (Configuration.MaxFileSize != 0))
                    {
                        StartNewFile();
                    }

                    _lastSentence = sentence;
                }
            }
        }

        /// <inheritdoc />
        public override void StopDecode()
        {
            lock (_lock)
            {
                if (_logFile != null && _textWriter != null)
                {
                    _textWriter.Flush();
                    _logFile.Close();
                    _logFile = null;
                    _textWriter = null;
                }
            }
        }
    }
}
