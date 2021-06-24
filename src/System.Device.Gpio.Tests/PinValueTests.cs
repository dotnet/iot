// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System;
using System.Collections.Generic;
using System.Text;
using Xunit;

namespace System.Device.Gpio.Tests
{
    public class PinValueTests
    {
        [Fact]
        public void VerifyConstants()
        {
            Assert.True(PinValue.High.Equals(1));
            Assert.True(PinValue.Low.Equals(0));

            Assert.True(PinValue.High.IsHigh);
            Assert.False(PinValue.High.IsLow);

            Assert.True(PinValue.Low.IsLow);
            Assert.False(PinValue.Low.IsHigh);
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

        [Fact]
        public void VerifyIsLowOrIsHigh()
        {
            Assert.True(((PinValue)0).IsLow);
            Assert.False(((PinValue)0).IsHigh);

            Assert.True(((PinValue)1).IsHigh);
            Assert.False(((PinValue)1).IsLow);

            Assert.True(((PinValue)int.MaxValue).IsHigh);
            Assert.False(((PinValue)int.MaxValue).IsLow);

            Assert.True(((PinValue)int.MinValue).IsHigh);
            Assert.False(((PinValue)int.MinValue).IsLow);
        }
    }
}
