// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System;
using System.Collections.Generic;
using System.Globalization;
using System.Text;
using System.Drawing;
using Iot.Device.CharacterLcd;
using Moq;
using Xunit;

namespace CharacterLcd.Tests
{
    public sealed class LcdValueUnitDisplayTests : IDisposable
    {
        private readonly Mock<ICharacterLcd> _displayMock;

        public LcdValueUnitDisplayTests()
        {
            _displayMock = new Mock<ICharacterLcd>(MockBehavior.Strict);
        }

        public void Dispose()
        {
            _displayMock.VerifyAll();
        }

        [Fact]
        public void LargeValueDisplayInitializes()
        {
            int numChars = 0;
            _displayMock.Setup(x => x.Size).Returns(new Size(20, 4));
            _displayMock.Setup(x => x.NumberOfCustomCharactersSupported).Returns(8);
            _displayMock.Setup(x => x.CreateCustomCharacter(It.IsAny<int>(), It.Is<byte[]>(y => y.Length == 8))).Callback(() => numChars++);
            _displayMock.SetupSet(x => x.BlinkingCursorVisible = false);
            _displayMock.SetupSet(x => x.UnderlineCursorVisible = false);
            _displayMock.SetupSet(x => x.BacklightOn = true);
            _displayMock.SetupSet(x => x.DisplayOn = true);
            _displayMock.Setup(x => x.Clear());
            LcdValueUnitDisplay display = new LcdValueUnitDisplay(_displayMock.Object, CultureInfo.InvariantCulture);

            // This would throw in case the character map was not available
            display.InitForRom("A00");
            Assert.Equal(7, numChars);
        }

        [Fact]
        public void UseWithoutInitializationThrows()
        {
            _displayMock.Setup(x => x.Size).Returns(new Size(20, 4));
            _displayMock.Setup(x => x.NumberOfCustomCharactersSupported).Returns(8);
            LcdValueUnitDisplay display = new LcdValueUnitDisplay(_displayMock.Object, CultureInfo.InvariantCulture);
            DateTime time = new DateTime(2020, 5, 3, 21, 35, 10);
            Assert.Throws<InvalidOperationException>(() => display.DisplayTime(time));
        }

        [Fact]
        public void UseDisplay()
        {
            // this are the raw byte values that are sent to the display. Since we're really using byte positions 0-7, this looks a bit weird
            const string firstLine = "\0\u0006\u0002 \0\u0006 \0\u0006\u0002\u0006\u0006\u0006       ";
            const string secondLine = " \0\u0003\0\u0003\u0006\u0007 \0\u0003\u0006         ";
            const string thirdLine = "\0\u0003   \u0006\u0007 \u0001\u0002\u0005\u0005\u0002       ";
            const string fourthLine = "\u0006\u0006\u0003  \u0006 \u0001\u0006\u0003\u0001\u0006\u0003       ";

            // Enable this expectation to be able to see how things get printed (if the above needs updating)
            // const string testLine = "0:\0 1:\u0001 2:\u0002 3:\u0003 4:\u0004 5:\u0005 6:\u0006 7:\u0007 8:\u0008";
            // _displayMock.Setup(x => x.Write(testLine));
            Assert.Equal(20, firstLine.Length);
            Assert.Equal(20, secondLine.Length);
            Assert.Equal(20, thirdLine.Length);
            Assert.Equal(20, fourthLine.Length);
            _displayMock.Setup(x => x.Size).Returns(new Size(20, 4));
            _displayMock.Setup(x => x.NumberOfCustomCharactersSupported).Returns(8);
            _displayMock.Setup(x => x.CreateCustomCharacter(It.IsAny<int>(), It.Is<byte[]>(y => y.Length == 8)));
            _displayMock.SetupSet(x => x.BlinkingCursorVisible = false);
            _displayMock.SetupSet(x => x.UnderlineCursorVisible = false);
            _displayMock.SetupSet(x => x.BacklightOn = true);
            _displayMock.SetupSet(x => x.DisplayOn = true);
            _displayMock.Setup(x => x.Clear());
            LcdValueUnitDisplay display = new LcdValueUnitDisplay(_displayMock.Object, CultureInfo.InvariantCulture);

            display.InitForRom("A00");

            _displayMock.Setup(x => x.SetCursorPosition(0, 0));
            _displayMock.Setup(x => x.Write(firstLine));
            _displayMock.Setup(x => x.SetCursorPosition(0, 1));
            _displayMock.Setup(x => x.Write(secondLine));
            _displayMock.Setup(x => x.SetCursorPosition(0, 2));
            _displayMock.Setup(x => x.Write(thirdLine));
            _displayMock.Setup(x => x.SetCursorPosition(0, 3));
            _displayMock.Setup(x => x.Write(fourthLine));
            DateTime time = new DateTime(2020, 5, 3, 21, 35, 10);
            display.DisplayTime(time);
        }
    }
}
