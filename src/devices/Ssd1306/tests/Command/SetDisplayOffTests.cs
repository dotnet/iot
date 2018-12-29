// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using Iot.Device.Ssd1306.Command;
using Xunit;

namespace Iot.Device.Mcp23xxx.Tests
{
    public class SetDisplayOffTests
    {
        [Theory]
        [InlineData(new byte[] { 0xAE })]
        public void Get_Bytes(byte[] expectedBytes)
        {
            SetDisplayOff setDisplayOff = new SetDisplayOff();
            byte[] actualBytes = setDisplayOff.GetBytes();
            Assert.Equal(expectedBytes, actualBytes);
        }
    }
}