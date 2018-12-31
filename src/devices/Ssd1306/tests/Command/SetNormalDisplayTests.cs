// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using Iot.Device.Ssd1306.Command;
using Xunit;

namespace Iot.Device.Mcp23xxx.Tests
{
    public class SetNormalDisplayTests
    {
        [Theory]
        [InlineData(new byte[] { 0xA6 })]
        public void Get_Bytes(byte[] expectedBytes)
        {
            SetNormalDisplay setNormalDisplay = new SetNormalDisplay();
            byte[] actualBytes = setNormalDisplay.GetBytes();
            Assert.Equal(expectedBytes, actualBytes);
        }
    }
}
