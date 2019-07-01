// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using Iot.Device.Ssd13xx.Commands;
using Xunit;

namespace Iot.Device.Ssd13xx.Tests
{
    public class SetDisplayOnTests
    {
        [Fact]
        public void Get_Bytes()
        {
            SetDisplayOn setDisplayOn = new SetDisplayOn();
            byte[] actualBytes = setDisplayOn.GetBytes();
            Assert.Equal(new byte[] { 0xAF }, actualBytes);
        }
    }
}
