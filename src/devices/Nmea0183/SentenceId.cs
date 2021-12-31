// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System;

namespace Iot.Device.Nmea0183
{
    /// <summary>
    /// Represents 3 character NMEA0183 sentence identifier
    /// </summary>
    public struct SentenceId : IEquatable<SentenceId>
    {
        /// <summary>
        /// A filter placeholder
        /// </summary>
        public static SentenceId Any => new SentenceId('*', ' ', ' ');

        /// <summary>
        /// The first letter of the ID
        /// </summary>
        public char Id1 { get; private set; }

        /// <summary>
        /// The second letter of the ID
        /// </summary>
        public char Id2 { get; private set; }

        /// <summary>
        /// The third letter of the ID
        /// </summary>
        public char Id3 { get; private set; }

        /// <summary>
        /// Returns the three-letter sentence ID
        /// </summary>
        /// <returns>The three-letter sentence ID</returns>
        public override string ToString() => $"{Id1}{Id2}{Id3}";

        /// <summary>
        /// Constructs NMEA0183 sentence identifier
        /// </summary>
        /// <param name="id1">first character identifying the sentence</param>
        /// <param name="id2">second character identifying the sentence</param>
        /// <param name="id3">third character identifying the sentence</param>
        public SentenceId(char id1, char id2, char id3)
        {
            Id1 = id1;
            Id2 = id2;
            Id3 = id3;
        }

        /// <summary>
        /// Constructs NMEA sentence identifier from string. Must be exactly 3 chars long
        /// </summary>
        /// <param name="identifier">Sentence identifier, i.e. GGA</param>
        public SentenceId(string identifier)
            : this(identifier[0], identifier[1], identifier[2])
        {
            if (identifier.Length != 3)
            {
                throw new ArgumentException("Identifier must be exactly 3 chars long", nameof(identifier));
            }
        }

        /// <summary>
        /// Equality member
        /// </summary>
        public override bool Equals(object? obj)
        {
            if (obj is SentenceId other)
            {
                return Equals(other);
            }

            return false;
        }

        /// <summary>
        /// Hash function
        /// </summary>
        public override int GetHashCode()
        {
            return Id1 << 16 ^ Id2 << 8 ^ Id3;
        }

        /// <summary>
        /// Equality member
        /// </summary>
        public bool Equals(SentenceId other)
        {
            return Id1 == other.Id1 && Id2 == other.Id2 && Id3 == other.Id3;
        }

        /// <summary>
        /// Equality operator
        /// </summary>
        public static bool operator ==(SentenceId obj1, SentenceId obj2)
        {
            return obj1.Equals(obj2);
        }

        /// <summary>
        /// Unequality operator
        /// </summary>
        public static bool operator !=(SentenceId obj1, SentenceId obj2) => !(obj1 == obj2);
    }
}
