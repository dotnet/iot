// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;

namespace Iot.Device.Common.Samples
{
    internal class MyTestComponent
    {
        private ILogger _logger;

        public MyTestComponent()
        {
            _logger = this.GetCurrentClassLogger();
        }

        public void DoSomeLogging()
        {
            _logger.LogInformation("An informative message");
            _logger.LogError("An error situation");
            _logger.LogWarning(new PlatformNotSupportedException("Something is not supported"), "With exception context");
        }
    }
}
