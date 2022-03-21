// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using Xunit;

namespace Iot.Device.Common.Tests
{
    public class FrameworkCompatibilityExtensionsTest
    {
        [Fact]
        public void SpanTests()
        {
            string hello = "Hello!";
            ReadOnlySpan<char> helloSpan = hello.AsSpan();
            Assert.True(helloSpan.StartsWith("He", StringComparison.OrdinalIgnoreCase));
            Assert.True(helloSpan.StartsWith("hello", StringComparison.OrdinalIgnoreCase));
            Assert.True(helloSpan.Contains("!".AsSpan(), StringComparison.Ordinal));
            string hello2 = "Hello!";
            Assert.True(helloSpan.CompareTo(hello2.AsSpan(), StringComparison.CurrentCulture) == 0);
        }

        [Fact]
        public void StreamTests()
        {
            MemoryStream ms = new MemoryStream();
            byte[] data = new byte[10];
            data[0] = 10;
            Span<byte> helloSpan = data.AsSpan();
            ms.Write(helloSpan);
            ms.Position = 0;

            Span<byte> reply = stackalloc byte[10];
            ms.Read(reply);
            Assert.Equal(data.ToList(), reply.ToArray());
        }
    }
}
