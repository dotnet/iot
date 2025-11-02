// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;

#pragma warning disable 1591
namespace Iot.Device.Common
{
    public class MultiTargetLoggerFactory : ILoggerFactory
    {
        private List<ILoggerFactory> _factories = new List<ILoggerFactory>();
        public MultiTargetLoggerFactory()
        {
        }

        public MultiTargetLoggerFactory(params ILoggerFactory[] factories)
        {
            _factories = factories.ToList();
        }

        public void AddProvider(ILoggerProvider provider)
        {
        }

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

        public void Dispose()
        {
            foreach (var d in _factories)
            {
                d.Dispose();
            }
        }
    }
}
