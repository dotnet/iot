// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.IO.Ports;
using System.Security.Cryptography.X509Certificates;
using System.Threading;
using Iot.Device.Nmea0183;
using Iot.Device.Nmea0183.Sentences;
using Iot.Device.Nmea0183.Tests;
using Iot.Device.Seatalk1;
using Iot.Device.Seatalk1.Messages;
using UnitsNet;
using Xunit;
using static System.Runtime.InteropServices.JavaScript.JSType;

namespace Iot.Device.Nmea0183.Tests
{
    public class Seatalk1Tests
    {
        private readonly FakeSerialPort _port;

        public Seatalk1Tests()
        {
            _port = new FakeSerialPort();
        }

        [Fact]
        public void DecodeSomeSentences()
        {
            string data = @"9c 01 12 00
84 06 12 00 00 00 00 00 08
86 01 02 fd
84 06 12 00 00 00 00 00 08
9c 01 12 00
84 06 12 00 00 00 00 00 08";
            byte[] bytes = GetBytesFromInputString(data);

            int numMessages = 0;

            _port.SimulateReceive(bytes);

            Seatalk1Parser parser = new Seatalk1Parser("Seatalk1", _port);
            parser.OnNewSequence += (src, message) =>
            {
                if (message is RawSentence)
                {
                    return;
                }

                numMessages++;
                Assert.NotNull(message);
                var msg = message as SeatalkMessage;
                Assert.NotNull(msg);
                Assert.True(msg.ExpectedLength >= 3);
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
                Thread.Sleep(500);
            }

            parser.StopDecode();

            Assert.True(cnt > 0); // timeout?

            parser.Dispose();
        }

        [Fact]
        public void SkipGarbageToResync()
        {
            Seatalk1Parser parser = new Seatalk1Parser("Test", new FakeSerialPort());

            // Expecting it to drop the first two bytes (0A 02) and then parse the rest as a valid message
            var msg = parser.ParseSentence("0A,02,86,01,02,fd", out NmeaError error);
            Assert.NotNull(msg);
        }

        [Fact]
        public void SkipSomeGarbageWhileDecoding()
        {
            // The second row is garbled (for this test, at least 18 bytes must be in the input after the garbage)
            string data = @"9c 01 12 00
0A 02 12 00 00 FF DD 00 08
86 01 02 fd
84 06 12 00 00 00 00 00 08
9c 01 12 00
84 06 12 00 00 00 00 00 08";
            var ms = GetBytesFromInputString(data);

            int numMessages = 0;
            Seatalk1Parser parser = new Seatalk1Parser("Test", new FakeSerialPort(ms));
            parser.OnNewSequence += (src, message) =>
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
            Assert.True(numMessages > 1);

            parser.Dispose();
        }

        [Fact]
        public void ParityBitCheckForMessageThatWorks()
        {
            // This message (-10 key) worked right away
            byte[] datagram = new byte[]
            {
                0x86, 0x11, 0x06, 0xf9
            };

            var withParity = Seatalk1Parser.CalculateParityForEachByte(datagram);
            Assert.Equal(0x86, withParity[0].B);
            Assert.Equal(0, withParity[0].Index);
            // 0x86 is odd (not the value, but it's bitcount), and we want the stopbit to be 1, so the parity should be even
            Assert.Equal(Parity.Even, withParity[0].P);
            // 0x11 is even, but we need the stopbit to be 0, so parity still needs to be even
            Assert.Equal(Parity.Even, withParity[1].P);
            // 0x06 is even, we need the stopbit to be 0, so parity still needs to be even
            Assert.Equal(Parity.Even, withParity[2].P);
            // 0xf9 is even, we need the stopbit to be 0, so parity still needs to be even
            Assert.Equal(Parity.Even, withParity[3].P);
        }

        [Fact]
        public void ParityBitCheckForMessageThatMustWork()
        {
            // This message took a bit to get right
            byte[] datagram = new byte[]
            {
                0x86, 0x11, 0x08, 0xf7
            };

            var withParity = Seatalk1Parser.CalculateParityForEachByte(datagram);
            Assert.Equal(0x86, withParity[0].B);
            Assert.Equal(0, withParity[0].Index);
            // 0x86 is odd (not the value, but it's bitcount), and we want the stopbit to be 1, so the parity should be even
            Assert.Equal(Parity.Even, withParity[0].P);
            // 0x11 is even, but we need the stopbit to be 0, so parity still needs to be even
            Assert.Equal(Parity.Even, withParity[1].P);
            // 0x08 is odd, we need the stopbit to be 0, so parity needs to be odd
            Assert.Equal(Parity.Odd, withParity[2].P);
            // 0xf7 is odd, we need the stopbit to be 0, so parity needs to be odd
            Assert.Equal(Parity.Odd, withParity[3].P);
        }

