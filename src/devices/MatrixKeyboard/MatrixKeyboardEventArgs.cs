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
        /// Current button's row index
        /// </summary>
        public int Row;

        /// <summary>
        /// Current button's column index
        /// </summary>
        public int Column;

        internal MatrixKeyboardEventArgs(PinEventTypes eventType, int row, int column)
        {
            EventType = eventType;
            Row = row;
            Column = column;
        }
    }
}
