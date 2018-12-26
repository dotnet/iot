// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using Xunit;

namespace Iot.Device.Mcp23xxx.Tests
{
    public class OpCodeTests
    {
        [Theory]
        // Writing
        [InlineData(0, false, 0b0100_0000)]
        [InlineData(1, false, 0b0100_0010)]
        [InlineData(2, false, 0b0100_0100)]
        [InlineData(3, false, 0b0100_0110)]
        [InlineData(4, false, 0b0100_1000)]
        [InlineData(5, false, 0b0100_1010)]
        [InlineData(6, false, 0b0100_1100)]
        [InlineData(7, false, 0b0100_1110)]
        // Reading
        [InlineData(0, true, 0b0100_0001)]
        [InlineData(1, true, 0b0100_0011)]
        [InlineData(2, true, 0b0100_0101)]
        [InlineData(3, true, 0b0100_0111)]
        [InlineData(4, true, 0b0100_1001)]
        [InlineData(5, true, 0b0100_1011)]
        [InlineData(6, true, 0b0100_1101)]
        [InlineData(7, true, 0b0100_1111)]
        public void Get_OpCode(int deviceAddress, bool isReadCommand, byte expectedOpCode)
        {
            byte actualOpCode = OpCode.GetOpCode(deviceAddress, isReadCommand);
            Assert.Equal(expectedOpCode, actualOpCode);
        }
    }
}
