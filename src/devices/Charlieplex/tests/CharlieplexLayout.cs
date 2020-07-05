using System;
using Xunit;
using Iot.Device.Multiplex;

namespace Charlietests
{
    public class CharlieplexLayoutTests
    {
        [Fact]
        public void TwoPinLayout()
        {
            var pins = new int[] { 1, 2 };

            var loads = Charlieplex.GetCharlieLoads(pins);
            Assert.True(loads.Length == 2);
            Assert.True(loads[0].Anode == 1 && loads[0].Cathode == 2);
            Assert.True(loads[1].Anode == 2 && loads[1].Cathode == 1);
        }

        [Fact]
        public void ThreePinLayout()
        {
            var pins = new int[] { 1, 2, 3 };

            var loads = Charlieplex.GetCharlieLoads(pins);
            Assert.True(loads.Length == 6);
            Assert.True(loads[0].Anode == 1 && loads[0].Cathode == 2);
            Assert.True(loads[1].Anode == 2 && loads[1].Cathode == 1);
            Assert.True(loads[2].Anode == 2 && loads[2].Cathode == 3);
            Assert.True(loads[3].Anode == 3 && loads[3].Cathode == 2);
            Assert.True(loads[4].Anode == 1 && loads[4].Cathode == 3);
            Assert.True(loads[5].Anode == 3 && loads[5].Cathode == 1);
        }

        [Fact]
        public void FourPinLayout()
        {
            var pins = new int[] { 1, 2, 3, 4 };
            var loads = Charlieplex.GetCharlieLoads(pins);
            Assert.True(true);
            Assert.True(loads.Length == 12);
            Assert.True(loads[0].Anode == 1 && loads[0].Cathode == 2);
            Assert.True(loads[1].Anode == 2 && loads[1].Cathode == 1);
            Assert.True(loads[2].Anode == 2 && loads[2].Cathode == 3);
            Assert.True(loads[3].Anode == 3 && loads[3].Cathode == 2);
            Assert.True(loads[4].Anode == 3 && loads[4].Cathode == 4);
            Assert.True(loads[5].Anode == 4 && loads[5].Cathode == 3);
            Assert.True(loads[6].Anode == 1 && loads[6].Cathode == 3);
            Assert.True(loads[7].Anode == 3 && loads[7].Cathode == 1);
            Assert.True(loads[8].Anode == 2 && loads[8].Cathode == 4);
            Assert.True(loads[9].Anode == 4 && loads[9].Cathode == 2);
            Assert.True(loads[10].Anode == 1 && loads[10].Cathode == 4);
            Assert.True(loads[11].Anode == 4 && loads[11].Cathode == 1);
        }
    }
}
