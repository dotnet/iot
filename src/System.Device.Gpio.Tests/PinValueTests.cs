// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using Xunit;

namespace System.Device.Gpio.Tests;

public class PinValueTests
{
    [Fact]
    public void VerifyConstants()
    {
        Assert.True(PinValue.High.Equals(1));
        Assert.True(PinValue.Low.Equals(0));
    }

    [Fact]
    public void VerifyEquality()
    {
        Assert.True(PinValue.High == 1);
        Assert.True(PinValue.Low == 0);
    }

    [Fact]
    public void VerifyInequality()
    {
        Assert.True(PinValue.High != 0);
        Assert.True(PinValue.Low != 1);
        Assert.True(PinValue.Low != 2);
    }

    [Fact]
    public void Inverse()
    {
        Assert.Equal(PinValue.Low, !PinValue.High);
        Assert.Equal(PinValue.High, !PinValue.Low);
    }
}
