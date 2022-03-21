// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System;
using System.Collections.Generic;
using System.Text;
using Microsoft.Extensions.Logging;
using Xunit;
using Moq;

namespace Iot.Device.Common.Tests
{
    public sealed class LoggingTests : IDisposable
    {
        public void Dispose()
        {
            LogDispatcher.LoggerFactory = null;
        }

        [Fact]
        public void ConsoleLoggerCausesNoException()
        {
            var logger = new SimpleConsoleLogger("Test");
            logger.LogWarning("A test message");
        }

        [Fact]
        public void NotHavingALoggerRegisteredDoesNotCauseAnException()
        {
            LogDispatcher.LoggerFactory = null;
            var logger = this.GetCurrentClassLogger();
            logger.LogDebug("This isn't logged");
        }

        [Fact]
        public void LoggingWorks()
        {
            var loggerFactoryMock = new Mock<ILoggerFactory>(MockBehavior.Strict);
            var loggerMock = new Mock<ILogger>(MockBehavior.Loose);
            loggerFactoryMock.Setup(x => x.CreateLogger("Iot.Device.Common.Tests." + nameof(LoggingTests))).Returns(loggerMock.Object);
            // Unfortunately, this doesn't work, because the exact method being called is Log<FormattedLogValues>(), but that class isn't public
            // loggerMock.Setup(x => x.Log(LogLevel.Information, new EventId(), "This test works (maybe)", null, It.IsAny<Func<string, Exception, string>>()));

            // assign the factory
            LogDispatcher.LoggerFactory = loggerFactoryMock.Object;
            var logger = this.GetCurrentClassLogger();
            Assert.Equal(loggerMock.Object, logger);
            logger.LogInformation("This test works (maybe)");

            loggerFactoryMock.VerifyAll();
            loggerMock.VerifyAll();
        }
    }
}
