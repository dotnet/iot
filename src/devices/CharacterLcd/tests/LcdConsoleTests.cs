// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System;
using System.Collections.Generic;
using System.Drawing;
using System.Globalization;
using System.Text;
using Iot.Device.CharacterLcd;
using Iot.Device.Graphics;
using Moq;
using Xunit;

namespace CharacterLcd.Tests
{
    public sealed class LcdConsoleTests
    {
        private readonly Mock<ICharacterLcd> _lcd;

        public LcdConsoleTests()
        {
            _lcd = new Mock<ICharacterLcd>(MockBehavior.Strict);
        }

        [Fact]
        public void Init()
        {
            _lcd.Setup(x => x.Size).Returns(new Size(20, 4));
            _lcd.SetupProperty(x => x.BacklightOn);
            _lcd.SetupProperty(x => x.BlinkingCursorVisible);
            _lcd.SetupProperty(x => x.DisplayOn);
            _lcd.SetupProperty(x => x.UnderlineCursorVisible);
            _lcd.Setup(x => x.Clear());
            var console = new LcdConsole(_lcd.Object, "A00", true);
            Assert.True(console.BacklightOn);
            Assert.True(console.DisplayOn);
            Assert.Equal(20, console.Size.Width);
        }

        [Fact]
        public void Write()
        {
            _lcd.Setup(x => x.Size).Returns(new Size(20, 4));
            _lcd.SetupProperty(x => x.BacklightOn);
            _lcd.SetupProperty(x => x.BlinkingCursorVisible);
            _lcd.SetupProperty(x => x.DisplayOn);
            _lcd.SetupProperty(x => x.UnderlineCursorVisible);
            _lcd.Setup(x => x.Clear());
            var console = new LcdConsole(_lcd.Object, "A00", true);

            char[] expect = "Some short text".ToCharArray();
            _lcd.Setup(x => x.Write(expect));
            console.Write("Some short text");
        }

        [Fact]
        public void WriteWithTruncate()
        {
            _lcd.Setup(x => x.Size).Returns(new Size(20, 4));
            _lcd.SetupProperty(x => x.BacklightOn);
            _lcd.SetupProperty(x => x.BlinkingCursorVisible);
            _lcd.SetupProperty(x => x.DisplayOn);
            _lcd.SetupProperty(x => x.UnderlineCursorVisible);
            _lcd.Setup(x => x.Clear());
            var console = new LcdConsole(_lcd.Object, "A00", true);
            console.LineFeedMode = LineWrapMode.Truncate;

            char[] expect = "Some short text".ToCharArray();
            _lcd.Setup(x => x.Write(expect));
            console.Write("Some short text");

            _lcd.Setup(x => x.SetCursorPosition(It.IsAny<int>(), It.IsAny<int>()));
            char[] expect2 = "Lengt".ToCharArray();
            _lcd.Setup(x => x.Write(expect2));
            char[] expect3 = "hy Text".ToCharArray();
            _lcd.Setup(x => x.Write(expect3));
            console.WriteLine("Lengthy Text, more text than what fits on a line.");
        }

        [Fact]
        public void WriteWithWordWrap()
        {
            _lcd.Setup(x => x.Size).Returns(new Size(20, 4));
            _lcd.SetupProperty(x => x.BacklightOn);
            _lcd.SetupProperty(x => x.BlinkingCursorVisible);
            _lcd.SetupProperty(x => x.DisplayOn);
            _lcd.SetupProperty(x => x.UnderlineCursorVisible);
            _lcd.Setup(x => x.Clear());
            var console = new LcdConsole(_lcd.Object, "A00", true);
            console.LineFeedMode = LineWrapMode.WordWrap;

            // Space-filled, to fully clear the line, even if we would wrap before the last char
            char[] expect = "Some short text     ".ToCharArray();
            _lcd.Setup(x => x.Write(expect));
            console.Write("Some short text");

            _lcd.Setup(x => x.SetCursorPosition(It.IsAny<int>(), It.IsAny<int>()));
            char[] expect2 = "Lengt".ToCharArray();
            _lcd.Setup(x => x.Write(expect2));
            char[] expect3 = "hy Text, more text  ".ToCharArray();
            _lcd.Setup(x => x.Write(expect3));
            console.WriteLine("Lengthy Text, more text");
        }

        [Fact]
        public void LoadEncoding()
        {
            _lcd.Setup(x => x.Size).Returns(new Size(20, 4));
            _lcd.SetupProperty(x => x.BacklightOn);
            _lcd.SetupProperty(x => x.BlinkingCursorVisible);
            _lcd.SetupProperty(x => x.DisplayOn);
            _lcd.SetupProperty(x => x.UnderlineCursorVisible);
            _lcd.SetupGet(x => x.NumberOfCustomCharactersSupported).Returns(8);
            _lcd.Setup(x => x.Clear());
            _lcd.Setup(x => x.CreateCustomCharacter(It.IsAny<int>(), It.IsAny<byte[]>()));
            var console = new LcdConsole(_lcd.Object, "A00", true);
            var encoding = LcdConsole.CreateEncoding(CultureInfo.CreateSpecificCulture("de-CH"), "A00");
            console.LoadEncoding(encoding);
            char[] expect = "\u0002".ToCharArray();
            _lcd.Setup(x => x.Write(expect));
            console.Write("Ä");
        }
    }
}
