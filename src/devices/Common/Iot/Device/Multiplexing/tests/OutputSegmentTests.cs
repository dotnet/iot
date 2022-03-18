// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System;
using System.Threading;
using System.Device.Gpio;
using Iot.Device.Multiplexing.Utility;
using Xunit;

namespace Iot.Device.Multiplexing
{
    public class OutputSegmentTests
    {
        [Fact]
        public void SegmentLength()
        {
            VirtualOutputSegment segment = new(2);
            Assert.True(segment.Length == 2);
        }

        [Fact]
        public void SegmentValuesWritePinValues()
        {
            VirtualOutputSegment segment = new(4);
            for (int i = 0; i < 4; i++)
            {
                segment.Write(i, i % 2);
            }

            Assert.True(
                segment[0] == 0 &&
                segment[1] == 1 &&
                segment[2] == 0 &&
                segment[3] == 1);
        }

        [Fact]
        public void SegmentValuesWriteByteOffset()
        {
            // segment length > written value
            VirtualOutputSegment segment = new(12);
            segment.Write(0b_1001_0110);

            var expected = new PinValue[] { 0, 1, 1, 0, 1, 0, 0, 1 };

            for (int i = 0; i < expected.Length; i++)
            {
                Assert.Equal(expected[i], segment[i]);
            }
        }

        [Fact]
        public void SegmentValuesWriteByte()
        {
            VirtualOutputSegment segment = new(8);
            segment.Write(0b_1001_0110);

            var expected = new PinValue[] { 0, 1, 1, 0, 1, 0, 0, 1 };

            for (int i = 0; i < expected.Length; i++)
            {
                Assert.Equal(expected[i], segment[i]);
            }
        }

        [Fact]
        public void SegmentWriteLongByte()
        {
            // Scenario: values same as byteLength
            VirtualOutputSegment segment = new(16);
            segment.Write(new byte[] { 0b_1001_0110, 0b_1111_0000 });
            var expected = new PinValue[] { 0, 1, 1, 0, 1, 0, 0, 1, 0, 0, 0, 0, 1, 1, 1, 1 };

            for (int i = 0; i < expected.Length; i++)
            {
                Assert.Equal(expected[i], segment[i]);
            }
        }

        [Fact]
        public void SegmentWriteLongByteValueLonger()
        {
            // Scenario: values longer than byteLength
            VirtualOutputSegment segment = new(12);
            byte[] value = new byte[] { 0b_1001_0110, 0b_1111_0000 };

            Assert.Throws<ArgumentException>(() => segment.Write(value));
        }

        [Fact]
        public void SegmentWriteLongByteValueShorter()
        {
            // Scenario: values shorter than byteLength
            VirtualOutputSegment segment = new(24);
            byte[] value = new byte[] { 0b_1001_0110, 0b_1111_0000 };
            segment.Write(value);
            var expected = new PinValue[]
            {
                0, 1, 1, 0, 1, 0, 0, 1,
                0, 0, 0, 0, 1, 1, 1, 1,
                0, 0, 0, 0, 0, 0, 0, 0
            };

            for (int i = 0; i < expected.Length; i++)
            {
                Assert.Equal(expected[i], segment[i]);
            }
        }

        [Fact]
        public void SegmentValuesClear()
        {
            VirtualOutputSegment segment = new(8);
            segment.Write(255);
            Assert.True(segment[3] == 1);
            segment.TurnOffAll();

            for (int i = 0; i < segment.Length; i++)
            {
                Assert.True(segment[i] == 0);
            }
        }
    }
}
