// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using Xunit;

namespace Iot.Device.Pcx857x.Tests
{
    public class ConstructionTests : Pcx857xTest
    {
        [Theory]
        [MemberData(nameof(TestDevices))]
        public void RegisterInitialization(TestDevice testDevice)
        {
            // All pins should be set to output.
            Pcx857x device = testDevice.Device;
            if (device.PinCount == 8)
            {
                Assert.Equal(0x00, device.ReadByte());
            }
            else
            {
                Assert.Equal(16, device.PinCount);
                Pcx8575 device16 = (Pcx8575)device;
                Assert.Equal(0x0000, device16.ReadUInt16());
            }
        }
    }
}
