// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Xunit;

namespace Iot.Device.Arduino.Tests
{
    public class DebugFormattingIssues
    {
        [Fact]
        public void FormatDouble()
        {
            double value = 20.23;
            string formatted = Number.FormatDouble(value, null, NumberFormatInfo.CurrentInfo);
            Assert.Equal("20.23", formatted);
        }
    }
}
