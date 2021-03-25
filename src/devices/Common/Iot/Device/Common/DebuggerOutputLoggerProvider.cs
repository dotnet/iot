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
    /// Creates a debugger logger
    /// </summary>
    public class DebuggerOutputLoggerProvider : ILoggerProvider
    {
        /// <inheritdoc />
        public ILogger CreateLogger(string categoryName)
        {
            return new DebuggerOutputLogger();
        }

        /// <inheritdoc />
        public void Dispose()
        {
        }
    }
}
