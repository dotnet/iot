// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Iot.Device.Common;
using Iot.Device.Nmea0183.Sentences;
using UnitsNet;
using Xunit;

namespace Iot.Device.Nmea0183.Tests
{
    public class Nmea2000Tests
    {
        [Fact(Skip = "Only passes if hardware is present")]
        public async Task SearchForDevice()
        {
            var result = await Nmea2000YdwgParser.FindCompatibleDevice("YDWG", null);
            Assert.NotNull(result);
        }

        [Fact]
        public void ParseRawNmea2000SentenceSingle()
        {
            var m = new MemoryStream();
            var parser = new Nmea2000YdwgParser("Test", m, null);
            var sentence = parser.ParseSentence("17:33:21.141 R 09F80115 A0 7D E6 18 C0 05 FB D5", out NmeaError error);
            Assert.NotNull(sentence);
            Assert.IsType<TalkerSentence>(sentence);
            Assert.StartsWith("$PCDIN,09F801,0000F6E1,15,A07DE618C005FBD5*", sentence.ToString());
        }

        [Fact]
        public void ParseRawNmea2000SentenceFastPacket()
        {
            var m = new MemoryStream();
            NmeaError error = NmeaError.None;
            var parser = new Nmea2000YdwgParser("Test", m, null);
            var sentence = parser.ParseSentence("07:07:25.846 R DED6703 80 15 00 00 EF 01 FF FF", out error);
            Assert.Null(sentence);
            sentence = parser.ParseSentence("07:07:25.847 R DED6703 81 FF FF FF FF 04 01 3B", out error);
            Assert.Null(sentence);
            sentence = parser.ParseSentence("07:07:25.848 R DED6703 82 07 03 04 04 6C 05 23", out error);
            Assert.Null(sentence);
            sentence = parser.ParseSentence("07:07:25.848 R DED6703 83 50 FF FF FF FF FF FF", out error);
            Assert.NotNull(sentence);
            Assert.IsType<TalkerSentence>(sentence);
            Assert.Equal("$PCDIN,0DED67,0000642D,03,0000EF01FFFFFFFFFFFF04013B070304046C052350FFFFFFFFFFFF*55", sentence.ToString());
        }

        [Fact]
        public void ParseRawNmea2000SentenceFastPacketAndDecode()
        {
            var m = new MemoryStream();
            NmeaError error;
            var parser = new Nmea2000YdwgParser("Test", m, null);
            parser.ParseSentence("07:08:25.258 R DED6703 00 11 01 63 FF 00 F8 04", out error);
            parser.ParseSentence("07:07:25.847 R DED6703 01 01 3B 07 03 04 04 00", out error);
            var sentence = parser.ParseSentence("07:07:25.848 R DED6703 02 01 05 FF FF FF FF FF", out error);
            Assert.NotNull(sentence);
            Assert.IsType<TalkerSentence>(sentence);
            DateTimeOffset lastMessageTime = DateTimeOffset.MinValue;
            var typed = (GroupFunctionMessage?)sentence.TryGetTypedValue(ref lastMessageTime);
            Assert.NotNull(typed);
            Assert.Equal(65379u, typed.Pgn);
            Assert.NotEmpty(typed.Parameters);
            Assert.Equal("Manufacturer", typed.Parameters[0].Description);
            Assert.Equal(1851, typed.Parameters[0].Value); // Raymarine
            Assert.Equal("Industry Code", typed.Parameters[2].Description);
            Assert.Equal(4, typed.Parameters[2].Value); // Marine
            Assert.Equal("Pilot Mode", typed.Parameters[3].Description);
            Assert.Equal(256, typed.Parameters[3].Value); // "Wind mode"
            Assert.Equal("Sub Mode", typed.Parameters[4].Description);
            Assert.Equal(0xFFFF, typed.Parameters[4].Value); // "Don't care"
        }

