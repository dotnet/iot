// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;

namespace Iot.Device.Common
{
    /// <summary>
    /// Provides a very simple console logger that does not require a reference to Microsoft.Extensions.Logging.dll
    /// </summary>
    public class SimpleConsoleLoggerFactory : ILoggerFactory
    {
        /// <summary>
        /// Constructs a console logger with the default log level.
        /// </summary>
        public SimpleConsoleLoggerFactory()
        {
            MinLogLevel = LogLevel.Information;
        }

        /// <summary>
        /// Constructs a logger factory that creates loggers with the given log level
        /// </summary>
        /// <param name="minLogLevel">Minimum log level to use</param>
        public SimpleConsoleLoggerFactory(LogLevel minLogLevel)
        {
            MinLogLevel = minLogLevel;
        }

        /// <summary>
        /// Sets the log level for new loggers.
        /// Note that changing this value will only affect new loggers
        /// </summary>
        public LogLevel MinLogLevel
        {
            get;
            set;
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
            return new SimpleConsoleLogger(categoryName, MinLogLevel);
        }

        /// <inheritdoc />
        public void Dispose()
        {
        }
    }
}
