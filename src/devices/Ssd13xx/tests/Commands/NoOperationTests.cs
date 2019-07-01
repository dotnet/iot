// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using Iot.Device.Ssd13xx.Commands;
using Iot.Device.Ssd13xx.Commands.Ssd1306Commands;
using Xunit;

namespace Iot.Device.Ssd13xx.Tests
{
    public class NoOperationTests
    {
        [Fact]
        public void Get_Bytes()
        {
            NoOperation noOperation = new NoOperation();
            byte[] actualBytes = noOperation.GetBytes();
            Assert.Equal(new byte[] { 0xE3 }, actualBytes);
        }
    }
}
