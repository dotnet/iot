// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using System;
using System.Diagnostics;

namespace Iot.Device.Rfid
{
    /// <summary>
    /// The log level
    /// </summary>
    public enum LogLevel
    {
        None = 0,
        Info = 1,
        Debug = 2
    }

    /// <summary>
    /// Output where to log the information
    /// </summary>
    [Flags]
    public enum LogTo
    {
        Console = 0b0000_00001,
        Debug = 0b0000_0010
    }

    /// <summary>
    /// Simple log class to help in debugging the communication
    /// between the PN532 and the host
    /// </summary>
    public class LogInfo
    {
        public static LogLevel LogLevel { get; set; }

        public static LogTo LogTo { get; set; } = LogTo.Console;

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

