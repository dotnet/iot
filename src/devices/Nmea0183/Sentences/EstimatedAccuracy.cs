// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnitsNet;

namespace Iot.Device.Nmea0183.Sentences
{
    /// <summary>
    /// PGRME sentence. This sentence is a Garmin proprietary sentence.
    /// See https://developer.garmin.com/downloads/legacy/uploads/2015/08/190-00684-00.pdf
    /// </summary>
    public class EstimatedAccuracy : NmeaSentence
    {
        /// <summary>
        /// This sentence's id
        /// </summary>
        public static SentenceId Id => new SentenceId("RME");

        private static bool Matches(SentenceId sentence) => Id == sentence;
        private static bool Matches(TalkerSentence sentence) => Matches(sentence.Id);

        /// <summary>
        /// Constructs a new PGRME sentence. The talker ID is fixed on this sequence.
        /// </summary>
        /// <param name="horizontalPositionError">Estimated horizontal position error</param>
        /// <param name="verticalPositionError">Estimated vertical position error</param>
        /// <param name="positionError">Estimated total position error</param>
        public EstimatedAccuracy(Length horizontalPositionError, Length verticalPositionError, Length positionError)
            : base(TalkerId.GarminProprietary, Id, DateTimeOffset.UtcNow)
        {
            HorizontalPositionError = horizontalPositionError;
            VerticalPositionError = verticalPositionError;
            PositionError = positionError;
            Valid = true;
        }

        /// <summary>
        /// Internal constructor
        /// </summary>
        public EstimatedAccuracy(TalkerSentence sentence, DateTimeOffset time)
            : this(sentence.TalkerId, Matches(sentence) ? sentence.Fields : throw new ArgumentException($"SentenceId does not match expected id '{Id}'"), time)
        {
        }

        /// <summary>
        /// Constructor that decodes a message.
        /// </summary>
        public EstimatedAccuracy(TalkerId talkerId, IEnumerable<string> fields, DateTimeOffset time)
            : base(talkerId, Id, time)
        {
            IEnumerator<string> field = fields.GetEnumerator();

            double? hpe = ReadValue(field);
            ReadString(field); // Always "M"
            double? vpe = ReadValue(field);
            ReadString(field); // Always "M"
            double? err = ReadValue(field);
            ReadString(field); // Always "M"

            if (hpe.HasValue)
            {
                HorizontalPositionError = Length.FromMeters(hpe.Value);
            }

            if (vpe.HasValue)
            {
                VerticalPositionError = Length.FromMeters(vpe.Value);
            }

            if (err.HasValue)
            {
                PositionError = Length.FromMeters(err.Value);
            }

            Valid = true;
        }

        /// <summary>
        /// This is true for this message type
        /// </summary>
        public override bool ReplacesOlderInstance => true;

        /// <summary>
        /// Estimated horizontal position error
        /// </summary>
        public Length? HorizontalPositionError
        {
            get;
        }

        /// <summary>
        /// Estimated vertical position error
        /// </summary>
        public Length? VerticalPositionError
        {
            get;
        }

        /// <summary>
        /// Estimated total position error
        /// </summary>
        public Length? PositionError
        {
            get;
        }

        /// <summary>
        /// Presents this message as output
        /// </summary>
        public override string ToNmeaParameterList()
        {
            if (Valid)
            {
                return FormattableString.Invariant(
                    $"{FromLength(HorizontalPositionError)},M,{FromLength(VerticalPositionError)},M,{FromLength(PositionError)},M");
            }

            return string.Empty;
        }

        /// <inheritdoc />
        public override string ToReadableContent()
        {
            if (Valid)
            {
                return $"HPE: {FromLength(HorizontalPositionError)}m, VPE {FromLength(VerticalPositionError)}m, Total error {FromLength(PositionError)}m";
            }

            return "No valid data";
        }

        private static string FromLength(Length? length)
        {
            if (length.HasValue == false)
            {
                return string.Empty;
            }
            else
            {
                return length.Value.Meters.ToString("F1", CultureInfo.InvariantCulture);
            }
        }
    }
}
