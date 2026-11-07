// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System;
using System.Buffers.Binary;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Iot.Device.Common;
using Iot.Device.Nmea0183.Sentences;
using Microsoft.Extensions.Logging;
using UnitsNet;

namespace Iot.Device.Nmea0183
{
    /// <summary>
    /// A parser that decodes YDWG NMEA2000 messages into PCDIN sequences.
    /// This message is used by Yacht Device's YDWG-02 Wifi-to-NMEA2000 interface.
    /// </summary>
    public class Nmea2000YdwgParser : NmeaParser
    {
        private const int TransmitConfirmationTimeout = 1000;
        private ulong _pgnAwaitingSend = 0;
        private Dictionary<uint, string> _unknownPgnsSeen = new Dictionary<uint, string>();
        private uint _fastPacketSequencer = 0;

        private Dictionary<uint, List<byte>> _incompleteFastPackets =
            new Dictionary<uint, List<byte>>();

        /// <summary>
        /// Constructs an instance of this type
        /// </summary>
        /// <param name="interfaceName">Friendly name of this interface (used for filtering and eventually logging)</param>
        /// <param name="dataSource">Data source (may be connected to a serial port, a network interface, or whatever). It is recommended to use a blocking Stream,
        /// to prevent unnecessary polling</param>
        /// <param name="dataSink">Optional data sink, to send information. Can be null, and can be identical to the source stream</param>
        public Nmea2000YdwgParser(string interfaceName, Stream dataSource, Stream? dataSink)
            : base(interfaceName, dataSource, dataSink, new Raw8BitEncoding())
        {
            SenderId = 0;
        }

        /// <summary>
        /// Our own sender ID. Typically ignored during sending, as the interface substitutes its own address automatically.
        /// When using a low-level CAN-Bus interface, this needs to be set after sending an ISO Address Request Message.
        /// </summary>
        public byte SenderId
        {
            get;
            set;
        }

        /// <summary>
        /// Parses a NMEA 2000 Message in Yacht Devices RAW format.
        /// Data format is as follows:
        /// <example>
        /// 17:33:21.107 R 19F51323 01 2F 30 70 00 2F 30 70
        /// 17:33:21.108 R 19F51323 02 00
        /// 17:33:21.141 R 09F80115 A0 7D E6 18 C0 05 FB D5
        /// </example>
        /// </summary>
        /// <param name="currentLine">The current line, see example</param>
        /// <param name="error">Receives a parser error type, if any</param>
        /// <returns>A sentence in NMEA0183 raw format, or null</returns>
        protected internal override TalkerSentence? ParseSentence(string currentLine, out NmeaError error)
        {
            string[] splits = currentLine.Split(new char[] { ' ', ',' }, StringSplitOptions.TrimEntries);
            if (splits.Length <= 4)
            {
                error = NmeaError.MessageToShort;
                return null;
            }

            if (splits[1] == "T")
            {
                if (UInt32.TryParse(splits[2], NumberStyles.HexNumber, CultureInfo.InvariantCulture, out uint pgnTransmit))
                {
                    pgnTransmit = (pgnTransmit >> 8) & 0x1FFFF;
                    if (pgnTransmit == _pgnAwaitingSend)
                    {
                        Logger.LogDebug($"Received confirmation that we sent {currentLine.Trim()}");
                        Interlocked.Exchange(ref _pgnAwaitingSend, 0);
                    }
                }

                error = NmeaError.None;
                return null;
            }

            if (splits[1] != "R")
            {
                error = NmeaError.NoSyncByte;
                return null;
            }

            if (UInt32.TryParse(splits[2], NumberStyles.HexNumber, CultureInfo.InvariantCulture, out uint pgn))
            {
                // This value appears to be pretty random and just indicates time since application/converter start
                TimeSpan timeStamp;
                if (!TimeSpan.TryParse(splits[0], CultureInfo.InvariantCulture, out timeStamp))
                {
                    timeStamp = TimeSpan.Zero;
                }

                byte[] bytes;
                try
                {
                    var s = string.Join(string.Empty, splits.Skip(3));
                    bytes = Convert.FromHexString(s);
                }
                catch (FormatException x)
                {
                    Logger.LogError($"Exception {x.Message} when parsing msg {currentLine}");
                    error = NmeaError.InvalidChecksum;
                    return null;
                }

                var declaration = Nmea2000Declarations.GetByPgn(pgn >> 8);

                if (declaration != null)
                {
                    TalkerSentence? result = null;
                    if (declaration.FastPacket && bytes.Length > 0)
                    {
                        // If this is the first part of a multipart sequence, start the merger
                        if ((bytes[0] & 0xF) == 0)
                        {
                            // In this dictionary, the key is deliberately including the source, since
                            // there could be two fast packet messages with the same pgn from two different sources
                            if (_incompleteFastPackets.ContainsKey(pgn))
                            {
                                _incompleteFastPackets[pgn]!.Clear();
                            }
                            else
                            {
                                _incompleteFastPackets[pgn] = new List<byte>();
                            }
                        }

                        if (_incompleteFastPackets.TryGetValue(pgn, out var toUse))
                        {
                            toUse.AddRange(bytes);
                        }

                        if (toUse != null && toUse.Count > 2)
                        {
                            result = CreateSentence(declaration, timeStamp, pgn >> 8, pgn & 0xFF, toUse);
                            if (result != null)
                            {
                                _incompleteFastPackets[pgn]!.Clear();
                            }
                        }
                    }
                    else
                    {
                        List<byte> activeData = new List<byte>(50);
                        activeData.AddRange(bytes);
                        result = CreateSentence(declaration, timeStamp, pgn >> 8, pgn & 0xFF, activeData);
                    }

                    if (result != null)
                    {
                        error = NmeaError.None;
                        return result;
                    }
                }
                else if (declaration == null)
                {
                    uint rawpgn = (pgn >> 8) & 0x1FFFF;
                    if (_unknownPgnsSeen.TryAdd(rawpgn, currentLine))
                    {
                        Logger.LogInformation($"New Unknown PGN: {rawpgn:X6} with line {currentLine}");
                    }

                    error = NmeaError.None;
                    return null;
                }

                // Message is known, but incomplete
                error = NmeaError.None;
                return null;
            }

            error = NmeaError.NoSyncByte;
            return null;
        }

