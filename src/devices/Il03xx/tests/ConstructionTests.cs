// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using System;
using System.Device.I2c;
using Xunit;

namespace Iot.Device.Il03xx.Tests
{
    public class ConstructionTests : Il03xxTest
    {
        [Theory]
        [MemberData(nameof(TestDevices))]
        public void Stub(TestDevice testDevice)
        {
        }
    }
}
