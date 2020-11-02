// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using Iot.Device.Ssd13xx.Commands;
using Xunit;

namespace Iot.Device.Ssd13xx.Tests
{
    public class DeactivateScrollTests
    {
        [Fact]
        public void Get_Bytes()
        {
            DeactivateScroll deactivateScroll = new DeactivateScroll();
            byte[] actualBytes = deactivateScroll.GetBytes();
            Assert.Equal(new byte[] { 0x2E }, actualBytes);
        }
    }
}