// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System;
using System.Collections.Generic;
using Iot.Device.Ft200X.Samples;
using Xunit;

namespace Iot.Device.Ft200X.Tests
{
    public class TemperatureSensorTests
    {
        [Theory]
        [InlineData("00000000", "00002001")]
        [InlineData("00010000", "0001092E")]
        [InlineData("00020000", "00020001")]
        [InlineData("02010000", "01010000")]
        [InlineData("00FF0000", "02FF0000")]
        [InlineData("01FF0000", "02FF0000")]
        [InlineData("01000001", "03000000")]
        [InlineData("01010001", "03010000")]
        [InlineData("01020002", "04020000")]
        [InlineData("01020100", "04020000")]
        [InlineData("00010001", "04010000")]
        [InlineData("00010100", "04010000")]
        public void ImplementsRegisterMapAndExplicitErrorReplies(string requestHex, string expectedHex)
        {
            var sensor = new TemperatureSensor();
            Assert.Equal(Convert.FromHexString(expectedHex), Exchange(sensor, Convert.FromHexString(requestHex)));
        }

        [Fact]
        public void ConfigurationDisablesAndReenablesTemperatureReads()
        {
            var sensor = new TemperatureSensor();
            Assert.Equal(Convert.FromHexString("00020000"), Exchange(sensor, Convert.FromHexString("01020000")));
            Assert.Equal(Convert.FromHexString("00020000"), Exchange(sensor, Convert.FromHexString("00020000")));
            Assert.Equal(Convert.FromHexString("05010000"), Exchange(sensor, Convert.FromHexString("00010000")));
            Assert.Equal(Convert.FromHexString("00002001"), Exchange(sensor, Convert.FromHexString("00000000")));
            Assert.Equal(Convert.FromHexString("00020001"), Exchange(sensor, Convert.FromHexString("01020001")));
            Assert.Equal(Convert.FromHexString("0001092E"), Exchange(sensor, Convert.FromHexString("00010000")));
        }

        [Fact]
        public void InvalidWriteDoesNotChangeConfiguration()
        {
            var sensor = new TemperatureSensor();
            Exchange(sensor, Convert.FromHexString("01020000"));
            Assert.Equal(Convert.FromHexString("04020000"), Exchange(sensor, Convert.FromHexString("0102FFFF")));
            Assert.Equal(Convert.FromHexString("00020000"), Exchange(sensor, Convert.FromHexString("00020000")));
        }

        [Theory]
        [InlineData(1)]
        [InlineData(2)]
        [InlineData(3)]
        public void PartialRequestsProduceNoReplyAndRetainTheirBytes(int split)
        {
            var sensor = new TemperatureSensor();
            byte[] request = Convert.FromHexString("00010000");
            byte[] response = Convert.FromHexString("FFFFFFFF");
            for (int i = 0; i < split; i++)
            {
                Assert.False(sensor.TryProcessByte(request[i], response));
            }

            Assert.Equal(Convert.FromHexString("FFFFFFFF"), response);
            for (int i = split; i < request.Length; i++)
            {
                Assert.Equal(i == request.Length - 1, sensor.TryProcessByte(request[i], response));
            }

            Assert.Equal(Convert.FromHexString("0001092E"), response);
        }

        [Fact]
        public void ConsecutiveFramesHaveIndependentRepliesWithoutStaleBytes()
        {
            var sensor = new TemperatureSensor();
            byte[] response = new byte[4];
            var replies = new List<string>();
            foreach (byte value in Convert.FromHexString("000000000001000000FF000000020000"))
            {
                if (sensor.TryProcessByte(value, response))
                {
                    replies.Add(Convert.ToHexString(response));
                }
            }

            Assert.Equal(new[] { "00002001", "0001092E", "02FF0000", "00020001" }, replies);
        }

        [Theory]
        [InlineData(0)]
        [InlineData(3)]
        [InlineData(5)]
        public void InvalidResponseBufferDoesNotConsumeInput(int length)
        {
            var sensor = new TemperatureSensor();
            Assert.Throws<ArgumentException>(() => sensor.TryProcessByte(0xFF, new byte[length]));
            Assert.Equal(Convert.FromHexString("00002001"), Exchange(sensor, Convert.FromHexString("00000000")));
        }

        private static byte[] Exchange(TemperatureSensor sensor, byte[] request)
        {
            byte[] response = new byte[4];
            for (int i = 0; i < request.Length; i++)
            {
                Assert.Equal(i == request.Length - 1, sensor.TryProcessByte(request[i], response));
            }

            return response;
        }
    }
}
