using System;
using Iot.Device.Multiplexing;
using Xunit;

namespace Charlietests
{
    public class CharlieplexLayoutTests
    {
        [Fact]
        public void TwoPinLayout()
        {
            var pins = new int[] { 1, 2 };

            CharlieplexSegmentNode[] nodes = CharlieplexSegment.GetNodes(pins);
            Assert.True(nodes.Length == 2);
            Assert.True(nodes[0].Anode == 1 && nodes[0].Cathode == 2);
            Assert.True(nodes[1].Anode == 2 && nodes[1].Cathode == 1);
        }

        [Fact]
        public void ThreePinLayout()
        {
            var pins = new int[] { 1, 2, 3 };

            CharlieplexSegmentNode[] nodes = CharlieplexSegment.GetNodes(pins);
            Assert.True(nodes.Length == 6);
            Assert.True(nodes[0].Anode == 1 && nodes[0].Cathode == 2);
            Assert.True(nodes[1].Anode == 2 && nodes[1].Cathode == 1);
            Assert.True(nodes[2].Anode == 2 && nodes[2].Cathode == 3);
            Assert.True(nodes[3].Anode == 3 && nodes[3].Cathode == 2);
            Assert.True(nodes[4].Anode == 1 && nodes[4].Cathode == 3);
            Assert.True(nodes[5].Anode == 3 && nodes[5].Cathode == 1);
        }

        [Fact]
        public void FourPinLayout()
        {
            var pins = new int[] { 1, 2, 3, 4 };
            CharlieplexSegmentNode[] nodes = CharlieplexSegment.GetNodes(pins);
            Assert.True(true);
            Assert.True(nodes.Length == 12);
            Assert.True(nodes[0].Anode == 1 && nodes[0].Cathode == 2);
            Assert.True(nodes[1].Anode == 2 && nodes[1].Cathode == 1);
            Assert.True(nodes[2].Anode == 2 && nodes[2].Cathode == 3);
            Assert.True(nodes[3].Anode == 3 && nodes[3].Cathode == 2);
            Assert.True(nodes[4].Anode == 3 && nodes[4].Cathode == 4);
            Assert.True(nodes[5].Anode == 4 && nodes[5].Cathode == 3);
            Assert.True(nodes[6].Anode == 1 && nodes[6].Cathode == 3);
            Assert.True(nodes[7].Anode == 3 && nodes[7].Cathode == 1);
            Assert.True(nodes[8].Anode == 2 && nodes[8].Cathode == 4);
            Assert.True(nodes[9].Anode == 4 && nodes[9].Cathode == 2);
            Assert.True(nodes[10].Anode == 1 && nodes[10].Cathode == 4);
            Assert.True(nodes[11].Anode == 4 && nodes[11].Cathode == 1);
        }

        [Fact]
        public void FivePinLayout()
        {
            var pins = new int[] { 1, 2, 3, 4, 5 };
            CharlieplexSegmentNode[] nodes = CharlieplexSegment.GetNodes(pins);
            Assert.True(true);
            Assert.True(nodes.Length == 20);
            Assert.True(nodes[0].Anode == 1 && nodes[0].Cathode == 2);
            Assert.True(nodes[1].Anode == 2 && nodes[1].Cathode == 1);
            Assert.True(nodes[2].Anode == 2 && nodes[2].Cathode == 3);
            Assert.True(nodes[3].Anode == 3 && nodes[3].Cathode == 2);
            Assert.True(nodes[4].Anode == 3 && nodes[4].Cathode == 4);
            Assert.True(nodes[5].Anode == 4 && nodes[5].Cathode == 3);
            Assert.True(nodes[6].Anode == 4 && nodes[6].Cathode == 5);
            Assert.True(nodes[7].Anode == 5 && nodes[7].Cathode == 4);
            Assert.True(nodes[8].Anode == 1 && nodes[8].Cathode == 3);
            Assert.True(nodes[9].Anode == 3 && nodes[9].Cathode == 1);
            Assert.True(nodes[10].Anode == 2 && nodes[10].Cathode == 4);
            Assert.True(nodes[11].Anode == 4 && nodes[11].Cathode == 2);
            Assert.True(nodes[12].Anode == 3 && nodes[12].Cathode == 5);
            Assert.True(nodes[13].Anode == 5 && nodes[13].Cathode == 3);
            Assert.True(nodes[14].Anode == 1 && nodes[14].Cathode == 4);
            Assert.True(nodes[15].Anode == 4 && nodes[15].Cathode == 1);
            Assert.True(nodes[16].Anode == 2 && nodes[16].Cathode == 5);
            Assert.True(nodes[17].Anode == 5 && nodes[17].Cathode == 2);
            Assert.True(nodes[18].Anode == 1 && nodes[18].Cathode == 5);
            Assert.True(nodes[19].Anode == 5 && nodes[19].Cathode == 1);
        }
    }
}
