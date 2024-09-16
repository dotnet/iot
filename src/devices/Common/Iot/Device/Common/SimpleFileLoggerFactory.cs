// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;

namespace Iot.Device.Common
{
    /// <summary>
    /// Provides a very simple console logger that does not require a reference to Microsoft.Extensions.Logging.dll
    /// </summary>
    public class SimpleFileLoggerFactory : ILoggerFactory, IDisposable
    {
        private TextWriter? _writer;
        private List<SimpleFileLogger> _createdLoggers;

        /// <summary>
        /// Create a logger factory that creates loggers to logs to the specified file
        /// </summary>
        /// <param name="fileName">File name to log to (full path)</param>
        public SimpleFileLoggerFactory(string fileName)
        {
            _writer = TextWriter.Synchronized(new StreamWriter(fileName, true, Encoding.UTF8));
            _createdLoggers = new List<SimpleFileLogger>();
        }

        /// <summary>
        /// The console logger is built-in here
        /// </summary>
        /// <param name="provider">Argument is ignored</param>
        public void AddProvider(ILoggerProvider provider)
        {
        }

        /// <inheritdoc/>
        public ILogger CreateLogger(string categoryName)
        {
            if (_writer == null)
            {
                return NullLogger.Instance;
            }

            var newLogger = new SimpleFileLogger(categoryName, _writer);
            _createdLoggers.Add(newLogger);
            return newLogger;
        }

        /// <inheritdoc />
        public void Dispose()
        {
            foreach (var d in _createdLoggers)
            {
                d.Enabled = false;
            }

            _createdLoggers.Clear();

            if (_writer != null)
            {
                _writer.Close();
                _writer.Dispose();
                _writer = null;
            }
        }
    }
}