        [Fact]
        public void ParityBitCheckForMessageWithOddCommandByte()
        {
            byte[] datagram = new byte[]
            {
                0x9C, 0x01, 0x03, 0x0f
            };

            var withParity = Seatalk1Parser.CalculateParityForEachByte(datagram);
            Assert.Equal(0x9c, withParity[0].B);
            Assert.Equal(0, withParity[0].Index);
            // 0x9c is even (not the value, but it's bitcount), and we want the stopbit to be 1, so the parity should be odd
            Assert.Equal(Parity.Odd, withParity[0].P);
            // 0x01 is odd, we need the stopbit to be 0, so parity needs to be odd
            Assert.Equal(Parity.Odd, withParity[1].P);
            // 0x03 is even, we need the stopbit to be 0, so parity needs to be even
            Assert.Equal(Parity.Even, withParity[2].P);
        }

        [Fact]
        public void CanDecodeAllMessageTypes()
        {
            Seatalk1Parser parser = new Seatalk1Parser("Test", _port);
            foreach (var m in parser.MessageTypes)
            {
                // we already have an instance of each message type
                byte[] data = m.CreateDatagram();
                var decoded = parser.GetDataForNextMessage(data, out int bytesOfNewMessage);
                Assert.NotNull(decoded);
                Assert.NotEqual(0, bytesOfNewMessage);
                Assert.Equal(data[0], decoded[0]);
            }
        }

        [Theory]
        [InlineData("9c 01 12 00")]
        [InlineData("84 06 12 00 00 00 00 00 08")]
        [InlineData("86 01 02 fd")]
        [InlineData("95 86 26 96 02 00 00 00 08")]
        [InlineData("87 00 01")]
        [InlineData("87 00 02")]
        [InlineData("10 01 00 01")]
        [InlineData("85 06 00 00 C0 0D 17 00 E8")]
        [InlineData("11 01 12 04")]
        public void Parse(string inputSentence)
        {
            var split = inputSentence.Split(new char[] { '\n', '\r', ' ' }, StringSplitOptions.RemoveEmptyEntries);
            // FromHexString expects an uninterrupted sequence of two-char pairs
            byte[] inputDatagram = Convert.FromHexString(string.Join(string.Empty, split));

            MemoryStream ms = new MemoryStream();
            Seatalk1Parser parser = new Seatalk1Parser("Test", new FakeSerialPort(ms.GetBuffer()));

            var msg = parser.GetDataForNextMessage(inputDatagram, out int bytesInMessage);
            Assert.NotNull(msg);
            Assert.NotEqual(0, bytesInMessage);
            Assert.Equal(msg.Count, inputDatagram.Length);
        }

        [Fact]
        public void DecodeNothing()
        {
            var m = new MemoryStream();
            var parser = new Seatalk1Parser("Test", new FakeSerialPort(m.GetBuffer()));
            parser.StartDecode();
            parser.StopDecode();
        }

        [Fact]
        public void ParseSomeMessages()
        {
            string data = @"9c 01 12 00
84 06 12 00 00 00 00 00 08
86 01 02 fd
84 06 12 00 00 00 00 00 08
9c 01 12 00
84 06 12 00 00 00 00 00 08";
            var ms = GetBytesFromInputString(data);
            var parser = new Seatalk1Parser("Test", _port);
            _port.SimulateReceive(ms);

            List<SeatalkMessage> messages = new List<SeatalkMessage>();
            parser.OnNewSequence += (src, message) =>
            {
                Assert.NotNull(message);
                if (message is RawSentence)
                {
                    return;
                }

                var msg = message as SeatalkMessage;
                Assert.NotNull(msg);
                Assert.True(msg.ExpectedLength >= 3);
                messages.Add(msg);
            };

            parser.StartDecode();
            Thread.Sleep(500);
            parser.StopDecode();

            Assert.Equal(6, messages.Count);
        }

        private static MemoryStream GetStreamFromInputString(string data)
        {
            var split = data.Split(new char[] { '\n', '\r', ' ' }, StringSplitOptions.RemoveEmptyEntries);
            // FromHexString expects an uninterrupted sequence of two-char pairs
            byte[] input = Convert.FromHexString(string.Join(string.Empty, split));

            MemoryStream ms = new MemoryStream(input);
            return ms;
        }

        private static byte[] GetBytesFromInputString(string data)
        {
            var split = data.Split(new char[] { '\n', '\r', ' ' }, StringSplitOptions.RemoveEmptyEntries);
            // FromHexString expects an uninterrupted sequence of two-char pairs
            byte[] input = Convert.FromHexString(string.Join(string.Empty, split));
            return input;
        }
    }
}
