// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Iot.Device.Seatalk1;
using Iot.Device.Seatalk1.Messages;
using UnitsNet;
using Xunit;

namespace Iot.Device.Tests.Seatalk1
{
    public class MessageTests
    {
        private Seatalk1Parser _parser;
        public MessageTests()
        {
            _parser = new Seatalk1Parser(new MemoryStream());
        }

        [Theory]
        [InlineData("95 86 26 97 02 00 00 00 08", typeof(CompassHeadingAutopilotCourseAlt))]
        [InlineData("95 86 0E 00 10 00 00 04 08", typeof(CompassHeadingAutopilotCourseAlt))]
        [InlineData("84 86 26 97 02 00 00 00 08", typeof(CompassHeadingAutopilotCourse))]
        [InlineData("9c 01 12 00", typeof(CompassHeadingAndRudderPosition))]
        [InlineData("87 00 01", typeof(DeadbandSetting))]
        [InlineData("86 01 02 fd", typeof(Keystroke))]
        public void KnownMessageTypeDecode(string msg, Type expectedType)
        {
            msg = msg.Replace(" ", string.Empty);
            byte[] data = Convert.FromHexString(msg);
            var actualType = _parser.GetTypeOfNextMessage(data, out int length);
            Assert.NotNull(actualType);
            Assert.Equal(data.Length, length);

            Assert.Equal(expectedType, actualType.GetType());
        }

        [Fact]
        public void WithKeywordCreatesCopy()
        {
            var obj = new CompassHeadingAndRudderPosition()
            {
                CompassHeading = Angle.FromDegrees(10)
            };

            var copy = obj with
            {
                CompassHeading = Angle.FromDegrees(12)
            };

            Assert.False(ReferenceEquals(obj, copy));

            Assert.NotEqual(obj, copy);
        }

        [Fact]
        public void EqualKeysAreEqual()
        {
            Keystroke ks1 = new Keystroke(AutopilotButtons.PlusTen, 1);
            Keystroke ks2 = new Keystroke(AutopilotButtons.PlusTen, 1);
            Assert.Equal(ks1, ks2);
            Assert.True(ks1.Equals(ks2));
            Assert.Equal(ks1.GetHashCode(), ks2.GetHashCode());
        }

        [Fact]
        public void UnEqualKeysAreNotEqual()
        {
            Keystroke ks1 = new Keystroke(AutopilotButtons.PlusTen, 1);
            Keystroke ks2 = new Keystroke(AutopilotButtons.MinusTen, 1);
            Assert.NotEqual(ks1, ks2);
            Assert.False(ks1.Equals(ks2));
            Assert.NotEqual(ks1.GetHashCode(), ks2.GetHashCode());
        }
    }
}
