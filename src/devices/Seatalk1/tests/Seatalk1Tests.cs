// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System;
using System.ComponentModel;
using System.IO;
using System.Threading;
using Iot.Device.Seatalk1;
using Iot.Device.Seatalk1.Messages;
using UnitsNet;
using Xunit;

namespace Iot.Device.Tests.Seatalk1
{
    public class Seatalk1Tests
    {
        [Fact]
        public void DecodeSomeSentences()
        {
            string data = @"9c 01 12 00 
84 06 12 00 00 00 00 00 08 
86 01 02 fd 
84 06 12 00 00 00 00 00 08 
9c 01 12 00 
84 06 12 00 00 00 00 00 08";
            MemoryStream ms = GetStreamFromInputString(data);

            int numMessages = 0;
            Seatalk1Parser parser = new Seatalk1Parser(ms);
            parser.NewMessageDecoded += message =>
            {
                numMessages++;
                Assert.NotNull(message);
                Assert.True(message.ExpectedLength >= 3);
                if (message is CompassHeadingAndRudderPosition ch)
                {
                    Assert.Equal(Angle.FromDegrees(36), ch.CompassHeading);
                }

                if (message is Keystroke ks)
                {
                    Assert.Equal(AutopilotButtons.StandBy, ks.ButtonsPressed);
                }
            };
            parser.StartDecode();

            int cnt = 10;
            while (numMessages != 6 && cnt-- >= 0)
            {
                Thread.Sleep(100);
            }

            parser.StopDecode();

            Assert.True(cnt > 0); // timeout?

            parser.Dispose();
        }

        [Fact]
        public void SkipSomeGarbage()
        {
            // The second row is garbled (for this test, at least 18 bytes must be in the input after the garbage)
            string data = @"9c 01 12 00 
0A 02 12 00 00 FF DD 00 08 
86 01 02 fd 
84 06 12 00 00 00 00 00 08 
9c 01 12 00 
84 06 12 00 00 00 00 00 08";
            MemoryStream ms = GetStreamFromInputString(data);

            int numMessages = 0;
            Seatalk1Parser parser = new Seatalk1Parser(ms);
            parser.NewMessageDecoded += message =>
            {
                numMessages++;
            };
            parser.StartDecode();

            int cnt = 100;
            while (numMessages != 5 && cnt-- >= 0)
            {
                Thread.Sleep(100);
            }

            parser.StopDecode();

            Assert.True(cnt > 0); // timeout?

            parser.Dispose();
        }

        private static MemoryStream GetStreamFromInputString(string data)
        {
            var split = data.Split(new char[] { '\n', '\r', ' ' }, StringSplitOptions.RemoveEmptyEntries);
            // FromHexString expects an uninterrupted sequence of two-char pairs
            byte[] input = Convert.FromHexString(string.Join(string.Empty, split));

            MemoryStream ms = new MemoryStream(input);
            return ms;
        }
    }
}
