// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using Iot.Device.Ssd1306.Command;
using Xunit;

namespace Iot.Device.Mcp23xxx.Tests
{
    public class SetChargePumpTests
    {
        [Theory]
        [InlineData(false, new byte[] { 0x8D, 0x10 })]
        [InlineData(true, new byte[] { 0x8D, 0x14 })]
        public void Get_Bytes(bool enableChargePump, byte[] expectedBytes)
        {
            SetChargePump setChargePump = new SetChargePump(enableChargePump);
            byte[] actualBytes = setChargePump.GetBytes();
            Assert.Equal(expectedBytes, actualBytes);
        }
    }
}