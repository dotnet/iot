// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

namespace Iot.Device.Buzzer.Samples
{
    /// <summary>
    /// Pause element to define silence duration between sounds in melody.
    /// </summary>
    internal class PauseElement : MelodyElement
    {
        /// <summary>
        /// Create Pause element.
        /// </summary>
        /// <param name="duration">Duration of pause in melody sequence timeline.</param>
        public PauseElement(Duration duration)
            : base(duration)
        {
        }
    }
}