        /// <summary>
        /// Returns the list of (so-far) seen unknown PGNs together with an example payload.
        /// </summary>
        /// <returns>A dictionary</returns>
        public Dictionary<uint, string> GetListOfUnknownPgns()
        {
            return _unknownPgnsSeen;
        }

        private TalkerSentence? CreateSentence(Nmea2000PgnDeclaration declaration, TimeSpan? timeStamp, uint pgn, uint sender, List<byte> allData)
        {
            if (allData.Count < declaration.Length && declaration.FastPacket == false)
            {
                return null;
            }

            var fields = new List<string>();

            // Should usually be equivalent to declaration.Pgn, but may include a destination address,
            // which we shouldn't be loosing
            fields.Add($"{pgn:X6}");
            if (timeStamp.HasValue)
            {
                fields.Add($"{((long)timeStamp.Value.TotalSeconds):X8}");
            }
            else
            {
                fields.Add("00000000");
            }

            fields.Add($"{sender:X2}");
            if (declaration.FastPacket)
            {
                if (allData.Count < 2)
                {
                    Logger.LogWarning("Found a FastPacket message with less than 2 bytes");
                    return null;
                }

                int sequenceIdentifier = allData[0] >> 5;
                int sequenceNo = allData[0] & 0x1F;
                if (sequenceNo != 0)
                {
                    allData.Clear();
                    return null;
                }

                int dataLength = allData[1];
                List<byte> fullSequence = new List<byte>(dataLength);
                int srcIndex = 2;

                while (srcIndex < allData.Count)
                {
                    fullSequence.Add(allData[srcIndex]);

                    srcIndex++;
                    if (srcIndex >= allData.Count)
                    {
                        break;
                    }

                    if (srcIndex % 8 == 0)
                    {
                        int newSequenceIdentifier = allData[srcIndex] >> 5;
                        if (newSequenceIdentifier != sequenceIdentifier)
                        {
                            allData.Clear();
                            return null;
                        }

                        int newSequenceNo = allData[srcIndex] & 0x1F;
                        if (newSequenceNo != sequenceNo + 1)
                        {
                            allData.Clear();
                            return null;
                        }

                        sequenceNo++;
                        // If this indeed is the continuation of the correct message, skip the byte and continue
                        srcIndex++;
                    }
                }

                if (fullSequence.Count >= dataLength)
                {
                    fields.Add(Convert.ToHexString(fullSequence.ToArray()));
                }
                else
                {
                    return null;
                }
            }
            else
            {
                fields.Add(Convert.ToHexString(allData.ToArray()));
            }

            var ret = new TalkerSentence(TalkerId.Proprietary, Nmea2000PackedMessage.Id, fields);
            return ret;
        }

