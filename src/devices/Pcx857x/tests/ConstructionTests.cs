// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

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
            if (testDevice.Controller.PinCount == 8)
            {
                Assert.Equal(0x00, device.ReadByte());
            }
            else
            {
                Assert.Equal(16, testDevice.Controller.PinCount);
                Pcx8575 device16 = (Pcx8575)device;
                Assert.Equal(0x0000, device16.ReadUInt16());
            }
        }
    }
}
