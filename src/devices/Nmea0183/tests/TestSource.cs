// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System;
using Iot.Device.Nmea0183.Sentences;

namespace Iot.Device.Nmea0183.Tests
{
    internal class TestSource : NmeaSinkAndSource
    {
        public event Action<NmeaSinkAndSource, NmeaSentence>? MessageReceived;

        public TestSource()
            : base("TestInterface")
        {
        }

        public override void StartDecode()
        {
        }

        public override void SendSentence(NmeaSinkAndSource source, NmeaSentence sentence)
        {
            MessageReceived?.Invoke(source, sentence);
        }

        public void SendSentenceToTestClass(NmeaSentence sentence)
        {
            DispatchSentenceEvents(sentence);
        }

        public override void StopDecode()
        {
        }
    }
}
