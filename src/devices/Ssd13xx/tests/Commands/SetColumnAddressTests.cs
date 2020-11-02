// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System;
using Xunit;
using Iot.Device.Ssd13xx.Commands;
using Iot.Device.Ssd13xx.Commands.Ssd1306Commands;

namespace Iot.Device.Ssd13xx.Tests
{
    public class SetColumnAddressTests
    {
        [Fact]
        public void Get_Bytes_With_Default_Values()
        {
            SetColumnAddress setColumnAddress = new SetColumnAddress();
            byte[] actualBytes = setColumnAddress.GetBytes();
            Assert.Equal(new byte[] { 0x21, 0x00, 0x7F }, actualBytes);
        }

        [Theory]
        // StartAddress
        [InlineData(0x00, 0x00, new byte[] { 0x21, 0x00, 0x00 })]
        [InlineData(0x7F, 0x00, new byte[] { 0x21, 0x7F, 0x00 })]
        // EndAddress
        [InlineData(0x00, 0x7F, new byte[] { 0x21, 0x00, 0x7F })]
        public void Get_Bytes(byte startAddress, byte endAddress, byte[] expectedBytes)
        {
            SetColumnAddress setColumnAddress = new SetColumnAddress(startAddress, endAddress);
            byte[] actualBytes = setColumnAddress.GetBytes();
            Assert.Equal(expectedBytes, actualBytes);
        }

        [Theory]
        // StartAddress invalid
        [InlineData(0x80, 0x00)]
        [InlineData(0x80, 0x7F)]
        // EndAddress invalid
        [InlineData(0x00, 0x80)]
        [InlineData(0x7F, 0x80)]
        // StartAddress and EndAddress invalid
        [InlineData(0x80, 0x80)]
        [InlineData(0xFF, 0xFF)]
        public void Invalid_Addresses(byte startAddress, byte endAddress)
        {
            Assert.Throws<ArgumentOutOfRangeException>(() =>
            {
                SetColumnAddress setColumnAddress = new SetColumnAddress(startAddress, endAddress);
            });
        }
    }
}
