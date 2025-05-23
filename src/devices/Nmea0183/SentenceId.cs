// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System;

namespace Iot.Device.Nmea0183
{
    /// <summary>
    /// Represents a NMEA0183 sentence identifier.
    /// The standard has only 3-character sentence identifiers, but some receivers use proprietary messages with 2-6 letters, too.
    /// </summary>
    public struct SentenceId : IEquatable<SentenceId>
    {
        /// <summary>
        /// A filter placeholder
        /// </summary>
        public static SentenceId Any => new SentenceId("*");

        /// <summary>
        /// Ais sentence from our own ship (or about or own ship)
        /// </summary>
        public static SentenceId Vdo => new SentenceId("VDO");

        /// <summary>
        /// Ais sentences from any other ship
        /// </summary>
        public static SentenceId Vdm => new SentenceId("VDM");

        /// <summary>
        /// The sentence Id, typically a 3-letter code
        /// </summary>
        public string Id { get; init; }

        /// <summary>
        /// Returns the three-letter sentence ID
        /// </summary>
        /// <returns>The three-letter sentence ID</returns>
        public override string ToString() => Id;

        /// <summary>
        /// Constructs NMEA0183 sentence identifier from three letters
        /// </summary>
        /// <param name="id1">first character identifying the sentence</param>
        /// <param name="id2">second character identifying the sentence</param>
        /// <param name="id3">third character identifying the sentence</param>
        public SentenceId(char id1, char id2, char id3)
        {
            Id = $"{id1}{id2}{id3}";
        }

        /// <summary>
        /// Constructs NMEA sentence identifier from string.
        /// </summary>
        /// <param name="identifier">Sentence identifier, i.e. GGA</param>
        public SentenceId(string identifier)
        {
            Id = identifier;
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
            return Id.GetHashCode();
        }

        /// <summary>
        /// Equality member
        /// </summary>
        public bool Equals(SentenceId other)
        {
            return Id == other.Id;
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
