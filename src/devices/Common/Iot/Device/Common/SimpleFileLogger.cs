// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;

namespace Iot.Device.Common
{
    /// <summary>
    /// A simple logger that creates textual log files. Created via <see cref="SimpleFileLoggerFactory"/>
    /// </summary>
    public sealed class SimpleFileLogger : ILogger
    {
        private readonly string _category;
        private TextWriter _writer;

        /// <summary>
        /// Creates a new logger
        /// </summary>
        /// <param name="category">Logger category name</param>
        /// <param name="writer">The text writer for logging.</param>
        /// <remarks>
        /// The <paramref name="writer"/> must be a thread-safe file writer!
        /// </remarks>
        public SimpleFileLogger(string category, TextWriter writer)
        {
            _category = category;
            _writer = writer;
            Enabled = true;
        }

        /// <summary>
        /// Used by the factory to terminate all its loggers
        /// </summary>
        internal bool Enabled
        {
            get;
            set;
        }

        /// <summary>
        /// Does nothing and returns an empty IDisposable
        /// </summary>
        /// <typeparam name="TState">Current logger state</typeparam>
        /// <param name="state">State argument</param>
        /// <returns>An empty <see cref="IDisposable"/></returns>
        public IDisposable BeginScope<TState>(TState state)
            where TState : notnull
        {
            return new LogDispatcher.ScopeDisposable();
        }

        /// <inheritdoc />
        public bool IsEnabled(LogLevel logLevel)
        {
            return Enabled;
        }

        /// <inheritdoc />
        public void Log<TState>(LogLevel logLevel, EventId eventId, TState state, Exception? exception, Func<TState, Exception?, string> formatter)
        {
            if (Enabled)
            {
                string msg = formatter(state, exception);
                var time = DateTime.Now;
                _writer.WriteLine($"{time.ToShortDateString()} {time.ToLongTimeString()} - {_category} - {logLevel} - {msg}");
            }
        }
    }
}
