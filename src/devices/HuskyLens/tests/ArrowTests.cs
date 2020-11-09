// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using Xunit;

namespace Iot.Device.HuskyLens.Tests
{
    public class ArrowTests
    {
        [Theory]
        [InlineData(new byte[] { 0x00, 0x01, 0x02, 0x03, 0x04, 0x05, 0x06, 0x07, 0x08, 0x09 },
            0x0100, 0x0302, 0x0504, 0x0706, 0x0908)]
        public void FromData_Unpacks_Data(
            byte[] data, int ox, int oy, int tx, int ty, int id)
        {
            var arrow = Arrow.FromData(data);

            Assert.Equal(arrow.Origin.X, ox);
            Assert.Equal(arrow.Origin.Y, oy);
            Assert.Equal(arrow.Target.X, tx);
            Assert.Equal(arrow.Target.Y, ty);
            Assert.Equal(arrow.Id, id);
        }
    }
}
