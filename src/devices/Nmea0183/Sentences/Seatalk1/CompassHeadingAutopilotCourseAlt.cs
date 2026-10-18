// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Iot.Device.Seatalk1.Messages
{
    /// <summary>
    /// This message is sometimes sent instead of <see cref="CompassHeadingAutopilotCourse"/> when the pilot is somehow busy
    /// It seems we don't really need to distinguish.
    /// </summary>
    public record CompassHeadingAutopilotCourseAlt : CompassHeadingAutopilotCourse
    {
        /// <inheritdoc />
        public override byte CommandByte => 0x95;
    }
}
