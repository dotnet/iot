// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using Iot.Device.Ssd13xx.Commands;
using Xunit;

namespace Iot.Device.Ssd13xx.Tests
{
    public class SetInverseDisplayTests
    {
        [Fact]
        public void Get_Bytes()
        {
            SetInverseDisplay setInverseDisplay = new SetInverseDisplay();
            byte[] actualBytes = setInverseDisplay.GetBytes();
            Assert.Equal(new byte[] { 0xA7 }, actualBytes);
        }
    }
}
