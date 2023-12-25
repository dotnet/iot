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
            byte[] input = Convert.FromHexString(data);

            MemoryStream ms = new MemoryStream(input);

            int numMessages = 0;
            Seatalk1Parser parser = new Seatalk1Parser(ms);
            parser.NewMessageDecoded += message =>
            {
                numMessages++;
                Assert.NotNull(message);
                Assert.True(message.ExpectedLength >= 3);
                if (message is CompassHeadingAndRudderPosition ch)
                {
                    Assert.Equal(Angle.FromDegrees(40), ch.CompassHeading);
                }
            };
            parser.StartDecode();

            while (numMessages != 6)
            {
                Thread.Sleep(100);
            }

            parser.StopDecode();

            parser.Dispose();
        }
    }
}
