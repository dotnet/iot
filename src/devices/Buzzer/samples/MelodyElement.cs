// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

namespace Iot.Device.Buzzer.Samples
{
    /// <summary>
    /// A base class for melody sequence elements.
    /// </summary>
    internal abstract class MelodyElement
    {
        /// <summary>
        /// Duration which defines how long should element take on melody sequence timeline.
        /// </summary>
        public Duration Duration { get; set; }

        public MelodyElement(Duration duration)
        {
            Duration = duration;
        }
    }
}
