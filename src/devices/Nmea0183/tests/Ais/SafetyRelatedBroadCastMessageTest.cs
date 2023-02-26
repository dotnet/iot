// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

// Original code taken from https://github.com/yellowfeather/AisParser, under MIT License
using System;
using Iot.Device.Nmea0183.Ais;
using Iot.Device.Nmea0183.AisSentences;
using Shouldly;
using Xunit;

namespace Iot.Device.Nmea0183.Tests.Ais
{
    public class SafetyRelatedBroadCastMessageTest : MessageTestBase
    {
        [Fact]
        public void ShouldGenerateMessage()
        {
            SafetyRelatedBroadcastMessage msg = new SafetyRelatedBroadcastMessage();
            msg.Text = "HELLO WORLD";

            var sentences = Parser.ToSentences(msg);

            sentences.ShouldNotBeEmpty();

            sentences[0].ToNmeaMessage().ShouldBeEquivalentTo("!AIVDO,1,1,,A,>000000PDhhv1Lu8h@,2*62");

            foreach (var m in sentences)
            {
                var reverse = Parser.Parse(m);
                if (reverse != null)
                {
                    SafetyRelatedBroadcastMessage msg2 = (SafetyRelatedBroadcastMessage)reverse;
                    msg.Text.ShouldBeEquivalentTo(msg2.Text);
                    return;
                }
            }

            throw new InvalidOperationException("Message was not decoded");
        }
    }
}