        /// <inheritdoc/>
        protected internal override void FormatAndSendSentence(NmeaSentence sentence)
        {
            if (sentence is Nmea2000PackedMessage packed)
            {
                uint priority = 7;
                if (packed.Priority.HasValue)
                {
                    priority = packed.Priority.Value;
                }
                else if (packed.PgnDeclaration != null)
                {
                    priority = packed.PgnDeclaration.Priority;
                }

                // This is a bit hacky, as we go through the string representation of the object.
                // But having also a binary representation increases complexity for the individual messages.
                // Maybe we improve that later.
                string nmea0183 = packed.ToNmeaParameterList();
                string[] splits = nmea0183.Split(',', StringSplitOptions.TrimEntries);
                if (!uint.TryParse(splits[0], NumberStyles.HexNumber, CultureInfo.InvariantCulture, out uint pgn))
                {
                    Logger.LogError($"Attempting to send invalid composed message: {nmea0183}");
                }

                uint pgnToSend = pgn | ((priority & 0x7) << 18);

                StringBuilder data;
                StringBuilder sendData;
                if (packed.PgnDeclaration != null && packed.PgnDeclaration.FastPacket)
                {
                    sendData = new StringBuilder();
                    int totalBytesInMessage = splits[3].Length / 2;
                    uint messageSequence = Interlocked.Increment(ref _fastPacketSequencer) % 8;
                    data = new StringBuilder();
                    int bytesProcessed = 0;
                    int bytesInMessage = 2;
                    uint sequenceNo = 0;
                    uint sequenceAndNumber = (messageSequence & 0x7) << 5 | sequenceNo;
                    data.Append(sequenceAndNumber.ToString("X2", CultureInfo.InvariantCulture));
                    data.Append(' ');
                    data.Append(totalBytesInMessage.ToString("X2", CultureInfo.InvariantCulture));
                    data.Append(' ');
                    while (bytesProcessed < totalBytesInMessage)
                    {
                        data.Append(splits[3].Substring(bytesProcessed * 2, 2));
                        data.Append(' ');
                        bytesInMessage++;
                        bytesProcessed++;
                        if (bytesInMessage >= 8)
                        {
                            sendData.AppendLine($"{pgnToSend << 8:X8} {data}");
                            sequenceNo++;
                            data.Clear();
                            sequenceAndNumber = (messageSequence & 0x7) << 5 | sequenceNo;
                            data.Append(sequenceAndNumber.ToString("X2", CultureInfo.InvariantCulture));
                            data.Append(' ');
                            bytesInMessage = 1;
                        }
                    }

                    // If there's just one header byte here, we don't need to send this sequence
                    if (bytesInMessage > 1)
                    {
                        int fillers = 8 - bytesInMessage;
                        for (int i = 0; i < fillers; i++)
                        {
                            data.Append("FF ");
                        }

                        sendData.AppendLine($"{pgnToSend << 8:X8} {data}");
                    }
                }
                else
                {
                    data = new StringBuilder(splits[3]);

                    int idx = 0;
                    while (idx < data.Length)
                    {
                        idx += 2;
                        data.Insert(idx, ' ');
                        idx += 1;
                    }

                    // The PGN (including priority and destination address) is 29 bits long.
                    sendData = new StringBuilder($"{pgnToSend << 8:X8} {data}\r\n");
                }

                int loops = TransmitConfirmationTimeout / 20;
                ////while (Interlocked.Read(ref _pgnAwaitingSend) != 0 && loops-- >= 0)
                ////{
                ////    Thread.Sleep(10);
                ////}

                if (loops < 0)
                {
                    Logger.LogWarning($"Previous outgoing message with pgn {pgn:X8} was not confirmed");
                }

                Interlocked.Exchange(ref _pgnAwaitingSend, pgn);

                var outgoingString = sendData.ToString();

                byte[] buffer = StreamEncoding.GetBytes(outgoingString);

                // Logger.LogInformation($"Attempting to send PGN {pgn}");
                Sink?.Write(buffer, 0, buffer.Length);
            }
            else
            {
                Logger.LogWarning("Can only send Nmea2000PackedMessage instances with this interface ($PCDIN sequences)");
                return;
            }
        }

        /// <inheritdoc/>
        protected override void Dispose(bool disposing)
        {
            base.Dispose(disposing);
            foreach (var kp in _unknownPgnsSeen)
            {
                Logger.LogInformation($"PGN {kp.Key} is not known but was seen");
            }
        }

        /// <summary>
        /// Finds a Device from "yacht devices" on the local network.
        /// </summary>
        /// <param name="identifier">The name of the device.
        /// For YDWG-03, the string is "YDWG", other devices like the YDEN-02 and YDNR-02
        /// should work as well, but have not been tested and their identification string is uncertain</param>
        /// <param name="preferred">Try this one first</param>
        /// <param name="logger">Optional logger</param>
        /// <returns>The IP address of the first device or null if none was found</returns>
        /// <remarks>
        /// This only tests for the presence of the device, it does not check which ports are available
        /// and how it is configured.
        /// </remarks>
        public static async Task<IPAddress?> FindCompatibleDevice(string identifier, IPAddress? preferred, ILogger? logger = null)
        {
            var interf = NetworkServiceSearcher.GetPrimaryNetworkInterface();
            var list = NetworkServiceSearcher.GetAllValidAddressesInSubnet(interf.Address, interf.Mask);
            if (preferred != null)
            {
                list.Insert(0, preferred);
            }

            IPAddress? result = null;
            using (var client = new HttpClient())
            {
                ParallelOptions po = new ParallelOptions() { MaxDegreeOfParallelism = 3 };
                object oneLock = new object();
                await Parallel.ForEachAsync(list, po, async (candidate, token) =>
                {
                    lock (oneLock)
                    {
                        if (result != null)
                        {
                            return;
                        }
                    }

                    if (await NetworkServiceSearcher.IsYachtDevicesInterface(client, candidate, logger, identifier))
                    {
                        lock (oneLock)
                        {
                            result = candidate;
                        }
                    }
                });
            }

            return result;
        }
    }
}
