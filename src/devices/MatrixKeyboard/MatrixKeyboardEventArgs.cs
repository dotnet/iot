// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using System;
using System.Device.Gpio;

namespace Iot.Device.MatrixKeyboard
{
    /// <summary>
    /// Keyboard event
    /// </summary>
    public class MatrixKeyboardEventArgs : EventArgs
    {
        /// <summary>
        /// Event type of current button. PinEventTypes.Rising is pressed，PinEventTypes.Falling is released
        /// </summary>
        public PinEventTypes EventType;

        /// <summary>
        /// Current button's output index
        /// </summary>
        public int Output;

        /// <summary>
        /// Current button's input index
        /// </summary>
        public int Input;

        internal MatrixKeyboardEventArgs(PinEventTypes eventType, int output, int input)
        {
            EventType = eventType;
            Output = output;
            Input = input;
        }
    }
}
