// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System;
using Iot.Device.Nmea0183;

namespace Iot.Device.Seatalk1.Messages
{
    /// <summary>
    /// This message is sometimes sent instead of <see cref="CompassHeadingAutopilotCourse"/> when the pilot is somehow busy
    /// It seems we don't really need to distinguish.
    /// </summary>
    public class CompassHeadingAutopilotCourseAlt : CompassHeadingAutopilotCourse
    {
        /// <summary>
        /// Initializes a new instance.
        /// </summary>
        public CompassHeadingAutopilotCourseAlt()
        {
        }

        /// <summary>
        /// Initializes a new instance from sentence data.
        /// </summary>
        public CompassHeadingAutopilotCourseAlt(TalkerSentence sentence, DateTimeOffset time)
            : base(sentence, time)
        {
        }

        /// <inheritdoc />
        public override byte CommandByte => 0x95;
    }
}
