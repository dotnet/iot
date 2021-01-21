// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using Iot.Device.Ssd13xx.Commands;
using Xunit;

namespace Iot.Device.Ssd13xx.Tests
{
    public class ActivateScrollTests
    {
        [Fact]
        public void Get_Bytes()
        {
            ActivateScroll activateScroll = new ActivateScroll();
            byte[] actualBytes = activateScroll.GetBytes();
            Assert.Equal(new byte[] { 0x2F }, actualBytes);
        }
    }
}
