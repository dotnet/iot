// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Iot.Device.Common;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;

namespace ArduinoCsCompiler
{
    public static class ErrorManager
    {
        private static List<CompilerMessage> _messages = new List<CompilerMessage>();

        static ErrorManager()
        {
            Logger = NullLogger.Instance;
            ShowProgress = false;
        }

        public static ILogger Logger
        {
            get;
            set; // Set to something usable on startup by Run<T>
        }

        /// <summary>
        /// Set this to false to suppress printing progress messages (e.g. for CI environments)
        /// </summary>
        /// <remarks>Default is false, to suppress progress during CI (just clutters the output)</remarks>
        public static bool ShowProgress
        {
            get;
            set;
        }

        /// <summary>
        /// Returns the number of errors currently in the list
        /// </summary>
        public static int NumErrors
        {
            get
            {
                return _messages.Count(x => x.Level == LogLevel.Error);
            }
        }

        /// <summary>
        /// Returns the number of warnings currently in the list
        /// </summary>
        public static int NumWarnings
        {
            get
            {
                return _messages.Count(x => x.Level == LogLevel.Warning);
            }
        }

        public static void Add(CompilerMessage msg)
        {
            // Avoid exact duplicates
            if (_messages.Contains(msg))
            {
                return;
            }

            Logger.Log(msg.Level, $"{msg.ErrorCode}: {msg.Message}");
            _messages.Add(msg);
        }

        public static void Add(LogLevel level, string errorCode, string message)
        {
            Add(new CompilerMessage(level, errorCode, message));
        }

        public static void AddWarning(string errorCode, string message)
        {
            Add(new CompilerMessage(LogLevel.Warning, errorCode, message));
        }

        public static void AddError(string errorCode, string message)
        {
            Add(new CompilerMessage(LogLevel.Error, errorCode, message));
        }

        public static void PrintImporantMessages()
        {
            Console.WriteLine("Repeating warnings and errors:");
            foreach (var msg in _messages)
            {
                Console.WriteLine(msg.ToString());
            }
        }

        public static void Clear()
        {
            _messages.Clear();
        }
    }
}
