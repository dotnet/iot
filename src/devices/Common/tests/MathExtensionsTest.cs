// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System;
using System.Collections.Generic;
using System.Text;
using Xunit;

namespace Iot.Device.Common.Tests
{
    /// <summary>
    /// Note that these tests test the framework implementation in .NET Core based builds
    /// </summary>
    public class MathExtensionsTest
    {
        [Fact]
        public void ClampDouble()
        {
            Assert.Equal(23.0, MathExtensions.Clamp(23.0, 0.0, 100.0));
            Assert.Equal(0.0, MathExtensions.Clamp(-10, 0.0, 100.0));
            Assert.Equal(100.0, MathExtensions.Clamp(222, 0.0, 100.0));
        }

        [Fact]
        public void ClampByte()
        {
            Assert.Equal(23, MathExtensions.Clamp((byte)23, (byte)0, (byte)100));
            Assert.Equal(0, MathExtensions.Clamp((byte)0, (byte)0, (byte)100));
            Assert.Equal(100, MathExtensions.Clamp((byte)200, (byte)0, (byte)100));
        }

        [Fact]
        public void ClampInt()
        {
            Assert.Equal(2000, MathExtensions.Clamp(200, 2000, 10000));
            Assert.Equal(-10, MathExtensions.Clamp(-100, -10, 100));
            Assert.Equal(500, MathExtensions.Clamp(600, -10, 500));
        }

        [Fact]
        public void ClampUint()
        {
            Assert.Equal(2000u, MathExtensions.Clamp(200u, 2000u, 10000u));
            Assert.Equal(10u, MathExtensions.Clamp(0u, 10u, 100u));
            Assert.Equal(500u, MathExtensions.Clamp(600u, 10u, 500u));
        }
    }
}
