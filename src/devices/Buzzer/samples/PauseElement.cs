// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

namespace Iot.Device.Buzzer.Samples
{
    /// <summary>
    /// Pause element to define silence duration between sounds in melody.
    /// </summary>
    public class PauseElement : MelodyElement
    {
        /// <summary>
        /// Create Pause element.
        /// </summary>
        /// <param name="duration">Duration of pause in melody sequence timeline.</param>
        public PauseElement(Duration duration) : base(duration) {}
    }
}