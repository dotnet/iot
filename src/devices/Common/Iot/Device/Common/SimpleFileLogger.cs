// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;

#pragma warning disable 1591
namespace Iot.Device.Common
{
    public sealed class SimpleFileLogger : ILogger
    {
        private readonly string _category;
        private TextWriter _writer;

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

        public IDisposable BeginScope<TState>(TState state)
            where TState : notnull
        {
            return new LogDispatcher.ScopeDisposable();
        }

        public bool IsEnabled(LogLevel logLevel)
        {
            return Enabled;
        }

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
