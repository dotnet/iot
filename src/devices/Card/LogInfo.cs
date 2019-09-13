// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using System;
using System.Diagnostics;

namespace Iot.Device.Card
{
    /// <summary>
    /// The log level
    /// </summary>
    public enum LogLevel
    {
        /// <summary>
        /// No log level
        /// </summary>
        None = 0,
        /// <summary>
        /// Only information
        /// </summary>
        Info = 1,
        /// <summary>
        /// Deep log for debug purpose
        /// </summary>
        Debug = 2
    }

    /// <summary>
    /// Output where to log the information
    /// </summary>
    [Flags]
    public enum LogTo
    {
        /// <summary>
        /// Log to console
        /// </summary>
        Console = 0b0000_00001,
        /// <summary>
        /// Log to debug
        /// </summary>
        Debug = 0b0000_0010
    }

    /// <summary>
    /// Simple log class to help in debugging the communication
    /// between the PN532 and the host
    /// </summary>
    public class LogInfo
    {
        /// <summary>
        /// Log Level
        /// </summary>
        public static LogLevel LogLevel { get; set; }

        /// <summary>
        /// Log to
        /// </summary>
        public static LogTo LogTo { get; set; } = LogTo.Console;

        /// <summary>
        /// Log something
        /// </summary>
        /// <param name="toLog">String to log</param>
        /// <param name="logLevel">Log level</param>
        public static void Log(string toLog, LogLevel logLevel)
        {
            if (LogLevel >= logLevel)
            {
                if ((LogTo & LogTo.Console) == LogTo.Console)
                    Console.WriteLine(toLog);
                if ((LogTo & LogTo.Debug) == LogTo.Debug)
                    Debug.WriteLine(toLog);
            }
        }
    }
}

