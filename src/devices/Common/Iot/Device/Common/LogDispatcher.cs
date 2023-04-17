// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System;
using System.Collections.Generic;
using Microsoft.Extensions.Logging;

namespace Iot.Device.Common
{
    /// <summary>
    /// This class contains static members that provide log support.
    /// </summary>
    public static class LogDispatcher
    {
        private static object _lock = new object();

        /// <summary>
        /// The default logger factory for the whole assembly.
        /// If this is null (the default), logging is disabled
        /// </summary>
        public static ILoggerFactory? LoggerFactory { get; set; } = null;

        /// <summary>
        /// Gets a logger with the given name
        /// </summary>
        /// <param name="loggerName">Name of the logger</param>
        /// <returns>A reference to a <see cref="ILogger"/>.</returns>
        public static ILogger GetLogger(string loggerName)
        {
            if (LoggerFactory == null)
            {
                return new NullLogger();
            }

            return LoggerFactory.CreateLogger(loggerName);
        }

        /// <summary>
        /// Gets a logger with the name of the current class
        /// </summary>
        /// <param name="currentClass">The class whose logger shall be retrieved</param>
        /// <returns>A <see cref="ILogger"/> instance</returns>
        public static ILogger GetCurrentClassLogger(this object currentClass)
        {
            string? name = currentClass.GetType().FullName;

            // This is true if the method is used from an incomplete (generic) type
            if (name == null)
            {
                name = currentClass.GetType().Name;
            }

            return GetLogger(name);
        }

        private class NullLogger : ILogger
        {
            public void Log<TState>(LogLevel logLevel, EventId eventId, TState state, Exception? exception, Func<TState, Exception?, string> formatter)
            {
            }

            public bool IsEnabled(LogLevel logLevel)
            {
                return false;
            }

            public IDisposable? BeginScope<TState>(TState state)
                where TState : notnull
            {
                return new ScopeDisposable();
            }
        }

        /// <summary>
        /// This doesn't really do anything
        /// </summary>
        internal sealed class ScopeDisposable : IDisposable
        {
            public void Dispose()
            {
            }
        }
    }
}
