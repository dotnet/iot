// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;

namespace Iot.Device.Common
{
    /// <summary>
    /// A logger that prints to the debug console
    /// </summary>
    public class DebuggerOutputLogger : ILogger
    {
        /// <summary>
        /// Creates a new instance of the <see cref="DebuggerOutputLogger"/>
        /// </summary>
        public DebuggerOutputLogger()
        {
            MinLogLevel = LogLevel.Debug;
        }

        /// <summary>
        /// Sets the minimum log level
        /// </summary>
        public LogLevel MinLogLevel
        {
            get;
            set;
        }

        /// <inheritdoc />
        public IDisposable? BeginScope<TState>(TState state)
            where TState : notnull
        {
            return new LogDispatcher.ScopeDisposable();
        }

        /// <inheritdoc />
        public bool IsEnabled(LogLevel logLevel)
        {
            return logLevel >= MinLogLevel;
        }

        /// <inheritdoc />
        public void Log<TState>(LogLevel logLevel, EventId eventId, TState state, Exception? exception, Func<TState, Exception?, string> formatter)
        {
            string msg = formatter(state, exception);
            Debug.WriteLine(msg);
        }
    }
}
