// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using Iot.Device.Ssd13xx.Commands;
using Iot.Device.Ssd13xx.Commands.Ssd1306Commands;
using Xunit;

namespace Iot.Device.Ssd13xx.Tests
{
    public class SetNormalDisplayTests
    {
        [Fact]
        public void Get_Bytes()
        {
            SetNormalDisplay setNormalDisplay = new SetNormalDisplay();
            byte[] actualBytes = setNormalDisplay.GetBytes();
            Assert.Equal(new byte[] { 0xA6 }, actualBytes);
        }
    }
}