        [Fact]
        public void ParseCommandGroupFunction()
        {
            var m = new MemoryStream();
            NmeaError error;
            var parser = new Nmea2000YdwgParser("Test", m, null);
            parser.ParseSentence("07:08:25.258 R DED6703,A0,11,01,50,FF,00,F8,04", out error);
            parser.ParseSentence("07:07:25.847 R DED6703,A1,01,3B,07,03,04,05,A4", out error);
            var sentence = parser.ParseSentence("07:07:25.848 R DED6703,A2,51,06,51,4E,FF,FF,FF", out error);
            Assert.NotNull(sentence);
            Assert.IsType<TalkerSentence>(sentence);
            DateTimeOffset lastMessageTime = DateTimeOffset.MinValue;
            var typed = (GroupFunctionMessage?)sentence.TryGetTypedValue(ref lastMessageTime);
            Assert.NotNull(typed);
            Assert.Equal(GroupFunctionMessage.HexId, (int)typed.Identifier);
            Assert.Equal(65360u, typed.Pgn);
            Assert.NotEmpty(typed.Parameters);
            Assert.Equal("Manufacturer", typed.Parameters[0].Description);
            Assert.Equal(1851, typed.Parameters[0].Value); // Raymarine
            Assert.Equal("Industry Code", typed.Parameters[2].Description);
            Assert.Equal(4, typed.Parameters[2].Value); // Marine
            Assert.Equal("Target Heading True", typed.Parameters[4].Description);
            Assert.Equal(20900, typed.Parameters[4].Value);
            Assert.Equal("Target Heading Magnetic", typed.Parameters[5].Description);
            Assert.Equal(20049, typed.Parameters[5].Value);
        }

        [Fact]
        public void ParseCommandGroupFunctionWithIntermediateOtherMessage()
        {
            var m = new MemoryStream();
            NmeaError error;
            var parser = new Nmea2000YdwgParser("Test", m, null);
            parser.ParseSentence("07:08:25.258 R DED6703,A0,11,01,50,FF,00,F8,04", out error);
            parser.ParseSentence("07:07:25.847 R DED6703,A1,01,3B,07,03,04,05,A4", out error);
            var otherSentence = parser.ParseSentence("08:33:14.874 R 1CFF6300,3B,9F,00,00,00,00,00,00", out error);
            Assert.NotNull(otherSentence);
            var sentence = parser.ParseSentence("07:07:25.848 R DED6703,A2,51,06,51,4E,FF,FF,FF", out error);
            Assert.NotNull(sentence);
            Assert.IsType<TalkerSentence>(sentence);
            DateTimeOffset lastMessageTime = DateTimeOffset.MinValue;
            var typed = (GroupFunctionMessage?)sentence.TryGetTypedValue(ref lastMessageTime);
            Assert.NotNull(typed);
            Assert.Equal(65360u, typed.Pgn);
            Assert.NotEmpty(typed.Parameters);
            Assert.Equal("Manufacturer", typed.Parameters[0].Description);
            Assert.Equal(1851, typed.Parameters[0].Value); // Raymarine
            Assert.Equal("Industry Code", typed.Parameters[2].Description);
            Assert.Equal(4, typed.Parameters[2].Value); // Marine
            Assert.Equal("Target Heading True", typed.Parameters[4].Description);
            Assert.Equal(20900, typed.Parameters[4].Value);
            Assert.Equal("Target Heading Magnetic", typed.Parameters[5].Description);
            Assert.Equal(20049, typed.Parameters[5].Value);
        }

        [Fact]
        public void ParseCommandGroupFunctionWithMissingFirstPart()
        {
            var m = new MemoryStream();
            NmeaError error;
            var parser = new Nmea2000YdwgParser("Test", m, null);
            var otherSentence = parser.ParseSentence("08:33:14.874 R 1CFF6300,3B,9F,00,00,00,00,00,00", out error);
            Assert.NotNull(otherSentence);
            var sentence = parser.ParseSentence("07:07:25.848 R DED6703,A2,51,06,51,4E,FF,FF,FF", out error);
            Assert.Null(sentence);
        }

