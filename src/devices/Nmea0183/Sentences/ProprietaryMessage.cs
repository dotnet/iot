// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SixLabors.ImageSharp;
using UnitsNet;

namespace Iot.Device.Nmea0183.Sentences
{
    /// <summary>
    /// Proprietary message used to pass NMEA2000 messages over NMEA0183, only supported
    /// by some converters and for some messages, for instance engine parameters.
    /// The messages are usually not fully documented, but the SeaSmart (v1.6.0) protocol
    /// specification may help (and some trying around)
    /// </summary>
    public abstract class ProprietaryMessage : NmeaSentence
    {
        /// <summary>
        /// This sentence's id
        /// </summary>
        public static SentenceId Id => new SentenceId("DIN");
        private static bool Matches(SentenceId sentence) => Id == sentence;

        /// <summary>
        /// Checks this message has the correct talker id
        /// </summary>
        /// <param name="sentence">The sentence to check</param>
        /// <returns>True if this input sentence matches this message type (but be careful that this message
        /// type needs further division by arguments)</returns>
        protected static bool Matches(TalkerSentence sentence) => Matches(sentence.Id);

        /// <summary>
        /// Creates a default message of this type
        /// </summary>
        protected ProprietaryMessage()
            : base(TalkerId.Proprietary, Id, DateTimeOffset.UtcNow)
        {
        }

        /// <summary>
        /// Used to create a message while decoding, see base class implementation
        /// </summary>
        protected ProprietaryMessage(TalkerId talker, SentenceId id, DateTimeOffset time)
            : base(talker, id, time)
        {
        }

        /// <summary>
        /// The hex identifier of this message type (first field of a PCDIN message)
        /// </summary>
        public abstract int Identifier
        {
            get;
        }

        /// <summary>
        /// Decodes a value from a longer hex string (PRDIN messages contain one blob of stringly-typed hex numbers)
        /// </summary>
        /// <param name="input">Input string</param>
        /// <param name="start">Start offset of required number</param>
        /// <param name="length">Length of required number. Must be 2, 4 or 8</param>
        /// <param name="inverseEndianess">True to inverse the endianess of the number (reverse the partial string)</param>
        /// <param name="value">The output value</param>
        /// <returns>True on success, false otherwise</returns>
        /// <exception cref="ArgumentException">Length is not 2, 4 or 8</exception>
        /// <remarks>
        /// Other erroneous inputs don't throw an exception but return false, e.g. string shorter than expected or
        /// value is not a hex number. This is to prevent an exception in case of a malformed message.
        /// </remarks>
        protected bool ReadFromHexString(string input, int start, int length, bool inverseEndianess, out int value)
        {
            if (length != 2 && length != 4 && length != 8)
            {
                throw new ArgumentException("Length must be 2, 4, or 8", nameof(length));
            }

            if (input.Length < start + length)
            {
                value = 0;
                return false;
            }

            string part = input.Substring(start, length);

            if (length == 4 && inverseEndianess)
            {
                part = part.Substring(2, 2) + part.Substring(0, 2);
            }

            if (length == 8 && inverseEndianess)
            {
                part = part.Substring(6, 2) + part.Substring(4, 2) + part.Substring(2, 2) + part.Substring(0, 2);
            }

            if (!Int32.TryParse(part, NumberStyles.HexNumber, CultureInfo.InvariantCulture, out int result))
            {
                value = 0;
                return false;
            }

            value = result;
            return true;
        }
    }
}
