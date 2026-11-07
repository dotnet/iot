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
    /// A logger factory that can log to multiple targets. This is useful for logging to both console and file, for example.
    /// </summary>
    public class MultiTargetLoggerFactory : ILoggerFactory
    {
        private List<ILoggerFactory> _factories = new List<ILoggerFactory>();

        /// <summary>
        /// Creates an empty MultiTargetLoggerFactory. You can add ILoggerFactory instances to it using the AddProvider method.
        /// Without any further configuration, this logger factory will not log anything.
        /// You can also use the constructor that takes an array of ILoggerFactory instances to create a MultiTargetLoggerFactory that logs to multiple targets.
        /// </summary>
        public MultiTargetLoggerFactory()
        {
        }

        /// <summary>
        /// Crates a MultiTargetLoggerFactory that logs to multiple targets. You can add ILoggerFactory instances to it using the AddProvider method.
        /// </summary>
        /// <param name="factories">A list of factories to use</param>
        public MultiTargetLoggerFactory(params ILoggerFactory[] factories)
        {
            _factories = factories.ToList();
        }

        /// <summary>
        /// Add a new loger factory to this MultiTargetLoggerFactory. This will allow logging to multiple targets.
        /// </summary>
        /// <param name="provider">The logger provider to add</param>
        public void AddProvider(ILoggerProvider provider)
        {
        }

        /// <summary>
        /// Creates a new logger.
        /// </summary>
        /// <param name="categoryName">The name of the category to create a logger for</param>
        /// <returns></returns>
        public ILogger CreateLogger(string categoryName)
        {
            return new MultiTargetLogger(categoryName, _factories);
        }

        private class MultiTargetLogger : ILogger
        {
            private List<ILogger> _loggers;

            public MultiTargetLogger(string name, List<ILoggerFactory> factories)
            {
                _loggers = new List<ILogger>();
                foreach (var f in factories)
                {
                    _loggers.Add(f.CreateLogger(name));
                }
            }

            public void Log<TState>(LogLevel logLevel, EventId eventId, TState state, Exception? exception, Func<TState, Exception?, string> formatter)
            {
                foreach (var l in _loggers)
                {
                    l.Log<TState>(logLevel, eventId, state, exception, formatter);
                }
            }

            public bool IsEnabled(LogLevel logLevel)
            {
                if (_loggers.Count == 0)
                {
                    return false;
                }

                return _loggers.Any(x => x.IsEnabled(logLevel));
            }

            public IDisposable BeginScope<TState>(TState state)
                where TState : notnull
            {
                return new InnerScopeDisposable<TState>(state, _loggers);
            }

            private sealed class InnerScopeDisposable<TState> : IDisposable
                where TState : notnull
            {
                private readonly List<IDisposable> _disposables;

                public InnerScopeDisposable(TState state, List<ILogger> loggers)
                {
                    _disposables = new List<IDisposable>();
                    foreach (var l in loggers)
                    {
                        var scopeDisposable = l.BeginScope(state);
                        if (scopeDisposable != null)
                        {
                            _disposables.Add(scopeDisposable);
                        }
                    }
                }

                public void Dispose()
                {
                    foreach (var d in _disposables)
                    {
                        d.Dispose();
                    }

                    _disposables.Clear();
                }
            }
        }

        /// <inheritdoc />
        public void Dispose()
        {
            foreach (var d in _factories)
            {
                d.Dispose();
            }
        }
    }
}
