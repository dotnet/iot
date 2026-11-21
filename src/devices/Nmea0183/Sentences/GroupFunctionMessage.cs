// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using UnitsNet;

#pragma warning disable CS1591
namespace Iot.Device.Nmea0183.Sentences
{
    /// <summary>
    /// This is a very versatile message of the Nmea2000 protocol. Depending on the function code
    /// (field 1) it can have many different meanings.
    /// </summary>
    public sealed class GroupFunctionMessage : Nmea2000PackedMessage
    {
        // This message is usually addressed, so the last byte of the Id is the destination address (0xFF for
        // broadcast)
        public const int HexId = 0x1ED00;

        public GroupFunctionMessage(GroupFunction function)
        {
            Function = function;
            Valid = true;
            Parameters = new List<FieldDeclaration>();
        }

        /// <summary>
        /// Create a message object from a sentence
        /// </summary>
        /// <param name="sentence">The sentence</param>
        /// <param name="time">The current time</param>
        public GroupFunctionMessage(TalkerSentence sentence, DateTimeOffset time)
            : this(sentence.TalkerId, Matches(sentence) ? sentence.Fields : throw new ArgumentException($"SentenceId does not match expected id '{Id}'"), time)
        {
        }

        /// <summary>
        /// Creates a message object from a decoded sentence
        /// </summary>
        /// <param name="talkerId">The source talker id</param>
        /// <param name="fields">The parameters</param>
        /// <param name="time">The current time</param>
        public GroupFunctionMessage(TalkerId talkerId, IEnumerable<string> fields, DateTimeOffset time)
            : base(talkerId, Id, time)
        {
            // We expect the fast-packet message to be decoded into a single frame here already
            IEnumerator<string> field = fields.GetEnumerator();

            ParseCommonFields(field, true);

            string data = ReadString(field);

            if (ReadByteFromHexString(data, 0, out byte f))
            {
                Function = (GroupFunction)f;
            }

            if (ReadUnsignedFromHexString(data, 2, 6, true, out uint pgn))
            {
                Pgn = (uint)pgn;
            }

            int nextByte;
            if (Function == GroupFunction.Request)
            {
                TransmissionInterval = null;
                if (ReadUintFromHexString(data, 8, out uint interval))
                {
                    // -1 is "Once" and -2 is "Reset to default" (which I have never observed as being in use)
                    if (interval > 0)
                    {
                        TransmissionInterval = TimeSpan.FromMilliseconds(interval);
                    }
                }

                TransmissionOffset = null;
                if (ReadUshortFromHexString(data, 16, out ushort offset))
                {
                    if (offset > 0)
                    {
                        TransmissionOffset = TimeSpan.FromMilliseconds(offset);
                    }
                }

                nextByte = 10;
            }
            else if (Function == GroupFunction.Command)
            {
                // Don't care about priority
                nextByte = 5;
            }
            else
            {
                // Unknown type of command message
                Valid = false;
                Parameters = new List<FieldDeclaration>();
                return;
            }

            NumberOfArguments = 0;
            if (ReadByteFromHexString(data, nextByte * 2, out byte argCnt))
            {
                NumberOfArguments = argCnt;
                nextByte += 1;
            }

            // Note: Need to get the declaration for the target PGN, not our own.
            var fieldDesc = Nmea2000Declarations.GetByPgn(Pgn)?.FieldDeclarations;
            if (fieldDesc != null)
            {
                List<FieldDeclaration> actualValues =
                    new List<FieldDeclaration>(
                        fieldDesc);

                for (int i = 0; i < NumberOfArguments; i++)
                {
                    if (!ReadByteFromHexString(data, nextByte * 2, out byte index))
                    {
                        break;
                    }

                    var thisField = actualValues.FirstOrDefault(x => x.FieldNumber == index);
                    if (thisField != null)
                    {
                        ReadSignedFromHexString(data, (nextByte + 1) * 2, thisField.FieldSize * 2, true, out int v);
                        thisField.Value = v;
                        nextByte = nextByte + 1 + thisField.FieldSize;
                    }
                }

                Parameters = actualValues;
                Console.WriteLine($"Received GroupFunction message {Function} for {pgn} with {NumberOfArguments} arguments");
            }
            else
            {
                Parameters = new List<FieldDeclaration>();
            }

            Valid = true;
        }

        public static List<FieldDeclaration> CloneParameters(List<FieldDeclaration> parameters)
        {
            var ret = new List<FieldDeclaration>(parameters.Count);
            foreach (var p in parameters)
            {
                ret.Add(p with { });
            }

            return ret;
        }

        /// <summary>
        /// Returns true if the constants (as declared in the FieldDeclarations of the target PGN)
        /// match with the command.
        /// </summary>
        public bool ParameterConstantsMatch()
        {
            foreach (var p in Parameters)
            {
                if (p.Constant.HasValue)
                {
                    if (!p.Value.HasValue)
                    {
                        // For constant fields, we expect a value
                        return false;
                    }

                    if (!p.Value.Equals(p.Constant))
                    {
                        return false;
                    }
                }
            }

            return true;
        }

