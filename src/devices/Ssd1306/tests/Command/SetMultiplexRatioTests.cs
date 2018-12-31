// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using Iot.Device.Ssd1306.Command;
using Xunit;

namespace Iot.Device.Mcp23xxx.Tests
{
    public class SetMultiplexRatioTests
    {
        [Theory]
        [InlineData(0x00, new byte[] { 0xA8, 0x00 })]
        [InlineData(0x3F, new byte[] { 0xA8, 0x3F })]
        [InlineData(0x40, new byte[] { 0xA8, 0x40 })]
        public void Get_Bytes(byte multiplexRatio, byte[] expectedBytes)
        {
            SetMultiplexRatio setMultiplexRatio = new SetMultiplexRatio(multiplexRatio);
            byte[] actualBytes = setMultiplexRatio.GetBytes();
            Assert.Equal(expectedBytes, actualBytes);
        }
    }
}
