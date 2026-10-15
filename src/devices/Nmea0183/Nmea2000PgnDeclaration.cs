// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Iot.Device.Nmea0183.Sentences;

#pragma warning disable CS9113
namespace Iot.Device.Nmea0183
{
    /// <summary>
    /// This class holds static information about a particular NMEA2000 PGN (message type)
    /// </summary>
    public sealed record class Nmea2000PgnDeclaration
    {
        /// <summary>
        /// This class holds static information about a particular NMEA2000 PGN (message type)
        /// </summary>
        /// <param name="pgn">The message number (usually given in hex)</param>
        /// <param name="name">The name of the message</param>
        /// <param name="priority">The typical priority this message uses</param>
        /// <param name="length">The length of the data part of this message, in bytes. Negative to indicate "at least x bytes"</param>
        /// <param name="fastPacket">True if this PGN typically consists of more than one packet</param>
        /// <param name="isAddressed">True if this is an addressed message</param>
        public Nmea2000PgnDeclaration(uint pgn, string name, uint priority, int length, bool fastPacket, bool isAddressed)
        {
            Pgn = pgn;
            Name = name;
            Priority = priority;
            Length = length;
            FastPacket = fastPacket;
            FieldDeclarations = new List<FieldDeclaration>();
            IsAddressed = isAddressed;
        }

        /// <summary>
        /// This class holds static information about a particular NMEA2000 PGN (message type)
        /// </summary>
        /// <param name="pgn">The message number (usually given in hex)</param>
        /// <param name="name">The name of the message</param>
        /// <param name="priority">The typical priority this message uses</param>
        /// <param name="length">The length of the data part of this message, in bytes. Negative to indicate "at least x bytes"</param>
        /// <param name="fastPacket">True if this PGN typically consists of more than one packet</param>
        /// <param name="fieldDeclarations">Field index/length pairs</param>
        /// <param name="isAddressed">True if this message is addressed</param>
        public Nmea2000PgnDeclaration(uint pgn, string name, uint priority, int length, bool fastPacket, bool isAddressed, List<FieldDeclaration> fieldDeclarations)
        {
            Pgn = pgn;
            Name = name;
            Priority = priority;
            Length = length;
            FastPacket = fastPacket;
            FieldDeclarations = fieldDeclarations;
            IsAddressed = isAddressed;
        }

        /// <summary>The message number (usually given in hex)</summary>
        public uint Pgn { get; init; }

        /// <summary>The name of the message</summary>
        public string Name { get; init; }

        /// <summary>The typical priority this message uses</summary>
        public uint Priority { get; init; }

        /// <summary>The length of the data part of this message, in bytes. Negative to indicate "at least x bytes"</summary>
        public int Length { get; init; }

        /// <summary>True if this PGN typically consists of more than one packet</summary>
        public bool FastPacket { get; init; }

        /// <summary>
        /// True if this message is to be addressed to a particular device
        /// </summary>
        public bool IsAddressed { get; init; }

        /// <summary>
        /// The list of fields of this message. Required if this message shall take part in an exchange using
        /// <see cref="GroupFunctionMessage"/> exchange
        /// </summary>
        public IReadOnlyList<FieldDeclaration> FieldDeclarations
        {
            get;
            init;
        }
    }
}
