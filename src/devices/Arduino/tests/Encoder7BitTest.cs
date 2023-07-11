// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System;
using System.Collections.Generic;
using System.Text;
using Iot.Device.Arduino;
using Xunit;

namespace Iot.Device.Arduino.Tests
{
    public class Encoder7BitTest
    {
        [Fact]
        public void EncodeDecode1()
        {
            byte[] dataToEncode = new byte[] { 1, 2, 3, 4, 5, 6, 7, 8, 9, 10 };

            var encoded = Encoder7Bit.Encode(dataToEncode);

            Assert.True(encoded.Length > dataToEncode.Length);

            var decoded = Encoder7Bit.Decode(encoded);

            Assert.Equal(dataToEncode, decoded);
        }

        [Fact]
        public void EncodeDecode2()
        {
            byte[] dataToEncode = new byte[] { 0, 0, 0, 0, 0, 0xFF };

            var encoded = Encoder7Bit.Encode(dataToEncode);

            Assert.True(encoded.Length > dataToEncode.Length);

            var decoded = Encoder7Bit.Decode(encoded);

            Assert.Equal(dataToEncode, decoded);
        }
    }
}
