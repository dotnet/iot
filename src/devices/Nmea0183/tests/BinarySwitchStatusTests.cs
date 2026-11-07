// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System;
using System.Collections.Generic;
using Iot.Device.Nmea0183.Sentences;
using Xunit;

namespace Iot.Device.Nmea0183.Tests
{
    public class BinarySwitchStatusTests
    {
        private DateTimeOffset _time;

        public BinarySwitchStatusTests()
        {
            _time = DateTimeOffset.UtcNow;
        }

        [Fact]
        public void DecodeFromSentence()
        {
            string data = "$PCDIN,01F20D,069F52A7,67,0200000000000000";
            var ts = TalkerSentence.FromSentenceString(data, TalkerId.Any, out var error);
            Assert.Equal(NmeaError.None, error);
            Assert.NotNull(ts);

            var decoded = ts!.TryGetTypedValue(ref _time) as BinarySwitchStatus;
            Assert.NotNull(decoded);
            Assert.True(decoded!.Valid);
            Assert.Equal(2, decoded.Instance);
            Assert.Equal(0x67, decoded.MessageSource);

            for (int i = 0; i < decoded.MaxNumberOfSwitches; i++)
            {
                Assert.Equal(SwitchStatus.Off, decoded.GetSwitchState(i));
            }
        }

        [Fact]
        public void IdentifierIsCorrect()
        {
            var msg = new BinarySwitchStatus(0);
            Assert.Equal((uint)BinarySwitchStatus.HexId, msg.Identifier);
        }

        [Fact]
        public void DefaultSwitchesAreOff()
        {
            var msg = new BinarySwitchStatus(5);
            Assert.Equal(5, msg.Instance);
            for (int i = 0; i < msg.MaxNumberOfSwitches; i++)
            {
                Assert.Equal(SwitchStatus.Off, msg.GetSwitchState(i));
            }
        }

        [Fact]
        public void CanSetAndGetSwitchState()
        {
            var msg = new BinarySwitchStatus(1);
            msg.SetSwitchState(0, SwitchStatus.On);
            msg.SetSwitchState(27, SwitchStatus.On);

            Assert.Equal(SwitchStatus.On, msg.GetSwitchState(0));
            Assert.Equal(SwitchStatus.On, msg.GetSwitchState(27));
            Assert.Equal(SwitchStatus.Off, msg.GetSwitchState(1));
        }

        [Fact]
        public void ConstructFromDictionary()
        {
            var switches = new Dictionary<int, SwitchStatus>
            {
                { 0, SwitchStatus.On },
                { 3, SwitchStatus.On },
            };

            var msg = new BinarySwitchStatus(10, switches);

            Assert.Equal(10, msg.Instance);
            Assert.Equal(SwitchStatus.On, msg.GetSwitchState(0));
            Assert.Equal(SwitchStatus.On, msg.GetSwitchState(3));
            Assert.Equal(SwitchStatus.Off, msg.GetSwitchState(1));
            Assert.Equal(SwitchStatus.Off, msg.GetSwitchState(2));
        }

        [Fact]
        public void ReplacesOlderInstanceIsTrue()
        {
            var msg = new BinarySwitchStatus(0);
            Assert.True(msg.ReplacesOlderInstance);
        }

        [Fact]
        public void ToReadableContentReturnsExpectedText()
        {
            var msg = new BinarySwitchStatus(0);
            Assert.Equal("Switch status", msg.ToReadableContent());
        }

        [Fact]
        public void EncodeAndDecodeRoundTrip()
        {
            var original = new BinarySwitchStatus(42);
            original.SetSwitchState(2, SwitchStatus.On);
            original.SetSwitchState(27, SwitchStatus.On);

            var sentence = new TalkerSentence(original);
            string text = sentence.ToString();

            var ts = TalkerSentence.FromSentenceString(text, out var error);
            Assert.Equal(NmeaError.None, error);
            Assert.NotNull(ts);

            var decoded = ts!.TryGetTypedValue(ref _time) as BinarySwitchStatus;
            Assert.NotNull(decoded);
            Assert.Equal(42, decoded!.Instance);

            for (int i = 0; i < decoded.MaxNumberOfSwitches; i++)
            {
                SwitchStatus expected = (i == 2 || i == 27) ? SwitchStatus.On : SwitchStatus.Off;
                Assert.Equal(expected, decoded.GetSwitchState(i));
            }
        }

        [Fact]
        public void ToNmeaParameterListEncodesSwitchStates()
        {
            var msg = new BinarySwitchStatus(2);
            string parameterList = msg.ToNmeaParameterList();

            // Header (PGN, timestamp, source) + instance "02" + 14 hex chars (7 bytes) of all-off switch data
            Assert.EndsWith("02" + "00000000000000", parameterList);
        }
    }
}