        public int NumberOfArguments { get; set; }

        public GroupFunction Function
        {
            get;
            set;
        }

        /// <inheritdoc/>
        public override bool ReplacesOlderInstance => false;

        public uint Pgn { get; set; }

        public List<FieldDeclaration> Parameters
        {
            get;
            private set;
        }

        public TimeSpan? TransmissionInterval
        {
            get;
            set;
        }

        public TimeSpan? TransmissionOffset
        {
            get;
            set;
        }

        /// <inheritdoc/>
        public override uint Identifier => HexId;

        /// <summary>
        /// This only applies when the Function code is "Acknowledge"
        /// </summary>
        public int PgnErrorCode { get; set; }

        public GroupFunctionMessage CreateAck()
        {
            if (Function != GroupFunction.Command)
            {
                throw new InvalidOperationException("Can only send an ack to Command requests");
            }

            var reply = new GroupFunctionMessage(GroupFunction.Acknowledge);
            reply.NumberOfArguments = NumberOfArguments;
            reply.Pgn = Pgn;
            reply.PgnErrorCode = 0;
            reply.DestinationAddress = MessageSource;
            reply.Parameters = CloneParameters(Parameters);
            // We're ignoring the transmission interval for now
            // We also don't really need to set up the parameter list. For now,
            // We just set ack to all fields.
            return reply;
        }

        public GroupFunctionMessage CreateNoAck(Func<FieldDeclaration, int?> parameterError)
        {
            if (Function != GroupFunction.Command && Function != GroupFunction.Request)
            {
                throw new InvalidOperationException("Can only send an ack to Command requests");
            }

            var reply = new GroupFunctionMessage(GroupFunction.Acknowledge);
            reply.NumberOfArguments = NumberOfArguments;
            reply.Pgn = Pgn;
            reply.PgnErrorCode = 0x4; // Not supported
            reply.DestinationAddress = MessageSource;
            reply.Parameters = CloneParameters(Parameters);
            foreach (var p in reply.Parameters)
            {
                p.ParameterError = parameterError(p);
            }

            // We're ignoring the transmission interval for now
            // We also don't really need to set up the parameter list. For now,
            // We just set ack to all fields.
            return reply;
        }

        public override string ToReadableContent()
        {
            return $"Group function {Function} for Pgn {Pgn}";
        }

        public override string ToNmeaParameterList()
        {
            StringBuilder sb = new StringBuilder();
            sb.Append(((int)Function).ToString("X2", CultureInfo.InvariantCulture));
            var pgnString = Pgn.ToString("X6", CultureInfo.InvariantCulture);
            pgnString = pgnString.Substring(4, 2) + pgnString.Substring(2, 2) + pgnString.Substring(0, 2);
            sb.Append(pgnString);

            if (Function == GroupFunction.Acknowledge)
            {
                sb.Append("0");
                sb.Append(PgnErrorCode.ToString("X1", CultureInfo.InvariantCulture));
                if (PgnErrorCode == 4)
                {
                    // See corrigendum note. In this case we don't need to continue here
                    sb.Append("FF");
                }
                else
                {
                    sb.Append(NumberOfArguments.ToString("X2", CultureInfo.InvariantCulture));
                    if (NumberOfArguments != Parameters.Count(x => x.Value.HasValue))
                    {
                        throw new ArgumentException(
                            "Problem: Ack message has a different number of arguments than values");
                    }

                    foreach (var p in Parameters.Where(x => x.Value != null))
                    {
                        if (p.ParameterError.HasValue)
                        {
                            sb.Append(p.ParameterError!.Value.ToString("X1", CultureInfo.InvariantCulture));
                        }
                        else
                        {
                            sb.Append('0');
                        }
                    }
                }

                return base.ToNmeaParameterList() + sb.ToString();
            }
            else if (Function == GroupFunction.Request)
            {
                sb.Append(TransmissionInterval.HasValue
                    ? TransmissionInterval.Value.Milliseconds.ToString("X8", CultureInfo.InvariantCulture)
                    : "FFFFFFFF");
                sb.Append(TransmissionOffset.HasValue
                    ? TransmissionOffset.Value.Milliseconds.ToString("X4", CultureInfo.InvariantCulture)
                    : "FFFF");
                sb.Append(NumberOfArguments.ToString("X2", CultureInfo.InvariantCulture));
                // Assuming NumberOfArguments matches the number of filled parameters
                foreach (var p in Parameters)
                {
                    if (p.Value.HasValue)
                    {
                        sb.Append(p.FieldNumber.ToString("X2", CultureInfo.InvariantCulture));
                        sb.Append(p.Value.Value.ToString($"X{p.FieldSize * 2}", CultureInfo.InvariantCulture));
                    }
                }

                return base.ToNmeaParameterList() + sb.ToString();
            }
            else
            {
                throw new NotImplementedException($"Cannot encode message of type {Function} yet");
            }
        }
    }
}
