// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using Iot.Device.Ssd13xx.Commands;
using Iot.Device.Ssd13xx.Commands.Ssd1306Commands;
using Xunit;

namespace Iot.Device.Ssd13xx.Tests
{
    public class EntireDisplayOnTests
    {
        [Theory]
        [InlineData(false, new byte[] { 0xA4 })]
        [InlineData(true, new byte[] { 0xA5 })]
        public void Get_Bytes(bool entireDisplay, byte[] expectedBytes)
        {
            EntireDisplayOn entireDisplayOn = new EntireDisplayOn(entireDisplay);
            byte[] actualBytes = entireDisplayOn.GetBytes();
            Assert.Equal(expectedBytes, actualBytes);
        }
    }
}
