// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

// Original code taken from https://github.com/yellowfeather/AisParser, under MIT License
using Iot.Device.Nmea0183.Ais;
using Shouldly;
using Xunit;

namespace Iot.Device.Nmea0183.Tests.Ais
{
    public class ParserTests
    {
        [Fact]
        public void Should_return_null_for_message_with_empty_payload1()
        {
            const string sentence = "!AIVDM,1,1,,B,,0*25";

            var parser = new AisParser();

            var result = parser.Parse(sentence);
            result.ShouldBeNull();
        }

        [Fact]
        public void Should_return_null_for_message_with_empty_payload2()
        {
            const string sentence = "!AIVDM,1,1,,A,,0*26";

            var parser = new AisParser();

            var result = parser.Parse(sentence);
            result.ShouldBeNull();
        }

        [Fact]
        public void ParseStandardMessage1()
        {
            const string sentence1 = "!AIVDM,1,1,,B,139aspP000PWq54NlIKLbajr00S;,0*74";
            var parser = new AisParser();

            var result = parser.Parse(sentence1);
            result.ShouldNotBeNull();
        }

        [Fact]
        public void ParseStandardMessage2()
        {
            const string sentence1 = "!AIVDM,1,1,,B,H39aKq4TCBD5?f446HAj0008<22w,0*63";
            var parser = new AisParser();

            var result = parser.Parse(sentence1);
            result.ShouldNotBeNull();
        }
    }
}
