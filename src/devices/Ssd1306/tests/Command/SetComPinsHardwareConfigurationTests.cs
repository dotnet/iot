// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using Iot.Device.Ssd1306.Command;
using Xunit;

namespace Iot.Device.Mcp23xxx.Tests
{
    public class SetComPinsHardwareConfigurationTests
    {
        [Theory]
        // AlternativeComPinConfiguration
        [InlineData(false, false, new byte[] { 0xDA, 0x02 })]
        [InlineData(true, false, new byte[] { 0xDA, 0x12 })]
        // EnableLeftRightRemap
        [InlineData(false, true, new byte[] { 0xDA, 0x22 })]
        public void Get_Bytes(bool alternativeComPinConfiguration, bool enableLeftRightRemap, byte[] expectedBytes)
        {
            SetComPinsHardwareConfiguration setComPinsHardwareConfiguration = new SetComPinsHardwareConfiguration(alternativeComPinConfiguration, enableLeftRightRemap);
            byte[] actualBytes = setComPinsHardwareConfiguration.GetBytes();
            Assert.Equal(expectedBytes, actualBytes);
        }
    }
}
