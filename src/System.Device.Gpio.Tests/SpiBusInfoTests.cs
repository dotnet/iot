// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System.Device.Spi;
using System.Runtime.InteropServices;
using Xunit;

namespace System.Device.Gpio.Tests;

public class SpiBusInfoTests
{
    [Fact]
    public void VerifyBufferSize()
    {
        if (RuntimeInformation.IsOSPlatform(OSPlatform.Linux))
        {
            Assert.True(SpiBusInfo.BufferSize > 0, "SpiBusInfo.BufferSize returned 0 or less");
        }
        else
        {
            Assert.Equal(-1, SpiBusInfo.BufferSize);
        }
    }
}