        [Fact]
        public void ParseRawNmea2000SentenceAndDecode()
        {
            var m = new MemoryStream();
            var parser = new Nmea2000YdwgParser("Test", m, null);
            var sentence = parser.ParseSentence("17:33:21.141 R 01F20002 00 00 3C FF FF 64 FF FF", out NmeaError error);
            Assert.NotNull(sentence);
            Assert.IsType<TalkerSentence>(sentence);
            Assert.Equal("$PCDIN,01F200,0000F6E1,02,00003CFFFF64FFFF*51", sentence.ToString());
            DateTimeOffset lastMessageTime = DateTimeOffset.MinValue;
            var typed = (SeaSmartEngineFast?)sentence.TryGetTypedValue(ref lastMessageTime);
            Assert.NotNull(typed);
            Assert.Equal(0, typed.EngineNumber);
            Assert.Equal(3600, typed.RotationalSpeed.RevolutionsPerMinute);
        }

        [Fact]
        public void EncodeGroupFunctionAcknowledgement()
        {
            // The message we received: "Seatalk: Pilot Mode" change to auto
            GroupFunctionMessage msg = new GroupFunctionMessage(GroupFunction.Command);
            msg.MessageSource = 0x55;
            msg.Pgn = 65379u;
            // Note: Often not equal to the number of declared fields (as e.g. reserved fields are skipped)
            msg.NumberOfArguments = 4;
            var decl = Nmea2000Declarations.GetByPgn(65379u);
            msg.Parameters.Clear();
            msg.Parameters.AddRange(decl!.FieldDeclarations);
            msg.Parameters[0].Value = (int)ManufacturerCode.Raymarine;
            msg.Parameters[2].Value = (int)IndustryCode.Marine;
            msg.Parameters[3].Value = (int)64; // Set to auto
            msg.Parameters[4].Value = (int)0;

            var reply = msg.CreateAck();
            Assert.Equal(0, reply.PgnErrorCode);
            // Note: The PCDIN message is one message only, regardless of the payload length. So fastpacket headers
            // are not included in the payload.
            Assert.Equal("$PCDIN,01ED55,00000000,00,0263FF0000040000*53", reply.ToNmeaMessage());
            Assert.True(reply.PgnDeclaration!.FastPacket);
        }

        [Fact]
        public void EncodeGroupFunctionNoAcknowledgement()
        {
            GroupFunctionMessage msg = new GroupFunctionMessage(GroupFunction.Command);
            msg.MessageSource = 55;
            msg.Pgn = 65379u;
            // Note: Often not equal to the number of declared fields (as e.g. reserved fields are skipped)
            msg.NumberOfArguments = 4;
            var decl = Nmea2000Declarations.GetByPgn(65379u);
            msg.Parameters.Clear();
            msg.Parameters.AddRange(decl!.FieldDeclarations);

            var reply = msg.CreateNoAck(x => x.FieldNumber == 4 ? 4 : null);
            Assert.Equal(4, reply.PgnErrorCode);
            // Note: The PCDIN message is one message only, regardless of the payload length. So fastpacket headers
            // are not included in the payload.
            Assert.Equal("$PCDIN,01ED37,00000000,00,0263FF0004FF*57", reply.ToNmeaMessage());
            Assert.True(reply.PgnDeclaration!.FastPacket);
        }

        [Fact]
        public void SendSimpleMessageToNmea2000()
        {
            var m = new MemoryStream();
            var parser = new Nmea2000YdwgParser("Test", m, m);
            parser.StartDecode();
            try
            {
                SeaSmartEngineFast fast = new SeaSmartEngineFast(new EngineData(0, EngineStatus.CheckEngine,
                    0, RotationalSpeed.FromRevolutionsPerMinute(1000), Ratio.Zero, TimeSpan.FromHours(102.1), null));

                Assert.NotNull(fast);
                parser.SenderId = 2;
                parser.SendSentence(fast);
                int iterations = 100;
                while (m.Length < 30 && iterations-- > 0)
                {
                    Thread.Sleep(100);
                }

                Thread.Sleep(100);
                m.Position = 0;
                string output = Encoding.ASCII.GetString(m.ToArray());
                Assert.NotEmpty(output);
                Assert.Equal("09F20000 00 00 11 FF FF 00 FF FF \r\n", output);
            }
            finally
            {
                parser.StopDecode();
            }
        }

