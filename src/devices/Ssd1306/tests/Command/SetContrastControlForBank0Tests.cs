// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using Iot.Device.Ssd1306.Command;
using Xunit;

namespace Iot.Device.Mcp23xxx.Tests
{
    public class SetContrastControlForBank0Tests
    {
        [Theory]
        [InlineData(0x00, new byte[] { 0x81, 0x00 })]
        [InlineData(0xFF, new byte[] { 0x81, 0xFF })]
        public void Get_Bytes(byte contrastSetting, byte[] expectedBytes)
        {
            SetContrastControlForBank0 setContrastControlForBank0 = new SetContrastControlForBank0(contrastSetting);
            byte[] actualBytes = setContrastControlForBank0.GetBytes();
            Assert.Equal(expectedBytes, actualBytes);
        }
    }
}
