// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System;
using System.Threading;
using Iot.Device.Common;
using Iot.Device.Common.Samples;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.Extensions.Logging.Console;
using Microsoft.Extensions.Options;

namespace Common.Samples.Test
{
    internal static class Program
    {
        public static int Main(string[] args)
        {
            LogWithStandardProvider();

            LogWithSimpleProvider();

            return 0;
        }

        /// <summary>
        /// This example shows how to enable logging to the console by using the implementations in
        /// Microsoft.Extensions.Logging.dll and Microsoft.Extensions.Logging.Console.dll. Both packages need to be referenced.
        /// </summary>
        private static void LogWithStandardProvider()
        {
            using var loggerFactory = LoggerFactory.Create(builder =>
            {
                builder
                    .AddConsole();
            });

            // Statically register our factory. Note that this must be done before instantiation of any class that wants to use logging.
            LogDispatcher.LoggerFactory = loggerFactory;

            var testee = new MyTestComponent();

            testee.DoSomeLogging();
            LogDispatcher.LoggerFactory = null;
        }

        /// <summary>
        /// This example shows how to use logging with a very simple console logger that does not need any extra references.
        /// </summary>
        private static void LogWithSimpleProvider()
        {
            LogDispatcher.LoggerFactory = new SimpleConsoleLoggerFactory();

            var testee = new MyTestComponent();

            testee.DoSomeLogging();
            LogDispatcher.LoggerFactory = null;
        }
    }
}
