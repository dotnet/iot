// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System;
using System.Globalization;
using Iot.Device.CharacterLcd;
using Iot.Device.Graphics;
using Moq;
using Xunit;

namespace CharacterLcd.Tests
{
    public class CharacterLcdTests
    {
        [Fact]
        public void CharacterMapsAreConsistent()
        {
            // The constructor (or even the initializer) throws if the map is inconsistent (like the dictionary contains duplicate keys)
            LcdCharacterEncodingFactory factory = new LcdCharacterEncodingFactory();
            var encoding = factory.Create(new CultureInfo("de-CH"), "A00", '?', 8);
            byte[] bytes = encoding.GetBytes("ABÖ");
            Assert.Equal((byte)'A', bytes[0]);
            Assert.Equal((byte)'B', bytes[1]);
            Assert.Equal(3, bytes[2]);
            Assert.NotNull(encoding.ExtraCharacters[3]);
        }

        [Theory]
        [InlineData("de-CH", 7)]
        [InlineData("en-US", 2)]
        [InlineData("ja", 0)]
        [InlineData("fr-fr", 8)]
        [InlineData("no", 7)]
        public void CustomCharactersHaveCorrectSize(string culture, int numExtraChars)
        {
            LcdCharacterEncodingFactory factory = new LcdCharacterEncodingFactory();
            var encoding = factory.Create(new CultureInfo(culture), "A00", '?', 8);
            Assert.Equal(numExtraChars, encoding.ExtraCharacters.Count);
            foreach (var c in encoding.ExtraCharacters)
            {
                Assert.Equal(8, c.Length);
            }
        }

        [Fact]
        public void JapaneseWorksWithCharacterSetA00()
        {
            LcdCharacterEncodingFactory factory = new LcdCharacterEncodingFactory();
            var encoding = factory.Create(new CultureInfo("ja-JP"), "A00", '?', 8);
            Assert.Empty(encoding.ExtraCharacters);
        }

        [Fact]
        public void CyrillicWorksWithCharacterSetA02()
        {
            LcdCharacterEncodingFactory factory = new LcdCharacterEncodingFactory();
            var encoding = factory.Create(new CultureInfo("ru-RU"), "A02", '?', 8);
            Assert.Empty(encoding.ExtraCharacters);
        }

        [Fact]
        public void JapaneseDoesNotWorkWithOtherSets()
        {
            LcdCharacterEncodingFactory factory = new LcdCharacterEncodingFactory();
            Assert.Throws<NotSupportedException>(() => factory.Create(new CultureInfo("ja-JP"), "A02", '?', 8));
        }
    }
}
