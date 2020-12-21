// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using System.Device.Gpio;

namespace Iot.Device.KeyMatrix
{
    /// <summary>
    /// Keyboard event
    /// </summary>
    public class KeyMatrixEvent
    {
        /// <summary>
        /// Event type of current button. PinEventTypes.Rising is pressed，PinEventTypes.Falling is released
        /// </summary>
        public PinEventTypes EventType { get; internal set; }

        /// <summary>
        /// Current button's output index
        /// </summary>
        public int Output { get; internal set; }

        /// <summary>
        /// Current button's input index
        /// </summary>
        public int Input { get; internal set; }

        internal KeyMatrixEvent(PinEventTypes eventType, int output, int input)
        {
            EventType = eventType;
            Output = output;
            Input = input;
        }
    }
}
