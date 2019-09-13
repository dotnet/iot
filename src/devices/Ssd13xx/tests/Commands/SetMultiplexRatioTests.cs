// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using Iot.Device.Ssd13xx.Commands;
using System;
using Xunit;

namespace Iot.Device.Ssd13xx.Tests
{
    public class SetMultiplexRatioTests
    {
        [Fact]
        public void Get_Bytes_With_Default_Values()
        {
            SetMultiplexRatio setMultiplexRatio = new SetMultiplexRatio();
            byte[] actualBytes = setMultiplexRatio.GetBytes();
            Assert.Equal(new byte[] { 0xA8, 0x3F }, actualBytes);
        }

        [Theory]
        [InlineData(0x0F, new byte[] { 0xA8, 0x0F })]
        [InlineData(0x3F, new byte[] { 0xA8, 0x3F })]
        public void Get_Bytes(byte multiplexRatio, byte[] expectedBytes)
        {
            SetMultiplexRatio setMultiplexRatio = new SetMultiplexRatio(multiplexRatio);
            byte[] actualBytes = setMultiplexRatio.GetBytes();
            Assert.Equal(expectedBytes, actualBytes);
        }

        [Theory]
        [InlineData(0x0E)]
        public void Invalid_MultiplexRatio(byte multiplexRatio)
        {
            Assert.Throws<ArgumentException>(() =>
            {
                SetMultiplexRatio setMultiplexRatio = new SetMultiplexRatio(multiplexRatio);
            });
        }
    }
}