        [Fact]
        public void SendFastMessageToNmea2000()
        {
            var m = new MemoryStream();
            var parser = new Nmea2000YdwgParser("Test", m, m);
            parser.StartDecode();
            try
            {
                SeaSmartEngineDetail fast = new SeaSmartEngineDetail(new EngineData(0, EngineStatus.CheckEngine,
                    0, RotationalSpeed.FromRevolutionsPerMinute(1000), Ratio.Zero, TimeSpan.FromHours(102.1), Temperature.FromDegreesCelsius(200)));

                Assert.NotNull(fast);
                parser.SenderId = 2;
                parser.SendSentence(fast);
                int iterations = 100;
                while (m.Length < 30 && iterations-- > 0)
                {
                    Thread.Sleep(100);
                }

                Thread.Sleep(100);
                m.Position = 0;
                string output = Encoding.ASCII.GetString(m.ToArray());
                Assert.NotEmpty(output);
                Assert.Equal(@"09F20100 20 1A 00 00 00 FF FF D3 
09F20100 21 B8 00 05 00 00 C8 9B 
09F20100 22 05 00 FF FF 00 00 00 
09F20100 23 01 00 00 00 7F 7F FF 
", output);
            }
            finally
            {
                parser.StopDecode();
            }
        }

        [Fact]
        public void DecodeNothing()
        {
            var m = new MemoryStream();
            var parser = new Nmea2000YdwgParser("Test", m, null);
            parser.StartDecode();
            parser.StopDecode();
        }

        [Fact]
        public void VesselHeadingDecode1()
        {
            var ts = new TalkerSentence(TalkerId.Proprietary, Nmea2000PackedMessage.Id, new List<string>()
            {
                "1F112", "000074C5", "57", "FFDB59FF7F0C02FC"
            });

            var p = new VesselHeading(ts, DateTimeOffset.UnixEpoch);
            Assert.NotNull(p);
            Assert.Equal(131.8, p.Heading.Degrees, 0.1);
            Assert.True(p.Variation.HasValue);
            Assert.Equal(3.0, p.Variation.Value.Degrees, 0.1);
            var result = p.ToNmeaParameterList();
            Assert.Equal("01F112,000074C5,57,FFDB59FF7F0C02FC", result);
        }

        [Fact]
        public void VesselHeadingDecode2()
        {
            var ts = new TalkerSentence(TalkerId.Proprietary, Nmea2000PackedMessage.Id, new List<string>()
            {
                "1F112", "000074C5", "57", "FF5B3D98FCD106FD"
            });

            var p = new VesselHeading(ts, DateTimeOffset.UnixEpoch);
            Assert.NotNull(p);
            Assert.Equal(90, p.Heading.Degrees, 0.1);
            Assert.True(p.Variation.HasValue);
            Assert.Equal(10.0, p.Variation.Value.Degrees, 0.1);
            Assert.Equal(-5, p.Deviation.GetValueOrDefault().Degrees, 0.1);
        }

        [Fact]
        public void VesselHeadingEncode()
        {
            VesselHeading vs = new VesselHeading(Angle.FromDegrees(90), Angle.FromDegrees(-5), Angle.FromDegrees(10), true);
            Assert.Equal("01F112,00000000,00,FF5B3D98FCD106FD", vs.ToNmeaParameterList());
        }

        [Fact]
        public void RudderAngleDecode1()
        {
            var ts = new TalkerSentence(TalkerId.Proprietary, Nmea2000PackedMessage.Id, new List<string>()
            {
                "01F10D", "000074C5", "57", "00FF2A0AFF09FFFF"
            });

            var p = new Rudder(ts, DateTimeOffset.UnixEpoch);
            Assert.NotNull(p);
            Assert.Equal(14.9, p.DesiredAngle.GetValueOrDefault().Degrees, 0.1);
            Assert.Equal(14.7, p.ActualAngle.GetValueOrDefault().Degrees, 0.1);
            Assert.Equal(TurnDirection.NoCommand, p.DirectionOrder);
            var result = p.ToNmeaMessage();
            DateTimeOffset t = DateTimeOffset.MinValue;
            Assert.Equal(ts.GetAsRawSentence(ref t).ToNmeaMessage(), result);
        }

        [Fact]
        public void RudderAngleDecode2()
        {
            var ts = new TalkerSentence(TalkerId.Proprietary, Nmea2000PackedMessage.Id, new List<string>()
            {
                "01F10D", "000074C5", "57", "FCF82A0AFF7FFFFF"
            });

            var p = new Rudder(ts, DateTimeOffset.UnixEpoch);
            Assert.NotNull(p);
            Assert.Equal(14.9, p.DesiredAngle.GetValueOrDefault().Degrees, 0.1);
            Assert.False(p.ActualAngle.HasValue);
            Assert.Equal(TurnDirection.NoCommand, p.DirectionOrder);
            var result = p.ToNmeaMessage();
            DateTimeOffset t = DateTimeOffset.MinValue;
            Assert.Equal(ts.GetAsRawSentence(ref t).ToNmeaMessage(), result);
        }

        [Fact]
        public void RudderAngleEncode()
        {
            VesselHeading vs = new VesselHeading(Angle.FromDegrees(90), null, null, true);
            Assert.Equal("01F112,00000000,00,FF5B3DFF7FFF7FFD", vs.ToNmeaParameterList());
        }

        [Fact]
        public void FastPositionUpdate()
        {
            var ts = new TalkerSentence(TalkerId.Proprietary, Nmea2000PackedMessage.Id, new List<string>()
            {
                "01F801", "000074C5", "57", "46AED12063C85306"
            });

            var p = new FastPositionUpdate(ts, DateTimeOffset.UnixEpoch);
            Assert.NotNull(p);
            Assert.True(p.Latitude > 55 && p.Latitude < 55.1);
            Assert.True(p.Longitude > 10.5 && p.Longitude < 11);
            var result = p.ToNmeaParameterList();
            Assert.Equal("01F801,000074C5,57,46AED12063C85306", result);
        }

        [Fact]
        public void FastPositionUpdateWithNegativeLatLong()
        {
            // Encode
            var p = new FastPositionUpdate(new GeographicPosition(-10.21, -20.45, 0));
            var result = p.ToNmeaParameterList();
            Assert.Equal("01F801,00000000,00,E013EAF9E093CFF3", result);

            // Decode
            var ts = new TalkerSentence(TalkerId.Proprietary, Nmea2000PackedMessage.Id,
                result.Split(",", StringSplitOptions.TrimEntries));
            var p2 = new FastPositionUpdate(ts, DateTimeOffset.UnixEpoch);
            Assert.Equal(p.Latitude, p2.Latitude, 1E-7);
            Assert.Equal(p.Longitude, p2.Longitude, 1E-7);
        }

        [Fact]
        public void SeatalkNgPilotHeadingMessageDecode()
        {
            var ts = new TalkerSentence(TalkerId.Proprietary, Nmea2000PackedMessage.Id, new List<string>()
            {
                "00FF4F", "000074C5", "57", "3B9FFFFFFF46EDFF"
            });

            var p = new SeatalkNgPilotHeading(ts, DateTimeOffset.UnixEpoch);
            Assert.NotNull(p);
            Assert.Null(p.HeadingTrue);
            Assert.True(p.HeadingMagnetic.HasValue);
            Assert.Equal(IndustryCode.Marine, p.Industry);
            Assert.Equal(ManufacturerCode.Raymarine, p.Manufacturer);
            Assert.Equal(348.02, p.HeadingMagnetic.GetValueOrDefault().Degrees, 1E-2);
            var result = p.ToNmeaParameterList();
            Assert.Equal("00FF4F,000074C5,57,3B9FFFFFFF46EDFF", result);
        }

        [Fact]
        public void SeatalkNgPilotLockedHeadingMessageDecode()
        {
            var ts = new TalkerSentence(TalkerId.Proprietary, Nmea2000PackedMessage.Id, new List<string>()
            {
                "00FF50", "000074C5", "57", "3B9FFFFFFF46EDFF"
            });

            var p = new SeatalkNgPilotLockedHeading(ts, DateTimeOffset.UnixEpoch);
            Assert.NotNull(p);
            Assert.Null(p.TargetHeadingTrue);
            Assert.True(p.TargetHeadingMagnetic.HasValue);
            Assert.Equal(348.02, p.TargetHeadingMagnetic.GetValueOrDefault().Degrees, 1E-2);
            var result = p.ToNmeaParameterList();
            Assert.Equal("00FF50,000074C5,57,3B9FFFFFFF46EDFF", result);
        }

        [Fact]
        public void SeatalkNgConfigurationReply()
        {
            var msg = new SeatalkNgPilotConfigurationValue()
            {
                Command = 38, ProprietaryId = 108, MessageSource = 0,
                Value = true,
            };

            Assert.NotEmpty(msg.PgnDeclaration!.FieldDeclarations);
            Assert.Equal("$PCDIN,01EF00,00000000,00,3B9F6C260001000000*2C", msg.ToNmeaMessage());
            Assert.True(msg.PgnDeclaration.FastPacket);
        }

        [Theory]
        [InlineData("$PCDIN,00FF50,000074C5,57,3B9FFFFFFF46EDFF")]
        [InlineData("$PCDIN,00FF4F,000074C5,57,3B9FFFFFFF46EDFF")]
        [InlineData("$PCDIN,01F801,00000000,00,E013EAF9E093CFF3")]
        [InlineData("$PCDIN,01F200,0000F6E1,02,00003CFFFF64FFFF")]
        public void CanParseAllTheseMessages(string input)
        {
            DateTimeOffset lastPacketTime = DateTimeOffset.MinValue;
            var inSentence = TalkerSentence.FromSentenceString(input, out var error);
            Assert.Equal(NmeaError.None, error);
            Assert.NotNull(inSentence);
            var decoded = inSentence!.TryGetTypedValue(ref lastPacketTime);
            Assert.NotNull(decoded);
            Assert.False(decoded is RawSentence);
            Assert.True(decoded is Nmea2000PackedMessage);
        }

        [Theory]
        [InlineData(260001)]
        [InlineData(260002)]
        public void CzoneModuleAnnounceMessageRoundtrip(uint serialNumber)
        {
            var original = new CzoneModuleAnnounce(serialNumber, 0x40);
            Assert.Equal(serialNumber, original.SerialNumber);

            string sentenceText = original.ToNmeaMessage();
            if (serialNumber == 260001)
            {
                // Verify exactly the expected output for this known serial number
                Assert.Equal("$PCDIN,00FF0A,00000000,00,2799A1F703000040*22", sentenceText);
            }

            var sentence = TalkerSentence.FromSentenceString(sentenceText, out var error);

            Assert.Equal(NmeaError.None, error);
            Assert.NotNull(sentence);

            DateTimeOffset lastPacketTime = DateTimeOffset.MinValue;
            var decoded = sentence!.TryGetTypedValue(ref lastPacketTime);
            var roundtrip = Assert.IsType<CzoneModuleAnnounce>(decoded);

            Assert.Equal(original.Identifier, roundtrip.Identifier);
            Assert.Equal(original.DipSwitch, roundtrip.DipSwitch);
            Assert.Equal(original.ToNmeaParameterList(), roundtrip.ToNmeaParameterList());
            Assert.Equal(original.SerialNumber, roundtrip.SerialNumber);
        }
    }
}
