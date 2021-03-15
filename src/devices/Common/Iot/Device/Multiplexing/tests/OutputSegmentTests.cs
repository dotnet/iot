using System;
using System.Threading;
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
        public void SegmentValuesWriteByte()
        {
            VirtualOutputSegment segment = new(8);
            segment.Write(0b_1001_0110);

            Assert.True(
                segment[0] == 0 &&
                segment[1] == 1 &&
                segment[2] == 1 &&
                segment[3] == 0 &&
                segment[4] == 1 &&
                segment[5] == 0 &&
                segment[6] == 0 &&
                segment[7] == 1);
        }

        [Fact]
        public void SegmentValuesWriteLongByte()
        {
            VirtualOutputSegment segment = new(16);
            segment.Write(new byte[] { 0b_1101_0110, 0b_1111_0010 });

            Assert.True(
                segment[0] == 0 &&
                segment[1] == 1 &&
                segment[2] == 0 &&
                segment[3] == 0 &&
                segment[4] == 1 &&
                segment[5] == 1 &&
                segment[6] == 1 &&
                segment[7] == 1 &&
                segment[8] == 0 &&
                segment[9] == 1 &&
                segment[10] == 1 &&
                segment[11] == 0 &&
                segment[12] == 1 &&
                segment[13] == 0 &&
                segment[14] == 1 &&
                segment[15] == 1);
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
