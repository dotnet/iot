// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using Iot.Device.Ssd13xx.Commands;
using Iot.Device.Ssd13xx.Commands.Ssd1306Commands;
using System;
using Xunit;

namespace Iot.Device.Ssd13xx.Tests
{
    public class SetDisplayStartLineTests
    {
        [Fact]
        public void Get_Bytes_With_Default_Values()
        {
            SetDisplayStartLine setDisplayStartLine = new SetDisplayStartLine();
            byte[] actualBytes = setDisplayStartLine.GetBytes();
            Assert.Equal(new byte[] { 0x40 }, actualBytes);
        }

        [Theory]
        [InlineData(0x00, new byte[] { 0x40 })]
        [InlineData(0x3F, new byte[] { 0x7F })]
        public void Get_Bytes(byte displayStartLine, byte[] expectedBytes)
        {
            SetDisplayStartLine setDisplayStartLine = new SetDisplayStartLine(displayStartLine);
            byte[] actualBytes = setDisplayStartLine.GetBytes();
            Assert.Equal(expectedBytes, actualBytes);
        }

        [Theory]
        [InlineData(0x40)]
        [InlineData(0xFF)]
        public void Invalid_DisplayStartLine(byte displayStartLine)
        {
            Assert.Throws<ArgumentException>(() =>
            {
                SetDisplayStartLine setDisplayStartLine = new SetDisplayStartLine(displayStartLine);
            });
        }
    }
}
